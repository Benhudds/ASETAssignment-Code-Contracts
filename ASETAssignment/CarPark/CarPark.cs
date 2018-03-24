using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

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
        private readonly int permanentlyReservedSpaces;

        /// <summary>
        /// Configurable number of premium spaces
        /// </summary>
        private readonly int premiumSpaces;

        /// <summary>
        /// Collection of the spaces in the carpark
        /// </summary>
        public Space[] Spaces { get; }

        /// <summary>
        /// The number of free spaces
        /// </summary>
        public int FreeSpaces
        {
            get { return Spaces.Count(s => s.State == SpaceEnumeration.Free) - permanentlyReservedSpaces; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSize">The amount of spaces in the car park</param>
        /// <param name="permanentlyReservedSpaces">The number of spaces permanently reserved in the car park</param>
        /// <param name="premiumSpaces">The number of premium spaces</param>
        public CarPark(int maxSize, int permanentlyReservedSpaces, int premiumSpaces)
        {
            #region PreConditions
            Contract.Requires(maxSize < int.MaxValue);
            Contract.Requires(maxSize > 0);
            Contract.Requires(permanentlyReservedSpaces + premiumSpaces <= maxSize);
            Contract.Requires(permanentlyReservedSpaces >= 0);
            Contract.Requires(premiumSpaces >= 0);
            #endregion

            #region PostConditions
            // Success case - size of the car park
            Contract.Ensures(Spaces.Length == maxSize);

            // Success case - correct number of reserved spaces
            Contract.Ensures(Spaces.Count(s => s.State == SpaceEnumeration.Reserved && s.CustomerId == -1) == permanentlyReservedSpaces);

            // Success case - correct number of premium spaces
            Contract.Ensures(Spaces.Count(s => s.Premium) == premiumSpaces);
            #endregion

            #region Implementation
            // Initialise the members
            this.maxSize = maxSize;
            this.permanentlyReservedSpaces = permanentlyReservedSpaces;
            this.premiumSpaces = premiumSpaces;

            // Create the array of Spaces
            Spaces = new Space[maxSize];

            // Create the reserved spaces
            // Set customer id to -1 to indicate it is one of the permanently reserved spaces
            for (var i = 0; i < permanentlyReservedSpaces; i++)
            {
                Spaces[i] = new Space()
                {
                    State = SpaceEnumeration.Reserved,
                    CustomerId = -1
                };
            }

            // Create the premium spaces
            for (var i = permanentlyReservedSpaces; i < permanentlyReservedSpaces + premiumSpaces; i++)
            {
                Spaces[i] = new Space()
                {
                    State = SpaceEnumeration.Free,
                    Premium = true
                };
            }

            // Create the free spaces
            for (var i = permanentlyReservedSpaces + premiumSpaces; i < maxSize; i++)
            {
                Spaces[i] = new Space
                {
                    State = SpaceEnumeration.Free
                };
            }
            #endregion
        }
        
        /// <summary>
        /// Allow a customer to reserve a specific available space
        /// </summary>
        /// <param name="index">The index of the space to reserve</param>
        /// <param name="customer">The id of the customer reserving the space</param>
        /// <returns>True if the space is reserved/purchased by the customer, False if it is reserved by another</returns>
        public bool ReserveSpace(int index, int customer)
        {
            #region PreConditions
            #endregion

            #region PostConditions
            // Success case - return value
            // Bi-implication
            // The customer id being equal to the called parameter implies the result
            // The result implies the customer id is the called parameter
            Contract.Ensures((Spaces[index].CustomerId != customer || Contract.Result<bool>()) &&
                             !Contract.Result<bool>() || Spaces[index].CustomerId == customer);

            // Success case - given space state
            // The Space is reserved if it was not before, otherwise the space remains unchanged
            Contract.Ensures((Spaces[index].State == SpaceEnumeration.Reserved &&
                              Contract.OldValue(Spaces[index].State == SpaceEnumeration.Free) &&
                              Spaces[index].CustomerId == customer) ||
                             (Spaces[index].State == Contract.OldValue(Spaces[index].State) &&
                              Spaces[index].CustomerId == Contract.OldValue(Spaces[index].CustomerId)));

            // Space premium state does not change
            Contract.Ensures(Contract.OldValue(Spaces[index].Premium) == Spaces[index].Premium);

            // Success case - car park state
            // Ensure all other spaces remain the same
            Contract.Ensures(Contract.ForAll(0, maxSize, i => i == index ||
                                                              (Spaces[i].State == Contract.OldValue(Spaces[i].State) &&
                                                               Spaces[i].CustomerId == Contract.OldValue(Spaces[i].CustomerId) &&
                                                               Spaces[i].Premium == Contract.OldValue(Spaces[i].Premium))));

            // Failure case
            // Either the index is out of range, no more spaces can be reserved, or
            // the user is attempting to reserve a premium space
            Contract.EnsuresOnThrow<CarParkException>((index < 0 || index >= maxSize) ||
                                                      FreeSpaces <= 0 ||
                                                      Spaces[index].Premium);
            #endregion

            #region Implementation
            // Check the index is in range
            if (index < 0 || index >= maxSize)
            {
                throw new CarParkException("Space index out of range");
            }

            // Check there are free spaces
            if (FreeSpaces <= 0)
            {
                throw new CarParkException("No more spaces can be reserved in this car park");
            }

            // Check the space is not premium
            if (Spaces[index].Premium)
            {
                throw new CarParkException("Cannot reserve a premium space");
            }

            switch (Spaces[index].State)
            {
                // Reserve the space and return true
                case SpaceEnumeration.Free:
                    Spaces[index].State = SpaceEnumeration.Reserved;
                    Spaces[index].CustomerId = customer;
                    return true;
                // Return true if the space is already reserved/purchased by the customer, otherwise false
                case SpaceEnumeration.Purchased:
                case SpaceEnumeration.Reserved:
                    if (Spaces[index].CustomerId == customer)
                    {
                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
            #endregion
        }

        /// <summary>
        /// Allow a customer to buy a specific space that is either free or already reserved by them
        /// </summary>
        /// <param name="index">The index of the space to buy</param>
        /// <param name="customer">The id of the customer buying the space</param>
        /// <returns>True if the space is bought by the customer, False otherwise</returns>
        public bool BuySpace(int index, int customer)
        {
            #region PreConditions
            #endregion

            #region PostConditions

            #region Return value
            // Customer id of space same as caller implies result
            Contract.Ensures(Spaces[index].CustomerId != customer || Contract.Result<bool>());

            // Space was free implies result
            Contract.Ensures(Contract.OldValue(Spaces[index].State) != SpaceEnumeration.Free || Contract.Result<bool>());

            // Result implies either space was free or customer id of space same as called with
            Contract.Ensures(!Contract.Result<bool>() ||
                             (Contract.OldValue(Spaces[index].State) == SpaceEnumeration.Free ||
                              Contract.OldValue(Spaces[index].CustomerId) == customer));
            #endregion

            #region State of given space
            // Space not free implies customer id must not change
            Contract.Ensures(Contract.OldValue(Spaces[index]).State == SpaceEnumeration.Free ||
                             Contract.OldValue(Spaces[index]).CustomerId == Spaces[index].CustomerId);

            // Customer id changing implies the space is purchased, from free
            Contract.Ensures(Contract.OldValue(Spaces[index]).CustomerId == Spaces[index].CustomerId ||
                             Contract.OldValue(Spaces[index]).State == SpaceEnumeration.Free && Spaces[index].State == SpaceEnumeration.Purchased);

            // Space reserved by given customer implies it is purchased
            Contract.Ensures(!(Contract.OldValue(Spaces[index]).CustomerId == customer && Contract.OldValue(Spaces[index]).State == SpaceEnumeration.Reserved) ||
                             Spaces[index].State == SpaceEnumeration.Purchased);

            // Space free implies it is purchased
            Contract.Ensures(Contract.OldValue(Spaces[index]).State != SpaceEnumeration.Free ||
                             Spaces[index].State == SpaceEnumeration.Purchased);

            // Space already purchased implies it is purchased
            Contract.Ensures(Contract.OldValue(Spaces[index]).State != SpaceEnumeration.Purchased ||
                             Spaces[index].State == SpaceEnumeration.Purchased);

            // Space reserved by different customer implies no state change
            Contract.Ensures(Contract.OldValue(Spaces[index].CustomerId) == customer ||
                             (Contract.OldValue(Spaces[index].State) == Spaces[index].State &&
                              Contract.OldValue(Spaces[index].CustomerId) == Spaces[index].CustomerId));

            // Space premium state not change
            Contract.Ensures(Contract.OldValue(Spaces[index].Premium) == Spaces[index].Premium);
            #endregion

            #region State of rest of car park
            // Ensure all other spaces remain the same
            Contract.Ensures(Contract.ForAll(0, maxSize, i => i == index ||
                                                              (Spaces[i].State == Contract.OldValue(Spaces[i].State) &&
                                                               Spaces[i].CustomerId == Contract.OldValue(Spaces[i].CustomerId) &&
                                                               Spaces[i].Premium == Contract.OldValue(Spaces[i].Premium))));
            #endregion

            #region Exceptions
            // The index is out of range
            Contract.EnsuresOnThrow<CarParkException>((index < 0 || index >= maxSize));
            #endregion

            #endregion

            #region Implementation
            // Check the index is in range
            if (index < 0 || index >= maxSize)
            {
                throw new CarParkException("Space index out of range");
            }
            
            switch (Spaces[index].State)
            {
                // Buy the space, return true
                case SpaceEnumeration.Free:
                    Spaces[index].State = SpaceEnumeration.Reserved;
                    Spaces[index].CustomerId = customer;
                    return true;
                // Buy the space if reserved by the customer, otherwise return false
                case SpaceEnumeration.Reserved:
                    if (Spaces[index].CustomerId == customer)
                    {
                        Spaces[index].State = SpaceEnumeration.Purchased;
                        return true;
                    }
                    break;
                // Return true if this space has already been purchased by the customer, otherwise false
                case SpaceEnumeration.Purchased:
                    if (Spaces[index].CustomerId == customer)
                    {
                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
            #endregion
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
