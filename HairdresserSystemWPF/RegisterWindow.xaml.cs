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
    public partial class RegisterWindow : Window
    {
        private DatabaseAdapter db;

        public RegisterWindow(DatabaseAdapter databaseAdapter)
        {
            InitializeComponent();
            db = databaseAdapter;
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            // Проверка заполнения полей
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = db.RegisterClient(login, password, fullName, phone, email);

            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти в систему.",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка регистрации. Возможно, такой логин или телефон уже существует.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
