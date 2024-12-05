using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using System.Data.Common;
using System.Data;
using System.Globalization;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для Prosmotr.xaml
    /// </summary>
    public partial class Prosmotr : System.Windows.Window
    {
        public Prosmotr()
        {
            InitializeComponent();
            DataContext = this;
            com = query;
            FillDataGrid(_currentPage, com);
            cb2.SelectedIndex = 2;
            try
            {
                CalculateTotalPages(); // Рассчитать общее количество страниц
                _currentPage = 1; // Установить начальную страницу
                UpdatePageInfo(); // Обновить информацию о текущей странице
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public class Client {
            public string name { get; set; }
            public string surname { get; set; }
            public string email { get; set; }
            public string number { get; set; }
            public string dateRegistration { get; set; }
            public string passport { get; set; }
            public string dateOfBirthday { get; set; }
        }
        public class Home
        {
            public string unit_id { get; set; }
            public string unit_name { get; set; }
            public string unit_type { get; set; }
            public string capacity { get; set; }
            public string price_per_night { get; set; }
            public string description { get; set; }
            public string status { get; set; }
            public BitmapImage photo { get; set; }
        }
        public class Employee
        {
            public string employee_id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string position { get; set; }
            public string hire_date { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string login { get; set; }
            public string password { get; set; }
            public string role { get; set; }
        }
        public class Booking
        {
            public string id_booking { get; set; }
            public string guests { get; set; }
            public string employee { get; set; }
            public string unit { get; set; }
            public string check_in_date { get; set; }
            public string check_out_date { get; set; }
            public string total_price { get; set; }
            public string booking_status { get; set; }
            public string created_at { get; set; }
            public DateTime? Сheck_in_date
            {
                get
                {
                    if (DateTime.TryParseExact(check_in_date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        return date;
                    return null; // Если формат даты неверный
                }
            }

            // Свойство для цвета строки
            public Brush RowColor
            {
                get
                {
                    if (Сheck_in_date.HasValue)
                    {
                        var daysDifference = (DateTime.Now - Сheck_in_date.Value).Days;

                        if (daysDifference < 0)
                            return Brushes.LightGreen; // Будущее
                        if (daysDifference <= 3)
                            return Brushes.Yellow; // Менее 3 дней
                        return Brushes.Red; // Просрочено
                    }
                    return Brushes.Gray; // Если дата отсутствует
                }
            }
        }

        
        public class Services
        {
            public string id_service { get; set; }
            public string service_name { get; set; }
            public string description { get; set; }
            public string price { get; set; }
        }
        string filePath = "";
        string query = @"SELECT 
                        b.booking_id,
                        glampingunits.unit_name,
                        CONCAT(guests.first_name, ' ', guests.last_name) AS guest,
                        CONCAT(employees.first_name, ' ', employees.last_name) AS employee,
                        b.check_in_date, 
                        b.check_out_date, 
                        b.total_price, 
                        b.booking_status, 
                        b.created_at
                    FROM 
                        glamping.bookings b
                    LEFT JOIN 
                        guests ON guests.guest_id = b.booking_id
					LEFT JOIN 
                        glampingunits ON glampingunits.unit_id = b.unit_id
                    LEFT JOIN 
                        employees ON employees.employee_id = b.booking_id";
        string com = "";
        int raspred = 0;
        string dopCom0 = string.Empty;
        string dopCom1 = string.Empty;
        string dopCom2 = string.Empty;
        string saveQuery = string.Empty;

        int _pageSize = 10;
        int _totalPages = 0;
        int _currentPage = 1;

        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<Booking> Bookings { get; set; } = new ObservableCollection<Booking>();
        public ObservableCollection<Services> Servic { get; set; } = new ObservableCollection<Services>();
        public ObservableCollection<Home> Homes { get; set; } = new ObservableCollection<Home>();
        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
        string connectionString = Class1.connection;

        public void FillDataGrid(int _currentPage,string com)
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                CalculateTotalPages();
                UpdatePageInfo();     
                GeneratePageButtons();

                int offset = (_currentPage - 1) * _pageSize;
                MySqlCommand command = new MySqlCommand(com + $" LIMIT {_pageSize} OFFSET {offset}", connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                Clients.Clear(); // Очистка коллекции перед загрузкой данных
                Bookings.Clear();
                Servic.Clear();
                Homes.Clear();
                Employees.Clear();
                while (reader.Read())
                {
                    if (raspred == 1)
                    {
                        bookings.Visibility = Visibility.Collapsed;
                        service.Visibility = Visibility.Collapsed;
                        clients.Visibility = Visibility.Visible;
                        employee.Visibility = Visibility.Collapsed;
                        homes.Visibility = Visibility.Collapsed;

                        Clients.Add(new Client
                        {
                            name = reader["first_name"].ToString(),
                            surname = reader["last_name"].ToString(),
                            email = reader["email"].ToString(),
                            number = reader["phone"].ToString(),
                            dateRegistration = reader["registration_date"].ToString(),
                            passport = reader["passport_number"].ToString(),
                            dateOfBirthday = reader["date_of_birth"].ToString(),
                        });
                    }
                    if (raspred == 0)
                    {
                        clients.Visibility = Visibility.Collapsed;
                        service.Visibility = Visibility.Collapsed;
                        bookings.Visibility = Visibility.Visible;
                        employee.Visibility = Visibility.Collapsed;
                        homes.Visibility = Visibility.Collapsed;
                        Bookings.Add(new Booking
                        {
                            id_booking = reader["booking_id"].ToString(),
                            guests = reader["guest"].ToString(),
                            employee = reader["employee"].ToString(),
                            unit = reader["unit_name"].ToString(),
                            check_in_date = reader["check_in_date"].ToString(),
                            check_out_date = reader["check_out_date"].ToString(),
                            total_price = reader["total_price"].ToString(),
                            booking_status = reader["booking_status"].ToString(),
                            created_at = reader["created_at"].ToString(),
                        });
                    }
                    if (raspred == 2)
                    {
                        clients.Visibility = Visibility.Collapsed;
                        bookings.Visibility = Visibility.Collapsed;
                        service.Visibility = Visibility.Visible;
                        employee.Visibility = Visibility.Collapsed;
                        homes.Visibility = Visibility.Collapsed;
                        Servic.Add(new Services
                        {
                            id_service = reader["service_id"].ToString(),
                            service_name = reader["service_name"].ToString(),
                            description = reader["description"].ToString(),
                            price = reader["price"].ToString(),

                        });
                    }
                    if (raspred == 3)
                    {
                        clients.Visibility = Visibility.Collapsed;
                        bookings.Visibility = Visibility.Collapsed;
                        service.Visibility = Visibility.Collapsed;
                        employee.Visibility = Visibility.Visible;
                        homes.Visibility = Visibility.Collapsed;
                        Class1.employee_id = Convert.ToInt32(reader["employee_id"]);
                        Employees.Add(new Employee
                        {

                            first_name = reader["first_name"].ToString(),
                            last_name = reader["last_name"].ToString(),
                            position = reader["position"].ToString(),
                            hire_date = reader["hire_date"].ToString(),
                            phone = reader["phone"].ToString(),
                            email = reader["email"].ToString(),
                            login = reader["login"].ToString(),
                            password = reader["password"].ToString(),
                            role = reader["role"].ToString(),

                        });
                        
                    }
                    if (raspred == 4)
                    {
                        clients.Visibility = Visibility.Collapsed;
                        bookings.Visibility = Visibility.Collapsed;
                        service.Visibility = Visibility.Collapsed;
                        employee.Visibility = Visibility.Collapsed;
                        homes.Visibility = Visibility.Visible;
                        string fileName = ".\\home\\" + reader["photo"]?.ToString();
                        string filepath = Path.GetFullPath(fileName); 

                        // Загрузка изображения
                        BitmapImage bitmap = new BitmapImage();
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(filepath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }
                        
                        // Добавление данных
                        Homes.Add(new Home
                        {
                            unit_id = reader["unit_id"].ToString(),
                            unit_name = reader["unit_name"].ToString(),
                            unit_type = reader["unit_type"].ToString(),
                            capacity = reader["capacity"].ToString(),
                            price_per_night = reader["price_per_night"].ToString(),
                            description = reader["description"].ToString(),
                            photo = bitmap,
                        });
                    }


                }
                reader.Close();
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
                MainWindow mainWindow = new MainWindow();
                this.Close();
                mainWindow.Show();
            }
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == true)
            {
                tt_home.Visibility = Visibility.Collapsed;
                tt_service.Visibility = Visibility.Collapsed;
                tt_suppliers.Visibility = Visibility.Collapsed;
                tt_booking.Visibility = Visibility.Collapsed;
            }
            else
            {
                tt_home.Visibility = Visibility.Visible;
                tt_service.Visibility = Visibility.Visible;
                tt_suppliers.Visibility = Visibility.Visible;
                tt_booking.Visibility = Visibility.Visible;
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Class1.saveQuery = com;
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
        }

        private void Service_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Class1.role == 0)
            {
                addService.Visibility = Visibility.Visible;
            }
            
            placeholder.Visibility = Visibility.Visible;
            addHouse.Visibility = Visibility.Collapsed;   
            tb1.Visibility = Visibility.Visible;
            addEmployee.Visibility = Visibility.Collapsed;
            addUser.Visibility = Visibility.Collapsed;
            Add_Booking.Visibility = Visibility.Collapsed;
            addUser.Visibility = Visibility.Collapsed;
            cb2.Width = 270;
            cb2.Margin = new Thickness(370, 50, 0, 0);
            tb1.Width = 270;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            raspred = 2;
            tbNameForm.Text = "Сервисы";
            query = "select * from services";
            cb1.SelectedIndex = 4;
            cb1.Visibility = Visibility.Collapsed;
            cb2.Visibility = Visibility.Visible;
            cb2.SelectedIndex = 2;
            tb1.Clear();
            com = query;
            FillDataGrid(_currentPage, com);
        }

        private void StackPanel_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            addHouse.Visibility = Visibility.Collapsed;
            placeholder.Visibility = Visibility.Visible;
            addService.Visibility = Visibility.Collapsed;
            tb1.Visibility = Visibility.Visible;
            addEmployee.Visibility = Visibility.Collapsed;
            addUser.Visibility = Visibility.Collapsed;
            Add_Booking.Visibility = Visibility.Visible;
            tb1.Visibility = Visibility.Visible;
            cb1.Visibility = Visibility.Visible;
            cb2.Visibility = Visibility.Visible;
            addUser.Visibility = Visibility.Collapsed;
            cb2.Width = 210;
            cb2.Margin = new Thickness(550, 50, 0, 0);
            tb1.Width = 210;
            tbNameForm.Text = "Бронирование";
            query = @"SELECT 
                        b.booking_id,
                        glampingunits.unit_name,
                        CONCAT(guests.first_name, ' ', guests.last_name) AS guest,
                        CONCAT(employees.first_name, ' ', employees.last_name) AS employee,
                        b.check_in_date, 
                        b.check_out_date, 
                        b.total_price, 
                        b.booking_status, 
                        b.created_at
                    FROM 
                        glamping.bookings b
                    LEFT JOIN 
                        guests ON guests.guest_id = b.booking_id
					LEFT JOIN 
                        glampingunits ON glampingunits.unit_id = b.unit_id
                    LEFT JOIN 
                        employees ON employees.employee_id = b.booking_id";
            raspred = 0;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(_currentPage, com);
            cb1.Visibility = Visibility.Visible;
            cb2.SelectedIndex = 2;
            tb1.Text = "";

        }

        private void StackPanel_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            addHouse.Visibility = Visibility.Collapsed;
            placeholder.Visibility = Visibility.Visible;
            addService.Visibility = Visibility.Collapsed;
            tb1.Visibility = Visibility.Visible;
            addEmployee.Visibility = Visibility.Collapsed;
            Add_Booking.Visibility = Visibility.Collapsed;
            addUser.Visibility = Visibility.Visible;    
            cb2.Width = 270;
            cb2.Margin = new Thickness(370, 50, 0,0);
            tb1.Width = 270;
            tbNameForm.Text = "Клиент";
            query = "SELECT first_name, last_name, email, phone, date_of_birth, passport_number, registration_date FROM guests";
            raspred = 1;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(_currentPage, com); cb2.SelectedIndex = 4;
            cb1.Visibility = Visibility.Collapsed;
            cb2.Visibility = Visibility.Visible;
            cb2.SelectedIndex = 2;
            tb1.Text = "";
        }

        private void tb1_TextChanged(object sender, TextChangedEventArgs e)
        {
            com = "";
            if (raspred == 0)
            {
                
                dopCom0 = $" WHERE CONCAT(guests.first_name, \" \", guests.last_name) like '%{tb1.Text}%'";
            }
            if (raspred == 1)
            {
                dopCom0 = $" where first_name like '{tb1.Text}%' or last_name like '{tb1.Text}%'";
            }
            if (raspred == 2)
            {
                dopCom0 = $" where service_name like '%{tb1.Text}%'";
            }
            if (dopCom2 != "" && dopCom0 != "")
            {
                com = query + dopCom0 + " AND " + dopCom2 + dopCom1;
                FillDataGrid(_currentPage, com);
            }
            if(dopCom2 == "")
            {
                com = query + dopCom0 + dopCom1;
                FillDataGrid(_currentPage, com);
            }
            
        }

        private void cb2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string column = String.Empty;
            if (raspred == 0)
            {
                column = "guest";
            }
            if (raspred == 1)
            {
                column = "first_name";
            }
            if (raspred == 2)
            {
                column = "service_name";
            }
            if (cb2.SelectedIndex == 0)
            {

                dopCom1 = $" ORDER BY {column}";
            }
            if (cb2.SelectedIndex == 1)
            {
                dopCom1 = $" ORDER BY {column} DESC";
            }
            if (cb2.SelectedIndex == 2)
            {
                dopCom1 = "";
                if (dopCom0 != "" && dopCom2 != "")
                {
                    com = query + dopCom0 + " AND " + dopCom2;
                    FillDataGrid(_currentPage, com);
                }
                if (dopCom0 == "" && dopCom2 == "")
                {
                    com = query;
                    FillDataGrid(_currentPage, com);
                }
                if (dopCom0 != "" && dopCom2 == "")
                {
                    com = query + dopCom0;
                    FillDataGrid(_currentPage, com);
                }
                if (dopCom0 == "" && dopCom2 != "")
                {
                    com = query + " WHERE " + dopCom2;
                    FillDataGrid(_currentPage, com);
                }
            }
            if (dopCom0 != "" && dopCom2 !="")
            {
                com = query + dopCom0 + " AND " + dopCom2 + dopCom1;
                FillDataGrid(_currentPage, com);
            }
            if (dopCom0 == ""|| dopCom2 == "")
            {
                if (cb2.SelectedIndex != 2)
                {

                    if (dopCom0 == "" && dopCom2 != "")
                    {
                        com = query + " WHERE " + dopCom2 + dopCom1;
                        FillDataGrid(_currentPage, com);
                    }
                    if (dopCom0 == "" && dopCom2 == "")
                    {
                        com = query + dopCom1;
                        FillDataGrid(_currentPage, com);
                    }
                    if (dopCom2 == "" && dopCom0 != "")
                    {
                        com = query + dopCom0 + dopCom1;
                        FillDataGrid(_currentPage, com);
                    }
                }
                
            }
        }

        private void cb1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (raspred == 0)
            {
                if (cb1.SelectedItem != null)
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)cb1.SelectedItem;

                    string selectedStatus = selectedItem.Content.ToString();

                    dopCom2 = $" booking_status = '{selectedStatus}'";

                    if (cb1.SelectedIndex == 4)
                    {
                        dopCom2 = "";
                        com = query + dopCom0 + dopCom1;
                        FillDataGrid(_currentPage, com);
                    }
                    if (dopCom0 != "" && dopCom1 != "")
                    {
                        com = query + dopCom0 + " AND " + dopCom2 + dopCom1;
                        FillDataGrid(_currentPage, com);
                    }
                    if (dopCom0 == "" || dopCom1 == "")
                    {
                        if (cb1.SelectedIndex != 4) {
                            if (dopCom0 == "")
                            {
                                com = query + " WHERE " + dopCom2 + dopCom1;
                                FillDataGrid(_currentPage, com);
                            }

                            if (dopCom1 == "")
                            {
                                com = query + dopCom0 + " AND " + dopCom2;
                                FillDataGrid(_currentPage, com);
                            }
                        }
                    }

                }
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Получаем данные строки через Tag кнопки
            var client = button?.Tag as Client; // Замените Client на ваш класс данных

            if (client != null)
            {
                Class1.numberPhone = client.number;
            }
            redactKlient redactKlient = new redactKlient();
            redactKlient.Focus();

            redactKlient.Show();
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Class1.add = 1;
                redactKlient redactKlient = new redactKlient();
                redactKlient.Focus();

                redactKlient.Show();
            }
        }

        private void StackPanel_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            addHouse.Visibility = Visibility.Collapsed;
            placeholder.Visibility = Visibility.Collapsed;
            addService.Visibility = Visibility.Collapsed;
            addEmployee.Visibility = Visibility.Visible;
            addUser.Visibility = Visibility.Collapsed;
            Add_Booking.Visibility = Visibility.Collapsed;
            tb1.Visibility = Visibility.Collapsed;
            cb1.Visibility = Visibility.Collapsed;
            cb2.Visibility = Visibility.Collapsed;
            tbNameForm.Text = "Сотрудники";
            query = "SELECT employee_id, first_name,last_name,position, hire_date, phone, email, login,password, role FROM employees";
            raspred = 3;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(_currentPage, com);
        }

        private void StackPanel_MouseDown_4(object sender, MouseButtonEventArgs e)
        {
            if (Class1.role == 0)
            {
                addHouse.Visibility = Visibility.Visible;
            }
            
            tb1.Visibility = Visibility.Collapsed;
            addService.Visibility = Visibility.Collapsed;
            placeholder.Visibility = Visibility.Collapsed;
            addEmployee.Visibility = Visibility.Collapsed;
            addUser.Visibility = Visibility.Collapsed;
            Add_Booking.Visibility = Visibility.Collapsed;
            tb1.Visibility = Visibility.Collapsed;
            cb1.Visibility = Visibility.Collapsed;
            cb2.Visibility = Visibility.Collapsed;
            tbNameForm.Text = "Дома";
            query = "SELECT unit_id, unit_name,unit_type, capacity, price_per_night, description, photo FROM glampingunits";
            raspred = 4;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(_currentPage, com);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Class1.role == 1)
            {
                redactBooking redactBooking = new redactBooking();
                redactBooking.Show();
            }
            else
            {
                // Группировка бронирований по unit
                var revenueReport = Bookings
                    .GroupBy(b => b.unit)
                    .Select(group => (dynamic)new
                    {
                        Unit = group.Key,
                        RentalCount = group.Count(),
                        TotalRevenue = group.Sum(b => decimal.Parse(b.total_price))
                    })
                    .ToList();

                // Генерация отчёта
                GenerateOfficialWordReport(revenueReport);
            }
        }
        static void GenerateOfficialWordReport(List<dynamic> report)
        {
            // Создаем приложение Word
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            Document document = wordApp.Documents.Add();

            try
            {
                // Вставляем логотип (при необходимости замените путь)
                string logoPath = Path.GetFullPath("Logo.png"); // Замените на путь к вашему логотипу
                if (System.IO.File.Exists(logoPath))
                {
                    Range logoRange = document.Range(0, 0);
                    Shape logoShape = document.Shapes.AddPicture(logoPath, false, true, 0, 0, 100, 50);
                    logoShape.WrapFormat.Type = WdWrapType.wdWrapTopBottom;
                }

                // Заголовок отчёта
                Word.Paragraph titleParagraph = document.Content.Paragraphs.Add();
                titleParagraph.Range.Text = "Официальный Отчёт по Выручке";
                titleParagraph.Range.Font.Size = 20;
                titleParagraph.Range.Font.Bold = 1;
                titleParagraph.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                titleParagraph.Range.InsertParagraphAfter();

                // Подзаголовок
                Word.Paragraph subtitleParagraph = document.Content.Paragraphs.Add();
                subtitleParagraph.Range.Text = $"Дата формирования отчёта: {DateTime.Now:dd.MM.yyyy}";
                subtitleParagraph.Range.Font.Size = 12;
                subtitleParagraph.Range.Font.Italic = 1;
                subtitleParagraph.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                subtitleParagraph.Range.InsertParagraphAfter();

                // Пустая строка перед таблицей
                document.Content.Paragraphs.Add();

                // Таблица отчёта
                Word.Table table = document.Tables.Add(document.Content.Paragraphs.Add().Range, report.Count + 1, 3);
                table.Borders.Enable = 1; // Включаем границы
                table.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                table.Range.Font.Size = 12;

                // Заголовки столбцов
                table.Cell(1, 1).Range.Text = "Дом";
                table.Cell(1, 2).Range.Text = "Количество аренд";
                table.Cell(1, 3).Range.Text = "Общая выручка";

                table.Rows[1].Range.Font.Bold = 1; // Выделяем заголовки жирным
                table.Rows[1].Shading.BackgroundPatternColor = WdColor.wdColorGray20; // Задаём фон заголовков

                // Заполнение таблицы данными
                for (int i = 0; i < report.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = report[i].Unit;
                    table.Cell(i + 2, 2).Range.Text = report[i].RentalCount.ToString();
                    table.Cell(i + 2, 3).Range.Text = $"{report[i].TotalRevenue:C}"; // Форматируем как валюту
                }

                // Итоговая строка
                decimal totalRevenue = 0m;
                foreach (var item in report) totalRevenue += item.TotalRevenue;

                Row totalRow = table.Rows.Add();
                totalRow.Cells[1].Range.Text = "ИТОГО";
                totalRow.Cells[2].Merge(totalRow.Cells[3]); // Объединяем последние две ячейки
                totalRow.Cells[2].Range.Text = $"{totalRevenue:C}";
                totalRow.Range.Font.Bold = 1;

                // Сохранение документа
                string filePath = @"C:\Users\dshma\OneDrive\Рабочий стол\Курсовой проект\Kyrsovoi\Kyrsovoi\bin\Debug\homeОфициальный_Отчёт.docx";
                document.SaveAs2(filePath);

                Console.WriteLine($"Отчёт успешно сохранён: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                // Закрываем документ и приложение Word
                if (document != null)
                {
                    document.Close();
                    Marshal.ReleaseComObject(document);
                }

                if (wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Получаем данные строки через Tag кнопки
            var emloye = button?.Tag as Employee; // Замените Client на ваш класс данных

            if (emloye != null)
            {
                Class1.numberPhoneEmploye = emloye.phone;
            }
            redactEmployee redactEmployee = new redactEmployee();
            redactEmployee.Focus();

            redactEmployee.Show();
        }

        private void addEmployee_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Class1.add = 1;
                redactEmployee redactEmployee = new redactEmployee();
                redactEmployee.Focus();

                redactEmployee.Show();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Получаем данные строки через Tag кнопки
            var home = button?.Tag as Home; // Замените Client на ваш класс данных

            if (home != null)
            {
                Class1.unit_id = Convert.ToInt32(home.unit_id);
            }
            addHouse addHouse = new addHouse();
            addHouse.Focus();

            addHouse.Show();
        }

        private void addHouse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Class1.add = 1;
                addHouse addHouse = new addHouse();
                addHouse.Focus();

                addHouse.Show();
            }
        }

        private void Tg_Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Получаем данные строки через Tag кнопки
            var booking = button?.Tag as Booking; // Замените Client на ваш класс данных

            if (booking != null)
            {
                Class1.booking_id = booking.id_booking;
            }
            redactBooking redactBooking = new redactBooking();
            redactBooking.Focus();

            redactBooking.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBrush imageBrush = new ImageBrush();
            
            if (Class1.role == 0)
            {
                lvSupplier.Visibility = Visibility.Visible;
                var button = this.FindName("redactButHome") as Button;
                if (button != null)
                {
                    button.Visibility = Visibility.Visible;
                }
                addHouse.Visibility = Visibility.Visible;
                addService.Visibility = Visibility.Visible;
                imageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath("report.png"), UriKind.RelativeOrAbsolute));
                Add_Booking.Background = imageBrush;
            }
            else
            {
                lvSupplier.Visibility = Visibility.Collapsed;
                var button = this.FindName("redactButHome") as Button;
                if (button != null)
                {
                    button.Visibility = Visibility.Collapsed;
                }
                addHouse.Visibility = Visibility.Collapsed;
                addService.Visibility = Visibility.Collapsed;
                imageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath("addBrone.png"), UriKind.RelativeOrAbsolute));
                Add_Booking.Background = imageBrush;
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Получаем данные строки через Tag кнопки
            var service = button?.Tag as Services; // Замените Client на ваш класс данных

            if (service != null)
            {
                Class1.id_service = Convert.ToInt32(service.id_service);
            }
            redactService redactService = new redactService();
            redactService.Focus();

            redactService.Show();
        }

        private void addService_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Class1.add = 1;
                redactService redactService = new redactService();
                redactService.Focus();

                redactService.Show();
            }
        }

        private void tb1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[а-яА-Я]+$");
        }
        private void GeneratePageButtons()
        {
            // Очищаем старые кнопки
            PageButtonsPanel.Children.Clear();

            // Генерируем кнопки для всех страниц
            for (int i = 1; i <= _totalPages; i++)
            {
                Button pageButton = new Button
                {
                    Content = i.ToString(),
                    Margin = new Thickness(5),
                    Width = 30,
                    Height = 30,
                    Tag = i // Сохраняем номер страницы в свойстве Tag
                };

                // Событие клика на кнопку
                pageButton.Click += PageButton_Click;

                // Добавляем кнопку в панель
                PageButtonsPanel.Children.Add(pageButton);
            }
        }
        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем номер страницы из Tag кнопки
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int pageNumber))
            {
                _currentPage = pageNumber; // Обновляем текущую страницу
                FillDataGrid(_currentPage, com); // Загружаем данные для выбранной страницы
            }
        }
        private void UpdatePageInfo()
        {
            labelPageInfo.Text = $"Страница {_currentPage} из {_totalPages}";

            // Обновляем кнопки страниц
        }
        private void CalculateTotalPages()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string sql = "";
                    conn.Open();
                    if (dopCom2 != "" )
                    {
                        sql = " JOIN guests ON bookings.guest_id = guests.guest_id WHERE " + dopCom2 ;
                    }
                    if (dopCom2 != "" && dopCom0 != "")
                    {
                        sql = " JOIN guests ON bookings.guest_id = guests.guest_id " + dopCom0 + " AND " + dopCom2 ;
                    }
                    if (dopCom2 == "")
                    {
                        sql = " JOIN guests ON bookings.guest_id = guests.guest_id " + dopCom0;
                    }
                    MySqlCommand countCmd = new MySqlCommand($"SELECT COUNT(*) FROM bookings {sql}", conn);
                    int totalItems = Convert.ToInt32(countCmd.ExecuteScalar());
                    _totalPages = (int)Math.Ceiling((double)totalItems / _pageSize);

                    // Генерация кнопок страниц
                    GeneratePageButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                FillDataGrid(_currentPage, com);
                UpdatePageInfo();
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                FillDataGrid(_currentPage, com);
                UpdatePageInfo();
            }
        }
    }
}
