using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    public class GasStation
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int PumpsCount { get; set; }
        public bool IsWorking24h { get; set; }
        public string StationStatus { get; set; }

        public string GetStationInfo()
        {
            string workingHours = IsWorking24h ? "круглосуточно" : "с 6:00 до 23:00";
            return $"АЗС: {Name}\nАдрес: {Address}\nКоличество колонок: {PumpsCount}\nРежим работы: {workingHours}";
        }
    }
}
