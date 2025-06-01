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

        private void Tg_Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Hide();
            mainWindow.ShowDialog();
            this.Close();
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
                HostNameTextBox.IsEnabled = true;
                UserTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                DatabaseTextBox.IsEnabled = true;
                EditSaveButton.Content = "Сохранить";
                isEditing = true;
            }
            else
            {
                // Сохранение настроек и возврат в режим просмотра
                Properties.Settings.Default.host = HostNameTextBox.Text;
                Properties.Settings.Default.user = UserTextBox.Text;
                Properties.Settings.Default.passwordDB = PasswordTextBox.Text;
                Properties.Settings.Default.database = DatabaseTextBox.Text;
                Properties.Settings.Default.Save(); // Сохраняем изменения

                HostNameTextBox.IsEnabled = false;
                UserTextBox.IsEnabled = false;
                PasswordTextBox.IsEnabled = false;
                DatabaseTextBox.IsEnabled = false;
                EditSaveButton.Content = "Изменить";
                isEditing = false;
            }
        }
    }
}
