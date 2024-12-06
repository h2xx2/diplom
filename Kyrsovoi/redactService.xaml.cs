using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
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
using static System.Net.Mime.MediaTypeNames;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для redactService.xaml
    /// </summary>
    public partial class redactService : Window
    {
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
        private string oldName = "";
        private string oldSurname = "";
        private string oldEmail = "";

        public redactService()
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
        string connectionString = Class1.connection;
        string id = "";
        public void FillTextBox()
        {
            string strCmd = $"SELECT * FROM services WHERE service_id={Class1.id_service}";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {

                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(strCmd, con);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        id = rdr["service_id"].ToString();
                        var service = (Service)this.DataContext;
                        service.Name = rdr["service_name"].ToString();
                        oldName = service.Name;
                        service.Description = rdr["description"].ToString();
                        oldSurname = service.Description;
                        service.Price = rdr["price"].ToString();
                        oldEmail = service.Price;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Close();
                //Prosmotr prosmotr = new Prosmotr();
                //prosmotr.DoSomething();

            }
        }


        public class Service : INotifyPropertyChanged, IDataErrorInfo
        {
            private string _name;
            private string _description;
            private string _price;

            // Реализация интерфейса INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            // Свойства с уведомлением об изменении значений
            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            public string Description
            {
                get => _description;
                set
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }

            public string Price
            {
                get => _price;
                set
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }


            // Реализация интерфейса IDataErrorInfo
            public string Error => null; // Общая ошибка на уровне объекта не используется.

            public string this[string columnName]
            {
                get
                {
                    // Возвращаем сообщения об ошибках для конкретных свойств
                    switch (columnName)
                    {
                        case nameof(Name):
                            if (string.IsNullOrWhiteSpace(Name))
                                return "Поле 'Название' обязательно для заполнения.";
                            break;

                        case nameof(Description):
                            if (string.IsNullOrWhiteSpace(Description))
                                return "Поле 'Описание' обязательно для заполнения.";
                            break;

                        case nameof(Price):
                            if (string.IsNullOrWhiteSpace(Price))
                                return "Поле 'Цена' обязательно для заполнения.";
                            break;

                        
                    }
                    return null;
                }
            }
        }
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            foreach (Control control in new[] { name, description, price })
            {
                if (control is TextBox textBox)
                {
                    textBox.IsReadOnly = isReadOnly;
                }
            }
        }
        private bool AreFieldsFilled()
        {
            if (string.IsNullOrEmpty(name.Text) ||
                string.IsNullOrEmpty(description.Text) ||
                string.IsNullOrEmpty(price.Text) 

               )
            {
                return false;
            }
            return true;
        }
        private bool IsTextChanged(string name, string surname, string email)
        {
            // Пример: Если одно из значений изменилось
            if (name != oldName || surname != oldSurname || email != oldEmail )
            {
                // Обновляем старые значения
                oldName = name;
                oldSurname = surname;
                oldEmail = email;

                return true;
            }

            return false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetFieldsReadOnly(false);
            button.Content = "Сохранить";
            delete.Visibility = Visibility.Collapsed;
            string query = String.Empty;
            string names = name.Text;
            string descriptions = description.Text;
            string prices = price.Text;


            if (AreFieldsFilled())
            {
                if (Class1.add != 1)
                {
                    query = "UPDATE services SET service_name = @Name, description = @Description, price = @Price WHERE service_id = @ID";

                }
                else
                {
                    query = "INSERT services(service_name, description, price) VALUES(@Name,@Description,@Price)";

                }

                if (IsTextChanged(name.Text, description.Text, price.Text))
                {

                    // Создаем подключение и команду
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        // Открываем подключение
                        connection.Open();

                        // Создаем команду с параметрами
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {

                            // Добавляем параметры
                            command.Parameters.AddWithValue("@Name", names);
                            command.Parameters.AddWithValue("@Description", descriptions);
                            command.Parameters.AddWithValue("@Price", prices);
                            command.Parameters.AddWithValue("@ID", id);

                            // Выполняем запрос
                            int rowsAffected = command.ExecuteNonQuery();

                            // Проверяем количество измененных строк
                            if (rowsAffected > 0 && Class1.add != 1)
                            {
                                MessageBox.Show("Данные успешно обновлены.");
                            }
                            if (Class1.add == 1)
                            {
                                MessageBox.Show("Данные успешно добавлены.");
                                Class1.add = 0;
                            }
                            else
                            {
                                MessageBox.Show("Ошибка при обновлении данных.");
                                Class1.add = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Данные не изменены");
            }

        }

        private void name_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Class1.role == 0)
            {
                if (Class1.add != 1)
                {
                    var service = new Service();
                    this.DataContext = service; // Устанавливаем DataContext
                    FillTextBox(); // Теперь DataContext точно не null
                    SetFieldsReadOnly(true);
                    button.Visibility = Visibility.Visible;
                    delete.Visibility = Visibility.Visible;

                }
                else
                {
                    var service = new Service();
                    this.DataContext = service;
                    SetFieldsReadOnly(false);
                    button.Content = "Сохранить";
                    button.Visibility = Visibility.Visible;
                }
            }
            else
            {
                var service = new Service();
                this.DataContext = service;
                FillTextBox();
                SetFieldsReadOnly(false);
                button.Visibility = Visibility.Collapsed;
                delete.Visibility = Visibility.Collapsed;
            }
            

        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            string strCmd = $"DELETE FROM services WHERE service_id = {id}";

            using (MySqlConnection con = new MySqlConnection())
            {
                try
                {
                    con.ConnectionString = connectionString;

                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(strCmd, con);

                    MessageBoxResult result = MessageBox.Show(
                        "Удалить запись?",               // Сообщение
                        "Внимание!!",                   // Заголовок окна
                        MessageBoxButton.YesNo,         // Кнопки
                        MessageBoxImage.Warning         // Значок
                    );

                    // Проверяем результат
                    if (result == MessageBoxResult.Yes)
                    {
                        // Ваш код удаления записи
                        int res = cmd.ExecuteNonQuery();

                        // Уведомление об удалении
                        MessageBox.Show(
                            "Запись удалена " + res.ToString(),
                            "Внимание!!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information

                        );
                        this.Close();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void name_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[а-яА-Я]+$");
        }

        private void price_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}
