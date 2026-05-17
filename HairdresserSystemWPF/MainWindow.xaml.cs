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
using HairdresserSystem;  // ссылка на ваш проект с DatabaseAdapter


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

            // ВАЖНО: Убедитесь, что ваш метод Login возвращает 5 значений
            var (success, role, userId, clientId, masterId) = db.Login(login, password);

            if (success)
            {
                if (role == "Client")
                {
                    MessageBox.Show($"Добро пожаловать, клиент!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (role == "Master")
                {
                    MessageBox.Show($"Добро пожаловать, мастер!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (role == "Admin")
                {
                    MessageBox.Show($"Добро пожаловать, администратор!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show("Функция регистрации будет добавлена позже", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}