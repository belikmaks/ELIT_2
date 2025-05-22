using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace ELIT_2
{
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(GradeBox.Text) ||
                string.IsNullOrWhiteSpace(SchoolBox.Text))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля.");
                return;
            }

            if (!float.TryParse(GradeBox.Text, out float grade))
            {
                MessageBox.Show("Некоректний формат середнього балу.");
                return;
            }

            string connectionString = "server=192.168.0.169; user=remote_user; password=1234; database=applicants;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO applicantlist (LastName, FirstName, ExamGrades, School) VALUES (@last, @first, @grade, @school)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@last", LastNameBox.Text);
                        cmd.Parameters.AddWithValue("@first", FirstNameBox.Text);
                        cmd.Parameters.AddWithValue("@grade", grade);
                        cmd.Parameters.AddWithValue("@school", SchoolBox.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Абітурієнта додано.");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні: " + ex.Message);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}