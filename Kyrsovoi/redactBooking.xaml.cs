using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
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

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для redactBooking.xaml
    /// </summary>
    
    public partial class redactBooking : Window
    {
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
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
        private decimal totalServiceCost = 0;
        private string oldGuest = "";
        
        private string oldUnit = "";
        private string dateIn = "";
        private string dateOut = "";
        private string service = "";

        private string bookingstatus = "";
        private string price = "";
        public DateTime? MinDate { get; set; }
        private List<int> selectedServiceIds = new List<int>();
        string connectionString = Class1.connection;
        int count = 0;
        string phoneNumber = "";
        int id_guest = 0;
        int id_unit = 0;
        string totalPrice = "";
        int cost = 0;
        int savecost = 0;
        private bool isSelecting = false;
        string id = "";
        string query = @"
        SELECT unit_id, unit_name, capacity, price_per_night, `description`
        FROM glampingunits
        WHERE NOT EXISTS (
            SELECT 1
            FROM bookings
            WHERE bookings.unit_id = glampingunits.unit_id
              AND (@StartDate < bookings.check_in_date AND @EndDate > bookings.check_out_date)
        )";
        public class ServiceModel : INotifyPropertyChanged
        {
            public int service_id { get; set; }
            public string service_name { get; set; }
            public string description { get; set; }
            public string price { get; set; }

            public bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        string com = "";
        public class House
        {
            public string id { get; set; }
            public string name { get; set; }
            public string capacity { get; set; }
            public string price { get; set; }
            public string description { get; set; }
        }
        public ObservableCollection<House> Houses { get; set; } = new ObservableCollection<House>();
        public ObservableCollection<Booking> Book { get; set; } = new ObservableCollection<Booking>();
        public ObservableCollection<ServiceModel> ServiceModels { get; set; } = new ObservableCollection<ServiceModel>();
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
                                GROUP_CONCAT(services.service_name SEPARATOR ' ') AS services,
                                b.check_in_date, 
                                b.check_out_date, 
                                b.total_price, 
                                b.booking_status, 
                                b.created_at
                            FROM 
                                glamping.bookings b
                            LEFT JOIN 
                                guests ON guests.guest_id = b.guest_id
                            LEFT JOIN 
                                bookingservices ON bookingservices.booking_id = b.booking_id
                            LEFT JOIN 
                                services ON services.service_id = bookingservices.service_id 
                            LEFT JOIN 
                                glampingunits ON glampingunits.unit_id = b.unit_id 
                            LEFT JOIN 
                                employees ON employees.employee_id = b.employees_id
                            WHERE 
                                b.booking_id = " + Class1.booking_id + @" and bookingservices.`status` = 'Активный'
                            GROUP BY 
                                b.booking_id;
                            ";

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
                        booking.Service_name = rdr["services"].ToString();
                        service = booking.Service_name;
                        if (Class1.add ==1)
                        {
                            booking.Employees_id = rdr["employee"].ToString();
                        }
                        
                        booking.Check_in_date = rdr["check_in_date"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rdr["check_in_date"]);
                        DateTime date1 = Convert.ToDateTime(booking.Check_in_date);
                        dateIn = date1.ToString("MM.dd.yyyy");
                        booking.Check_out_date = rdr["check_out_date"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rdr["check_out_date"]);

                        DateTime date = Convert.ToDateTime(booking.Check_out_date);
                        dateOut = date.ToString("MM.dd.yyyy");
                        booking.Total_price = rdr["total_price"].ToString();
                        totalPrice = booking.Total_price;
                        booking.Booking_status = rdr["booking_status"].ToString();
                        bookingstatus = booking.Booking_status;
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
            private string _service_name;
            private string _employees_id;
            private DateTime? _check_in_date;
            private DateTime? _check_out_date;
            private string _total_price;
            private string _booking_status;

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
            public string Service_name
            {
                get => _service_name;
                set
                {
                    _service_name = value;
                    OnPropertyChanged(nameof(Service_name));
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
            public string Booking_status
            {
                get => _booking_status;
                set
                {
                    _booking_status = value;
                    OnPropertyChanged(nameof(Booking_status));
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
                this.Close();
            }
        }

        private void cb1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
                            Class1.add = 1;
                            redactKlient redactKlient = new redactKlient();
                            redactKlient.Show();
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

        private void UpdateTotalCost()
        {
            try
            {
                // Суммируем стоимость выбранных услуг, проверяя значение price
                totalServiceCost = ServiceModels
                    .Where(service => service.IsSelected)
                    .Sum(service =>
                    {
                        if (decimal.TryParse(service.price, out var servicePrice))
                            return servicePrice; // Если цена корректна, добавляем её
                        else
                            return 0; // Если некорректна, добавляем 0
                    });



                // Пересчитываем итоговую стоимость с учетом выбранных услуг
                cost = (int)(savecost + totalServiceCost);

                // Форматируем и отображаем итоговую цену
                TotalPrice.Text = cost.ToString("0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчёте стоимости: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                        }
                    }
                    else
                    {
                        CheckOutDate.Text = null;
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

        private void listService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private List<int> GetSelectedServiceIdsFromBooking(int bookingId)
        {
            List<int> ids = new List<int>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT service_id FROM bookingservices WHERE booking_id = @booking_id AND status = 'Активный'";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@booking_id", bookingId);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ids.Add(Convert.ToInt32(reader["service_id"]));
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке выбранных услуг: {ex.Message}");
                }
            }

            return ids;
        }
        private List<int> temporarySelectedServiceIds = new List<int>();
        private void FillDataServiceWithSelection(List<int> selectedIds)
        {
            ServiceModels.Clear(); // Очистка перед загрузкой

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT service_id, service_name, description, price FROM services";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int serviceId = Convert.ToInt32(reader["service_id"]);
                        var service = new ServiceModel
                        {
                            service_id = serviceId,
                            service_name = reader["service_name"].ToString(),
                            description = reader["description"].ToString(),
                            price = reader["price"].ToString(),
                            // Помечаем как выбранную, если ID есть в списке selectedIds
                            IsSelected = selectedIds.Contains(serviceId) || temporarySelectedServiceIds.Contains(serviceId)
                        };

                        ServiceModels.Add(service);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}");
                }
            }
        }
        
        private void ID_GotFocus_1(object sender, RoutedEventArgs e)
        {

        }
        private void UpdateServiceText()
        {


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
            if (guest != oldGuest || unit != oldUnit || datein != dateIn || dateout != dateOut || status != bookingstatus || totalprice != totalPrice)
            {
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
                if (IsTextChanged(GuestID.Text, UnitID.Text, CheckInDate.Text, CheckOutDate.Text, StatusBooking.Text, TotalPrice.Text))
                {
                    if (Class1.add != 1)
                    {
                        query = "UPDATE bookings SET guest_id = @guest_id, unit_id = @unit_id, check_in_date = @check_in_date, check_out_date = @check_out_date, total_price = REPLACE(@total_price, ',', '.'), booking_status =@booking_status WHERE booking_id = @id";

                    }
                    else
                    {
                        query = "INSERT bookings(guest_id, unit_id, employees_id, check_in_date, check_out_date, total_price,booking_status, created_at) VALUES(@guest_id,@unit_id,@employees_id,@check_in_date, @check_out_date,@total_price,@booking_status, @created_at); SELECT LAST_INSERT_ID();";
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
                            command.Parameters.AddWithValue("@booking_status", StatusBooking.Text);
                            command.Parameters.AddWithValue("@created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            // Выполняем запрос
                            try
                            {
                                object result = command.ExecuteScalar();
                                if (Class1.add != 1)
                                {
                                    // Проверяем количество измененных строк
                                    if (result != null)
                                    {
                                        SaveServiceBooking(result);
                                        MessageBox.Show("Данные успешно добавлены.");
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
                                }
                            }
                            catch(Exception)
                            {
                                MessageBox.Show("Ошибка при обновление данных.");
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Не все поля заполнены");
            }
        }
        private void SaveServiceBooking(object idBooking)
        {

            const int batchSize = 1000; // Размер партии
            List<string> batch = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                foreach (var id in selectedServiceIds)
                {
                    batch.Add($"({idBooking}, {id}, 1, 'Активный')");

                    if (batch.Count >= batchSize) // Пакетная вставка
                    {
                        ExecuteBatchInsert(connection, batch);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    ExecuteBatchInsert(connection, batch);
                }
            }
        }
        private void DeleteOldServices(object idBooking)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM bookingservices WHERE booking_id = @booking_id";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@booking_id", idBooking);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            foreach (Control control in new[] { GuestID,  })
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
        }
        private void ExecuteBatchInsert(MySqlConnection connection, List<string> batch)
        {
            DeleteOldServices(Class1.booking_id);
            string query = "INSERT INTO bookingservices(booking_id, service_id, quantity, status) VALUES " + string.Join(", ", batch) + ";";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            delete.Visibility = Visibility.Visible;
            if (Class1.add != 1)
            {
                var booking = new Booking();
                this.DataContext = booking; // Устанавливаем DataContext
                SetFieldsReadOnly(true);
                FillTextBox(); // Теперь DataContext точно не null
                SpEmpoy.Visibility = Visibility.Collapsed;
                


            }
            else
            {
                var booking = new Booking();
                this.DataContext = booking;
                SetFieldsReadOnly(false);
                button.Content = "Сохранить";
                SpEmpoy.Visibility = Visibility.Visible;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                // Получаем соответствующую услугу
                var service = checkBox.DataContext as ServiceModel;
                if (service != null)
                {
                    // Если услуга еще не была выбрана, добавляем её
                    if (!selectedServiceIds.Contains(service.service_id))
                    {
                        selectedServiceIds.Add(service.service_id);
                        service.IsSelected = true; // Помечаем как выбранную
                    }

                    // Обновляем текст и пересчитываем стоимость
                    UpdateServiceText();
                    UpdateTotalCost();
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Приводим sender к CheckBox
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                // Получаем соответствующую услугу
                var service = checkBox.DataContext as ServiceModel;
                if (service != null)
                {
                    // Если услуга была выбрана, убираем её
                    if (selectedServiceIds.Contains(service.service_id))
                    {
                        selectedServiceIds.Remove(service.service_id);
                        service.IsSelected = false; // Снимаем пометку
                    }

                    // Обновляем текст и пересчитываем стоимость
                    UpdateServiceText();
                    UpdateTotalCost();
                }
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

        }
    }
}
