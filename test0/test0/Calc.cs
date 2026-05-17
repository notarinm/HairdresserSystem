using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test0
{
    public class Calc
    {
        public static double numAvg (int[] num)
        {
            if (num == null || num.Length == 0)
            {
                throw new ArgumentException("Массив не дложен быть пустым");
            }

            double sum = 0;
            foreach (int i in num)
            {
                sum += i;
            }
            return sum/ num.Length;
        }
    }
}
