using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HairdresserSystem.Tests
{
    [TestClass]
    public class ScheduleTests
    {
        private DatabaseAdapter db;

        [TestInitialize]
        public void Setup()
        {
            db = new DatabaseAdapter();
        }

        /// <summary>
        /// Тест получения расписания мастера
        /// </summary>
        [TestMethod]
        public void TestGetMasterSchedule()
        {
            var schedule = db.GetMasterSchedule(1, DateTime.Today);
            Assert.IsNotNull(schedule, "Расписание мастера не должно быть null");
        }

        /// <summary>
        /// Тест получения общего расписания (для администратора)
        /// </summary>
        [TestMethod]
        public void TestGetGeneralSchedule()
        {
            var schedule = db.GetGeneralSchedule(DateTime.Today);
            Assert.IsNotNull(schedule, "Общее расписание не должно быть null");
        }
    }
}