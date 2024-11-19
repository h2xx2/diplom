using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
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

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для redactBooking.xaml
    /// </summary>
    public partial class redactBooking : Window
    {
        public redactBooking()
        {
            InitializeComponent();
            this.DataContext = this;
            EmployeeID.Text = Class1.fioEmploes;

        }
        public DateTime? MinDate { get; set; }
        private List<int> selectedServiceIds = new List<int>();
        string connectionString = Class1.connection;
        int count = 0;
        string phoneNumber = "";
        int id_guest = 0;
        int id_unit = 0;
        string price = "";
        int cost = 0;
        private bool isSelecting = false;
        string query = @"
        SELECT unit_id, unit_name, capacity, price_per_night, `description`
        FROM glampingunits
        WHERE NOT EXISTS (
            SELECT 1
            FROM bookings
            WHERE bookings.unit_id = glampingunits.unit_id
              AND (@StartDate < bookings.check_in_date AND @EndDate > bookings.check_out_date)
        )";
        public class ServiceModel
        {
            public int service_id { get; set; }
            public string service_name { get; set; }
            public string description { get; set; }
            public string price { get; set; }
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

        public void FillDataService()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    MySqlCommand command = new MySqlCommand("SELECT service_id,service_name,description,price FROM services", connection);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    ServiceModels.Clear(); // Очистка коллекции перед загрузкой данных

                    while (reader.Read())
                    {
                        ServiceModels.Add(new ServiceModel
                        {
                            service_id = Convert.ToInt32(reader["service_id"].ToString()),
                            service_name = reader["service_name"].ToString(),
                            description = reader["description"].ToString(),
                            price = reader["price"].ToString(),
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
            FillDataGrid(com,checkIn,checkOut);
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
            FillDataGrid(query, checkIn,checkOut);
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
                            TotalPrice.Text = cost.ToString();
                        }
                    }
                }
            }
        }
        public static decimal CalculateTotalCost(List<ServiceModel> allServices, List<int> selectedServiceIds)
        {
            decimal totalCost = 0;

            // Перебираем все выбранные идентификаторы услуг
            foreach (var serviceId in selectedServiceIds)
            {
                // Находим услугу по ID
                var service = allServices.FirstOrDefault(s => s.service_id == serviceId);
                if (service != null)
                {
                    // Преобразуем строковое значение цены в decimal и добавляем к общей стоимости
                    if (decimal.TryParse(service.price, out decimal price))
                    {
                        totalCost += price;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка при преобразовании цены услуги с ID {serviceId}");
                    }
                }
            }

            return totalCost;
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
            foreach (var addedItem in e.AddedItems)
            {
                var service = addedItem as ServiceModel; // Приведение к модели данных
                if (service != null && !selectedServiceIds.Contains(service.service_id))
                {
                    selectedServiceIds.Add(service.service_id);
                    Service.Text += $"{service.service_name} "; // Добавляем название в TextBox
                }
            }

            // Убираем отмененные элементы
            foreach (var removedItem in e.RemovedItems)
            {
                var service = removedItem as ServiceModel; // Приведение к модели данных
                if (service != null && selectedServiceIds.Contains(service.service_id))
                {
                    selectedServiceIds.Remove(service.service_id);
                    // Удаляем текст из TextBox (если необходимо)
                    Service.Text = Service.Text.Replace($"{service.service_name} ", string.Empty);
                }
            }
        }

        private void ID_GotFocus_1(object sender, RoutedEventArgs e)
        {
            FillDataService();
            AnimateListViewHeight(listService, 0, 200, 0.5);
        }

        private void listService_LostFocus(object sender, RoutedEventArgs e)
        {
            AnimateListViewHeight(listService, 200, 0, 0.5);
        }
        private bool AreFieldsFilled()
        {
            if (string.IsNullOrEmpty(Fio.Text) ||
                string.IsNullOrEmpty(EmployeeID.Text) ||
                string.IsNullOrEmpty(CheckInDate.Text) ||
                string.IsNullOrEmpty(CheckOutDate.Text) ||
                string.IsNullOrEmpty(UnitID.Text) ||
                string.IsNullOrEmpty(Service.Text) ||
                string.IsNullOrEmpty(TotalPrice.Text) ||
                string.IsNullOrEmpty(StatusBooking.Text)
               )
            {
                return false;
            }
            return true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (AreFieldsFilled())
            {

                query = "INSERT bookings(guest_id, unit_id, employees_id, check_in_date, check_out_date, total_price,booking_status, created_at) VALUES(@guest_id,@unit_id,@employees_id,@check_in_date, @check_out_date,@total_price,@booking_status, @created_at); SELECT LAST_INSERT_ID();";

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
                            command.Parameters.AddWithValue("@guest_id", id_guest);
                            command.Parameters.AddWithValue("@unit_id",id_unit);
                            command.Parameters.AddWithValue("@employees_id", Class1.id_employes);
                            command.Parameters.AddWithValue("@check_in_date", dbs);
                            command.Parameters.AddWithValue("@check_out_date", dbs1);
                            command.Parameters.AddWithValue("@total_price", TotalPrice.Text);
                            command.Parameters.AddWithValue("@booking_status", StatusBooking.Text);
                            command.Parameters.AddWithValue("@created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        // Выполняем запрос
                        object result = command.ExecuteScalar();

                            // Проверяем количество измененных строк
                            if ( result != null)
                            {
                            SaveServiceBooking(result);
                                MessageBox.Show("Данные успешно обновлены.");
                            }
                            
                            else
                            {
                                MessageBox.Show("Ошибка при обновлении данных.");
                                Class1.add = 0;
                            }
                        
                    }
                }
            }
            else
            {
                MessageBox.Show("Данные не изменены");
            }
        }
        private void SaveServiceBooking(object idBooking)
        {

            if (AreFieldsFilled())
            {
                const int batchSize = 1000; // Размер партии
                List<string> batch = new List<string>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var id in selectedServiceIds)
                    {
                        batch.Add($"({idBooking}, {id}, 1, 'Активный')");

                        // Если партия достигла размера batchSize, выполняем запрос
                        if (batch.Count >= batchSize)
                        {
                            ExecuteBatchInsert(connection, batch);
                            batch.Clear();
                        }
                    }

                    // Выполняем оставшуюся часть
                    if (batch.Count > 0)
                    {
                        ExecuteBatchInsert(connection, batch);
                    }

                    MessageBox.Show("Все данные успешно добавлены.");
                }
            }
            else
            {
                MessageBox.Show("Данные не изменены");
            }
        }
        private void ExecuteBatchInsert(MySqlConnection connection, List<string> batch)
        {
            string query = "INSERT INTO bookingservices(booking_id, service_id, quantity, status) VALUES " + string.Join(", ", batch) + ";";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

    }
}
