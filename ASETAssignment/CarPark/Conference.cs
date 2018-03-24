using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASETAssignment.CarPark
{
    public class Conference
    {
        private readonly int days;

        private CarPark[] carParks;

        public Conference(int days, int carParkSize, int carParkReservedSpaces, int carParkPremiumSpaces)
        {
            this.days = days;

            carParks = new CarPark[days];

            for (var i = 0; i < days; i++)
            {
                carParks[i] = new CarPark(carParkSize, carParkReservedSpaces, carParkPremiumSpaces);
            }
        }

        public bool ReserveSpace(int day, int space, int customerId)
        {
            return carParks[day].ReserveSpace(space, customerId);
        }
    }
}
