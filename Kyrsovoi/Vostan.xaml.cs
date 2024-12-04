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
        public static string conString = $"host=localhost;uid=root;pwd=root;database=;";
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RestoreDatabase();
        }
        private void RestoreDatabase()
        {
            try
            {
                // Открываем SQL файл для чтения
                string filepath = System.IO.Path.GetFullPath("glap.sql");
                string sqlScript = File.ReadAllText(filepath);

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
                            Import import = new Import();
                            this.Close();
                            import.ShowDialog();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Import import = new Import();
            this.Close();
            import.ShowDialog();    
        }
    }
}
