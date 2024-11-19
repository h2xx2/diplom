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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;


namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для Prosmotr.xaml
    /// </summary>
    public partial class Prosmotr : Window
    {
        public Prosmotr()
        {
            InitializeComponent();
            DataContext = this;
            com = query;
            FillDataGrid(com);
            cb2.SelectedIndex = 2;
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
            public string salary { get; set; }
            public string login { get; set; }
            public string password { get; set; }
            public string role { get; set; }
        }
        public class Booking
        {
            public string guests { get; set; }
            public string employee { get; set; }
            public string check_in_date { get; set; }
            public string check_out_date { get; set; }
            public string total_price { get; set; }
            public string booking_status { get; set; }
            public string created_at { get; set; }
        }
        public class Services
        {
            public string service_name { get; set; }
            public string description { get; set; }
            public string price { get; set; }
        }
        
        string query = @"SELECT 
                        CONCAT(guests.first_name, "" "", guests.last_name) AS guest,
                        CONCAT(employees.first_name, "" "", employees.last_name) AS employee,
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
                        employees ON employees.employee_id = b.booking_id";
        string com = "";
        int raspred = 0;
        string dopCom0 = string.Empty;
        string dopCom1 = string.Empty;
        string dopCom2 = string.Empty;
        string saveQuery = string.Empty;

        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<Booking> Bookings { get; set; } = new ObservableCollection<Booking>();
        public ObservableCollection<Services> Servic { get; set; } = new ObservableCollection<Services>();
        public ObservableCollection<Home> Homes { get; set; } = new ObservableCollection<Home>();
        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
        string connectionString = Class1.connection;

        public void FillDataGrid(string com)
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(com, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                Clients.Clear(); // Очистка коллекции перед загрузкой данных
                Bookings.Clear();
                Servic.Clear();
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
                            guests = reader["guest"].ToString(),
                            employee = reader["employee"].ToString(),
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
                            salary = reader["salary"].ToString(),
                            login = reader["login"].ToString(),
                            password = reader["password"].ToString(),
                            role = reader["role"].ToString(),

                        });
                        
                    }
                    if (raspred == 4)
                    {

                        string imagePath = "C:\\Users\\dshma\\OneDrive\\Рабочий стол\\Курсовой проект\\Kyrsovoi\\Kyrsovoi\\bin\\Debug\\home\\" + reader["photo"].ToString(); // Извлекаем путь или имя файла
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        clients.Visibility = Visibility.Collapsed;
                        service.Visibility = Visibility.Collapsed;
                        bookings.Visibility = Visibility.Collapsed;
                        employee.Visibility = Visibility.Collapsed;
                        homes.Visibility = Visibility.Visible;
                        Class1.unit_id = Convert.ToInt32(reader["unit_id"]);
                        Homes.Add(new Home
                        {

                            unit_name = reader["unit_name"].ToString(),
                            unit_type = reader["unit_type"].ToString(),
                            capacity = reader["capacity"].ToString(),
                            price_per_night = reader["price_per_night"].ToString(),
                            description = reader["description"].ToString(),
                            status = reader["status"].ToString(),
                            photo = bitmap,
                        }); ; ;
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

        private void Tg_Btn_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void Tg_Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Tg_Btn.IsChecked = false;
        }

        private void Tg_Btn_Click(object sender, RoutedEventArgs e)
        {
            //Tg_Btn.IsChecked = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Class1.saveQuery = com;
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Service_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
            FillDataGrid(com);
            
        }

        private void StackPanel_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            tb1.Visibility = Visibility.Visible;
            cb1.Visibility = Visibility.Visible;
            cb2.Visibility = Visibility.Visible;
            addUser.Visibility = Visibility.Collapsed;
            cb2.Width = 210;
            cb2.Margin = new Thickness(550, 50, 0, 0);
            tb1.Width = 210;
            tbNameForm.Text = "Бронирование";
            query = @"SELECT 
                        CONCAT(guests.first_name, "" "", guests.last_name) AS guest,
                        CONCAT(employees.first_name, "" "", employees.last_name) AS employee,
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
                        employees ON employees.employee_id = b.booking_id";
            raspred = 0;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(com);
            cb1.Visibility = Visibility.Visible;
            cb2.SelectedIndex = 2;
            tb1.Text = "";

        }

        private void StackPanel_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
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
            FillDataGrid(com);
            cb2.SelectedIndex = 4;
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
            if (dopCom2 != "")
            {
                com = query + dopCom0 + " AND " + dopCom2 + dopCom1;
                FillDataGrid(com);
            }
            if(dopCom2 == "")
            {
                com = query + dopCom0 + dopCom1;
                FillDataGrid(com);
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
                    FillDataGrid(com);
                }
                if (dopCom0 == "" && dopCom2 == "")
                {
                    com = query;
                    FillDataGrid(com);
                }
                if (dopCom0 != "" && dopCom2 == "")
                {
                    com = query + dopCom0;
                    FillDataGrid(com);
                }
                if (dopCom0 == "" && dopCom2 != "")
                {
                    com = query + " WHERE " + dopCom2;
                    FillDataGrid(com);
                }
            }
            if (dopCom0 != "" && dopCom2 !="")
            {
                com = query + dopCom0 + " AND " + dopCom2 + dopCom1;
                FillDataGrid(com);
            }
            if (dopCom0 == ""|| dopCom2 == "")
            {
                if (cb2.SelectedIndex != 2)
                {

                    if (dopCom0 == "" && dopCom2 != "")
                    {
                        com = query + " WHERE " + dopCom2 + dopCom1;
                        FillDataGrid(com);
                    }
                    if (dopCom0 == "" && dopCom2 == "")
                    {
                        com = query + dopCom1;
                        FillDataGrid(com);
                    }
                    if (dopCom2 == "" && dopCom0 != "")
                    {
                        com = query + dopCom0 + dopCom1;
                        FillDataGrid(com);
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
                        FillDataGrid(com);
                    }
                    if (dopCom0 != "" && dopCom1 != "")
                    {
                        com = query + dopCom0 + " AND " + dopCom2 + dopCom1;
                        FillDataGrid(com);
                    }
                    if (dopCom0 == "" || dopCom1 == "")
                    {
                        if (cb1.SelectedIndex != 4) {
                            if (dopCom0 == "")
                            {
                                com = query + " WHERE " + dopCom2 + dopCom1;
                                FillDataGrid(com);
                            }

                            if (dopCom1 == "")
                            {
                                com = query + dopCom0 + " AND " + dopCom2;
                                FillDataGrid(com);
                            }
                        }
                    }

                }
            }

        }

        //public void DoSomething()
        //{
        //    FillDataGrid(Class1.saveQuery);
        //}

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
            tb1.Visibility = Visibility.Collapsed;
            cb1.Visibility = Visibility.Collapsed;
            cb2.Visibility = Visibility.Collapsed;
            tbNameForm.Text = "Сотрудники";
            query = "SELECT employee_id, first_name,last_name,position, hire_date, phone, email, salary, login,password, role FROM employees";
            raspred = 3;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(com);
        }

        private void StackPanel_MouseDown_4(object sender, MouseButtonEventArgs e)
        {
            

            tb1.Visibility = Visibility.Collapsed;
            cb1.Visibility = Visibility.Collapsed;
            cb2.Visibility = Visibility.Collapsed;
            tbNameForm.Text = "Дома";
            query = "SELECT unit_id, unit_name,unit_type, capacity, price_per_night, description, status, photo FROM glampingunits";
            raspred = 4;
            com = query;
            dopCom0 = "";
            dopCom1 = "";
            dopCom2 = "";
            FillDataGrid(com);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            redactBooking redactBooking = new redactBooking();
            redactBooking.Show();
        }
    }
}
