using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Система учета топлива ===");
            List<FuelPrice> fuels = new List<FuelPrice>();

            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1 - Ручной ввод данных");
                Console.WriteLine("2 - Загрузка из файла");
                Console.WriteLine("0 - Выход");

                string choice = Console.ReadLine();

                try
                {
                    if (choice == "1")
                    {
                        fuels.AddRange(ProcessManualInput());
                        if (fuels.Count > 0) break;
                    }
                    else if (choice == "2")
                    {
                        fuels.AddRange(ProcessFileInput());
                        if (fuels.Count > 0) break;
                    }
                    else if (choice == "0") return;
                    else Console.WriteLine("Неверная команда.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"! Критическая ошибка: {ex.Message}");
                }
            }

            if (fuels.Count > 0)
                ProcessBusinessLogic(fuels.ToArray());
            else
                Console.WriteLine("Список топлива пуст. Работа завершена.");

            Console.ReadKey();
        }

        private static List<FuelPrice> ProcessManualInput()
        {
            var result = new List<FuelPrice>();
            int count = 0;

            while (true)
            {
                Console.WriteLine("Сколько типов топлива вы хотите ввести?");
                if (int.TryParse(Console.ReadLine(), out count) && count > 0) break;
                Console.WriteLine("Введите целое число > 0.");
            }

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine($"\nЗапись #{i + 1}. Формат: Тип, гггг.мм.дд, Цена");
                try
                {
                    var price = FuelParser.Parse(Console.ReadLine());
                    result.Add(price);
                    Console.WriteLine($"-> OK: {price.GetInfo()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    i--; 
                }
            }
            return result;
        }

        private static List<FuelPrice> ProcessFileInput()
        {
            Console.WriteLine("Путь к файлу (Enter для 'data.txt'):");
            string fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "data.txt";

            var result = new List<FuelPrice>();
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Файл не найден.");
                return result;
            }

            string[] lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                try
                {
                    result.Add(FuelParser.Parse(line));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Строка пропущена: {ex.Message}");
                }
            }
            return result;
        }

        private static void ProcessBusinessLogic(FuelPrice[] fuels)
        {
            try
            {
                Console.WriteLine("\n--- ВВОД ДАННЫХ О ПОСТАВЩИКЕ ---");
                int idx = GetValidIndex(fuels.Length, "поставщика");

                Console.Write("Компания: "); string supplier = Console.ReadLine();
                Console.Write("Договор: "); string contract = Console.ReadLine();
                Console.Write("Доставка: "); string delivery = Console.ReadLine();

                Console.WriteLine(new FuelPriceWithSupplier
                {
                    BasePrice = fuels[idx],
                    SupplierCompany = supplier,
                    ContactNumber = contract,
                    DeliveryMethod = delivery
                }.GetSupplierInfo());

                Console.WriteLine("\n--- ВВОД СТАТИСТИКИ ---");
                idx = GetValidIndex(fuels.Length, "статистики");

                double liters = GetValidDouble("Литры");
                double revenue = GetValidDouble("Выручка");
                Console.Write("Период: "); string period = Console.ReadLine();

                Console.WriteLine(new FuelSalesStats
                {
                    FuelType = fuels[idx],
                    TotalLitersSold = liters,
                    TotalRevenue = revenue,
                    SalesPeriod = period
                }.GetStatsInfo());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка логики: {ex.Message}");
            }
        }

        private static int GetValidIndex(int max, string ctx)
        {
            while (true)
            {
                Console.Write($"Номер топлива для {ctx} (1-{max}): ");
                if (int.TryParse(Console.ReadLine(), out int i) && i >= 1 && i <= max) return i - 1;
            }
        }

        private static double GetValidDouble(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                if (double.TryParse(Console.ReadLine()?.Replace('.', ','), out double v) && v >= 0) return v;
            }
        }
    }
}
        
    

