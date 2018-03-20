using System.Diagnostics.Contracts;

namespace ASETAssignment.CarPark
{
    [ContractClass(typeof(ICarParkContracts))]
    public interface ICarPark
    {
        bool ReserveSpace();
        bool BuySpace(Space s);
        bool ReturnSpace(Space s);
        void CancelReservations();
        void CheckAvailability();
        string CheckCustomer(Space s);
    }
}
