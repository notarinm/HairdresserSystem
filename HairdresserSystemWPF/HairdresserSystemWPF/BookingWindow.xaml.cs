using HairdresserSystem;
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

namespace HairdresserSystemWPF
{
    public partial class BookingWindow : Window
    {
        private DatabaseAdapter db;
        private int userId;
        private List<FreeSlot> currentSlots;

        public class SlotDisplay
        {
            public FreeSlot Slot { get; set; }
            public string DisplayText { get; set; }
        }

        public BookingWindow(int userId, DatabaseAdapter databaseAdapter)
        {
            InitializeComponent();
            db = databaseAdapter;
            this.userId = userId;

            LoadServices();
            dpDate.SelectedDate = DateTime.Today.AddDays(1);
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
            var selected = lstSlots.SelectedItem as SlotDisplay;
            if (selected == null)
            {
                MessageBox.Show("Выберите время для записи!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var service = (ServiceInfo)cmbService.SelectedItem;
            var slot = selected.Slot;

            bool success = db.MakeAppointment(userId, slot.MasterID, slot.WorkplaceID,
                                              service.ServiceID, slot.StartTime);

            if (success)
            {
                MessageBox.Show("Вы успешно записаны!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Не удалось записаться. Возможно, это время уже занято.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
