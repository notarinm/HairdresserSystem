using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HairdresserSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ЗАПУСК СИСТЕМЫ ПАРИКМАХЕРСКОЙ ===\n");

            DatabaseAdapter db = new DatabaseAdapter();

            // 1. Регистрация
            Console.WriteLine("1. Регистрация клиента...");
            bool regOk = db.RegisterClient("anna_login", "pass123", "Анна Иванова", "+79991234567", "anna@mail.ru");
            Console.WriteLine(regOk ? "✅ Регистрация успешна!" : "❌ Ошибка регистрации");

            // 2. Авторизация
            Console.WriteLine("\n2. Авторизация...");
            var (success, role, userId) = db.Login("anna_login", "pass123");
            Console.WriteLine(success ? $"✅ Вход выполнен! Роль: {role}, ID: {userId}" : "❌ Ошибка входа");

            // 3. Свободные слоты
            Console.WriteLine("\n3. Поиск свободных слотов...");
            var slots = db.GetFreeSlots(1, DateTime.Today.AddDays(1)); // на завтра
            Console.WriteLine($"Найдено слотов: {slots.Count}");
            foreach (var slot in slots)
            {
                Console.WriteLine($"   {slot}");
            }

            // 4. Создание записи (если есть слоты)
            if (slots.Count > 0)
            {
                Console.WriteLine("\n4. Создание записи...");
                var firstSlot = slots[0];
                bool aptOk = db.MakeAppointment(1, firstSlot.MasterID, firstSlot.WorkplaceID, 1, firstSlot.StartTime);
                Console.WriteLine(aptOk ? "✅ Запись создана!" : "❌ Не удалось создать запись");
            }

            // 5. Просмотр записей клиента
            Console.WriteLine("\n5. Записи клиента:");
            db.GetClientAppointments(1);



            Console.WriteLine("\n=== ВСЕ ТЕСТЫ ВЫПОЛНЕНЫ ===");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();  // <-- БЕЗ ЭТОГО КОНСОЛЬ СРАЗУ ЗАКРОЕТСЯ
        }
    }
}
