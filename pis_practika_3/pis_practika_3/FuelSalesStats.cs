using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    public class FuelSalesStats
    {
        public FuelPrice FuelType { get; set; }
        public double TotalLitersSold { get; set; }
        public double TotalRevenue { get; set; }
        public string SalesPeriod { get; set; }

        public string GetStatsInfo()
        {
            return $"Статистика продаж: {FuelType?.Type ?? "Неизвестно"}\n" +
                   $"Период: {SalesPeriod}\n" +
                   $"Продано: {TotalLitersSold} л\n" +
                   $"Выручка: {TotalRevenue} руб.\n" +
                   $"Текущая цена: {FuelType?.Cost} руб./л";
        }
    }
}
