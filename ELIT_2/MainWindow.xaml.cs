using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using Word = Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;

namespace ELIT_2
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Applicants> applicantList = new ObservableCollection<Applicants>();
        private ICollectionView applicantsView;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            string connectionString = "server=192.168.0.169; user=remote_user; password=1234; database=applicants;";
            applicantList.Clear();

            try
            {
                using (MySqlConnection connect = new MySqlConnection(connectionString))
                {
                    connect.Open();
                    string sql = "SELECT Id, LastName, FirstName, ExamGrades, School FROM applicantlist;";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connect))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var applicant = new Applicants
                            {
                                Id = reader.GetInt32("Id"),
                                LastName = reader.GetString("LastName"),
                                FirstName = reader.GetString("FirstName"),
                                ExamGrades = reader.GetFloat("ExamGrades"),
                                School = reader.GetString("School")
                            };
                            applicantList.Add(applicant);
                        }
                    }
                }

                // Створюємо CollectionView для фільтрації і сортування
                applicantsView = CollectionViewSource.GetDefaultView(applicantList);
                ApplicantsGrid.ItemsSource = applicantsView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при з'єднанні з БД: " + ex.Message);
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(FilterTextBox.Text, out float minGrade))
            {
                applicantsView.Filter = obj =>
                {
                    if (obj is Applicants a)
                        return a.ExamGrades >= minGrade;
                    return false;
                };
                applicantsView.Refresh();
            }
            else
            {
                MessageBox.Show("Введіть коректне число для фільтрації за середнім балом.");
            }
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            applicantsView.Filter = null;
            FilterTextBox.Text = "";
            applicantsView.Refresh();
        }

        private void SortAsc_Click(object sender, RoutedEventArgs e)
        {
            applicantsView.SortDescriptions.Clear();
            applicantsView.SortDescriptions.Add(new SortDescription(nameof(Applicants.LastName), ListSortDirection.Ascending));
            applicantsView.Refresh();
        }

        private void SortDesc_Click(object sender, RoutedEventArgs e)
        {
            applicantsView.SortDescriptions.Clear();
            applicantsView.SortDescriptions.Add(new SortDescription(nameof(Applicants.LastName), ListSortDirection.Descending));
            applicantsView.Refresh();
        }

        private void ClearSort_Click(object sender, RoutedEventArgs e)
        {
            applicantsView.SortDescriptions.Clear();
            applicantsView.Refresh();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Edit editWindow = new Edit();
            editWindow.ShowDialog();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.ShowDialog();
            LoadData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantsGrid.SelectedItem is Applicants selected)
            {
                MessageBoxResult result = MessageBox.Show($"Ви впевнені, що хочете видалити абітурієнта {selected.LastName} {selected.FirstName}?",
                                                          "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    string connectionString = "server=192.168.0.169; user=remote_user; password=1234; database=applicants;";
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sql = "DELETE FROM applicantlist WHERE Id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", selected.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        LoadData();
                        MessageBox.Show("Абітурієнта видалено.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка видалення: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Виберіть абітурієнта для видалення.");
            }
        }

        private void ElitApplicants_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(ElitGradeTextBox.Text, out float elitPassGrade))
            {
                applicantsView.Filter = obj =>      
                {
                    if (obj is Applicants a)
                        return a.ExamGrades >= elitPassGrade;
                    return false;
                };
                applicantsView.Refresh();
            }
            else
            {
                MessageBox.Show("Введіть коректне число для прохідного балу.");
            }
        }

        private void SchoolAndGradeFilter_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(MinExamGradeTextBox.Text, out float minGrade) &&
                int.TryParse(SchoolNumberTextBox.Text, out int schoolNumber))
            {
                applicantsView.Filter = obj =>
                {
                    if (obj is Applicants a)
                    {
                        return a.ExamGrades > minGrade &&
                               !string.IsNullOrEmpty(a.School) &&
                               a.School.Contains($"№{schoolNumber}");
                    }
                    return false;
                };
                applicantsView.Refresh();
            }
            else
            {
                MessageBox.Show("Введіть правильні значення для бала та номера школи.");
            }
        }

        private int? ExtractSchoolNumber(string school)
        {
            var match = Regex.Match(school, @"№\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                return number;

            return null;
        }

        private void ExportToWord(System.Collections.Generic.List<Applicants> applicants)
        {
            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                wordApp = new Word.Application();
                wordApp.Visible = false;
                doc = wordApp.Documents.Add();

                Word.Paragraph para = doc.Content.Paragraphs.Add();
                para.Range.Text = "Відібрані абітурієнти";
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 16;
                para.Range.InsertParagraphAfter();

                Word.Table table = doc.Tables.Add(para.Range, applicants.Count + 1, 5);
                table.Borders.Enable = 1;

                // Заголовки
                table.Cell(1, 1).Range.Text = "ID";
                table.Cell(1, 2).Range.Text = "Прізвище";
                table.Cell(1, 3).Range.Text = "Ім’я";
                table.Cell(1, 4).Range.Text = "Середній бал";
                table.Cell(1, 5).Range.Text = "Школа";

                // Дані
                for (int i = 0; i < applicants.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = applicants[i].Id.ToString();
                    table.Cell(i + 2, 2).Range.Text = applicants[i].LastName;
                    table.Cell(i + 2, 3).Range.Text = applicants[i].FirstName;
                    table.Cell(i + 2, 4).Range.Text = applicants[i].ExamGrades.ToString("F2");
                    table.Cell(i + 2, 5).Range.Text = applicants[i].School;
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = System.IO.Path.Combine(desktopPath, "Результати_відбору.docx");

                doc.SaveAs2(fileName);
                doc.Close();
                wordApp.Quit();

                MessageBox.Show("Документ збережено на робочому столі.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час експорту до Word: " + ex.Message);
            }
            finally
            {
                if (doc != null) Marshal.ReleaseComObject(doc);
                if (wordApp != null) Marshal.ReleaseComObject(wordApp);

                doc = null;
                wordApp = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void ExportFiltered_Click(object sender, RoutedEventArgs e)
        {
            var filteredApplicants = applicantsView.Cast<Applicants>().ToList();

            if (filteredApplicants.Any())
            {
                ExportToWord(filteredApplicants);
            }
            else
            {
                MessageBox.Show("Немає даних для експорту. Спочатку застосуйте фільтр.");
            }
        }
    }
}
