using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    public static class SequenceFinder
    {
        public static int LengthFromPosition(string text, int startPos)
        {
            if (string.IsNullOrEmpty(text)  startPos < 0  startPos >= text.Length)
                return -1;

            char startChar = text[startPos];

            if (!char.IsUpper(startChar))
                return -1;

            int digitCount = 0;

            for (int i = startPos + 1; i < text.Length; i++)
            {
                char currentChar = text[i];

                if (currentChar == startChar)
                {
                    if (digitCount > 0)
                    {
                        return digitCount + 2;
                    }
                    return -1;
                }
                else if (char.IsDigit(currentChar))
                {
                    digitCount++;
                }
                else
                {
                    return -1;
                }
            }

            return -1;
        }
    }
}
