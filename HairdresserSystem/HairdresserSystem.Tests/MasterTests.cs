using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HairdresserSystem.Tests
{
    [TestClass]
    public class MasterTests
    {
        private DatabaseAdapter db;

        [TestInitialize]
        public void Setup()
        {
            db = new DatabaseAdapter();
        }

        /// <summary>
        /// Тест добавления мастера
        /// </summary>
        [TestMethod]
        public void TestAddMaster()
        {
            // Генерируем уникальные значения с помощью Guid
            string uniqueId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string uniqueLogin = $"master_{uniqueId}";
            string uniquePhone = $"+7999{uniqueId}";

            var services = db.GetAllServices();
            List<int> serviceIds = new List<int>();
            if (services.Count > 0)
            {
                serviceIds.Add(services.First().ServiceID);
            }

            bool result = db.AddMaster(uniqueLogin, "123456", "Тестовый Мастер", uniquePhone, serviceIds);
            Assert.IsTrue(result, $"Не удалось добавить мастера. Логин: {uniqueLogin}, Телефон: {uniquePhone}");
        }

        [TestMethod]
        public void TestGetAllMasters()
        {
            var masters = db.GetAllMasters();
            Assert.IsNotNull(masters, "Список мастеров не должен быть null");
        }

        /// <summary>
        /// Тест деактивации мастера
        /// </summary>
        [TestMethod]
        public void TestDeactivateMaster()
        {
            // Генерируем уникальные значения
            string uniqueId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string uniqueLogin = $"master_deact_{uniqueId}";
            string uniquePhone = $"+7999{uniqueId}";

            var services = db.GetAllServices();
            List<int> serviceIds = new List<int>();
            if (services.Count > 0)
            {
                serviceIds.Add(services.First().ServiceID);
            }

            // Добавляем мастера
            bool created = db.AddMaster(uniqueLogin, "123456", "Тестовый Мастер", uniquePhone, serviceIds);
            Assert.IsTrue(created, $"Не удалось создать мастера для теста. Логин: {uniqueLogin}, Телефон: {uniquePhone}");

            // Получаем ID созданного мастера
            var masters = db.GetAllMasters();
            var newMaster = masters.FirstOrDefault(m => m.Phone == uniquePhone);
            Assert.IsNotNull(newMaster, "Созданный мастер не найден");

            // Деактивируем мастера
            bool result = db.DeactivateMaster(newMaster.MasterID);
            Assert.IsTrue(result, "Не удалось деактивировать мастера");
        }
    }
}