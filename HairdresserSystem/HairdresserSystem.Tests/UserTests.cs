using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HairdresserSystem.Tests
{
    [TestClass]
    public class UserTests
    {
        private DatabaseAdapter db;

        [TestInitialize]
        public void Setup()
        {
            db = new DatabaseAdapter();
        }

        /// <summary>
        /// Тест регистрации клиента
        /// </summary>
        [TestMethod]
        public void TestRegisterClient()
        {
            // Генерируем уникальные значения с помощью Guid
            string uniqueId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string uniqueLogin = $"test_user_{uniqueId}";
            string uniquePhone = $"+7999{uniqueId}";
            string uniqueEmail = $"{uniqueLogin}@test.ru";

            bool result = db.RegisterClient(uniqueLogin, "123456", "Тестовый Клиент", uniquePhone, uniqueEmail);
            Assert.IsTrue(result, $"Ошибка при регистрации клиента. Логин: {uniqueLogin}, Телефон: {uniquePhone}");
        }

        [TestMethod]
        public void TestLogin_Success()
        {
            var (success, role, userId) = db.Login("admin", "admin123");
            Assert.IsTrue(success, "Не удалось войти в систему");
            Assert.AreEqual("Admin", role, "Роль должна быть Admin");
        }

        [TestMethod]
        public void TestLogin_WrongPassword()
        {
            var (success, role, userId) = db.Login("admin", "wrongpassword");
            Assert.IsFalse(success, "Вход с неверным паролем не должен быть выполнен");
        }

        [TestMethod]
        public void TestLogin_WrongLogin()
        {
            var (success, role, userId) = db.Login("nonexistent", "123456");
            Assert.IsFalse(success, "Вход с несуществующим логином не должен быть выполнен");
        }

        [TestMethod]
        public void TestLogin_Client()
        {
            var (success, role, userId) = db.Login("client_ivan", "client123");
            Assert.IsTrue(success, "Клиент не смог войти");
            Assert.AreEqual("Client", role, "Роль должна быть Client");
        }

        [TestMethod]
        public void TestLogin_Master()
        {
            var (success, role, userId) = db.Login("master_anna", "master123");
            Assert.IsTrue(success, "Мастер не смог войти");
            Assert.AreEqual("Master", role, "Роль должна быть Master");
        }
    }
}