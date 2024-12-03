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
using static System.Net.Mime.MediaTypeNames;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для Import.xaml
    /// </summary>
    public partial class Import : Window
    {
        public Import()
        {
            InitializeComponent();
        }
        public static string filePath = "";
        public static string conString = Class1.connection;
        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "csv files (*.csv)|*.csv",
                Title = "Выберите csv файл",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                 filePath = openFileDialog.FileName;
            }
            tb.Text = filePath;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (cb.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта данных.");
                return;
            }


            ImportCsvDataToTable(filePath, cb.Text);
        }
        private void ImportCsvDataToTable(string csvFilePath, string tableName)
        {
            try
            {
                // Открываем CSV файл для чтения
                using (StreamReader reader = new StreamReader(csvFilePath, Encoding.Default))
                {
                    // Чтение первой строки (заголовков CSV файла)
                    string headerLine = reader.ReadLine();
                    string[] csvHeaders = headerLine.Split(';');

                    // Проверяем количество колонок в выбранной таблице
                    int tableColumnCount = GetTableColumnCount(tableName);
                    if (csvHeaders.Length != tableColumnCount)
                    {
                        MessageBox.Show($"Количество колонок в CSV ({csvHeaders.Length}) не совпадает с количеством колонок в таблице ({tableColumnCount}).");
                        return;
                    }

                    // Открытие соединения с базой данных
                    using (MySqlConnection conn = new MySqlConnection(conString))
                    {
                        conn.Open();

                        // Чтение строк данных из CSV
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] values = line.Split(';');

                            if (values.Length != tableColumnCount)
                            {
                                MessageBox.Show($"Ошибка: строка данных имеет несоответствующее количество колонок. Ожидалось {tableColumnCount}, но найдено {values.Length}.");
                                continue;
                            }

                            // Преобразование значений по типам данных
                            string[] formattedValues = values.Select(v => FormatValue(v)).ToArray();

                            // Создание SQL запроса для вставки данных
                            string query = $"INSERT INTO `{tableName}` VALUES ({string.Join(",", formattedValues)})";

                            MySqlCommand command = new MySqlCommand(query, conn);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Импорт данных успешно завершён.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Метод для форматирования значений в зависимости от типа данных
        private string FormatValue(string value)
        {
            // Проверка на null или пустое значение
            if (string.IsNullOrWhiteSpace(value))
                return "NULL";

            // Проверка, является ли значение числом
            if (double.TryParse(value, out _))
                return value; // Числа оставляем без изменений

            // Проверка, является ли значение датой
            if (DateTime.TryParse(value, out DateTime dateTime))
                return $"'{dateTime:yyyy-MM-dd HH:mm:ss}'"; // Приводим дату к формату для SQL

            // Все остальное считаем строкой и заключаем в кавычки
            return $"{value.Replace("'", "''")}"; // Экранируем одинарные кавычки в строке
        }

        // Функция для получения количества колонок в таблице
        private int GetTableColumnCount(string tableName)
        {
            int columnCount = 0;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(conString))
                {
                    conn.Open();
                    string query = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'glamping' AND TABLE_NAME = '{tableName}'";
                    MySqlCommand command = new MySqlCommand(query, conn);
                    columnCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении структуры таблицы: {ex.Message}");
            }

            return columnCount;
        }
    }
}
