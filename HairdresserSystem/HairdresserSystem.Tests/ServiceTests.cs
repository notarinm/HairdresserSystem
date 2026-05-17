using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HairdresserSystem.Tests
{
    [TestClass]
    public class ServiceTests
    {
        private DatabaseAdapter db;  // ← Исправлено: DatabaseAdapter, а не DatabaseAdapterServiceImpl

        [TestInitialize]
        public void Setup()
        {
            db = new DatabaseAdapter();
        }

        /// <summary>
        /// Тест получения всех услуг
        /// </summary>
        [TestMethod]
        public void TestGetAllServices()
        {
            var services = db.GetAllServices();
            Assert.IsNotNull(services, "Список услуг не должен быть null");
            Assert.IsTrue(services.Count > 0, "В базе данных должны быть услуги");
        }

        /// <summary>
        /// Тест добавления услуги
        /// </summary>
        [TestMethod]
        public void TestAddService()
        {
            string uniqueTitle = $"Тестовая услуга_{DateTime.Now.Ticks}";
            bool result = db.AddService(uniqueTitle, 1000m, 60);
            Assert.IsTrue(result, "Не удалось добавить услугу");
        }

        /// <summary>
        /// Тест обновления услуги
        /// </summary>
        [TestMethod]
        public void TestUpdateService()
        {
            var services = db.GetAllServices();
            if (services.Count > 0)
            {
                var service = services.First();
                // Используем правильные имена полей: ServiceID, Title, Price, DurationMinutes
                bool result = db.UpdateService(service.ServiceID, service.Title + " (обновлено)", service.Price + 100, service.DurationMinutes);
                Assert.IsTrue(result, "Не удалось обновить услугу");
            }
        }

        /// <summary>
        /// Тест удаления услуги (деактивации)
        /// </summary>
        [TestMethod]
        public void TestDeleteService()
        {
            string uniqueTitle = $"Услуга_для_удаления_{DateTime.Now.Ticks}";
            db.AddService(uniqueTitle, 500m, 30);

            var services = db.GetAllServices();
            var serviceToDelete = services.FirstOrDefault(s => s.Title == uniqueTitle);

            if (serviceToDelete != null && serviceToDelete.ServiceID != 0)
            {
                bool result = db.DeleteService(serviceToDelete.ServiceID);
                Assert.IsTrue(result, "Не удалось удалить услугу");
            }
        }
    }
}