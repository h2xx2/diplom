using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
        private string _captchaText;
        private int _failedAttempts;
        private DispatcherTimer _blockTimer;
        private bool _isBlocked;
        public Window1()
        {
            InitializeComponent(); GenerateCaptcha();
            _blockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _blockTimer.Tick += UnblockLogin;
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
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBlocked)
            {
                ErrorLabel.Content = "Вы заблокированы. Подождите.";
                return;
            }

            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;
            string captcha = CaptchaTextBox.Text;

            if (_failedAttempts >= 1)
            {
                if (captcha != _captchaText)
                {
                    ErrorLabel.Content = "Неверная CAPTCHA.";
                    StartBlock();
                    return;
                }
            }

            if (login != "admin" || password != "password")
            {
                _failedAttempts++;
                ErrorLabel.Content = "Неверный логин или пароль.";
                if (_failedAttempts == 1)
                {
                    ShowCaptcha();
                }
                return;
            }

            ErrorLabel.Content = "Добро пожаловать!";
            ResetState();
        }

        private void RefreshCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            _captchaText = new string(Enumerable.Range(0, 4)
                .Select(_ => (char)new Random().Next('A', 'Z' + 1))
                .ToArray());

            Bitmap bitmap = new Bitmap(120, 50);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.White);
                Random rnd = new Random();
                for (int i = 0; i < _captchaText.Length; i++)
                {
                    g.DrawString(_captchaText[i].ToString(),
                        new Font("Arial", 20),
                        Brushes.Black,
                        new PointF(20 * i + rnd.Next(5), rnd.Next(5)));
                }
                for (int i = 0; i < 5; i++) // Шум
                {
                    g.DrawLine(Pens.Black,
                        rnd.Next(0, 120), rnd.Next(0, 50),
                        rnd.Next(0, 120), rnd.Next(0, 50));
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

        private void ShowCaptcha()
        {
            CaptchaLabel.Visibility = Visibility.Visible;
            CaptchaTextBox.Visibility = Visibility.Visible;
            CaptchaImage.Visibility = Visibility.Visible;
            RefreshCaptchaButton.Visibility = Visibility.Visible;
        }

        private void StartBlock()
        {
            _isBlocked = true;
            _blockTimer.Start();
            ErrorLabel.Content = "Вы заблокированы на 10 секунд.";
        }

        private void UnblockLogin(object sender, EventArgs e)
        {
            _isBlocked = false;
            _blockTimer.Stop();
            ErrorLabel.Content = "";
        }

        private void ResetState()
        {
            _failedAttempts = 0;
            _isBlocked = false;
            ErrorLabel.Content = "";
            CaptchaLabel.Visibility = Visibility.Hidden;
            CaptchaTextBox.Visibility = Visibility.Hidden;
            CaptchaImage.Visibility = Visibility.Hidden;
            RefreshCaptchaButton.Visibility = Visibility.Hidden;
        }
    }
}

