using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ELIT_2
{
    public partial class MainWindow : Window
    {
        private List<Applicants> applicantList = new List<Applicants>();

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
                    ApplicantsGrid.ItemsSource = applicantList.ToList();
                }
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
                ApplicantsGrid.ItemsSource = applicantList
                    .Where(a => a.ExamGrades >= minGrade)
                    .ToList();
            }
            else
            {
                MessageBox.Show("Введіть коректне число для фільтрації за середнім балом.");
            }
        }


        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            FilterTextBox.Text = "";
        }

        private void SortAsc_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantsGrid.ItemsSource is List<Applicants> list)
            {
                ApplicantsGrid.ItemsSource = list.OrderBy(a => a.LastName).ToList();
            }
        }

        private void SortDesc_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantsGrid.ItemsSource is List<Applicants> list)
            {
                ApplicantsGrid.ItemsSource = list.OrderByDescending(a => a.LastName).ToList();
            }
        }

        private void ClearSort_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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

    }
}
