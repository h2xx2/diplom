using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Xml.Linq;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для addHouse.xaml
    /// </summary>
    public partial class addHouse : Window
    {
        private Timer _idleTimer;
        private int _idleTimeout; // Время ожидания (секунды)
        private string oldName = "";
        private string oldType = "";
        private string oldCapacity = "";
        private string oldDescription = "";
        private string oldPrice = "";
        private string oldPhoto = "";

        private string path = "";
        private string oldpath = "";
        public addHouse()
        {
            InitializeComponent();
            Smena.IsEnabled = false;
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
            string strCmd = $"SELECT * FROM glampingunits WHERE unit_id='{Class1.unit_id}'";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {

                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand(strCmd, con);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        string relativePath = System.IO.Path.Combine("home", rdr["photo"].ToString());
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string imagePath = System.IO.Path.Combine(baseDir, relativePath);

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        id = rdr["unit_id"].ToString();
                        var house = (House)this.DataContext;
                        house.Name = rdr["unit_name"].ToString();
                        oldName = house.Name;
                        house.Type = rdr["unit_type"].ToString();
                        oldType = house.Type;
                        house.Capacity = rdr["capacity"].ToString();
                        oldCapacity = house.Capacity;
                        house.Price = rdr["price_per_night"].ToString();
                        oldPrice = house.Price;
                        house.Description = rdr["description"].ToString();
                        oldDescription = house.Description;
                        house.Photo = bitmap;
                        oldPhoto = rdr["photo"].ToString();
                        path = oldPhoto;
                        oldpath = oldPhoto;
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
                Class1.add = 0;
                Prosmotr prosmotr = new Prosmotr();
                this.Hide();
                prosmotr.ShowDialog();
                this.Close();

            }
        }


        public class House : INotifyPropertyChanged, IDataErrorInfo
        {
            private string _name;
            private string _type;
            private string _capacity;
            private string _price_per_night;
            private string _description;
            private BitmapImage _photo;


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

            public string Type
            {
                get => _type;
                set
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }

            public string Capacity
            {
                get => _capacity;
                set
                {
                    _capacity = value;
                    OnPropertyChanged(nameof(Capacity));
                }
            }

            public string Price
            {
                get => _price_per_night;
                set
                {
                    _price_per_night = value;
                    OnPropertyChanged(nameof(Price));
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
            public BitmapImage Photo
            {
                get => _photo;
                set
                {
                    _photo = value;
                    OnPropertyChanged(nameof(Photo));
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

                        case nameof(Type):
                            if (string.IsNullOrWhiteSpace(Type))
                                return "Поле 'Тип' обязательно для заполнения.";
                            break;

                        case nameof(Capacity):
                            if (string.IsNullOrWhiteSpace(Capacity))
                                return "Поле 'Вместимость' обязательно для заполнения.";
                            break;

                        case nameof(Price):
                            if (string.IsNullOrWhiteSpace(Price))
                                return "Поле 'Цена за ночь' обязательно для заполнения.";
                            break;
                        case nameof(Description):
                            if (string.IsNullOrWhiteSpace(Description))
                                return "Поле 'Описание' обязательно для заполнения.";
                            break;
                        
                    }
                    return null;
                }
            }
        }
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            foreach (Control control in new[] { nameHouse, capacity, price, description })
            {
                if (control is TextBox textBox)
                {
                    textBox.IsReadOnly = isReadOnly;
                }

            }
            type.IsEnabled = !isReadOnly;
        }
        private bool AreFieldsFilled()
        {
            if (string.IsNullOrEmpty(nameHouse.Text) ||
                string.IsNullOrEmpty(type.Text) ||
                string.IsNullOrEmpty(capacity.Text) ||
                string.IsNullOrEmpty(price.Text) ||
                string.IsNullOrEmpty(description.Text) ||
                string.IsNullOrEmpty(image.Name)
               )
            {
                return false;
            }
            return true;
        }
        private bool IsTextChanged(string name, string type, string capacity, string description, string price, string path)
        {
            // Пример: Если одно из значений изменилось
            if (name != oldName || type != oldType || capacity != oldCapacity || price != oldPrice || description != oldDescription || path != oldPhoto)
            {
                // Обновляем старые значения
                oldName = name;
                oldType = type;
                oldCapacity = capacity;
                oldPrice = price;
                oldDescription = description;

                return true;
            }

            return false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetFieldsReadOnly(false);
            Smena.IsEnabled = true;

            button.Content = "Сохранить";

            string query = String.Empty;
            string name = nameHouse.Text;
            string types = type.Text;
            string capacitys = capacity.Text;
            decimal prices = Convert.ToDecimal(price.Text);
            string descriptions = description.Text;
            int rowsAffected = 0;

            if (AreFieldsFilled())
            {
                if (Class1.add != 1)
                {
                    query = "UPDATE glampingunits SET unit_name = @Name, unit_type = @Type, capacity = @Capacity, price_per_night = @Price, description = @Description, photo = @Photo WHERE unit_id = @ID";

                }
                else
                {
                    query = "INSERT glampingunits(unit_name, unit_type, capacity, price_per_night, description, photo) VALUES(@Name,@Type,@Capacity,@Price, @Description,@Photo)";

                }

                if (IsTextChanged(nameHouse.Text, type.Text, capacity.Text, description.Text, price.Text, oldpath))
                {

                    // Создаем подключение и команду
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        // Открываем подключение
                        connection.Open();

                        // Создаем команду с параметрами
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {

                            // Преобразованная дата в формате YYYY-MM-DD
                            // Добавляем параметры
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@Type", types);
                            command.Parameters.AddWithValue("@Capacity", capacitys);
                            command.Parameters.AddWithValue("@Price", prices);
                            command.Parameters.AddWithValue("@Description", descriptions);
                            command.Parameters.AddWithValue("@Photo", path);
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
            if (Class1.role == 0)
            {
                if (Class1.add != 1)
                {
                    var house = new House();
                    this.DataContext = house;
                    FillTextBox();
                    SetFieldsReadOnly(true);
                    button.Visibility = Visibility.Visible;
                    image.Visibility = Visibility.Visible;
                }
                else
                {
                    var house = new House();
                    this.DataContext = house;
                    SetFieldsReadOnly(false);
                    button.Content = "Сохранить";
                    button.Visibility = Visibility.Visible;
                    image.Visibility = Visibility.Visible;
                }
            }
            else
            {
                var house = new House();
                this.DataContext = house;
                FillTextBox();
                SetFieldsReadOnly(false);
                button.Visibility = Visibility.Collapsed;
                image.Visibility = Visibility.Visible;
            }
            
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                    Title = "Выберите изображение для дома",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                    // Ограничение размера файла в 5 МБ
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Файл слишком большой. Выберите файл до 5 МБ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    try
                    {
                        // Определяем путь к папке home в bin/debug
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory; // Например, bin/Debug/
                        string homeDir = System.IO.Path.Combine(baseDir, "home");
                        if (!Directory.Exists(homeDir))
                        {
                            Directory.CreateDirectory(homeDir);
                        }

                        // Формируем новое имя файла с уникальным суффиксом
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName);
                        string fileExtension = System.IO.Path.GetExtension(openFileDialog.SafeFileName);
                        string newFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";
                        string destinationPath = System.IO.Path.Combine(homeDir, newFileName);

                        // Проверяем, существует ли файл с таким именем
                        int fileCounter = 1;
                        while (File.Exists(destinationPath))
                        {
                            newFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}_{fileCounter}{fileExtension}";
                            destinationPath = System.IO.Path.Combine(homeDir, newFileName);
                            fileCounter++;
                        }

                        // Копируем файл в папку home
                        File.Copy(openFileDialog.FileName, destinationPath, false);

                        // Сохраняем только имя файла (без пути home/)
                        path = newFileName;

                        // Загрузка изображения
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(destinationPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        // Установка изображения в Image
                        image.Source = bitmap;

                        // Обновляем старое значение фото, если редактирование
                        if (!string.IsNullOrEmpty(oldPhoto))
                        {
                            oldpath = oldPhoto;
                            oldPhoto = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке или сохранении изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            string strCmd = $"DELETE FROM glampingunits WHERE unit_id = {id}";

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

        private void nameHouse_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[а-яА-Я]+$");
        }

        private void capacity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}
