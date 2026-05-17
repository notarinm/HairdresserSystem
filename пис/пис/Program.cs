using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace FuelPriceParser
{
    public class FuelPrice
    {
        public string FuelType { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return $"Тип топлива: {FuelType}, Дата: {Date:yyyy.MM.dd}, Цена: {Price:F2}";
        }
    }
    
    public class Program
    {
        static void Main(string[] args)
        {
            
   
        }
    }
}
