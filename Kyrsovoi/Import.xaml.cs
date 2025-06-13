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
        private StringBuilder importLog = new StringBuilder();
        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Vostan mainWindow = new Vostan();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (cb.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта данных.");
                return;
            }
            if (tb.Text == null)
            {
                MessageBox.Show("Выберите sql файл для импорта.");
                return;
            }
            importLog.Clear();
            ImportCsvDataToTable(filePath, cb.Text);

            // Показываем лог после завершения импорта
            if (importLog.Length > 0)
            {
                MessageBox.Show($"Лог импорта:\n{importLog.ToString()}", "Лог", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void ImportCsvDataToTable(string csvFilePath, string tableName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(csvFilePath, Encoding.UTF8))
                {
                    // Читаем заголовки
                    string headerLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(headerLine))
                    {
                        MessageBox.Show("CSV-файл пустой или не содержит заголовков.");
                        return;
                    }

                    string[] csvHeaders = headerLine.Split(';').Select(h => h.Trim('"')).ToArray();
                    csvHeaders = csvHeaders.Skip(1).ToArray(); // Игнорируем первый столбец (id)

                    // Получаем колонки таблицы
                    var tableColumns = GetTableColumns(tableName);
                    var insertColumns = tableColumns.Where(c => !c.IsAutoIncrement).Select(c => c.ColumnName).ToList();

                    // Проверяем, что все заголовки CSV существуют в таблице
                    foreach (var header in csvHeaders)
                    {
                        if (!insertColumns.Contains(header))
                        {
                            MessageBox.Show($"В таблице '{tableName}' нет столбца '{header}' из CSV.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // Определяем уникальные колонки для проверки дубликатов
                    var uniqueColumns = GetUniqueColumns(tableName);

                    using (MySqlConnection conn = new MySqlConnection(conString))
                    {
                        conn.Open();

                        string line;
                        int addedRecords = 0, skippedRecords = 0;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            string[] values = line.Split(';').Select(v => v.Trim('"')).ToArray();
                            values = values.Skip(1).ToArray();

                            if (values.Length != csvHeaders.Length)
                            {
                                MessageBox.Show($"Ошибка: строка имеет {values.Length} колонок вместо {csvHeaders.Length}. Пропущено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                skippedRecords++;
                                continue;
                            }

                            // Сопоставляем заголовки и значения
                            var columnValues = new Dictionary<string, string>();
                            for (int i = 0; i < csvHeaders.Length; i++)
                            {
                                columnValues[csvHeaders[i]] = FormatValueForMySQL(values[i]);
                            }

                            // Проверка дубликата
                            bool isDuplicate = CheckForDuplicate(conn, tableName, uniqueColumns, columnValues);
                            if (isDuplicate)
                            {
                                skippedRecords++;
                                continue;
                            }

                            // Готовим запрос
                            var columnsForInsert = columnValues.Keys.ToList();
                            var valuesForInsert = columnValues.Values.ToList();

                            string insertQuery = $"INSERT INTO `{tableName}` (`{string.Join("`,`", columnsForInsert)}`) VALUES ({string.Join(",", valuesForInsert)})";

                            using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                            {
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                    addedRecords++;
                                }
                                catch (MySqlException ex)
                                {
                                    MessageBox.Show($"Ошибка вставки: {ex.Message}\nSQL: {insertQuery}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    skippedRecords++;
                                }
                            }
                        }

                        MessageBox.Show($"Импорт завершён.\nДобавлено: {addedRecords}\nПропущено: {skippedRecords}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для получения структуры таблицы
        private List<(string ColumnName, bool IsAutoIncrement)> GetTableColumns(string tableName)
        {
            var columns = new List<(string, bool)>();
            using (MySqlConnection conn = new MySqlConnection(conString))
            {
                try
                {
                    conn.Open();
                    string query = $"SELECT COLUMN_NAME, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = @database";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tableName", tableName);
                        cmd.Parameters.AddWithValue("@database", Properties.Settings.Default.database);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader.GetString("COLUMN_NAME");
                                string extra = reader.GetString("EXTRA");
                                bool isAutoIncrement = extra.Contains("auto_increment");
                                columns.Add((name, isAutoIncrement));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения структуры таблицы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return columns;
        }

        // Метод для определения уникальных столбцов для проверки дубликатов
        private List<string> GetUniqueColumns(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "booking_status":
                    return new List<string> { "booking_status" };
                case "bookings":
                    return new List<string> { "guest_id", "unit_id", "check_in_date", "check_out_date" };
                case "employees":
                    return new List<string> { "email" };
                case "glampingunits":
                    return new List<string> { "unit_name", "unit_type" };
                case "guests":
                    return new List<string> { "email" };
                case "pay_status":
                    return new List<string> { "pay_statuscol" };
                default:
                    return new List<string>();
            }
        }

        // Метод для проверки дубликатов
        private bool CheckForDuplicate(MySqlConnection conn, string tableName, List<string> uniqueColumns, Dictionary<string, string> columnValues)
        {
            if (uniqueColumns == null || uniqueColumns.Count == 0)
                return false;

            var conditions = new List<string>();
            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = conn;

                foreach (var col in uniqueColumns)
                {
                    if (!columnValues.ContainsKey(col)) continue;

                    string paramName = $"@{col}";
                    string val = columnValues[col];

                    if (val.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        conditions.Add($"`{col}` IS NULL");
                    else
                    {
                        conditions.Add($"`{col}` = {paramName}");
                        val = val.Trim('\''); // Убираем кавычки, если они есть
                        command.Parameters.AddWithValue(paramName, val);
                    }
                }

                if (conditions.Count == 0)
                    return false;

                string checkQuery = $"SELECT COUNT(*) FROM `{tableName}` WHERE {string.Join(" AND ", conditions)}";
                command.CommandText = checkQuery;

                long count = (long)command.ExecuteScalar();
                return count > 0;
            }
        }

        // Метод для форматирования значений
        private string FormatValueForMySQL(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.ToLower() == "null")
                return "NULL";

            // Заменяем запятую на точку для MySQL
            string replaced = value.Replace(",", ".");

            // Если это число (decimal)
            if (decimal.TryParse(replaced, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal number))
                return replaced; // Без кавычек (MySQL воспримет как число)

            // Если это дата
            if (DateTime.TryParse(value, out DateTime date))
                return $"'{date:yyyy-MM-dd HH:mm:ss}'";

            // Всё остальное — строка, экранируем
            return $"'{value.Replace("'", "''")}'";
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
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
    }
}