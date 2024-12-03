using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для Vostan.xaml
    /// </summary>
    public partial class Vostan : Window
    {
        public Vostan()
        {
            InitializeComponent();
        }
        public static string filePath = "";
        public static string conString = Class1.connection;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RestoreDatabase(filePath);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "sql files (*.sql)|*.sql",
                Title = "Выберите sql файл",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            tb.Text = filePath;
        }
        private void RestoreDatabase(string sqlFilePath)
        {
            try
            {
                // Открываем SQL файл для чтения
                string sqlScript = File.ReadAllText(sqlFilePath);

                // Подтверждение выполнения
                string message = "Вы уверены, что хотите восстановить структуру базы данных? Это может удалить текущие данные.";
                string caption = "Подтверждение восстановления";

                MessageBoxResult result = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(conString))
                        {
                            conn.Open();

                            MySqlCommand command = new MySqlCommand(sqlScript, conn);

                            // Выполняем запросы из SQL файла
                            command.ExecuteNonQuery();

                            MessageBox.Show("Структура базы данных успешно восстановлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при восстановлении структуры базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении базы данных: {ex.Message}");
            }
        }
    }
}
