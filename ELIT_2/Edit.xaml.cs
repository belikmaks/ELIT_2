using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace ELIT_2
{
    public partial class Edit : Window
    {
        private const string connectionString = "server=192.168.0.169; user=remote_user; password=1234; database=applicants;";

        public Edit()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SearchIdBox.Text, out int id))
            {
                MessageBox.Show("Введіть коректний числовий ID.");
                return;
            }

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    string sql = "SELECT * FROM applicantlist WHERE Id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                LastNameBox.Text = reader.GetString("LastName");
                                FirstNameBox.Text = reader.GetString("FirstName");
                                GpaBox.Text = reader.GetFloat("ExamGrades").ToString("0.00");
                                SchoolBox.Text = reader.GetString("School");

                                SaveButton.IsEnabled = true;
                                ClearButton.IsEnabled = true;
                            }
                            else
                            {
                                MessageBox.Show("Абітурієнта з таким ID не знайдено.");
                                ClearFields();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка під час з'єднання з базою даних: " + ex.Message);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SearchIdBox.Text, out int id) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                !float.TryParse(GpaBox.Text, out float examGrades) ||
                string.IsNullOrWhiteSpace(SchoolBox.Text))
            {
                MessageBox.Show("Будь ласка, заповніть усі обов’язкові поля коректно.");
                return;
            }

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    string sql = @"UPDATE applicantlist 
                                   SET LastName = @lastName, FirstName = @firstName, 
                                       ExamGrades = @examGrades, School = @school 
                                   WHERE Id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("@lastName", LastNameBox.Text);
                        cmd.Parameters.AddWithValue("@firstName", FirstNameBox.Text);
                        cmd.Parameters.AddWithValue("@examGrades", examGrades);
                        cmd.Parameters.AddWithValue("@school", SchoolBox.Text);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Дані успішно оновлено.");
                        }
                        else
                        {
                            MessageBox.Show("Оновлення не виконано. Перевірте ID.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при оновленні даних: " + ex.Message);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            LastNameBox.Text = "";
            FirstNameBox.Text = "";
            GpaBox.Text = "";
            SchoolBox.Text = "";
            SaveButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
        }
    }
}
