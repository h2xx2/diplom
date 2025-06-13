using Microsoft.Office.Interop.Word;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xaml;
using System.Xml.Linq;
using static Kyrsovoi.Prosmotr;
using static Kyrsovoi.redactBooking;
using Word = Microsoft.Office.Interop.Word;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для redactBooking.xaml
    /// </summary>
    
    public partial class redactBooking : System.Windows.Window
    {
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
        int err = 0;
        int red = 0;
        public redactBooking()
        {
            InitializeComponent();
            
            EmployeeID.Text = Class1.fioEmploes;
            listUnit.ItemsSource = Houses;
            
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
        private string oldGuest = "";
        
        private string oldUnit = "";
        private string dateIn = "";
        private string dateOut = "";

        private string bookingstatus = "";
        private string price = "";
        public DateTime? MinDate { get; set; }
        string connectionString = Class1.connection;
        string phoneNumber = "";
        int id_guest = 0;
        int id_unit = 0;
        string totalPrice = "";
        string payment = "";
        int cost = 0;
        int savecost = 0;
        private bool isSelecting = false;
        string id = "";
        string statusPay = "";
        string query = @"
        SELECT unit_id, unit_name, capacity, price_per_night, `description`
        FROM glampingunits
        WHERE NOT EXISTS (
            SELECT 1
            FROM bookings
            WHERE bookings.unit_id = glampingunits.unit_id
              AND (@StartDate < bookings.check_in_date AND @EndDate > bookings.check_out_date)
        )";
        
        string com = "";
        public class House
        {
            public string id { get; set; }
            public string name { get; set; }
            public string capacity { get; set; }
            public string price { get; set; }
            public string description { get; set; }
        }
        public class BookingStatus
        {
            public int Id { get; set; }
            public string Status { get; set; }
        }
        public class PayStatus
        {
            public int Id { get; set; }
            public string Status { get; set; }
        }
        public ObservableCollection<House> Houses { get; set; } = new ObservableCollection<House>();
        public ObservableCollection<Booking> Book { get; set; } = new ObservableCollection<Booking>();
        public void FillDataGrid(string com, string startDate, string endDate)
        {



            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    MySqlCommand command = new MySqlCommand(com, connection);
                    command.Parameters.AddWithValue("@StartDate", DateTime.Parse(startDate));
                    command.Parameters.AddWithValue("@EndDate", DateTime.Parse(endDate));

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    Houses.Clear(); // Очистка коллекции перед загрузкой данных

                    while (reader.Read())
                    {
                        Houses.Add(new House
                        {
                            id = reader["unit_id"].ToString(),
                            name = reader["unit_name"].ToString(),
                            capacity = reader["capacity"].ToString(),
                            price = reader["price_per_night"].ToString(),
                            description = reader["description"].ToString(),
                        });
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}");
                }
            }
        }
        public void FillTextBox()
        {
            string strCmd = @"SELECT 
                        b.booking_id,
                        CONCAT(guests.first_name, ' ', guests.last_name) AS guest,
                        guests.phone,
                        glampingunits.unit_id,
                        CONCAT(employees.first_name, ' ', employees.last_name) AS employee,
                        glampingunits.unit_name,
                        b.check_in_date, 
                        b.check_out_date, 
                        b.total_price, 
                        b.booking_status as 'idstatus',
                        booking_status.booking_status,
                        b.pay_status as 'idstatuspay',
                        pay_status.pay_statuscol,
                        b.upfront_payment,
                        b.created_at
                    FROM 
                        glamping.bookings b
                    LEFT JOIN 
                        guests ON guests.guest_id = b.guest_id
                    LEFT JOIN 
                        glampingunits ON glampingunits.unit_id = b.unit_id 
                    LEFT JOIN 
                        employees ON employees.employee_id = b.employees_id
                    LEFT JOIN 
                        booking_status ON booking_status.idbooking_status = b.booking_status
                    LEFT JOIN 
                        pay_status ON pay_status.idpay_status = b.pay_status
                    WHERE 
                        b.booking_id = " + Class1.booking_id + @"
                    GROUP BY 
                        b.booking_id;";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(strCmd, con);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        id = rdr["booking_id"].ToString();
                        id_unit = Convert.ToInt32(rdr["unit_id"]);
                        var booking = (Booking)this.DataContext;
                        booking.Guest_id = rdr["phone"].ToString();
                        FillTextBoxes(booking.Guest_id);
                        oldGuest = booking.Guest_id;
                        booking.Unit_id = rdr["unit_name"].ToString();
                        oldUnit = booking.Unit_id;
                        if (Class1.add == 1)
                        {
                            booking.Employees_id = rdr["employee"].ToString();
                        }

                        booking.Check_in_date = rdr["check_in_date"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rdr["check_in_date"]);
                        if (booking.Check_in_date.HasValue)
                        {
                            dateIn = booking.Check_in_date.Value.ToString("MM.dd.yyyy");
                        }
                        else
                        {
                            dateIn = null;
                        }

                        booking.Check_out_date = rdr["check_out_date"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rdr["check_out_date"]);
                        if (booking.Check_out_date.HasValue)
                        {
                            dateOut = booking.Check_out_date.Value.ToString("MM.dd.yyyy");
                        }
                        else
                        {
                            dateOut = null;
                        }

                        booking.Total_price = rdr["total_price"].ToString();
                        totalPrice = booking.Total_price;
                        Payment_cost.Text = rdr["upfront_payment"].ToString();
                        booking.Booking_status = rdr["booking_status"].ToString();
                        bookingstatus = rdr["idstatus"].ToString();
                        booking.Pay_status = rdr["pay_statuscol"].ToString();
                        statusPay = rdr["idstatuspay"].ToString();
                        FillComboBoxRedact();
                        FillComboBoxRedactPay();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public class Booking : INotifyPropertyChanged, IDataErrorInfo
        {



            private string _booking_id;
            private string _guest_id;
            private string _unit_id;
            private string _employees_id;
            private DateTime? _check_in_date;
            private DateTime? _check_out_date;
            private string _total_price;
            private string _upfront_payment;
            private string _booking_status;
            private string _pay_status;

            // Реализация интерфейса INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            // Свойства с уведомлением об изменении значений
            public string Booking_id
            {
                get => _booking_id;
                set
                {
                    _booking_id = value;
                    OnPropertyChanged(nameof(Booking_id));
                }
            }

            public string Guest_id
            {
                get => _guest_id;
                set
                {
                    _guest_id = value;
                    OnPropertyChanged(nameof(Guest_id));
                }
            }

            public string Unit_id
            {
                get => _unit_id;
                set
                {
                    _unit_id = value;
                    OnPropertyChanged(nameof(Unit_id));
                }
            }

            public string Employees_id
            {
                get => _employees_id;
                set
                {
                    _employees_id = value;
                    OnPropertyChanged(nameof(Employees_id));
                }
            }

            public DateTime? Check_in_date
            {
                get => _check_in_date;
                set
                {
                    _check_in_date = value;
                    OnPropertyChanged(nameof(Check_in_date));
                }
            }
            public DateTime? Check_out_date
            {
                get => _check_out_date;
                set
                {
                    _check_out_date = value;
                    OnPropertyChanged(nameof(Check_out_date));
                }
            }
            public string Total_price
            {
                get => _total_price;
                set
                {
                    _total_price = value;
                    OnPropertyChanged(nameof(Total_price));
                }
            }
            public string Upfront_payment
            {
                get => _upfront_payment;
                set
                {
                    _upfront_payment = value;
                    OnPropertyChanged(nameof(Upfront_payment));
                }
            }
            public string Booking_status
            {
                get => _booking_status;
                set
                {
                    _booking_status = value;
                    OnPropertyChanged(nameof(Booking_status));
                }
            }

            public string Pay_status
            {
                get => _pay_status;
                set
                {
                    _pay_status = value;
                    OnPropertyChanged(nameof(Pay_status));
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
                        case nameof(Booking_id):
                            if (string.IsNullOrWhiteSpace(Booking_id))
                                return "Поле 'Имя' обязательно для заполнения.";
                            break;

                        case nameof(Guest_id):
                            if (string.IsNullOrWhiteSpace(Guest_id))
                                return "Поле 'Фамилия' обязательно для заполнения.";
                            break;

                        case nameof(Employees_id):
                            if (string.IsNullOrWhiteSpace(Employees_id))
                                return "Поле 'Email' обязательно для заполнения.";
                            break;

                        case nameof(Unit_id):
                            if (string.IsNullOrWhiteSpace(Unit_id))
                                return "Поле 'Телефон' обязательно для заполнения.";
                            break;

                        case nameof(Check_in_date):
                            if (!Check_in_date.HasValue)
                                return "Поле 'Паспорт' обязательно для заполнения.";
                            break;
                        case nameof(Check_out_date):
                            if (!Check_out_date.HasValue)
                                return "Поле 'Дата рождения' обязательно для заполнения.";
                            break;
                        case nameof(Total_price):
                            if (string.IsNullOrWhiteSpace(Total_price))
                                return "Поле 'Email' обязательно для заполнения.";
                            break;

                        case nameof(Booking_status):
                            if (string.IsNullOrWhiteSpace(Booking_status))
                                return "Поле 'Телефон' обязательно для заполнения.";
                            break;
                    }
                    return null;
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
                Class1.add = 0;
                _idleTimer.Stop();
                Prosmotr prosmotr = new Prosmotr();
                this.Hide();
                prosmotr.ShowDialog();
                this.Close();  
            }
        }

        private void GuestID_LostFocus(object sender, RoutedEventArgs e)
        {
            phoneNumber = GuestID.Text; // Поле для номера телефона

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                CheckCountGuest(phoneNumber);
            }
            if (string.IsNullOrEmpty(phoneNumber))
            {
                spFio.Visibility = Visibility.Collapsed;
            }

        }
        private void CheckCountGuest(string phoneNumber)
        {
            string query = "SELECT Count(*) FROM guests WHERE phone = @Phone";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        command.Parameters.AddWithValue("@Phone", phoneNumber);

                        object result = command.ExecuteScalar();
                        int count = Convert.ToInt32(result);
                        if (count <= 0)
                        {
                            MessageBox.Show("Пользователь с таким номером телефона не найден.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            _idleTimer.Stop();
                            Class1.phone = phoneNumber;
                            Class1.add = 1;
                            redactKlient redactKlient = new redactKlient();
                            this.Hide();
                            redactKlient.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            FillTextBoxes(phoneNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        private void FillTextBoxes(string phoneNumber)
        {
            string query = "SELECT guest_id, concat(first_name, \" \", last_name) AS FIO FROM guests WHERE phone = @Phone";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Phone", phoneNumber);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                spFio.Visibility = Visibility.Visible;
                                Fio.Text = reader["FIO"].ToString();
                                id_guest = Convert.ToInt32(reader["guest_id"]);
                            }
                            else
                            {
                                MessageBox.Show("Номер телефона не найден.");
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void FillComboBox()
        {
            string query = "SELECT * FROM glamping.booking_status;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        DataTable table = new DataTable();
                        MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(command);
                        mySqlDataAdapter.Fill(table);

                        // Преобразуем DataTable в список объектов
                        var bookingStatuses = new List<BookingStatus>();
                        foreach (DataRow row in table.Rows)
                        {
                            bookingStatuses.Add(new BookingStatus
                            {
                                Id = Convert.ToInt32(row["idbooking_status"]),
                                Status = row["booking_status"].ToString()
                            });
                        }

                        // Привязываем коллекцию к ComboBox
                        StatusBooking.ItemsSource = bookingStatuses;
                        StatusBooking.DisplayMemberPath = "Status";
                        StatusBooking.SelectedValuePath = "Id";
                        StatusBooking.UpdateLayout();
                        // Принудительная синхронизация UI перед установкой SelectedValue
                        Dispatcher.Invoke(() =>
                        {
                            StatusBooking.SelectedValue = 1;
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        private void FillComboBoxRedact()
        {
            string query = "SELECT * FROM glamping.booking_status;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        DataTable table = new DataTable();
                        MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(command);
                        mySqlDataAdapter.Fill(table);

                        // Преобразуем DataTable в список объектов
                        var bookingStatuses = new List<BookingStatus>();
                        foreach (DataRow row in table.Rows)
                        {
                            bookingStatuses.Add(new BookingStatus
                            {
                                Id = Convert.ToInt32(row["idbooking_status"]), // Предполагаем, что есть колонка id
                                Status = row["booking_status"].ToString()
                            });
                        }

                        // Привязываем коллекцию к ComboBox
                        StatusBooking.ItemsSource = bookingStatuses;
                        StatusBooking.DisplayMemberPath = "Status"; // Поле для отображения
                        StatusBooking.SelectedValuePath = "Id";     // Поле для значения
                        StatusBooking.SelectedValue = bookingstatus;            // Устанавливаем значение по Id
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void FillComboBoxPay()
        {
            string query = "SELECT * FROM glamping.pay_status;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        DataTable table = new DataTable();
                        MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(command);
                        mySqlDataAdapter.Fill(table);

                        // Преобразуем DataTable в список объектов
                        var bookingStatuses = new List<PayStatus>();
                        foreach (DataRow row in table.Rows)
                        {
                            bookingStatuses.Add(new PayStatus
                            {
                                Id = Convert.ToInt32(row["idpay_status"]),
                                Status = row["pay_statuscol"].ToString()
                            });
                        }

                        // Привязываем коллекцию к ComboBox
                        Status_pay.ItemsSource = bookingStatuses;
                        Status_pay.DisplayMemberPath = "Status";
                        Status_pay.SelectedValuePath = "Id";
                        Status_pay.UpdateLayout();
                        // Принудительная синхронизация UI перед установкой SelectedValue
                        Dispatcher.Invoke(() =>
                        {
                            Status_pay.SelectedValue = 2;
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        
        private void FillComboBoxRedactPay()
        {
            string query = "SELECT * FROM glamping.pay_status;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        DataTable table = new DataTable();
                        MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(command);
                        mySqlDataAdapter.Fill(table);

                        // Преобразуем DataTable в список объектов
                        var bookingStatuses = new List<PayStatus>();
                        foreach (DataRow row in table.Rows)
                        {
                            bookingStatuses.Add(new PayStatus
                            {
                                Id = Convert.ToInt32(row["idpay_status"]), // Предполагаем, что есть колонка id
                                Status = row["pay_statuscol"].ToString()
                            });
                        }

                        // Привязываем коллекцию к ComboBox
                        Status_pay.ItemsSource = bookingStatuses;
                        Status_pay.DisplayMemberPath = "Status"; // Поле для отображения
                        Status_pay.SelectedValuePath = "Id";     // Поле для значения
                        Status_pay.UpdateLayout();
                        // Принудительная синхронизация UI перед установкой SelectedValue
                        Dispatcher.Invoke(() =>
                        {
                            Status_pay.SelectedValue = statusPay;
                        });// Устанавливаем значение по Id
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void AnimateListViewHeight(ListView listView, double fromHeight, double toHeight, double durationSeconds)
        {
            // Создаем анимацию для высоты
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                From = fromHeight,
                To = toHeight,
                Duration = new Duration(TimeSpan.FromSeconds(durationSeconds)),
                EasingFunction = new QuadraticEase() // Для плавного эффекта
            };

            // Применяем анимацию к свойству высоты
            listView.BeginAnimation(HeightProperty, heightAnimation);
        }


        private void UnitID_GotFocus_1(object sender, RoutedEventArgs e)
        {
            FillFuncBig();
            CollabsedHome.IsEnabled = true;
        }

        private void UnitID_LostFocus(object sender, RoutedEventArgs e)
        {
            FillFuncSmall();
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'cabin'";
            FillDataGrid(com, checkIn, checkOut);
        }

        private void Border_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'tent'";
            FillDataGrid(com, checkIn, checkOut);
        }

        private void Border_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'yurt'";
            FillDataGrid(com, checkIn, checkOut);
        }

        private void Border_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'treehouse'";
            FillDataGrid(com, checkIn, checkOut);
        }
        void FillFuncBig()
        {
            double currentHeight = listUnit.ActualHeight;
            double targetHeight = 200; // Конечная высота

            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            // Запускаем анимацию высоты и ширины
            AnimateListViewHeight(listUnit, currentHeight, targetHeight, 0.5);
            FillDataGrid(query, checkIn, checkOut);
        }
        void FillFuncSmall()
        {
            double currentHeight = listUnit.ActualHeight;
            double targetHeight = 0; // Конечная высота
            AnimateListViewHeight(listUnit, currentHeight, targetHeight, 1);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'cabin'";
            FillDataGrid(com, checkIn, checkOut);
        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'tent'";
            FillDataGrid(com, checkIn, checkOut);
        }

        private void TextBlock_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'yurt'";
            FillDataGrid(com, checkIn, checkOut);
        }

        private void TextBlock_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            string checkIn = CheckInDate.Text;
            string checkOut = CheckOutDate.Text;
            FillFuncBig();
            com = query + " AND unit_type = 'treehouse'";
            FillDataGrid(com, checkIn, checkOut);
        }

        
        private void listUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = sender as ListView;

            var house = list?.SelectedItem as House;

            if (house != null)
            {
                id_unit = Convert.ToInt32(house.id);
                UnitID.Text = house.name;
                price = house.price;
                if (CheckInDate.SelectedDate.HasValue && CheckOutDate.SelectedDate.HasValue)
                {
                    if (decimal.TryParse(price, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out decimal result))
                    {
                        // Преобразуем результат в целое число
                        int intValue = (int)result;

                        DateTime startDate = CheckInDate.SelectedDate.Value;
                        DateTime endDate = CheckOutDate.SelectedDate.Value;

                        if (startDate <= endDate)
                        {
                            int daysDifference = (endDate - startDate).Days;
                            cost = daysDifference * intValue;

                            if (daysDifference > 10)
                            {
                                cost = (int)(cost * 0.9); // применяем 10% скидку
                            }
                            TotalPrice.Text = cost.ToString();
                        }
                    }
                }
            }
        }
        private void CheckFields()
        {
            // Проверяем, выбраны ли значения в обоих полях
            if (CheckInDate.SelectedDate.HasValue && CheckOutDate.SelectedDate.HasValue)
            {
                spHome.IsEnabled = true;
            }
            else
            {
                spHome.IsEnabled = false;
            }
        }

        private void CheckInDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFields();
            if (CheckInDate.SelectedDate.HasValue)
            {
                
                    if (Class1.add != 0)
                    {
                        DateTime selectedDate = CheckInDate.SelectedDate.Value;
                        if (DateTime.Now < selectedDate)
                        {
                            // Ограничиваем минимальную дату второго DatePicker
                            CheckOutDate.DisplayDateStart = selectedDate.AddDays(1);

                            // Блокируем недопустимые даты
                            HighlightInvalidDates(CheckOutDate, DateTime.MinValue, selectedDate);

                            // Сбрасываем выбранную дату во втором, если она недопустима
                            if (CheckOutDate.SelectedDate.HasValue && CheckOutDate.SelectedDate <= selectedDate)
                            {
                                CheckOutDate.SelectedDate = null;
                            }
                        }
                        else
                        {
                            CheckOutDate.Text = null;
                        }
                    }
                    if (Class1.add != 0)
                    {
                        DateTime selectedDate = CheckInDate.SelectedDate.Value;
                        if (DateTime.Now < selectedDate)
                        {
                            // Ограничиваем минимальную дату второго DatePicker
                            CheckOutDate.DisplayDateStart = selectedDate.AddDays(1);

                            // Блокируем недопустимые даты
                            HighlightInvalidDates(CheckOutDate, DateTime.MinValue, selectedDate);

                            // Сбрасываем выбранную дату во втором, если она недопустима
                            if (CheckOutDate.SelectedDate.HasValue && CheckOutDate.SelectedDate <= selectedDate)
                            {
                                CheckOutDate.SelectedDate = null;
                                MessageBox.Show("Дата заезда не может быть раньше даты выезда");
                            }
                        }
                        else
                        {
                            CheckOutDate.SelectedDate = selectedDate;
                            MessageBox.Show("Дата заезда не может быть раньше сегоднешнего дня");
                        }
                    }
                
            }
            else
            {
                // Сбрасываем ограничения
                CheckOutDate.DisplayDateStart = null;
                CheckOutDate.BlackoutDates.Clear();
            }
        }

        private void CheckOutDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFields();

            if (CheckOutDate.SelectedDate.HasValue)
            {
                if (red == 1)
                {
                    if (Class1.add != 0)
                    {
                        DateTime selectedDate = CheckOutDate.SelectedDate.Value;
                        if (DateTime.Now < selectedDate)
                        {
                            // Ограничиваем максимальную дату первого DatePicker
                            CheckInDate.DisplayDateEnd = selectedDate.AddDays(-1);

                            // Блокируем недопустимые даты
                            HighlightInvalidDates(CheckInDate, selectedDate, DateTime.MaxValue);

                            // Сбрасываем выбранную дату в первом, если она недопустима
                            if (CheckInDate.SelectedDate.HasValue && CheckInDate.SelectedDate >= selectedDate)
                            {
                                CheckInDate.SelectedDate = null;
                                MessageBox.Show("Дата выезда не может быть раньше даты заезда");
                            }
                        }
                        else
                        {
                            CheckOutDate.Text = null;
                            MessageBox.Show("Дата выезда не может быть раньше сегоднешнего дня");

                        }
                    }
                    else
                    {
                        DateTime selectedDate = CheckOutDate.SelectedDate.Value;
                        if (DateTime.Now < selectedDate)
                        {
                            // Ограничиваем максимальную дату первого DatePicker
                            CheckInDate.DisplayDateEnd = selectedDate.AddDays(-1);

                            // Блокируем недопустимые даты
                            HighlightInvalidDates(CheckInDate, selectedDate, DateTime.MaxValue);

                            // Сбрасываем выбранную дату в первом, если она недопустима
                            if (CheckInDate.SelectedDate.HasValue && CheckInDate.SelectedDate >= selectedDate)
                            {
                                CheckInDate.SelectedDate = selectedDate;
                                MessageBox.Show("Дата выезда не может быть раньше даты заезда");

                            }
                        }
                        else
                        {
                            CheckInDate.SelectedDate = selectedDate;
                            MessageBox.Show("Дата выезда не может быть раньше сегоднешнего дня");

                        }
                    }
                }

            }
            else
            {
                // Сбрасываем ограничения
                CheckInDate.DisplayDateEnd = null;
                CheckInDate.BlackoutDates.Clear();
            }
        }
        private void HighlightInvalidDates(DatePicker datePicker, DateTime startDate, DateTime endDate)
        {
            datePicker.BlackoutDates.Clear();

            if (startDate < endDate)
            {
                datePicker.BlackoutDates.Add(new CalendarDateRange(startDate, endDate));
            }
        }

        private bool AreFieldsFilled()
        {
            if (Class1.add == 1)
            {
                if (string.IsNullOrEmpty(Fio.Text) ||
                string.IsNullOrEmpty(EmployeeID.Text) ||
                string.IsNullOrEmpty(CheckInDate.Text) ||
                string.IsNullOrEmpty(CheckOutDate.Text) ||
                string.IsNullOrEmpty(UnitID.Text) ||
                string.IsNullOrEmpty(TotalPrice.Text) ||
                string.IsNullOrEmpty(Payment_cost.Text) ||
                string.IsNullOrEmpty(StatusBooking.Text)
               )
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(Fio.Text) ||
                string.IsNullOrEmpty(CheckInDate.Text) ||
                string.IsNullOrEmpty(CheckOutDate.Text) ||
                string.IsNullOrEmpty(UnitID.Text) ||
                string.IsNullOrEmpty(TotalPrice.Text) ||
                string.IsNullOrEmpty(Payment_cost.Text) ||
                string.IsNullOrEmpty(StatusBooking.Text)
               )
                {
                    return false;
                }
                return true;
            }
        }
        private bool IsTextChanged(string guest, string unit, string datein, string dateout, string status, string totalprice)
        {
            // Пример: Если одно из значений изменилось
            if (guest != oldGuest || unit != oldUnit || datein != dateIn || dateout != dateOut || status != bookingstatus || totalprice != totalPrice || totalprice != totalPrice)
            {
                err = 0;
                red = 1;
                // Обновляем старые значения
                oldGuest = guest;
                oldUnit = unit;
                dateIn = datein;
                dateOut = dateout;
                bookingstatus = status;
                totalPrice = totalprice;

                return true;
            }

            return false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetFieldsReadOnly(false);
            button.Content = "Сохранить";
            delete.Visibility = Visibility.Collapsed;
            string cos = TotalPrice.Text.Replace(',', '.'); // Заменяем запятую на точку
            if (decimal.TryParse(cos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var decimalCost))
            {
                savecost = (int)decimalCost; // Приводим к целому числу
            }
            if (AreFieldsFilled())
            {
                if (IsTextChanged(
                GuestID.Text,
                UnitID.Text,
                DateTime.ParseExact(CheckInDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("MM.dd.yyyy"),
                DateTime.ParseExact(CheckOutDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("MM.dd.yyyy"),
                StatusBooking.SelectedValue.ToString(),
                TotalPrice.Text) && err == 0)
                {
                    if (Class1.add != 1)
                    {
                        query = "UPDATE bookings \r\nSET \r\n  guest_id = @guest_id, \r\n  unit_id = @unit_id, \r\n  check_in_date = @check_in_date, \r\n  check_out_date = @check_out_date, \r\n  total_price = CAST(REPLACE(@total_price, ',', '.') AS DECIMAL(10,2)), \r\n  upfront_payment = CAST(REPLACE(@upfront_payment, ',', '.') AS DECIMAL(10,2)), \r\n  pay_status = @pay_status, \r\n  booking_status = @booking_status \r\nWHERE booking_id = @id;\r\n";

                    }
                    else
                    {
                        query = "INSERT bookings(guest_id, unit_id, employees_id, check_in_date, check_out_date, total_price,booking_status, created_at, upfront_payment, pay_status) VALUES(@guest_id,@unit_id,@employees_id,@check_in_date, @check_out_date,@total_price,@booking_status, @created_at, @upfront_payment, @pay_status); SELECT LAST_INSERT_ID();";
                    }
                

                    // Создаем подключение и команду
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        // Открываем подключение
                        connection.Open();

                        // Создаем команду с параметрами
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            DateTime date = DateTime.ParseExact(CheckInDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            DateTime date1 = DateTime.ParseExact(CheckOutDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                            // Преобразованная дата в формате YYYY-MM-DD
                            string dbs = date.ToString("yyyy-MM-dd");
                            string dbs1 = date1.ToString("yyyy-MM-dd");
                            // Добавляем параметры
                            command.Parameters.AddWithValue("@id", Class1.booking_id);
                            command.Parameters.AddWithValue("@guest_id", id_guest);
                            command.Parameters.AddWithValue("@unit_id", id_unit);
                            command.Parameters.AddWithValue("@employees_id", Class1.id_employes);
                            command.Parameters.AddWithValue("@check_in_date", dbs);
                            command.Parameters.AddWithValue("@check_out_date", dbs1); 
                            command.Parameters.AddWithValue("@total_price", TotalPrice.Text);
                            command.Parameters.AddWithValue("@upfront_payment", Payment_cost.Text);
                            command.Parameters.AddWithValue("@pay_status", Status_pay.SelectedValue);
                            command.Parameters.AddWithValue("@booking_status", StatusBooking.SelectedValue);
                            command.Parameters.AddWithValue("@created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            // Выполняем запрос
                            try
                            {
                                object result = command.ExecuteScalar();
                                if (Class1.add == 1)
                                {
                                    Class1.add = 0;
                                    // Проверяем количество измененных строк
                                    if (result != null)
                                    {
                                        MessageBox.Show("Данные успешно добавлены.");
                                        if (decimal.TryParse(TotalPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalCost) &&
                                            decimal.TryParse(Payment_cost.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal paymentCost))
                                        {
                                            if (totalCost == paymentCost)
                                            {
                                                GenerateAndSaveContract(Convert.ToInt32(result.ToString()));
                                                GenerateReceipt(Convert.ToInt32(result.ToString()));
                                                MessageBox.Show("Договора успешно сохранен");
                                                _idleTimer.Stop();
                                                Prosmotr prosmotr = new Prosmotr();
                                                this.Hide();
                                                prosmotr.ShowDialog();
                                                this.Close();
                                            }
                                        }

                                        GenerateReceipt(Convert.ToInt32(result.ToString()));
                                        Prosmotr prosmotr1 = new Prosmotr();
                                        this.Hide();
                                        prosmotr1.ShowDialog();
                                        this.Close();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Ошибка при добавление данных.");
                                        Class1.add = 0;
                                        IsTextChanged("", "", "", "", "", "");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Данные успешно обновлены.");
                                    if (decimal.TryParse(TotalPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalCost) &&
                                             decimal.TryParse(Payment_cost.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal paymentCost))
                                    {
                                        if (totalCost == paymentCost)
                                        {
                                            GenerateAndSaveContract(Convert.ToInt32(Class1.booking_id));
                                            GenerateReceipt(Convert.ToInt32(Class1.booking_id));
                                            MessageBox.Show("Договора успешно сохранен");
                                            _idleTimer.Stop();
                                            Prosmotr prosmotr = new Prosmotr();
                                            this.Hide();
                                            prosmotr.ShowDialog();
                                            this.Close();
                                        }
                                    }

                                    GenerateReceipt(Convert.ToInt32(Class1.booking_id));
                                    Prosmotr prosmotr1 = new Prosmotr();
                                    this.Hide();
                                    prosmotr1.ShowDialog();
                                    this.Close();
                                }
                            }
                            catch(Exception)
                            {
                                MessageBox.Show("Ошибка при обновление данных.");
                            }
                        }
                    }
                }
                else
                {
                    
                    if (err >=1)
                    {
                        MessageBox.Show("Данные не изменены.");
                    }
                    err++;
                }
            }
            else
            {
                MessageBox.Show("Не все поля заполнены");
            }
        }
        
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            foreach (Control control in new[] { GuestID, Payment_cost })
            {
                if (control is TextBox textBox)
                {
                    textBox.IsReadOnly = isReadOnly;
                }
            }
            foreach (Control control in new[] { CheckInDate, CheckOutDate })
            {
                if (control is DatePicker textBox)
                {
                    textBox.IsEnabled = !isReadOnly;
                }
            }
            foreach (Control control in new[] { UnitID })
            {
                if (control is TextBox textBox)
                {
                    textBox.IsEnabled = !isReadOnly;
                }
            }
            unitButton.IsEnabled = !isReadOnly;
            StatusBooking.IsEnabled = !isReadOnly;
            Status_pay.IsEnabled = !isReadOnly;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckInDate.DisplayDateStart = DateTime.Now.Date;
            delete.Visibility = Visibility.Visible;
            if (Class1.add != 1)
            {
                var booking = new Booking();
                this.DataContext = booking; // Устанавливаем DataContext
                SetFieldsReadOnly(true);
                FillTextBox(); // Теперь DataContext точно не null
                if (booking.Check_out_date.HasValue)
                {
                    CheckOutDate.SelectedDate = booking.Check_out_date;
                    CheckInDate.SelectedDate = booking.Check_in_date;// Устанавливаем дату вручную
                }
                SpEmpoy.Visibility = Visibility.Collapsed;
                if (Class1.role == 0)
                {
                    panel_bron.Visibility = Visibility.Collapsed;
                }
                Status_pay.SelectedIndex = 1;
            }
            else
            {
                var booking = new Booking();
                this.DataContext = booking;
                SetFieldsReadOnly(false);
                if (Class1.klient == 1)
                {
                    booking.Guest_id = Class1.phone;
                    CheckCountGuest(Class1.phone);
                }
                button.Content = "Сохранить";
                SpEmpoy.Visibility = Visibility.Visible;
                FillComboBox();
                FillComboBoxPay();
                EmployeeID.Text = Class1.fioEmploes;
                delete.Visibility = Visibility.Collapsed;
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        { 
            string strCmd = $"DELETE FROM bookings WHERE booking_id = {id}; DELETE FROM bookingservices WHERE booking_id = {id} AND status = 'Активный';";

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

        private void GuestID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void CheckInDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FillFuncSmall();
        }

        private void Payment_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TotalPrice.Text != "")
                {
    // Преобразуем TotalPrice.Text в decimal
                    if (decimal.TryParse(TotalPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalCost) &&
                        decimal.TryParse(Payment_cost.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal paymentCost))
                    {
                        // Проверяем условие
                        if (totalCost * 0.3m <= paymentCost) // Используем m для decimal
                        {
                            Status_pay.SelectedValue = 2;
                        }
                        if (totalCost == paymentCost) // Используем m для decimal
                        {
                            Status_pay.SelectedValue = 1;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверный формат чисел в TotalPrice или Payment_cost.");
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }

        private void TotalPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
         

            double result = Convert.ToDouble(TotalPrice.Text) * 0.3;

            // с точкой (300.00)
            Payment_cost.Text = result.ToString("F2", CultureInfo.InvariantCulture);

            decimal totalPriceDecimal;
            decimal paymentCostDecimal;

            // указать культуру
            var ruCulture = new CultureInfo("ru-RU");
            var enCulture = new CultureInfo("en-US");

            // Преобразуем строки
            Decimal.TryParse(TotalPrice.Text, NumberStyles.Any, ruCulture, out totalPriceDecimal);
            Decimal.TryParse(Payment_cost.Text, NumberStyles.Any, enCulture, out paymentCostDecimal);

            if (totalPriceDecimal < paymentCostDecimal)
            {
                MessageBox.Show("Предоплата не может быть больше итоговой цены");
            }

        }
        public void GenerateAndSaveContract(int bookingId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                    SELECT 
                        b.booking_id,
                        b.check_in_date,
                        b.check_out_date,
                        b.total_price,
                        CONCAT(g.first_name, ' ', g.last_name) AS guest_name,
                        g.passport_number,
                        g.phone,
                        CONCAT(e.first_name, ' ', e.last_name) AS employee_name,
                        gu.unit_name
                    FROM 
                        glamping.bookings b
                    LEFT JOIN 
                        guests g ON g.guest_id = b.guest_id
                    LEFT JOIN 
                        employees e ON e.employee_id = b.employees_id
                    LEFT JOIN 
                        glampingunits gu ON gu.unit_id = b.unit_id
                    WHERE 
                        b.booking_id = @bookingId;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@bookingId", bookingId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Извлечение данных с проверкой на NULL
                                DateTime checkInDate;
                                if (reader.IsDBNull(reader.GetOrdinal("check_in_date")))
                                {
                                    throw new Exception("Дата заезда не может быть пустой.");
                                }
                                else
                                {
                                    checkInDate = reader.GetDateTime("check_in_date");
                                }

                                DateTime checkOutDate;
                                if (reader.IsDBNull(reader.GetOrdinal("check_out_date")))
                                {
                                    throw new Exception("Дата выезда не может быть пустой.");
                                }
                                else
                                {
                                    checkOutDate = reader.GetDateTime("check_out_date");
                                }

                                decimal totalPrice;
                                if (reader.IsDBNull(reader.GetOrdinal("total_price")))
                                {
                                    throw new Exception("Общая стоимость не может быть пустой.");
                                }
                                else
                                {
                                    totalPrice = reader.GetDecimal("total_price");
                                }

                                string guestName = reader.IsDBNull(reader.GetOrdinal("guest_name")) ? "Не указано" : reader.GetString("guest_name");
                                string passportNumber = reader.IsDBNull(reader.GetOrdinal("passport_number")) ? "Не указано" : reader.GetString("passport_number");
                                string phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? "Не указано" : reader.GetString("phone");
                                string employeeName = reader.IsDBNull(reader.GetOrdinal("employee_name")) ? "Не указано" : reader.GetString("employee_name");
                                string unitName = reader.IsDBNull(reader.GetOrdinal("unit_name")) ? "Не указано" : reader.GetString("unit_name");

                                // Вычисляем количество суток
                                int days = (checkOutDate - checkInDate).Days;
                                if (days <= 0)
                                {
                                    throw new Exception("Дата выезда должна быть позже даты заезда.");
                                }

                                decimal pricePerDay = days > 0 ? totalPrice / days : 0;
                                decimal deposit = totalPrice * 0.3m;

                                // Форматируем даты и числа
                                CultureInfo ruCulture = new CultureInfo("ru-RU");
                                string contractDate = DateTime.Now.ToString("dd MMMM yyyy", ruCulture); // 03 июня 2025
                                string checkInDateStr = checkInDate.ToString("dd MMMM yyyy", ruCulture);
                                string checkOutDateStr = checkOutDate.ToString("dd MMMM yyyy", ruCulture);
                                string totalPriceStr = totalPrice.ToString("N2", ruCulture);
                                string pricePerDayStr = pricePerDay.ToString("N2", ruCulture);
                                string depositStr = deposit.ToString("N2", ruCulture);

                                // Преобразуем числа в текст
                                string totalPriceText = NumberToWords(totalPrice);
                                string pricePerDayText = NumberToWords(pricePerDay);
                                string depositText = NumberToWords(deposit);

                                // Формируем относительный путь
                                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                string contractsDirectory = System.IO.Path.Combine(baseDirectory, "Contracts");
                                string filePath = System.IO.Path.Combine(contractsDirectory, $"Contract_{bookingId}.docx");

                                if (!Directory.Exists(contractsDirectory))
                                {
                                    Directory.CreateDirectory(contractsDirectory);
                                }

                                // Создание документа Word
                                Word.Application wordApp = null;
                                Word.Document wordDoc = null;

                                try
                                {
                                    wordApp = new Word.Application();
                                    wordApp.Visible = true; // Можно установить true для отладки
                                    wordDoc = wordApp.Documents.Add();

                                    // Устанавливаем шрифт для всего документа
                                    Word.Range range = wordDoc.Content;
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Заголовок "ДОГОВОР"
                                    range.InsertAfter("ДОГОВОР");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Подзаголовок "аренды (найма) жилого дома"
                                    range.InsertAfter("аренды (найма) жилого дома");
                                    range.Font.Bold = 0;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Дата
                                    range.InsertAfter($"«{DateTime.Now:dd}» {DateTime.Now:MMMM} {DateTime.Now:yyyy} г.");
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                                    range.ParagraphFormat.SpaceAfter = 24;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Арендодатель
                                    range.InsertAfter($"{employeeName}, именуемый в дальнейшем «Арендодатель», с одной стороны и");
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Арендатор
                                    range.InsertAfter($"{guestName},\nпаспорт {passportNumber}, именуемый (-ая) в дальнейшем «Арендатор», с другой стороны, совместно именуемые «Стороны», заключили настоящий договор о нижеследующем.");
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Раздел "1. Предмет договора"
                                    range.InsertAfter("1. Предмет договора");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 1.1
                                    range.InsertAfter($"1.1 Арендодатель предоставляет Арендатору во временное пользование с целью проживания жилой дом ({unitName}), состоящий из комнат с находящимся в них имуществом и прилегающим земельным участком.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("Дом предоставляется для проживания не более 4 человек и 2 домашних животных.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 1.2
                                    range.InsertAfter($"1.2 Срок найма составляет {days} {(days == 1 ? "сутки" : "суток")} и устанавливается с 12 часов 00 мин.\n{checkInDateStr} по 12 часов 00 мин. {checkOutDateStr}.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("По согласованию сторон договор может быть продлен, о чем составляется соответствующее приложение к нему.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Раздел "2. Цена и порядок оплаты"
                                    range.InsertAfter("2. Цена и порядок оплаты");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 2.1
                                    range.InsertAfter($"2.1. Оплата за аренду жилого дома составляет:\n       {pricePerDayStr} ({pricePerDayText}) руб. в сутки.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 2.2
                                    range.InsertAfter($"2.2. Стоимость аренды за весь указанный в п. 1.2 настоящего договора период составляет:\n       {totalPriceStr} ({totalPriceText}) руб.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 2.3
                                    range.InsertAfter($"2.3. Дополнительно, в соответствии со ст.381.1 Гражданского кодекса РФ, с Арендатора взимается\nобеспечительный платеж в размере:\n        {depositStr} ({depositText}) руб.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("Обеспечительный платеж выступает гарантией возмещения ущерба, который может быть причинен по вине Арендатора в период срока аренды.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункты 2.4 - 2.6
                                    range.InsertAfter("2.4. При отсутствии нарушений договора, причинения вреда имуществу Арендатора и прочих действий, предусматривающих наложение штрафа, обеспечительный платеж возвращается Арендатору в полном размере в момент расторжения настоящего договора.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("2.5. При совершении Арендатором действий, предусматривающих наложение штрафов, в момент расторжения настоящего договора обеспечительный платеж удерживается Арендодателем в полном размере.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("2.6. Оплата аренды жилого дома производится в момент подписания настоящего договора.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Раздел "3. Права и обязанности сторон"
                                    range.InsertAfter("3. Права и обязанности сторон");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("Арендодатель гарантирует, что дом не находится под арестом, не заложен, не является предметом каких-либо претензий со стороны третьих лиц.");
                                    range.ParagraphFormat.FirstLineIndent = 0;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 3.1
                                    range.InsertAfter("3.1. Арендодатель обязуется:");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Список с маркерами
                                    range.InsertAfter("· предоставить Арендатору указанный жилой дом на срок, обозначенный в п.1.2, и обеспечить свободный доступ в помещение в течение срока действия договора;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· осуществлять техобслуживание дома и его оборудования, исправлять последствия повреждений и неисправностей, возникших не по вине Арендатора;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· возвратить сумму обеспечительного платежа в соответствии с ч. 2 ст. 381.1 Гражданского кодекса РФ при отсутствии причиненного по вине Арендатора ущерба дому и находящемуся в нем имуществу, а также прилегающему земельному участку и имуществу на нем.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 3.2
                                    range.InsertAfter("3.2. Арендодатель вправе:");
                                    range.ParagraphFormat.LeftIndent = 0;
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· по предварительному уведомлению и в присутствии арендатора или его представителя входить в дом для проверки сохранности имущества и соблюдения условий настоящего договора.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· незамедлительно в одностороннем порядке расторгнуть договор, потребовать освобождения дома и возмещения причиненных убытков при выявлении фактов существенного нарушения Арендатором своих обязательств, предусмотренных п. 3.4.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· требовать возмещения ущерба, причиненного дому, земельному участку и находящемуся там имуществу по вине Арендатора.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· удерживать из суммы обеспечительного платежа по 50% за каждый час, следующий после указанного пунктом 1.2 настоящего договора времени, если Арендатор по каким-либо причинам не передал Арендодателю дом и/или ключи от него.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· закрыть входную дверь в жилое помещение (дом) дополнительным ключом, если Арендатор по каким-либо причинам в указанное пунктом 1.2 настоящего договора время не передал Арендодателю жилое помещение (дом).");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 3.3
                                    range.InsertAfter("3.3. Арендатор вправе:");
                                    range.ParagraphFormat.LeftIndent = 0;
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· использовать дом и находящееся в нем имущество для проживания в период аренды, включая проживание других лиц, в количестве, согласованном с Арендодателем и указанном в пункте 1.1 настоящего договора.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· потребовать от Арендодателя устранения неисправностей, препятствующих пользованию домом и возникших не по вине Арендатора.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· требовать в случаях, установленных законодательством Российской Федерации, изменения настоящего договора;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· расторгнуть настоящий договор, предупредив об этом Арендодателя не менее, чем за одни сутки;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· осуществлять другие права пользования жилым помещением, предусмотренные Жилищным кодексом Российской Федерации и федеральными законами.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 3.4
                                    range.InsertAfter("3.4. Арендатор обязан:");
                                    range.ParagraphFormat.LeftIndent = 0;
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· в полном объеме оплатить стоимость аренды дома, установленную п. 2.2 настоящего договора;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· использовать предоставленный дом только по назначению (в качестве жилья), без права передачи в субаренду;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· содержать дом в технически исправном и надлежащем санитарном состоянии;");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· соблюдать правила пожарной безопасности. Курение в доме категорически запрещено!");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· в случае причинения ущерба дому, земельному участку или находящемуся там имуществу, незамедлительно сообщить об этом арендодателю и в тот же день возместить причиненный ущерб в полном объеме.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· допускать арендодателя или его представителя в дом для проверки его состояния и состояния имущества, по предварительному уведомлению и в присутствии Арендатора.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· по окончании срока аренды освободить дом и передать его Арендодателю или его представителю. В случае несвоевременного освобождения дома, Арендатор уплачивает Арендодателю сумму в размере 1000 рублей за каждый час просрочки.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("· При передаче жилого дома Арендодателю забрать из него и с территории все свои личные вещи и личные вещи других гостей. Арендодатель не несет ответственности за имущество гостей, оставленное на территории или в коттеджах.");
                                    range.ParagraphFormat.LeftIndent = wordApp.CentimetersToPoints(2f);
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(-0.5f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Пункт 3.5
                                    range.InsertAfter("3.5. Арендатор несет ответственность за соблюдение обязательств, указанных в п. 3.4 настоящего договора, всеми лицами, находящимися в доме в период его аренды.");
                                    range.ParagraphFormat.LeftIndent = 0;
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Раздел "4. Ответственность сторон"
                                    range.InsertAfter("4. Ответственность сторон");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("4.1 Арендатор несет полную материальную ответственность за ущерб, причиненный дому или имуществу Арендодателя, независимо от того, является ли этот ущерб результатом умышленных действий, неосторожности или же результатом явного бездействия арендатора или лиц, проживающих с ним или присутствующих в доме.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("4.2 Размер ответственности Арендатора перед Арендодателем не ограничивается суммой обеспечительного платежа. Задолженность Арендатора, а также компенсация за нанесенный ущерб в первую очередь взимаются из суммы обеспечительного платежа, при этом итоговая сумма уменьшается на погашенную обеспечительным платежом сумму.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("4.3 При невыполнении обязательств по оплате или компенсации нанесенного ущерба в срок, Арендатор дополнительно оплачивает Арендодателю пени за просрочку фактически неоплаченной суммы в размере 1% от суммы задолженности за каждый день просрочки.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("4.4 Материальная ответственность за нанесенный ущерб определяется, исходя из определяемой настоящим договором полной стоимости дома и всего находящегося в нем имущества в размере 100%. Указанная сумма определяется как возмещаемая стоимость при полном уничтожении имущества или почти полном уничтожении, когда остаточное состояние имущества требует полного его демонтажа.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Раздел "5. Заключительные условия"
                                    range.InsertAfter("5. Заключительные условия");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("5.1 Все споры, связанные с исполнением настоящего договора, решаются путем переговоров для достижения общей договоренности.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("5.2. Стороны подтверждают, что ознакомлены со всеми условиями настоящего договора, полностью согласны с ними и лично несут ответственность за их соблюдение.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("5.3 Настоящий договор составлен в двух экземплярах, один из которых находится у Арендатора, второй – у Арендодателя.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("5.4 Договор вступает в силу с момента его подписания.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("5.5 Позднее заселение Арендатора в дом не влечет изменения срока окончания аренды, определенного договором.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 6;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("5.6 Арендодатель не несет ответственности за любой материальный и физический ущерб Арендатора или лиц, присутствующих либо проживающих с ним, а также за сохранность принадлежащих им вещей и ценностей, в том числе забытых при выселении.");
                                    range.ParagraphFormat.FirstLineIndent = wordApp.CentimetersToPoints(1.25f);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Раздел "6. Реквизиты и подписи сторон"
                                    range.InsertAfter("6. Реквизиты и подписи сторон");
                                    range.Font.Bold = 1;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Арендодатель
                                    range.InsertAfter("Арендодатель:\n" + employeeName);
                                    range.ParagraphFormat.FirstLineIndent = 0;
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("/____________________/");
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 24;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Арендатор
                                    range.InsertAfter("Арендатор:\n" + guestName + "\nТелефон " + phone);
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 12;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    range.InsertAfter("/____________________/");
                                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                    range.ParagraphFormat.SpaceAfter = 0;
                                    range.InsertParagraphAfter();
                                    range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                                    // Сохранение документа
                                    object fileName = filePath;
                                    object fileFormat = Word.WdSaveFormat.wdFormatDocumentDefault;
                                    wordDoc.SaveAs(ref fileName, ref fileFormat);
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = filePath,
                                        UseShellExecute = true // обязательно для открытия через ассоциацию с Word
                                    });

                                    Console.WriteLine("Договор успешно сохранен в: " + filePath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка при работе с Word: {ex.Message}");
                                    throw;
                                }
                            }
                            else
                            {
                                throw new Exception($"Бронирование с ID {bookingId} не найдено.");
                            }
                        }
                    }
                }
            
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при генерации договора: {ex.Message}");
        throw;
    }
}

        private string NumberToWords(decimal number)
        {
            int wholePart = (int)number;
            int fractionalPart = (int)((number - wholePart) * 100);

            string[] units = { "", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять" };
            string[] teens = { "десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" };
            string[] tens = { "", "", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто" };
            string[] hundreds = { "", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот" };

            string result = "";

            int thousands = wholePart / 1000;
            int remainder = wholePart % 1000;

            // тысячи
            if (thousands > 0)
            {
                if (thousands == 1)
                    result += "одна тысяча ";
                else if (thousands == 2)
                    result += "две тысячи ";
                else if (thousands >= 3 && thousands <= 4)
                    result += units[thousands] + " тысячи ";
                else if (thousands >= 5 && thousands <= 9)
                    result += units[thousands] + " тысяч ";
                else if (thousands >= 10 && thousands <= 19)
                    result += teens[thousands - 10] + " тысяч ";
                else
                    result += units[thousands] + " тысяч "; // fallback
            }

            // сотни
            int hundredsPart = remainder / 100;
            if (hundredsPart > 0)
                result += hundreds[hundredsPart] + " ";

            // десятки и единицы
            int tensPart = (remainder % 100) / 10;
            int unitsPart = remainder % 10;

            if (tensPart > 1)
            {
                result += tens[tensPart] + " ";
                if (unitsPart > 0)
                    result += units[unitsPart] + " ";
            }
            else if (tensPart == 1)
            {
                result += teens[unitsPart] + " ";
            }
            else if (unitsPart > 0)
            {
                result += units[unitsPart] + " ";
            }
            else if (wholePart == 0)
            {
                result = "ноль ";
            }

            return $"{result.Trim()} рублей {fractionalPart:D2} копеек";
        }
        private void GenerateReceipt(int bookingId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            select
	concat_ws(' ', guests.first_name, guests.last_name) AS GuestName,
    concat_ws(' ', employees.first_name, employees.last_name) AS EmployeeName,
    bookings.upfront_payment AS Prepayment,
    COALESCE(bookings.total_price - COALESCE(bookings.upfront_payment, 0), bookings.total_price) AS RemainingPayment,
    COALESCE(glampingunits.unit_name, 'Не указано') AS UnitName
    from bookings
left join
	guests on guests.guest_id = bookings.guest_id
left join
	employees on employees.employee_id = bookings.employees_id
left join
	glampingunits on glampingunits.unit_id = bookings.unit_id
    		where bookings.booking_id = @bookingId;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@bookingId", bookingId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show($"Бронирование с ID {bookingId} не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string guestName = reader.IsDBNull(reader.GetOrdinal("GuestName")) ? "Не указано" : reader.GetString("GuestName");
                        string employeeName = reader.IsDBNull(reader.GetOrdinal("EmployeeName")) ? "Не указано" : reader.GetString("EmployeeName");
                        decimal prepayment = reader.IsDBNull(reader.GetOrdinal("Prepayment")) ? 0m : reader.GetDecimal("Prepayment");
                        decimal remainingPayment = reader.IsDBNull(reader.GetOrdinal("RemainingPayment")) ? 0m : reader.GetDecimal("RemainingPayment");
                        string unitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? "Не указано" : reader.GetString("UnitName");

                        Word.Application wordApp = null;
                        Word.Document wordDoc = null;
                        Word.Range range = null;

                        try
                        {
                            wordApp = new Word.Application();
                            wordApp.Visible = false; // Можно установить true для отладки
                            wordDoc = wordApp.Documents.Add();

                            // Установка шрифта для всего документа
                            wordDoc.Content.Font.Name = "Times New Roman";
                            wordDoc.Content.Font.Size = 12;

                            // Инициализация range
                            range = wordDoc.Content;
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            // Логотип (если доступен)
                            string logoPath = System.IO.Path.GetFullPath("Logo.png");
                            if (File.Exists(logoPath))
                            {
                                Word.InlineShape logo = range.InlineShapes.AddPicture(logoPath);
                                logo.Width = 100;
                                logo.Height = 50;
                                range.InsertParagraphAfter();
                                range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                            }

                            // Шапка
                            range.InsertAfter("Глэмпинг");
                            range.Font.Bold = 1;
                            range.Font.Size = 16;
                            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            range.ParagraphFormat.SpaceAfter = 12;
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            range.InsertAfter("Кассовый чек");
                            range.Font.Bold = 1;
                            range.Font.Size = 14;
                            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            range.ParagraphFormat.SpaceAfter = 12;
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            range.InsertAfter($"Дата и время: {DateTime.Now:dd.MM.yyyy HH:mm}");
                            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                            range.ParagraphFormat.SpaceAfter = 12;
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            // Таблица
                            Word.Table table = wordDoc.Tables.Add(range, 5, 2, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
                            table.Borders.Enable = 1; // Включение границ таблицы
                            table.Range.Font.Size = 12;

                            // Заголовки таблицы
                            table.Cell(1, 1).Range.Text = "Параметр";
                            table.Cell(1, 2).Range.Text = "Значение";
                            table.Rows[1].Range.Font.Bold = 1;
                            table.Rows[1].Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray15; // Серый фон для заголовков

                            // Данные
                            table.Cell(2, 1).Range.Text = "Кто оплатил";
                            table.Cell(2, 2).Range.Text = guestName;

                            table.Cell(3, 1).Range.Text = "Кто принимал";
                            table.Cell(3, 2).Range.Text = employeeName;

                            table.Cell(4, 1).Range.Text = "Предоплата";
                            table.Cell(4, 2).Range.Text = $"{prepayment:N2} руб.";

                            table.Cell(5, 1).Range.Text = "Остаток к оплате";
                            table.Cell(5, 2).Range.Text = $"{remainingPayment:N2} руб.";

                            table.Rows[2].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            table.Rows[3].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            table.Rows[4].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            table.Rows[5].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                            // Добавляем название дома после таблицы
                            range = table.Range;
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            range.InsertAfter($"Дом: {unitName}");
                            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                            range.ParagraphFormat.SpaceAfter = 12;
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            // Подпись
                            range.InsertAfter("Подпись клиента: ____________________");
                            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            range.InsertAfter("Подпись сотрудника: ____________________");
                            range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                            range.InsertParagraphAfter();
                            range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                            // Сохранение документа
                            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            string receiptsDirectory = System.IO.Path.Combine(baseDirectory, "Receipts");
                            if (!Directory.Exists(receiptsDirectory))
                                Directory.CreateDirectory(receiptsDirectory);

                            string filePath = System.IO.Path.Combine(receiptsDirectory, $"Receipt_{bookingId}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx");
                            object fileName = filePath;
                            object fileFormat = Word.WdSaveFormat.wdFormatDocumentDefault;
                            wordDoc.SaveAs(ref fileName, ref fileFormat);
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true 
                            });

                            MessageBox.Show($"Чек успешно сохранён: {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при создании чека: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            
                        }
                    }
                }
            }
        }

    }
}
