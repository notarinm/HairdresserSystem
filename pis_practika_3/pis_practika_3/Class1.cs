using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis_practika_3
{
    public class ArrayHelper
    {
        public static int SumArray(int[] arr)
        {
            int sum = 0;

            foreach (int element in arr)
            {
                sum += element;
            }

            return sum;
        }
    }
}
