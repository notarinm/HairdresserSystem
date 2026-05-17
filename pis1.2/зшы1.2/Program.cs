using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace зшы1._2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int w = 80, h = 40;
            char[,] grid = new char[h, w];
            Random rnd = new Random();

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    grid[y, x] = rnd.Next(100) < 10 ? '#' : ' ';

            int px = 0, py = 0;
            bool found = false;

            for (int y = 0; y < h && !found; y++)
                for (int x = 0; x < w && !found; x++)
                    if (grid[y, x] == ' ')
                    {
                        px = x; py = y;
                        grid[y, x] = '*';
                        found = true;
                    }

            while (true)
            {
                Console.Clear();

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                        Console.Write(grid[y, x]);
                    Console.WriteLine();
                }

                Console.Write("Движение (wasd): ");
                char key = Console.ReadKey().KeyChar;
                Console.WriteLine();

                int oldX = px, oldY = py;

                if (key == 'w' && py > 0) py--;
                else if (key == 's' && py < h - 1) py++;
                else if (key == 'a' && px > 0) px--;
                else if (key == 'd' && px < w - 1) px++;

                if (grid[py, px] == ' ')
                {
                    grid[oldY, oldX] = ' ';
                    grid[py, px] = '*';
                }
                else
                {
                    px = oldX; py = oldY;
                    Console.WriteLine("Препятствие!");
                    Console.ReadKey();
                }
            }
        }
    }
}
