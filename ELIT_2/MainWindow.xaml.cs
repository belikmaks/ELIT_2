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
            string connectionString = "server=localhost; user=root; password=; database=applicants;";
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
            string filter = FilterTextBox.Text.ToLower();
            if (ApplicantsGrid.ItemsSource is List<Applicants> list)
            {
                ApplicantsGrid.ItemsSource = list
                    .Where(a => a.LastName.ToLower().Contains(filter) || a.FirstName.ToLower().Contains(filter))
                    .ToList();
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
    }
}
