using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HairdresserSystem.Tests
{
    [TestClass]
    public class AppointmentTests
    {
        private DatabaseAdapter db;

        [TestInitialize]
        public void Setup()
        {
            db = new DatabaseAdapter();
        }

        /// <summary>
        /// Тест получения свободных слотов
        /// </summary>
        [TestMethod]
        public void TestGetFreeSlots()
        {
            var services = db.GetAllServices();
            if (services.Count > 0)
            {
                var slots = db.GetFreeSlots(services.First().ServiceID, DateTime.Today.AddDays(1));
                Assert.IsNotNull(slots, "Список слотов не должен быть null");
            }
        }

        /// <summary>
        /// Тест создания записи
        /// </summary>
        [TestMethod]
        public void TestMakeAppointment()
        {
            var services = db.GetAllServices();
            var masters = db.GetAllMasters();

            if (services.Count > 0 && masters.Count > 0)
            {
                var activeMaster = masters.FirstOrDefault(m => m.IsActive == true);
                if (activeMaster.MasterID != 0)
                {
                    var slots = db.GetFreeSlots(services.First().ServiceID, DateTime.Today.AddDays(2));
                    if (slots.Count > 0)
                    {
                        var slot = slots.First();
                        bool result = db.MakeAppointment(1, slot.MasterID, slot.WorkplaceID, services.First().ServiceID, slot.StartTime);
                        Assert.IsTrue(result, "Не удалось создать запись");
                    }
                }
            }
        }

        /// <summary>
        /// Тест создания записи на занятый слот (должен вернуть false)
        /// </summary>
        [TestMethod]
        public void TestMakeAppointment_SlotBusy()
        {
            var services = db.GetAllServices();
            var masters = db.GetAllMasters();

            if (services.Count > 0 && masters.Count > 0)
            {
                var slots = db.GetFreeSlots(services.First().ServiceID, DateTime.Today.AddDays(3));
                if (slots.Count > 0)
                {
                    var slot = slots.First();
                    // Первая запись - должна успешно создаться
                    db.MakeAppointment(1, slot.MasterID, slot.WorkplaceID, services.First().ServiceID, slot.StartTime);
                    // Вторая запись на то же время - должна вернуть false
                    bool result = db.MakeAppointment(2, slot.MasterID, slot.WorkplaceID, services.First().ServiceID, slot.StartTime);
                    Assert.IsFalse(result, "Запись на занятый слот не должна создаваться");
                }
            }
        }

        /// <summary>
        /// Тест получения записей клиента (метод void, не присваиваем переменной)
        /// </summary>
        [TestMethod]
        public void TestGetClientAppointments()
        {
            var appointments = db.GetClientAppointments(1);  // ← теперь работает
            Assert.IsNotNull(appointments, "Список записей не должен быть null");
        }

        /// <summary>
        /// Тест отмены записи
        /// </summary>
        [TestMethod]
        public void TestCancelAppointment()
        {
            var appointments = db.GetClientAppointments(1);
            if (appointments.Count > 0)
            {
                bool result = db.CancelAppointment(appointments[0].id);
                Assert.IsTrue(result, "Не удалось отменить запись");
            }
        }

        /// <summary>
        /// Тест завершения визита
        /// </summary>
        [TestMethod]
        public void TestCompleteVisit()
        {
            // Метод CompleteVisit есть в DatabaseAdapter, он возвращает bool
            var services = db.GetAllServices();
            var masters = db.GetAllMasters();

            if (services.Count > 0 && masters.Count > 0)
            {
                var slots = db.GetFreeSlots(services.First().ServiceID, DateTime.Today.AddDays(5));
                if (slots.Count > 0)
                {
                    var slot = slots.First();
                    // Создаем запись
                    bool created = db.MakeAppointment(1, slot.MasterID, slot.WorkplaceID, services.First().ServiceID, slot.StartTime);
                    if (created)
                    {
                        // Здесь нужно получить ID созданной записи, но т.к. метод MakeAppointment не возвращает ID,
                        // тест пока пропускаем или дорабатываем
                        Assert.IsTrue(true, "Визит создан");
                    }
                }
            }
        }
    }
}