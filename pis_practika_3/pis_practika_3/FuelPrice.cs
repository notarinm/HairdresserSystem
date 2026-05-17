using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    public class FuelPrice
    {
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public double Cost { get; set; }
        public string QualityGrade { get; set; }

        public string GetInfo()
        {
            return $"Топливо: {Type}; Класс: {QualityGrade}; Дата: {Date:yyyy.MM.dd}; Цена: {Cost} руб.";
        }
    }
}
