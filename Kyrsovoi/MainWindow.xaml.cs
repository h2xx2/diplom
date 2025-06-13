using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private string _captchaText;
        private bool _isBlocked;
        private DispatcherTimer _blockTimer; // Таймер для блокировки
        int error = 0;
        int error1 = 0;

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (tb2.Password.Length > 0)
            {
                watermatk.Visibility = Visibility.Collapsed;
            }
            else
            {
                watermatk.Visibility = Visibility.Visible;
            }
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true; // Помечаем событие как обработанное
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var result = MessageBox.Show(
                        "Вы действительно хотите выйти?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Путь для сохранения резервной копии внутри проекта
                            string backupDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                            if (!Directory.Exists(backupDir))
                            {
                                Directory.CreateDirectory(backupDir);
                            }

                            // Автоматическое имя файла с датой и временем
                            string backupFileName = $"glamping_backup_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.sql";
                            string backupPath = System.IO.Path.Combine(backupDir, backupFileName);

                            // Используем параметры из строки подключения или настроек
                            string host = Properties.Settings.Default.host;
                            string database = Properties.Settings.Default.database;
                            string user = Properties.Settings.Default.user;
                            string password = Properties.Settings.Default.passwordDB;

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

                            // Запуск процесса
                            using (Process process = new Process { StartInfo = processInfo })
                            {
                                process.Start();
                                string error = process.StandardError.ReadToEnd();
                                process.WaitForExit();

                                if (process.ExitCode == 0)
                                {
                                }
                                else
                                {
                                    MessageBox.Show($"Ошибка: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }));
            }
        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private string GetHashPass(string password)
        {
            byte[] bytesPass = Encoding.UTF8.GetBytes(password);
            using (SHA256Managed hashstring = new SHA256Managed())
            {
                byte[] hash = hashstring.ComputeHash(bytesPass);
                string hashPasswd = string.Empty;
                foreach (byte x in hash)
                {
                    hashPasswd += String.Format("{0:x2}", x);
                }
                return hashPasswd;
            }
        }

        private void GenerateCaptcha()
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            _captchaText = new string(Enumerable.Range(0, 4)
                .Select(_ => characters[random.Next(characters.Length)])
                .ToArray());

            Bitmap bitmap = new Bitmap(120, 50);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.White);

                for (int i = 0; i < _captchaText.Length; i++)
                {
                    float x = 20 * i + random.Next(-5, 5);
                    float y = random.Next(5, 20);
                    float angle = random.Next(-30, 30);

                    using (Font font = new Font(new System.Drawing.FontFamily("Arial"), random.Next(18, 24), System.Drawing.FontStyle.Bold))
                    {
                        using (System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(
                                   random.Next(50, 200), random.Next(0, 255), random.Next(0, 255), random.Next(0, 255))))
                        {
                            g.TranslateTransform(x, y);
                            g.RotateTransform(angle);
                            g.DrawString(_captchaText[i].ToString(), font, brush, 0, 0);
                            g.ResetTransform();
                        }
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(
                               random.Next(50, 200), random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)), random.Next(1, 3)))
                    {
                        g.DrawLine(pen, random.Next(0, 120), random.Next(0, 50), random.Next(0, 120), random.Next(0, 50));
                    }
                }
            }

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                CaptchaImage.Source = image;
            }
        }

        private void BlockInputs()
        {
            _isBlocked = true;
            tb1.IsEnabled = false;
            tb2.IsEnabled = false;
            tb3.IsEnabled = false;
            bt1.IsEnabled = false; // Предполагается, что кнопка входа имеет x:Name="LoginButton" в XAML
            MessageBox.Show("Введена неверная капча. Поля заблокированы на 10 секунд.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);

            _blockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _blockTimer.Tick += (s, e) =>
            {
                _isBlocked = false;
                tb1.IsEnabled = true;
                tb2.IsEnabled = true;
                tb3.IsEnabled = true;
                bt1.IsEnabled = true;
                _blockTimer.Stop();
                GenerateCaptcha();
                tb3.Clear();
                MessageBox.Show("Поля разблокированы. Введите данные снова.", "Разблокировка", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            _blockTimer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_isBlocked)
            {
                return;
            }

            string login = tb1.Text;
            Class1.l = login;
            string hashPassword = tb2.Password;
            string hashbd = string.Empty;

            if (login.Length != 0)
            {
                string conString = Class1.connection;
                if (login != Properties.Settings.Default.login && hashbd != Properties.Settings.Default.password)
                {
                    using (MySqlConnection con = new MySqlConnection(conString))
                    {
                        using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM employees Where login = '" + login + "';", con))
                        {
                            cmd.CommandType = CommandType.Text;

                            using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    try
                                    {
                                        sda.Fill(dt);
                                        hashPassword = GetHashPass(hashPassword);
                                        try
                                        {
                                            Class1.id_employes = Convert.ToInt32(dt.Rows[0].ItemArray.GetValue(0));
                                            Class1.fioEmploes = dt.Rows[0].ItemArray.GetValue(1).ToString() + " " + dt.Rows[0].ItemArray.GetValue(2).ToString();
                                            hashbd = dt.Rows[0].ItemArray.GetValue(8).ToString();
                                            string role = dt.Rows[0].ItemArray.GetValue(9).ToString();

                                            if (_captchaText == tb3.Text || error == 0)
                                            {
                                                if (hashPassword == hashbd)
                                                {
                                                    if (role != "Администратор")
                                                    {
                                                        Class1.role = 1;
                                                        Prosmotr main = new Prosmotr();
                                                        this.Close();
                                                        main.ShowDialog();
                                                    }
                                                    else
                                                    {
                                                        Class1.role = 0;
                                                        Prosmotr main = new Prosmotr();
                                                        this.Close();
                                                        main.ShowDialog();
                                                    }
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Введен неправильный логин или пароль", "Ошибка авторизации");
                                                    error++;
                                                    error1 = Class1.k;
                                                    if (error >= 1 || error1 >= 1)
                                                    {
                                                        tb1.Clear();
                                                        tb2.Clear();
                                                        if (Math.Round(this.Width) == 350)
                                                        {
                                                            FillFuncBig();
                                                        }

                                                        GenerateCaptcha();
                                                        tb3.Clear();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("Неверно введена капча", "Ошибка авторизации");
                                                if (_captchaText != tb3.Text && error > 1)
                                                {
                                                    BlockInputs();
                                                }
                                                error++;
                                                GenerateCaptcha();
                                                tb3.Clear();
                                            }
                                        }
                                        catch (IndexOutOfRangeException)
                                        {
                                            MessageBox.Show("Такого пользователя не сущесвтует", "Ошибка");
                                            error++;
                                            error1 = Class1.k;
                                            if (error >= 1 || error1 >= 1)
                                            {
                                                tb1.Clear();
                                                tb2.Clear();
                                                if (Math.Round(this.Width) == 350)
                                                {
                                                    FillFuncBig();
                                                }
                                                GenerateCaptcha();
                                                if (_captchaText != tb3.Text && error > 1)
                                                {
                                                    BlockInputs();
                                                }
                                                tb3.Clear();
                                            }
                                        }
                                    }
                                    catch (MySqlException)
                                    {
                                        MessageBox.Show("Отсутствует соединение с БД");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    systemadmin vostan = new systemadmin();
                    this.Close();
                    vostan.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Введите логин или пароль", "Ошибка авторизации");
                error++;
                error1 = Class1.k;
                if (error >= 1 || error1 >= 1)
                {
                    tb1.Clear();
                    tb2.Clear();
                    if (Math.Round(this.Width) == 350)
                    {
                        FillFuncBig();
                    }
                    GenerateCaptcha();
                    if (_captchaText != tb3.Text && error > 1)
                    {
                        BlockInputs();
                    }
                    tb3.Clear();
                }
            }
        }

        private void tb1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        private void FillFuncBig()
        {
            double currentHeight = 350;
            double targetHeight = 700; // Конечная высота
            AnimateListViewHeight(this, currentHeight, targetHeight, 0.5);
        }

        private void AnimateListViewHeight(Window grid, double fromWidth, double toHeight, double durationSeconds)
        {
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                From = fromWidth,
                To = toHeight,
                Duration = new Duration(TimeSpan.FromSeconds(durationSeconds)),
                EasingFunction = new QuadraticEase()
            };
            grid.BeginAnimation(WidthProperty, heightAnimation);
        }

        private void CaptchaImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GenerateCaptcha();
            tb3.Clear();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GenerateCaptcha();
            tb3.Clear();
        }

        private void krest_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}