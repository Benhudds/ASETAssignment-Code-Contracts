using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASETAssignment.CarPark;

namespace ASETAssignment
{
    class Program
    {
        private const int Days = 3;
        private const int MaxSize = 42;
        private const int ReservedSpaces = 3;
        private const int PremiumSpaces = 5;

        static void Main(string[] args)
        {
            var con = new Conference(Days, MaxSize, ReservedSpaces, PremiumSpaces);
            try
            {
                Console.WriteLine(con.ReserveSpace(0, 3, 1));
                Console.WriteLine(con.ReserveSpace(0, 3, 2));
            }
            catch (CarParkException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}
