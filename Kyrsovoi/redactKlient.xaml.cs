using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
using System.Xml.Linq;
using static Kyrsovoi.Prosmotr;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для redactKlient.xaml
    /// </summary>
    public partial class redactKlient : Window
    {
        private string oldName = "";
        private string oldSurname = "";
        private string oldEmail = "";
        private string oldNumber = "";

        public redactKlient()
        {
            InitializeComponent();           

        }
        string connectionString = Class1.connection;
        string id = "";
        public void FillTextBox()
        {
            string strCmd = $"SELECT * FROM guests WHERE phone={Class1.numberPhone}";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                
                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(strCmd, con);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        id = rdr["guest_id"].ToString();
                        var client = (Client)this.DataContext;
                        client.Name = rdr["first_name"].ToString();
                        oldName = client.Name;
                        client.Surname = rdr["last_name"].ToString();
                        oldSurname = client.Surname;
                        client.Email = rdr["email"].ToString();
                        oldEmail = client.Email;
                        client.Phone = rdr["phone"].ToString();
                        oldNumber = client.Phone;
                        client.Db = rdr["date_of_birth"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rdr["date_of_birth"]);
                        client.Passport = rdr["passport_number"].ToString();
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


        public class Client : INotifyPropertyChanged, IDataErrorInfo
        {
        private string _name;
        private string _surname;
        private string _email;
        private string _phone;
        private string _passport;
        private DateTime? _db;

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

        public string Passport
        {
            get => _passport;
            set
            {
                _passport = value;
                OnPropertyChanged(nameof(Passport));
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

                        case nameof(Passport):
                            if (string.IsNullOrWhiteSpace(Phone))
                                return "Поле 'Паспорт' обязательно для заполнения.";
                            break;
                            case nameof(Db):
                                if (!Db.HasValue)
                                    return "Поле 'Дата рождения' обязательно для заполнения.";
                                break;
                        }
                    return null;
                }
            }
        }
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            foreach (Control control in new[] { name, surname, email, number, passport })
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
                string.IsNullOrEmpty(surname.Text) ||
                string.IsNullOrEmpty(email.Text) ||
                string.IsNullOrEmpty(number.Text) ||
                string.IsNullOrEmpty(db.Text) ||
                string.IsNullOrEmpty(passport.Text)
               )
            {
                return false;
            }
            return true;
        }
        private bool IsTextChanged(string name, string surname, string email, string number)
        {
            // Пример: Если одно из значений изменилось
            if (name != oldName || surname != oldSurname || email != oldEmail || number != oldNumber)
            {
                // Обновляем старые значения
                oldName = name;
                oldSurname = surname;
                oldEmail = email;
                oldNumber = number;

                return true;
            }

            return false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetFieldsReadOnly(false);
            button.Content = "Сохранить";
            db.IsEnabled = true;
            string query = String.Empty;
            string names = name.Text;
            string surnames = surname.Text;
            string emails = email.Text;
            string numbers = number.Text;
            string dbs = db.Text;
            string passports = passport.Text;

            if (AreFieldsFilled())
            {
                if (Class1.add != 1)
                {
                    query = "UPDATE guests SET first_name = @Name, last_name = @Surname, email = @Email, phone = @Number, date_of_birth = @Db, passport_number = @Passport WHERE guest_id = @ID";

                }
                else
                {
                    query = "INSERT guests(first_name, last_name, email, phone, date_of_birth, passport_number,registration_date) VALUES(@Name,@Surname,@Email,@Number, @Db,@Passport,@register)";

                }

                if (IsTextChanged(name.Text, surname.Text, email.Text, number.Text))
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
                            command.Parameters.AddWithValue("@Name", names);
                            command.Parameters.AddWithValue("@Surname", surnames);
                            command.Parameters.AddWithValue("@Email", emails);
                            command.Parameters.AddWithValue("@Number", numbers);
                            command.Parameters.AddWithValue("@Db", dbs);
                            command.Parameters.AddWithValue("@Passport", passports);
                            command.Parameters.AddWithValue("@register", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@ID", id);

                            // Выполняем запрос
                            int rowsAffected = command.ExecuteNonQuery();

                            // Проверяем количество измененных строк
                            if (rowsAffected > 0 && Class1.add != 1)
                            {
                                MessageBox.Show("Данные успешно обновлены.");
                            }
                            if(Class1.add == 1)
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
            if (Class1.add != 1)
            {
                var client = new Client();
                this.DataContext = client; // Устанавливаем DataContext
                FillTextBox(); // Теперь DataContext точно не null
                SetFieldsReadOnly(true);
                db.IsEnabled = false;
                
            }
            else
            {
                var client = new Client();
                this.DataContext = client;
                SetFieldsReadOnly(false);
                db.IsEnabled = true;
                button.Content = "Сохранить";
            }
        }
    }
}
