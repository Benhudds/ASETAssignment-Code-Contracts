using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASETAssignment.CarPark
{
    [ContractClassFor(typeof(ICarPark))]
    public abstract class ICarParkContracts : ICarPark
    {
        public bool ReserveSpace()
        {

            return false;
        }

        public bool ReserveSpace(int index, int customer)
        {
            throw new NotImplementedException();
        }

        public bool BuySpace(int index, int customer)
        {
            throw new NotImplementedException();
        }

        public bool ReturnSpace(int index, int customer)
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
