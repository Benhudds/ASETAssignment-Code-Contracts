using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ASETAssignment.CarPark
{
    public class CarPark : ICarPark
    {
        /// <summary>
        /// Configurable maximum size of the carpark
        /// </summary>
        private readonly int maxSize;

        /// <summary>
        /// Configurable number of permanently reserved spaces
        /// </summary>
        private readonly int reservedSpaces;

        /// <summary>
        /// Collection of the spaces in the carpark
        /// </summary>
        private readonly List<Space> spaces;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSize">The amount of spaces in the car park</param>
        /// <param name="reservedSpaces">The number of spaces permanently reserved in the car park</param>
        public CarPark(int maxSize, int reservedSpaces)
        {
            Contract.Requires(maxSize < int.MaxValue);
            Contract.Requires(maxSize > 0);
            Contract.Requires(reservedSpaces <= maxSize);
            Contract.Requires(reservedSpaces >= 0);

            this.maxSize = maxSize;
            this.reservedSpaces = reservedSpaces;

            spaces = new List<Space>(maxSize);
        }

        public bool ReserveSpace()
        {
            throw new NotImplementedException();
        }

        public bool BuySpace(Space s)
        {
            throw new NotImplementedException();
        }

        public bool ReturnSpace(Space s)
        {
            throw new NotImplementedException();
        }

        public void CancelReservations()
        {
            throw new NotImplementedException();
        }

        public void CheckAvailability()
        {
            throw new NotImplementedException();
        }

        public string CheckCustomer(Space s)
        {
            throw new NotImplementedException();
        }
    }
}
