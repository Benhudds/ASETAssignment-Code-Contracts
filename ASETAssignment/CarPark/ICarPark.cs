using System.Diagnostics.Contracts;

namespace ASETAssignment.CarPark
{
    [ContractClass(typeof(ICarParkContracts))]
    public interface ICarPark
    {
        bool ReserveSpace(int index, int customer);
        bool BuySpace(int index, int customer);
        bool ReturnSpace(int index, int customer);
        void CancelReservations();
        void CheckAvailability();
        string CheckCustomer(Space s);
    }
}
