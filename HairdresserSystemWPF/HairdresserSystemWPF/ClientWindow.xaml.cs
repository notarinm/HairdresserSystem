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
    public partial class ClientWindow : Window
    {
        private DatabaseAdapter db;
        private int clientId;

        public class AppointmentDisplay
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
            public DateTime StartTime { get; set; }
            public string Status { get; set; }
        }

        public ClientWindow(int clientId, DatabaseAdapter databaseAdapter)
        {
            InitializeComponent();
            db = databaseAdapter;
            this.clientId = clientId;

            lblWelcome.Text = $"👤 Добро пожаловать!";
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var appointments = db.GetClientAppointments(clientId);
            var displayList = new List<AppointmentDisplay>();

            if (appointments.Count == 0)
            {
                displayList.Add(new AppointmentDisplay
                {
                    Id = -1,
                    Status = "None",
                    DisplayText = "📭 У вас пока нет записей"
                });
            }
            else
            {
                int index = 1;
                foreach (var apt in appointments)
                {
                    string statusText = apt.status == "Scheduled" ? "⏳ Активна" :
                                       (apt.status == "Completed" ? "✅ Завершена" : "❌ Отменена");

                    displayList.Add(new AppointmentDisplay
                    {
                        Id = apt.id,
                        StartTime = apt.start,
                        Status = apt.status,
                        DisplayText = $"{index++}. {apt.start:dd.MM.yyyy HH:mm} - {apt.end:HH:mm} | {apt.service} | Мастер: {apt.master} | {statusText}"
                    });
                }
            }

            lstAppointments.ItemsSource = displayList;
            lstAppointments.DisplayMemberPath = "DisplayText";
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            BookingWindow bookingWindow = new BookingWindow(clientId, db);
            bookingWindow.ShowDialog();
            LoadAppointments(); // Обновляем список после записи
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstAppointments.SelectedItem as AppointmentDisplay;

            if (selected == null || selected.Id == -1)
            {
                MessageBox.Show("Выберите запись для отмены!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selected.Status != "Scheduled")
            {
                MessageBox.Show("Можно отменить только активные записи!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Отменить запись на {selected.StartTime:dd.MM.yyyy HH:mm}?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = db.CancelAppointment(selected.Id);
                if (success)
                {
                    MessageBox.Show("Запись успешно отменена!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAppointments(); // Обновляем список
                }
                else
                {
                    MessageBox.Show("Не удалось отменить запись!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}