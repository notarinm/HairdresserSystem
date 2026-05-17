using HairdresserSystem;
using System;
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
using static HairdresserSystem.DatabaseAdapter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HairdresserSystem;

namespace HairdresserSystemWPF
{
    public partial class AdminCreateAppointmentWindow : Window
    {
        private DatabaseAdapter db;
        private int? foundClientId = null;
        private List<FreeSlot> currentSlots;

        public class SlotDisplay
        {
            public FreeSlot Slot { get; set; }
            public string DisplayText { get; set; }
        }

        public AdminCreateAppointmentWindow(DatabaseAdapter databaseAdapter)
        {
            InitializeComponent();
            db = databaseAdapter;
            LoadServices();
            dpDate.SelectedDate = DateTime.Today.AddDays(1);

            txtNewClientLogin.Text = $"client_{DateTime.Now.Ticks}";
        }

        private void LoadServices()
        {
            var services = db.GetAllServices();
            cmbService.ItemsSource = services;
            cmbService.DisplayMemberPath = "Title";
            cmbService.SelectedValuePath = "ServiceID";

            if (services.Count > 0)
                cmbService.SelectedIndex = 0;
        }

        private void LoadFreeSlots()
        {
            if (cmbService.SelectedItem == null || dpDate.SelectedDate == null)
                return;

            var service = (ServiceInfo)cmbService.SelectedItem;
            DateTime date = dpDate.SelectedDate.Value;

            var slots = db.GetFreeSlots(service.ServiceID, date);
            currentSlots = slots;

            var displayList = slots.Select(s => new SlotDisplay
            {
                Slot = s,
                DisplayText = s.ToString()
            }).ToList();

            lstSlots.ItemsSource = displayList;
            lstSlots.DisplayMemberPath = "DisplayText";
        }

        private void BtnFindClient_Click(object sender, RoutedEventArgs e)
        {
            string phone = txtClientPhone.Text.Trim();
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Введите телефон клиента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, clientId) = db.FindOrCreateClient(phone);

            if (success && clientId > 0)
            {
                foundClientId = clientId;
                borderClientFound.Visibility = Visibility.Visible;
                borderNewClient.Visibility = Visibility.Collapsed;
                lblClientInfo.Text = $"Клиент найден! ID: {clientId}";
                lblClientName.Text = $"Телефон: {phone}";
            }
            else
            {
                foundClientId = null;
                borderClientFound.Visibility = Visibility.Collapsed;
                borderNewClient.Visibility = Visibility.Visible;
            }
        }

        private void TxtNewClientLogin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string login = txtNewClientLogin.Text.Trim();
            if (login.Length < 3)
            {
                lblLoginStatus.Text = "слишком короткий";
                lblLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                lblLoginStatus.Text = "допустимый";
                lblLoginStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        private void BtnCreateClient_Click(object sender, RoutedEventArgs e)
        {
            string phone = txtClientPhone.Text.Trim();
            string name = txtNewClientName.Text.Trim();
            string email = txtNewClientEmail.Text.Trim();
            string login = txtNewClientLogin.Text.Trim();
            string password = txtNewClientPassword.Password.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите ФИО клиента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                email = $"{login}@temp.ru";
            }

            bool success = db.RegisterClient(login, password, name, phone, email);

            if (success)
            {
                var (_, clientId) = db.FindOrCreateClient(phone);

                if (clientId > 0)
                {
                    foundClientId = clientId;
                    borderClientFound.Visibility = Visibility.Visible;
                    borderNewClient.Visibility = Visibility.Collapsed;
                    lblClientInfo.Text = $"Клиент создан! ID: {clientId}";
                    lblClientName.Text = name;

                    MessageBox.Show("Клиент успешно зарегистрирован!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Не удалось зарегистрировать клиента!\nВозможно, такой телефон или логин уже существует.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbService_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadFreeSlots();
        }

        private void DpDate_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadFreeSlots();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (foundClientId == null)
            {
                MessageBox.Show("Сначала найдите или зарегистрируйте клиента!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = lstSlots.SelectedItem as SlotDisplay;
            if (selected == null)
            {
                MessageBox.Show("Выберите время для записи!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbService.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var service = (ServiceInfo)cmbService.SelectedItem;
            var slot = selected.Slot;

            bool success = db.MakeAppointment(foundClientId.Value, slot.MasterID, slot.WorkplaceID,
                                              service.ServiceID, slot.StartTime);

            if (success)
            {
                MessageBox.Show("Запись успешно создана!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Не удалось создать запись. Возможно, это время уже занято.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}