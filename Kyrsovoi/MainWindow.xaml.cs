using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace Kyrsovoi
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        int error = 0;
        int error1 = 0;
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (tb2.Password.Length > 0 )
            {
                watermatk.Visibility = Visibility.Collapsed;
            }
            else
            {
                watermatk.Visibility = Visibility.Visible;
            }
        }

        private void krest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
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
            string login = tb1.Text;
            Class1.l = login;
            string hashPassword = string.Empty;
            string hashbd = string.Empty;
            if (login.Length != 0)
            {
                string conString = @"server=localhost;user=root;pwd=root;database=glamping;";

                using (MySqlConnection con = new MySqlConnection(conString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM employees Where login = '" + login + "';", con))
                    {
                        cmd.CommandType = CommandType.Text;

                        using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);

                                hashPassword = tb2.Password;
                                try
                                {
                                    Class1.id_employes = Convert.ToInt32(dt.Rows[0].ItemArray.GetValue(0));
                                    Class1.fioEmploes = dt.Rows[0].ItemArray.GetValue(1).ToString() + " " + dt.Rows[0].ItemArray.GetValue(2).ToString();
                                    hashbd = dt.Rows[0].ItemArray.GetValue(9).ToString();
                                    string role = dt.Rows[0].ItemArray.GetValue(10).ToString();

                                    if (hashPassword == hashbd)
                                    {
                                        if (role != "Администратор")
                                        {
                                            Prosmotr main = new Prosmotr();
                                            main.ShowDialog();
                                        }
                                        else
                                        {
                                            Prosmotr main = new Prosmotr();
                                            main.ShowDialog();
                                        }
                                        Close();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Введен не правильный логин или пароль", "Ошибка авторизации");
                                        error++;
                                        error1 = Class1.k;
                                        if (error > 1 || error1 > 1)
                                        {
                                            tb1.Clear();
                                            tb2.Clear();

                                        }
                                    }
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    MessageBox.Show("Введен не правильный логин или пароль", "Ошибка авторизации");
                                    error++;
                                    error1 = Class1.k;
                                    if (error > 1 || error1 > 1)
                                    {
                                        tb1.Clear();
                                        tb2.Clear();

                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Введен не правильный логин или пароль", "Ошибка авторизации");
                error++;
                error1 = Class1.k;
                if (error > 1 || error1 > 1)
                {
                    tb1.Clear();
                    tb2.Clear();

                }
            }
        }
    }
}
