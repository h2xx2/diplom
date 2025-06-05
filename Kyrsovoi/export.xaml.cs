using System;
using System.Collections.Generic;
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
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для export.xaml
    /// </summary>
    public partial class export : Window
    {
        public export()
        {
            InitializeComponent();
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Vostan mainWindow = new Vostan();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка, выбрана ли папка
                if (string.IsNullOrEmpty(tb.Text))
                {
                    System.Windows.MessageBox.Show("Пожалуйста, выберите папку для сохранения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка, выбрана ли таблица
                if (cb.SelectedItem == null)
                {
                    System.Windows.MessageBox.Show("Пожалуйста, выберите таблицу для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedTable = (cb.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Формирование строки подключения
                string connectionString = $"Server={Properties.Settings.Default.host};Uid={Properties.Settings.Default.user};Pwd={Properties.Settings.Default.passwordDB};Database={Properties.Settings.Default.database};";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Список таблиц для экспорта
                    string[] tablesToExport;
                    if (selectedTable == "Все таблицы")
                    {
                        DataTable schema = conn.GetSchema("Tables");
                        tablesToExport = schema.AsEnumerable()
                            .Select(row => row.Field<string>("TABLE_NAME"))
                            .ToArray();
                    }
                    else
                    {
                        tablesToExport = new[] { selectedTable };
                    }

                    foreach (string tableName in tablesToExport)
                    {
                        string backupPath = System.IO.Path.Combine(tb.Text, $"glamping_{tableName}_{timestamp}.csv");
                        StringBuilder csvContent = new StringBuilder();

                        // Экспорт данных таблицы
                        using (MySqlCommand cmdData = new MySqlCommand($"SELECT * FROM `{tableName}`", conn))
                        {
                            using (MySqlDataReader reader = cmdData.ExecuteReader())
                            {
                                // Добавление заголовков столбцов
                                var columnNames = Enumerable.Range(0, reader.FieldCount)
                                    .Select(i => $"\"{reader.GetName(i).Replace("\"", "\"\"")}\"");
                                csvContent.AppendLine(string.Join(";", columnNames));

                                // Экспорт данных
                                while (await reader.ReadAsync())
                                {
                                    var values = Enumerable.Range(0, reader.FieldCount)
                                        .Select(i => reader.IsDBNull(i)
                                            ? ""
                                            : $"\"{reader[i].ToString().Replace("\"", "\"\"")}\""); // Экранирование кавычек
                                    csvContent.AppendLine(string.Join(";", values));
                                }
                            }
                        }

                        // Сохранение в отдельный файл с кодировкой UTF-8
                        System.IO.File.WriteAllText(backupPath, csvContent.ToString(), new UTF8Encoding(true)); // true добавляет BOM для UTF-8
                    }

                    string message = tablesToExport.Length > 1
                        ? $"Данные успешно экспортированы в отдельные файлы в папке: {tb.Text}"
                        : $"Данные успешно экспортированы: {System.IO.Path.Combine(tb.Text, $"glamping_{selectedTable}_{timestamp}.csv")}";
                    System.Windows.MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для сохранения резервной копии";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tb.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
