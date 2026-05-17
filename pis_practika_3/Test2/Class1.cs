using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public static class Class1
    {
        public static string LenPos(string str, int startIndex)
        {
            if (startIndex < 0 || startIndex >= str.Length)
                return string.Empty;

            if (str[startIndex] < 'A' || str[startIndex] > 'Z')
                return string.Empty;

            char startLetter = str[startIndex];
            int pos = startIndex + 1;

            while (pos < str.Length && str[pos] >= '0' && str[pos] <= '9')
            {
                pos++;
            }

            if (pos < str.Length && str[pos] == startLetter)
            {
                return str.Substring(startIndex, pos - startIndex + 1);
            }

            return string.Empty;
        }
    }
}
