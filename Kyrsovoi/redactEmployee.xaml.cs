using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для redactEmployee.xaml
    /// </summary>
    public partial class redactEmployee : Window
    {
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
        private string oldName = "";
        private string oldSurname = "";
        private string oldEmail = "";
        private string oldNumber = "";
        private string oldhiredate = "";
        private string oldposition = "";
        private string oldlogin = "";
        private string oldpassword = "";
        private string oldrole = "";
        public redactEmployee()
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
            string strCmd = $"SELECT * FROM employees WHERE phone='{Class1.numberPhoneEmploye}'";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {

                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(strCmd, con);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        id = rdr["employee_id"].ToString();
                        var employee = (Employee)this.DataContext;
                        employee.Name = rdr["first_name"].ToString();
                        oldName = employee.Name;
                        employee.Surname = rdr["last_name"].ToString();
                        oldSurname = employee.Surname;
                        employee.Email = rdr["email"].ToString();
                        oldEmail = employee.Email;
                        employee.Phone = rdr["phone"].ToString();
                        oldNumber = employee.Phone;
                        employee.Db = rdr["hire_date"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rdr["hire_date"]);
                        employee.Position = rdr["position"].ToString();
                        oldposition = employee.Position;
                        employee.Login = rdr["login"].ToString();
                        oldlogin = employee.Login;
                        employee.Password = rdr["password"].ToString();
                        oldpassword = employee.Password;
                        employee.Role = rdr["role"].ToString();
                        oldrole = employee.Role;
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
                _idleTimer.Stop();
                Prosmotr prosmotr = new Prosmotr();
                this.Hide();
                prosmotr.ShowDialog();
                this.Close();
            }
        }


        public class Employee : INotifyPropertyChanged, IDataErrorInfo
        {
            private string _name;
            private string _surname;
            private string _email;
            private string _phone;
            private DateTime? _db;
            private string _position;
            private string _login;
            private string _password;
            private string _role;

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

            public string Surname
            {
                get => _surname;
                set
                {
                    _surname = value;
                    OnPropertyChanged(nameof(Surname));
                }
            }

            public string Email
            {
                get => _email;
                set
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }

            public string Phone
            {
                get => _phone;
                set
                {
                    _phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }
            public DateTime? Db
            {
                get => _db;
                set
                {
                    _db = value;
                    OnPropertyChanged(nameof(Db));
                }
            }
            public string Position
            {
                get => _position;
                set
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
            public string Login
            {
                get => _login;
                set
                {
                    _login = value;
                    OnPropertyChanged(nameof(Login));
                }
            }
            public string Password
            {
                get => _password;
                set
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
            public string Role
            {
                get => _role;
                set
                {
                    _role = value;
                    OnPropertyChanged(nameof(Role));
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
                                return "Поле 'Имя' обязательно для заполнения.";
                            break;

                        case nameof(Surname):
                            if (string.IsNullOrWhiteSpace(Surname))
                                return "Поле 'Фамилия' обязательно для заполнения.";
                            break;

                        case nameof(Email):
                            if (string.IsNullOrWhiteSpace(Email))
                                return "Поле 'Email' обязательно для заполнения.";
                            break;

                        case nameof(Phone):
                            if (string.IsNullOrWhiteSpace(Phone))
                                return "Поле 'Телефон' обязательно для заполнения.";
                            break;
                        case nameof(Db):
                            if (!Db.HasValue)
                                return "Поле 'Дата оформления' обязательно для заполнения.";
                            break;
                        case nameof(Position):
                            if (string.IsNullOrWhiteSpace(Phone))
                                return "Поле 'Должность' обязательно для заполнения.";
                            break;
                        case nameof(Login):
                            if (string.IsNullOrWhiteSpace(Phone))
                                return "Поле 'Логин' обязательно для заполнения.";
                            break;
                        case nameof(Password):
                            if (string.IsNullOrWhiteSpace(Phone))
                                return "Поле 'Пароль' обязательно для заполнения.";
                            break;
                        case nameof(Role):
                            if (string.IsNullOrWhiteSpace(Phone))
                                return "Поле 'Роль' обязательно для заполнения.";
                            break;
                    }
                    return null;
                }
            }
        }
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            foreach (Control control in new[] { name, surname, email, number, login, password })
            {
                if (control is TextBox textBox)
                {
                    textBox.IsReadOnly = isReadOnly;
                }
            }
            position.IsEnabled = !isReadOnly;
            GeneratePass.IsEnabled = !isReadOnly;
            role.IsEnabled = !isReadOnly;
        }
        private bool AreFieldsFilled()
        {
            if (string.IsNullOrEmpty(name.Text) ||
                string.IsNullOrEmpty(surname.Text) ||
                string.IsNullOrEmpty(email.Text) ||
                string.IsNullOrEmpty(number.Text) ||
                string.IsNullOrEmpty(db.Text) ||
                string.IsNullOrEmpty(position.Text) ||
                string.IsNullOrEmpty(login.Text) ||
                string.IsNullOrEmpty(password.Text) ||
                string.IsNullOrEmpty(role.Text)
               )
            {
                return false;
            }
            return true;
        }
        private bool IsTextChanged(string name, string surname, string email, string number, string position, string login, string password, string role)
        {
            // Пример: Если одно из значений изменилось
            if (name != oldName || surname != oldSurname || email != oldEmail || number != oldNumber || position != oldposition || login != oldlogin || password != oldpassword || role != oldrole)
            {
                // Обновляем старые значения
                oldName = name;
                oldSurname = surname;
                oldEmail = email;
                oldNumber = number;
                oldposition = position;
                oldlogin = login;
                oldpassword = password;
                oldrole = role;

                return true;
            }

            return false;
        }

        string GetHashPass(string password)
        {

            byte[] bytesPass = Encoding.UTF8.GetBytes(password);

            SHA256Managed hashstring = new SHA256Managed();

            byte[] hash = hashstring.ComputeHash(bytesPass);

            string hashPasswd = string.Empty;

            foreach (byte x in hash)
            {
                hashPasswd += String.Format("{0:x2}", x);
            }

            hashstring.Dispose();
            return hashPasswd;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetFieldsReadOnly(false);
            button.Content = "Сохранить";
            delete.Visibility = Visibility.Collapsed;   
            db.IsEnabled = true;
            string query = String.Empty;
            string names = name.Text;
            string surnames = surname.Text;
            string emails = email.Text;
            string numbers = number.Text;
            string dbs = db.Text;
            string positions = position.Text;
            string logins = login.Text;
            string passwords = password.Text;
            string roles = role.Text;
            int rowsAffected = 0;

            if (AreFieldsFilled())
            {
                if (Class1.add != 1)
                {
                    query = "UPDATE employees SET first_name = @Name, last_name = @Surname, email = @Email, phone = @Number, hire_date = @Db, position = @Position, login = @Login, password = @Password,role =@Role WHERE employee_id = @ID";

                }
                else
                {
                    query = "INSERT employees(first_name, last_name, email, phone, hire_date, position,login, password, role) VALUES(@Name,@Surname,@Email,@Number, @Db,@Position,@Login,@Password,@Role)";

                }

                if (IsTextChanged(name.Text, surname.Text, email.Text, number.Text, position.Text, login.Text, password.Text, role.Text))
                {

                    // Создаем подключение и команду
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        // Открываем подключение
                        connection.Open();

                        // Создаем команду с параметрами
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            DateTime date = DateTime.ParseExact(dbs, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                            // Преобразованная дата в формате YYYY-MM-DD
                            dbs = date.ToString("yyyy-MM-dd");
                            // Добавляем параметры

                            passwords = GetHashPass(passwords);

                            command.Parameters.AddWithValue("@Name", names);
                            command.Parameters.AddWithValue("@Surname", surnames);
                            command.Parameters.AddWithValue("@Email", emails);
                            command.Parameters.AddWithValue("@Number", numbers);
                            command.Parameters.AddWithValue("@Db", dbs);
                            command.Parameters.AddWithValue("@Position", positions);
                            command.Parameters.AddWithValue("@Login", logins);
                            command.Parameters.AddWithValue("@Password", passwords);
                            command.Parameters.AddWithValue("@Role", roles);
                            command.Parameters.AddWithValue("@ID", id);

                            try
                            {
                                // Выполняем запрос
                                rowsAffected = command.ExecuteNonQuery();
                            }
                            catch
                            {
                                MessageBox.Show("Ошибка");
                            }
                            // Проверяем количество измененных строк
                            if (rowsAffected > 0 && Class1.add != 1)
                            {
                                MessageBox.Show("Данные успешно обновлены.");
                                _idleTimer.Stop();
                                Prosmotr prosmotr = new Prosmotr();
                                this.Hide();
                                prosmotr.ShowDialog();
                                this.Close();
                            }
                            if (Class1.add == 1)
                            {
                                MessageBox.Show("Данные успешно добавлены.");
                                Class1.add = 0;
                                _idleTimer.Stop();
                                Prosmotr prosmotr = new Prosmotr();
                                this.Hide();
                                prosmotr.ShowDialog();
                                this.Close();
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
            if (Class1.add != 1)
            {
                var emplye = new Employee();
                this.DataContext = emplye; // Устанавливаем DataContext
                FillTextBox(); // Теперь DataContext точно не null
                SetFieldsReadOnly(true);
                db.IsEnabled = false;

            }
            else
            {
                var emplye = new Employee();
                this.DataContext = emplye;
                SetFieldsReadOnly(false);
                db.IsEnabled = true;
                button.Content = "Сохранить";
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {

            string strCmd = $"DELETE FROM employees WHERE employee_id = {id}";

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
                            
                        ) ;
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

        private void email_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9@.\-_]+$");
        }

        private void number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void db_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;

        }

        private void login_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9@_&]+$");
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            password.Text = GeneratePassword(12);
        }
        public static string GeneratePassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
