using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HairdresserSystem.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        private DatabaseAdapter db;

        [TestInitialize]
        public void Setup()
        {
            db = new DatabaseAdapter();
        }

        /// <summary>
        /// Тест подключения к базе данных
        /// </summary>
        [TestMethod]
        public void TestConnection()
        {
            bool result = db.TestConnection();
            Assert.IsTrue(result, "Не удалось подключиться к базе данных");
        }
    }


}
