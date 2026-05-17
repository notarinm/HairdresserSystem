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
    public partial class AdminWindow : Window
    {
        private DatabaseAdapter db;

        public class ServiceDisplay
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
        }

        public class MasterDisplay
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
            public bool IsActive { get; set; }
        }

        public class ScheduleDisplay
        {
            public int Id { get; set; }           // ID записи
            public string DisplayText { get; set; }
            public string Status { get; set; }    // Статус записи
        }

        // Класс для отображения мастера в комбобоксе
        public class MasterDisplayItem
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
        }

        // Класс для отображения услуги в списке
        public class ServiceDisplayItem
        {
            public int Id { get; set; }
            public string DisplayText { get; set; }
        }

        // Загрузка списка мастеров для комбобокса
private void LoadMastersForSpecialization()
{
    var masters = db.GetAllMasters();
    var displayList = masters.Where(m => m.IsActive == true).Select(m => new MasterDisplayItem
    {
        Id = m.MasterID,
        DisplayText = $"{m.FullName} | {m.Phone}"
    }).ToList();
    
    cmbMasterForSpecialization.ItemsSource = displayList;
    cmbMasterForSpecialization.DisplayMemberPath = "DisplayText";
    cmbMasterForSpecialization.SelectedValuePath = "Id";
    
    if (displayList.Count > 0)
        cmbMasterForSpecialization.SelectedIndex = 0;
}

// Загрузка текущих специализаций мастера
private void LoadCurrentSpecializations(int masterId)
{
    var specializations = db.GetMasterSpecializations(masterId);
    var displayList = specializations.Select(s => new ServiceDisplayItem
    {
        Id = s.ServiceID,
        DisplayText = $"{s.Title} - {s.Price} руб. ({s.DurationMinutes} мин)"
    }).ToList();
    
    lstCurrentSpecializations.ItemsSource = displayList;
    lstCurrentSpecializations.DisplayMemberPath = "DisplayText";
}

// Загрузка доступных услуг (которых ещё нет у мастера)
private void LoadAvailableServices(int masterId)
{
    var allServices = db.GetAllServices();
    var currentServices = db.GetMasterSpecializations(masterId);
    
    var currentIds = currentServices.Select(s => s.ServiceID).ToHashSet();
    var availableServices = allServices.Where(s => !currentIds.Contains(s.ServiceID)).ToList();
    
    var displayList = availableServices.Select(s => new ServiceDisplayItem
    {
        Id = s.ServiceID,
        DisplayText = $"{s.Title} - {s.Price} руб. ({s.DurationMinutes} мин)"
    }).ToList();
    
    cmbAvailableServices.ItemsSource = displayList;
    cmbAvailableServices.DisplayMemberPath = "DisplayText";
    cmbAvailableServices.SelectedValuePath = "Id";
    
    if (displayList.Count > 0)
        cmbAvailableServices.SelectedIndex = 0;
}

// Обработчик выбора мастера
private void CmbMasterForSpecialization_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
{
    if (cmbMasterForSpecialization.SelectedItem is MasterDisplayItem selected)
    {
        LoadCurrentSpecializations(selected.Id);
        LoadAvailableServices(selected.Id);
    }
}

// Добавление специализации
private void BtnAddSpecialization_Click(object sender, RoutedEventArgs e)
{
    if (!(cmbMasterForSpecialization.SelectedItem is MasterDisplayItem selectedMaster))
    {
        MessageBox.Show("Выберите мастера!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    if (!(cmbAvailableServices.SelectedItem is ServiceDisplayItem selectedService))
    {
        MessageBox.Show("Выберите услугу для добавления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }
    
    var serviceIds = new List<int> { selectedService.Id };
    bool success = db.UpdateMasterSpecializations(selectedMaster.Id, 
        db.GetMasterSpecializations(selectedMaster.Id).Select(s => s.ServiceID).Concat(serviceIds).ToList());
    
    if (success)
    {
        MessageBox.Show("Услуга добавлена мастеру!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        LoadCurrentSpecializations(selectedMaster.Id);
        LoadAvailableServices(selectedMaster.Id);
    }
    else
    {
        MessageBox.Show("Не удалось добавить услугу!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

// Удаление специализации
private void BtnRemoveSpecialization_Click(object sender, RoutedEventArgs e)
{
    if (!(cmbMasterForSpecialization.SelectedItem is MasterDisplayItem selectedMaster))
    {
        MessageBox.Show("Выберите мастера!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    if (!(lstCurrentSpecializations.SelectedItem is ServiceDisplayItem selectedService))
    {
        MessageBox.Show("Выберите услугу для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }
    
    var result = MessageBox.Show($"Удалить услугу \"{selectedService.DisplayText}\" у мастера?", 
                                 "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
    
    if (result == MessageBoxResult.Yes)
    {
        var currentIds = db.GetMasterSpecializations(selectedMaster.Id).Select(s => s.ServiceID).ToList();
        currentIds.Remove(selectedService.Id);
        
        bool success = db.UpdateMasterSpecializations(selectedMaster.Id, currentIds);
        
        if (success)
        {
            MessageBox.Show("Услуга удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCurrentSpecializations(selectedMaster.Id);
            LoadAvailableServices(selectedMaster.Id);
        }
        else
        {
            MessageBox.Show("Не удалось удалить услугу!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
        public AdminWindow(DatabaseAdapter databaseAdapter)
        {
            InitializeComponent();
            db = databaseAdapter;

            dpScheduleDate.SelectedDate = DateTime.Today;
            LoadServices();
            LoadMasters();
            LoadSchedule();
            LoadMastersForSpecialization();
        }

        private void LoadServices()
        {
            var services = db.GetAllServices();
            var displayList = new List<ServiceDisplay>();

            if (services.Count == 0)
            {
                displayList.Add(new ServiceDisplay
                {
                    Id = -1,
                    DisplayText = "📭 Список услуг пуст. Добавьте первую услугу!"
                });
            }
            else
            {
                displayList = services.Select(s => new ServiceDisplay
                {
                    Id = s.ServiceID,
                    DisplayText = $"{s.Title} - {s.Price} руб. ({s.DurationMinutes} мин)"
                }).ToList();
            }

            lstServices.ItemsSource = displayList;
        }

        private void LoadMasters()
        {
            try
            {
                var masters = db.GetAllMasters();
                var displayList = new List<MasterDisplay>();

                Console.WriteLine($"Найдено мастеров: {masters.Count}"); // Отладка

                if (masters.Count == 0)
                {
                    displayList.Add(new MasterDisplay
                    {
                        Id = -1,
                        IsActive = false,
                        DisplayText = "📭 Список мастеров пуст. Добавьте первого мастера!"
                    });
                }
                else
                {
                    displayList = masters.Select(m => new MasterDisplay
                    {
                        Id = m.MasterID,
                        IsActive = m.IsActive,
                        DisplayText = $"{m.FullName} | {m.Phone} | {(m.IsActive ? "✅ Активен" : "❌ Деактивирован")}"
                    }).ToList();
                }

                lstMasters.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSchedule()
        {
            if (dpScheduleDate.SelectedDate == null) return;

            var schedule = db.GetGeneralSchedule(dpScheduleDate.SelectedDate.Value);
            var displayList = new List<ScheduleDisplay>();

            if (schedule.Count == 0)
            {
                displayList.Add(new ScheduleDisplay
                {
                    Id = -1,
                    Status = "None",
                    DisplayText = $"📭 На {dpScheduleDate.SelectedDate.Value:dd.MM.yyyy} записей нет"
                });
            }
            else
            {
                int index = 1;
                foreach (var s in schedule)
                {
                    displayList.Add(new ScheduleDisplay
                    {
                        Id = s.id,
                        Status = s.status,
                        DisplayText = $"{index++}. {s.start:HH:mm} - {s.end:HH:mm} | Мастер: {s.masterName} | Клиент: {s.clientName} | Услуга: {s.service} | Место: {s.workplace} | {(s.status == "Scheduled" ? "⏳ Активна" : "✅ Завершена")}"
                    });
                }
            }

            lstGeneralSchedule.ItemsSource = displayList;
            lstGeneralSchedule.DisplayMemberPath = "DisplayText";
        }

        private void BtnShowSchedule_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

        private void BtnCreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            AdminCreateAppointmentWindow createWindow = new AdminCreateAppointmentWindow(db);
            createWindow.ShowDialog();
            LoadSchedule();
        }

        // Отмена записи администратором
        // Отмена записи администратором
        private void BtnCancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstGeneralSchedule.SelectedItem as ScheduleDisplay;

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

            var result = MessageBox.Show($"Отменить выбранную запись?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = db.CancelAppointment(selected.Id);

                if (success)
                {
                    MessageBox.Show("Запись успешно отменена!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSchedule(); // Обновляем расписание
                }
                else
                {
                    MessageBox.Show("Не удалось отменить запись!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            string title = txtServiceName.Text.Trim();
            string priceText = txtServicePrice.Text.Trim();
            string durationText = txtServiceDuration.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(priceText) || string.IsNullOrEmpty(durationText))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || !int.TryParse(durationText, out int duration))
            {
                MessageBox.Show("Неверный формат цены или длительности!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = db.AddService(title, price, duration);
            if (success)
            {
                MessageBox.Show("Услуга добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                txtServiceName.Text = "";
                txtServicePrice.Text = "";
                txtServiceDuration.Text = "";
                LoadServices();
            }
            else
            {
                MessageBox.Show("Не удалось добавить услугу!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstServices.SelectedItem as ServiceDisplay;
            if (selected == null || selected.Id == -1)
            {
                MessageBox.Show("Выберите услугу для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить услугу?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = db.DeleteService(selected.Id);
                if (success)
                {
                    MessageBox.Show("Услуга удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadServices();
                }
                else
                {
                    MessageBox.Show("Не удалось удалить услугу! Возможно, есть будущие записи.",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAddMaster_Click(object sender, RoutedEventArgs e)
        {
            string name = txtMasterName.Text.Trim();
            string phone = txtMasterPhone.Text.Trim();
            string login = txtMasterLogin.Text.Trim();
            string password = txtMasterPassword.Password.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = db.AddMaster(login, password, name, phone, new List<int>());
            if (success)
            {
                MessageBox.Show("Мастер добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                txtMasterName.Text = "";
                txtMasterPhone.Text = "";
                txtMasterLogin.Text = "";
                txtMasterPassword.Password = "";
                LoadMasters();
            }
            else
            {
                MessageBox.Show("Не удалось добавить мастера! Возможно, такой логин или телефон уже существует.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeactivateMaster_Click(object sender, RoutedEventArgs e)
        {
            var selected = lstMasters.SelectedItem as MasterDisplay;
            if (selected == null || selected.Id == -1)
            {
                MessageBox.Show("Выберите мастера для деактивации!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!selected.IsActive)
            {
                MessageBox.Show("Мастер уже деактивирован!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Деактивировать мастера?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = db.DeactivateMaster(selected.Id);
                if (success)
                {
                    MessageBox.Show("Мастер деактивирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMasters();
                }
                else
                {
                    MessageBox.Show("Не удалось деактивировать мастера! Возможно, есть будущие записи.",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
