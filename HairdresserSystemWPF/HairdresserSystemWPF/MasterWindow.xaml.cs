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
    public partial class MasterWindow : Window
    {
        private DatabaseAdapter db;
        private int userId;

        public class ScheduleDisplay
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
            public DateTime StartTime { get; set; }
            public string Status { get; set; }
        }

        public MasterWindow(int userId, DatabaseAdapter databaseAdapter)
        {
            InitializeComponent();
            db = databaseAdapter;
            this.userId = userId;

            lblWelcome.Text = $"Добро пожаловать! (Мастер)";
            dpDate.SelectedDate = DateTime.Today;
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            if (dpDate.SelectedDate == null) return;

            var schedule = db.GetMasterSchedule(userId, dpDate.SelectedDate.Value);
            var displayList = new List<ScheduleDisplay>();

            foreach (var apt in schedule)
            {
                displayList.Add(new ScheduleDisplay
                {
                    Id = apt.id,
                    StartTime = apt.start,
                    Status = apt.status,
                    DisplayText = $"{apt.start:HH:mm} - {apt.end:HH:mm} | Клиент: {apt.clientName} | Услуга: {apt.service} | {(apt.status == "Scheduled" ? "Ожидает" : "Завершён")}"
                });
            }

            lstSchedule.ItemsSource = displayList;
        }

        private void DpDate_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadSchedule();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstSchedule.SelectedItem as ScheduleDisplay;
            if (selected == null)
            {
                MessageBox.Show("Выберите запись для завершения!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Завершить визит клиента?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = db.CompleteVisit(selected.Id);
                if (success)
                {
                    MessageBox.Show("Визит успешно завершён!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSchedule();
                }
                else
                {
                    MessageBox.Show("Не удалось завершить визит!", "Ошибка",
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
