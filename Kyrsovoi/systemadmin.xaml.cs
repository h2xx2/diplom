using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kyrsovoi.Properties;
using MySql.Data.MySqlClient;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для systemadmin.xaml
    /// </summary>
    public partial class systemadmin : Window
    {
        private bool isEditing = false;
        public systemadmin()
        {
            InitializeComponent();
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
    "Вы действительно хотите выйти?",
    "Подтверждение",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question
);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                this.Hide();
                mainWindow.ShowDialog();
                this.Close();
            }

        }

        private void Service_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Vostan mainWindow = new Vostan();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
        }

        private void EditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditing)
            {
                // Переключение в режим редактирования
                isEditing = true;
                EditSaveButton.Content = "Сохранить";
                HostNameTextBox.IsEnabled = true;
                UserTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                DatabaseTextBox.IsEnabled = true;
                ConnectionStatus.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Проверка подключения и сохранение
                string host = HostNameTextBox.Text;
                string user = UserTextBox.Text;
                string password = PasswordTextBox.Text;
                string database = DatabaseTextBox.Text;

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(database))
                {
                    System.Windows.MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string connectionString = $"Server={host};Database={database};Uid={user};Password={password};";

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        ConnectionStatus.Text = "Подключение успешно!";
                        ConnectionStatus.Foreground = Brushes.Green;
                        ConnectionStatus.Visibility = Visibility.Visible;

                        // Сохранение настроек
                        Settings.Default.host = host;
                        Settings.Default.user = user;
                        Settings.Default.passwordDB = password;
                        Settings.Default.database = database;
                        Settings.Default.Save();

                        // Выход из режима редактирования
                        isEditing = false;
                        EditSaveButton.Content = "Изменить";
                        HostNameTextBox.IsEnabled = false;
                        UserTextBox.IsEnabled = false;
                        PasswordTextBox.IsEnabled = false;
                        DatabaseTextBox.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    ConnectionStatus.Text = $"Ошибка подключения: {ex.Message}";
                    ConnectionStatus.Foreground = Brushes.Red;
                    ConnectionStatus.Visibility = Visibility.Visible;
                }
            }
        }

        private void Tg_Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
