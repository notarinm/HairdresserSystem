using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    public static class FuelParser
    {
        public static FuelPrice Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Строка данных пуста.");

            string[] parts = input.Split(',');

            if (parts.Length < 3)
                throw new FormatException("Неверный формат. Нужно минимум 3 значения через запятую.");

            


            string type = parts[0].Trim(); 
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Тип топлива не указан.");


            
            string dateString = parts[1].Trim();
            if (!DateTime.TryParseExact(dateString, "yyyy.MM.dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                throw new FormatException($"Ошибка в дате '{dateString}'. Ожидается формат 'гггг.мм.дд' (например 2024.05.20).");
            }


            
            string costString = parts[2].Trim();
            if (!double.TryParse(costString, NumberStyles.Any, CultureInfo.InvariantCulture, out double cost))
            {
                throw new FormatException($"Ошибка в цене '{costString}'. Должно быть число (например 55.5).");
            }

            if (cost < 0)
                throw new ArgumentException("Цена не может быть отрицательной.");

            
 
            string quality = parts.Length > 3 ? parts[3].Trim() : "Стандарт";

  
            return new FuelPrice
            {
                Type = type,
                Date = date,
                Cost = cost,
                QualityGrade = quality
            };
        }
    }
}
