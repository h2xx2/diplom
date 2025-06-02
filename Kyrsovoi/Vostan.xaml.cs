using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
        public Vostan()
        {
            InitializeComponent();
            if (!int.TryParse(ConfigurationManager.AppSettings["IdleTimeout"], out _idleTimeout))
            {
                _idleTimeout = 30; // Значение по умолчанию
            }

            // Настройка таймера
            _idleTimer = new Timer(_idleTimeout * 1000); // Перевод в миллисекунды
            _idleTimer.Elapsed += OnIdleTimeout;
            _idleTimer.Start();

            // Обработчики событий для отслеживания активности
            this.MouseMove += ResetIdleTimer;
            this.KeyDown += ResetIdleTimer;
        }

        private void ResetIdleTimer(object sender, EventArgs e)
        {
            // Сбрасываем таймер при активности пользователя
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        private void OnIdleTimeout(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Остановка таймера
                _idleTimer.Stop();

                // Перенаправление на форму авторизации
                var loginWindow = new MainWindow(); // Предполагается, что LoginWindow — это форма авторизации
                loginWindow.Show();
                this.Close(); // Закрываем текущую форму
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            // Очистка ресурсов при закрытии
            _idleTimer?.Dispose();
            base.OnClosed(e);
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
                // Создаем диалоговое окно для выбора файла
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "SQL Files (*.sql)|*.sql", // Ограничиваем выбор только SQL-файлами
                    Title = "Выберите SQL-файл для восстановления базы данных",
                    InitialDirectory = System.IO.Path.GetFullPath("."), // Начальная директория — текущая папка проекта
                    Multiselect = false // Запрещаем выбор нескольких файлов
                };

                // Показываем диалоговое окно и проверяем, выбран ли файл
                bool? dialogResult = openFileDialog.ShowDialog();
                if (dialogResult != true)
                {
                    // Если пользователь отменил выбор файла, выходим из метода
                    return;
                }

                // Получаем путь к выбранному файлу
                string filepath = openFileDialog.FileName;
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
                MessageBox.Show($"Ошибка при восстановлении базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Import import = new Import();
            this.Close();
            import.ShowDialog();    
        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            systemadmin mainWindow = new systemadmin();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
        }
    }
}
