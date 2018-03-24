using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASETAssignment.CarPark
{
    public class CarParkException : Exception
    {
        public CarParkException(string message) : base(message)
        {

        }

        public CarParkException() : base()
        {

        }
    }
}
