using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    public class FuelPriceWithSupplier
    {
        public FuelPrice BasePrice { get; set; }
        public string SupplierCompany { get; set; }
        public string ContactNumber { get; set; }
        public string DeliveryMethod { get; set; }

        public string GetSupplierInfo()
        {
            return $"Поставщик: {SupplierCompany}\n" +
                   $"Договор: {ContactNumber}\n" +
                   $"Способ доставки: {DeliveryMethod}\n" +
                   $"Информация о топливе: {BasePrice?.GetInfo() ?? "Нет данных"}";
        }
    }
}
