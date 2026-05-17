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
using HairdresserSystem;

namespace HairdresserSystemWPF
{
    public partial class MainWindow : Window
    {
        private DatabaseAdapter db;

        public MainWindow()
        {
            InitializeComponent();
            db = new DatabaseAdapter();

            if (!db.TestConnection())
            {
                lblStatus.Text = "Ошибка подключения к БД";
                btnLogin.IsEnabled = false;
                btnRegister.IsEnabled = false;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, role, userId) = db.Login(login, password);

            if (success)
            {
                if (role == "Client")
                {
                    ClientWindow clientWindow = new ClientWindow(userId, db);
                    clientWindow.Show();
                    this.Close();
                }
                else if (role == "Master")
                {
                    MasterWindow masterWindow = new MasterWindow(userId, db);
                    masterWindow.Show();
                    this.Close();
                }
                else if (role == "Admin")
                {
                    AdminWindow adminWindow = new AdminWindow(db);
                    adminWindow.Show();
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // ИСПРАВЛЕНО: теперь открывается окно регистрации
            RegisterWindow registerWindow = new RegisterWindow(db);
            registerWindow.ShowDialog();
        }
    }
}