using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymanicContainer
{

    readonly struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; } 
        public int Y { get; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dynamic Collection sample"); 

            var collection = new DynamicCollection<Point>();
          
            for (int i = 0; i < int.MaxValue / 8; i++)
            {
                collection.Add(new Point(i , i *2));
            }

            foreach (var item in collection)
            {
                Console.WriteLine("Point {0}, {1}", item.X, item.Y);
            } 
            Console.WriteLine("Done");  
        } 

        // 
    }
}
