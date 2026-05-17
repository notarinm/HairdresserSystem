using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pis1._1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int width = 80;
            int height = 40;
            char[,] grid = new char[height, width];
            Random random = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[y, x] = random.Next(100) < 10 ? '#' : ' ';
                }
            }

            int playerX = 0;
            int playerY = 0;

            bool found = false;

            for (int y = 0; y < height && !found; y++)
            {
                for (int x = 0; x < width && !found; x++)
                {
                    if (grid[y, x] == ' ')
                    {
                        playerX = x;
                        playerY = y;
                        grid[y, x] = '*';
                        found = true;
                    }
                }
            }

            
            while (true)
            {
                
                Console.Clear();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Console.Write(grid[y, x]);
                    }
                    Console.WriteLine();
                }

                Console.Write("Введите направление (w-вверх, s-вниз, a-влево, d-вправо): ");
                char input = Console.ReadKey().KeyChar;
                Console.WriteLine();


                int oldX = playerX;
                int oldY = playerY;

                switch (input)
                {
                    case 'w': if (playerY > 0) playerY--; break;
                    case 's': if (playerY < height - 1) playerY++; break;
                    case 'a': if (playerX > 0) playerX--; break;
                    case 'd': if (playerX < width - 1) playerX++; break;
                }

                if (grid[playerY, playerX] == ' ')
                {
                    grid[oldY, oldX] = ' ';  
                    grid[playerY, playerX] = '*';  
                }
                else
                { 
                    playerX = oldX;
                    playerY = oldY;
                    Console.WriteLine("Нельзя пройти через препятствие!");
                    Console.ReadKey();
                }


            }


            
        }

    }
}

