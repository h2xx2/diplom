using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для Vostan.xaml
    /// </summary>
    public partial class Vostan : System.Windows.Window
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
        public static string connection = Class1.connection;
        public static string connectionString = Class1.connectionVostan;


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
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
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
                MessageBox.Show($"Ошибка при восстановлении базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _idleTimer.Stop();
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
            _idleTimer.Stop();
            systemadmin mainWindow = new systemadmin();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открытие диалогового окна для выбора места сохранения
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "SQL files (*.sql)|*.sql",
                    FileName = $"glamping_backup_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.sql",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string backupPath = saveFileDialog.FileName;

                    // Используем параметры из строки подключения
                    string host = Properties.Settings.Default.host;
                    string database = Properties.Settings.Default.database;
                    string user = Properties.Settings.Default.user;
                    string password = Properties.Settings.Default.passwordDB;

                    // Убедитесь, что папка для резервных копий существует
                    string backupDir = System.IO.Path.GetDirectoryName(backupPath);
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }

                    // Относительный путь к mysqldump
                    string mysqldumpPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "mysqldump.exe");
                    if (!File.Exists(mysqldumpPath))
                    {
                        throw new FileNotFoundException($"Файл mysqldump.exe не найден по пути: {mysqldumpPath}. Убедитесь, что файл добавлен в папку Tools проекта.");
                    }

                    // Формирование аргументов для mysqldump
                    string arguments = $"--host={host} --user={user} --password={password} --databases {database} --result-file=\"{backupPath}\"";

                    // Настройка процесса
                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        FileName = mysqldumpPath,
                        Arguments = arguments,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // Запуск процесса асинхронно
                    rezerv.IsEnabled = false;

                    using (Process process = new Process { StartInfo = processInfo })
                    {
                        process.Start();
                        string error = await process.StandardError.ReadToEndAsync();
                        await Task.Run(() => process.WaitForExit());

                        if (process.ExitCode == 0)
                        {
                            MessageBox.Show($"Резервная копия создана: {backupPath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выбор места сохранения отменен.");
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message + "\nУбедитесь, что mysqldump.exe добавлен в папку Tools проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                rezerv.IsEnabled = true;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _idleTimer.Stop();
            export export = new export();
            this.Hide();
            export.ShowDialog();
            this.Close();  
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}