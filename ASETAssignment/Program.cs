using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace ASETAssignment
{
    /// <summary>
    /// Abstract Class for a car park
    /// Uses the CarParkContracts class
    /// This implementation is no different to an interface functionally, tested Code Contracts with both
    /// </summary>
    [ContractClass(typeof(CarParkContracts))]
    public abstract class ICarPark
    {
        /*


            Properties included so that Code Contracts can access state.

            Kind of against the point of an interface, we shouldn't care how it is implementated, but required nonetheless.


        */

        /// <summary>
        /// Maximum size of the car park
        /// </summary>
        public abstract int MaxSize { get; }

        /// <summary>
        /// Number of spaces that cannot be reserved in the car park
        /// </summary>
        public abstract int CannotBeReserved { get; }

        /// <summary>
        /// Map of conferences
        /// </summary>
        public abstract Dictionary<string, Conference> Conferences { get; }

        /// <summary>
        /// Allow a customer to reserve a specific available space
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">The index of the space to reserve</param>
        /// <param name="customer">The id of the customer reserving the space</param>
        /// <returns>True if the space is reserved/purchased by the customer, False if it is reserved by another</returns>
        public abstract bool ReserveSpace(string conferenceKey, int index, int customer);

        /// <summary>
        /// Allow a customer to buy a specific space that is either free or already reserved by them
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">The index of the space to buy</param>
        /// <param name="customer">The id of the customer buying the space</param>
        /// <returns>True if the space is bought by the customer, False otherwise</returns>
        public abstract bool BuySpace(string conferenceKey, int index, int customer);

        /// <summary>
        /// Allow a customer to return a specific space previously reserved by them
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index"></param>
        /// <param name="customer"></param>
        public abstract void ReturnSpace(string conferenceKey, int index, int customer);

        /// <summary>
        /// Removes all the reservations made more than ten seconds days ago
        /// Ten seconds vs two days to demonstrate behaviour
        /// </summary>
        public abstract void CancelReservations();

        /// <summary>
        /// Return the number of spaces available
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <returns>The amount of spaces that are not NotInUse</returns>
        public abstract int CheckAvailability(string conferenceKey);

        /// <summary>
        /// Get the customer id of the space at the given index
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">Index of the given space</param>
        /// <returns></returns>
        public abstract int CheckCustomer(string conferenceKey, int index);


        /*


            Okay, here is where stuff gets weird.

            One flaw encountered with code contracts is the inability to use Contract.OldValue in the second level of a quantifier.
            Both the below lines compile, however, whilst the first line works correctly, the second does not as Contract.OldValue does not it, returns null (Exists not tested).

            // Contract.Ensures(Contract.ForAll(0, Contract.OldValue(Conferences).Count, i => true));
            // Contract.Ensures(Contract.ForAll(0, Contract.OldValue(Conferences).Count, i => Contract.ForAll(0, Contract.OldValue(Conferences).ToList()[i].Value.Spaces.Length, j => true)));

            One solution would be to rework the structure so that these types of contracts are not required, but making the implementation more complex because the specification tools cannot cope is insane.
            A nice solution to this problem is to separate the contracts into normal code (for loops) in separate Pure methods. These method results can then be used instead of the predicates in the post conditions.

            Unfortunately, Code Contracts abysmal interface/abstract class implementation does not allow use of private methods in contracts in the contract class; every method used must be declared in the interface/abstract class.

            As such, the below method has been included here.
            This has all been detailed again in the specific method headers and calls.

            
        */


        /// <summary>
        /// A method to check the post conditions of the cancel reservations method because they are too complex for Code Contracts
        /// </summary>
        /// <param name="current">The post state of the Conferences</param>
        /// <param name="old">The OldValue of the Conferences</param>
        /// <returns></returns>
        [Pure]
        public abstract bool CancelReservationPostConditions(IReadOnlyDictionary<string, Conference> current,
            IReadOnlyDictionary<string, Conference> old);
    }

    /// <inheritdoc />
    /// <summary>
    /// Contracts for CarPark class
    /// </summary>
    [ContractClassFor(typeof(ICarPark))]
    public abstract class CarParkContracts : ICarPark
    {
        /// <inheritdoc />
        /// <summary>
        /// Maximum size of the car park
        /// </summary>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public override int MaxSize { get; }

        /// <inheritdoc />
        /// <summary>
        /// Number of spaces that cannot be reserved in the car park
        /// </summary>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public override int CannotBeReserved { get; }

        /// <inheritdoc />
        /// <summary>
        /// Map of conferences
        /// </summary>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public override Dictionary<string, Conference> Conferences { get; }

        /// <inheritdoc />
        /// <summary>
        /// Allow a customer to reserve a specific available space
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">The index of the space to reserve</param>
        /// <param name="customer">The id of the customer reserving the space</param>
        /// <returns>True if the space is reserved/purchased by the customer, False if it is reserved by another</returns>
        public override bool ReserveSpace(string conferenceKey, int index, int customer)
        {
            #region PreConditions
            #endregion
            #region PostConditions

            #region Return Value
            // Bi-implication
            // The customer id being equal to the called parameter implies the result
            // The result implies the customer id is the called parameter
            Contract.Ensures((Conferences[conferenceKey].Spaces[index].CustomerId != customer || Contract.Result<bool>()) &&
                             !Contract.Result<bool>() || Conferences[conferenceKey].Spaces[index].CustomerId == customer);

            #endregion

            #region State of the given space

            // The Space is reserved if it was not before, otherwise the space remains unchanged
            Contract.Ensures((Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Reserved &&
                              Contract.OldValue(Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Free) &&
                              Conferences[conferenceKey].Spaces[index].CustomerId == customer &&
                              Conferences[conferenceKey].Spaces[index].ReservedAt != DateTime.MinValue) ||
                             (Conferences[conferenceKey].Spaces[index].State == Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) &&
                              Conferences[conferenceKey].Spaces[index].CustomerId == Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId)));

            // Space premium state does not change
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].Premium) == Conferences[conferenceKey].Spaces[index].Premium);
            #endregion

            #region State of the rest of the car park

            // Ensure all other spaces remain the same
            Contract.Ensures(Contract.ForAll(0, MaxSize, i => i == index ||
                                                              Conferences[conferenceKey].Spaces[i]
                                                                  .Equals(Contract.OldValue(Conferences[conferenceKey]
                                                                      .Spaces[i]))));
            #endregion

            #region State of other Conferences

            // Ensure the state of all the other Conferences remains unchanged
            Contract.Ensures(Contract.ForAll(Conferences, c => c.Key == conferenceKey ||
                                                               c.Value.Equals(Contract.OldValue(Conferences)[c.Key])));
            #endregion

            #region Exceptions

            // Failure case
            // Either the index is out of range, no more spaces can be reserved,
            // the user is attempting to reserve a premium space, or the space is not in use
            Contract.EnsuresOnThrow<CarParkException>((index < 0 || index >= MaxSize) ||
                                                      CheckAvailability(conferenceKey) <= CannotBeReserved ||
                                                      Conferences[conferenceKey].Spaces[index].Premium ||
                                                      Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.NotInUse);
            #endregion

            #endregion
            return default(bool);
        }

        /// <inheritdoc />
        /// <summary>
        /// Allow a customer to buy a specific space that is either free or already reserved by them
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">The index of the space to buy</param>
        /// <param name="customer">The id of the customer buying the space</param>
        /// <returns>True if the space is bought by the customer, False otherwise</returns>
        public override bool BuySpace(string conferenceKey, int index, int customer)
        {
            #region PreConditions
            #endregion
            #region PostConditions

            #region Return value
            // Customer id of space same as caller implies result
            Contract.Ensures(Conferences[conferenceKey].Spaces[index].CustomerId != customer || Contract.Result<bool>());

            // Space was free implies result
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) != SpaceEnumeration.Free || Contract.Result<bool>());

            // Result implies either space was free or customer id of space same as called with
            Contract.Ensures(!Contract.Result<bool>() ||
                             (Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) == SpaceEnumeration.Free ||
                              Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId) == customer));
            #endregion

            #region State of given space
            // Space not free implies customer id must not change
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index]).State == SpaceEnumeration.Free ||
                             Contract.OldValue(Conferences[conferenceKey].Spaces[index]).CustomerId == Conferences[conferenceKey].Spaces[index].CustomerId);

            // Customer id changing implies the space is purchased, from free
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index]).CustomerId == Conferences[conferenceKey].Spaces[index].CustomerId ||
                             Contract.OldValue(Conferences[conferenceKey].Spaces[index]).State == SpaceEnumeration.Free && Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Purchased);

            // Space reserved by given customer implies it is purchased
            Contract.Ensures(!(Contract.OldValue(Conferences[conferenceKey].Spaces[index]).CustomerId == customer && Contract.OldValue(Conferences[conferenceKey].Spaces[index]).State == SpaceEnumeration.Reserved) ||
                             Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Purchased);

            // Space free implies it is purchased
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) != SpaceEnumeration.Free ||
                             Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Purchased);

            // Space already purchased implies it is purchased
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index]).State != SpaceEnumeration.Purchased ||
                             Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Purchased);

            // Space reserved by different customer implies no state change
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId) == customer ||
                             Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId) == 0 ||
                             (Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) == Conferences[conferenceKey].Spaces[index].State &&
                              Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId) == Conferences[conferenceKey].Spaces[index].CustomerId));

            // Space premium state not change
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].Premium) == Conferences[conferenceKey].Spaces[index].Premium);


            // Space reserved at state not change
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].ReservedAt) == Conferences[conferenceKey].Spaces[index].ReservedAt);
            #endregion

            #region State of the rest of the car park

            // Ensure all other spaces remain the same
            Contract.Ensures(Contract.ForAll(0, MaxSize, i => i == index ||
                                                              Conferences[conferenceKey].Spaces[i]
                                                                  .Equals(Contract.OldValue(Conferences[conferenceKey]
                                                                      .Spaces[i]))));
            #endregion

            #region State of other Conferences

            // Ensure the state of all the other Conferences remains unchanged
            Contract.Ensures(Contract.ForAll(Conferences, c => c.Key == conferenceKey ||
                                                               c.Value.Equals(Contract.OldValue(Conferences)[c.Key])));
            #endregion

            #region Exceptions

            // The index is out of range, the conference does not exist, or the space is not in use
            Contract.EnsuresOnThrow<CarParkException>(index < 0 ||
                                                      index >= MaxSize ||
                                                      !Conferences.ContainsKey(conferenceKey) ||
                                                      Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.NotInUse);
            #endregion

            #endregion
            return default(bool);
        }

        /// <inheritdoc />
        /// <summary>
        /// Allow a customer to return a specific space previously reserved by them
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index"></param>
        /// <param name="customer"></param>
        public override void ReturnSpace(string conferenceKey, int index, int customer)
        {
            #region PreConditions
            #endregion
            #region PostConditions

            #region State of given space

            // Space free implies no state change
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) != SpaceEnumeration.Free ||
                             Conferences[conferenceKey].Spaces[index].Equals(Contract.OldValue(Conferences[conferenceKey].Spaces[index])));

            // Space reserved or bought and customer id == caller implies returned
            Contract.Ensures(!((Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) == SpaceEnumeration.Reserved) &&
                             Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId) == customer) ||
                             (Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Free &&
                             Conferences[conferenceKey].Spaces[index].CustomerId == 0 &&
                             Conferences[conferenceKey].Spaces[index].ReservedAt == DateTime.MinValue));

            // Returned implies space was reserved or bought and customer id == caller
            Contract.Ensures(!(Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Free &&
                             Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) != SpaceEnumeration.Free &&
                             Conferences[conferenceKey].Spaces[index].CustomerId == 0 &&
                             Conferences[conferenceKey].Spaces[index].ReservedAt == DateTime.MinValue) ||
                             (Contract.OldValue(Conferences[conferenceKey].Spaces[index].State) == SpaceEnumeration.Reserved) &&
                             Contract.OldValue(Conferences[conferenceKey].Spaces[index].CustomerId) == customer);

            // Space premium state not changed
            Contract.Ensures(Contract.OldValue(Conferences[conferenceKey].Spaces[index].Premium) == Conferences[conferenceKey].Spaces[index].Premium);
            #endregion

            #region State of the rest of the car park

            // Ensure all other spaces remain the same
            Contract.Ensures(Contract.ForAll(0, MaxSize, i => i == index ||
                                                              Conferences[conferenceKey].Spaces[i]
                                                                  .Equals(Contract.OldValue(Conferences[conferenceKey]
                                                                      .Spaces[i]))));
            #endregion

            #region State of other Conferences

            // Ensure the state of all the other Conferences remains unchanged
            Contract.Ensures(Contract.ForAll(Conferences, c => c.Key == conferenceKey ||
                                                               c.Value.Equals(Contract.OldValue(Conferences)[c.Key])));
            #endregion

            #region Exceptions

            // The index is out of range, the space has been reserved by another customer, the space has already been purchased (by any customer), or the space is not in use
            Contract.EnsuresOnThrow<CarParkException>(index < 0 ||
                                                      index >= MaxSize ||
                                                      !Conferences.ContainsKey(conferenceKey) ||
                                                      (Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Reserved &&
                                                      Conferences[conferenceKey].Spaces[index].CustomerId != customer) ||
                                                      Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.Purchased ||
                                                      Conferences[conferenceKey].Spaces[index].State == SpaceEnumeration.NotInUse);
            #endregion

            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes all the reservations made more than ten seconds days ago
        /// Ten seconds vs two days to demonstrate behaviour
        /// </summary>
        public override void CancelReservations()
        {
            #region PreConditions
            #endregion

            #region PostConditions

            #region State of the Conferences

            // Important observation - The first below line works correctly, the second does not as Contract.OldValue does not work in second level quantifiers (Exists not tested), it returns null
            // Contract.Ensures(Contract.ForAll(0, Contract.OldValue(Conferences).Count, i => true));
            // Contract.Ensures(Contract.ForAll(0, Contract.OldValue(Conferences).Count, i => Contract.ForAll(0, Contract.OldValue(Conferences).ToList()[i].Value.Spaces.Length, j => true)));
            // Solution - Create a pure method taking the OldValue as a parameter and returning a boolean, perform all the contract logic in the method

            // All space states that are free, bought, or not in use before implies the state does not change
            // All spaces that are previously reserved more than 11 seconds ago were made unreserved, premium state was not changed
            // All spaces that were previously reserved less than 11 seconds ago implies they were not changed
            // Method is static so cannot change state
            Contract.Ensures(CancelReservationPostConditions(Conferences, Contract.OldValue(Conferences)));
            #endregion

            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Return the number of spaces available
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <returns>The amount of spaces that are not NotInUse</returns>
        [Pure]
        public override int CheckAvailability(string conferenceKey)
        {
            #region PreConditions
            #endregion

            #region PostConditions

            #region Result

            // Contract result is the number of free spaces
            Contract.Ensures(Contract.Result<int>() == Conferences[conferenceKey].Spaces.Count(s => s.State == SpaceEnumeration.Free));
            #endregion

            #region State of the rest of the car park

            // Ensure all other spaces remain the same
            Contract.Ensures(Contract.ForAll(0, MaxSize, i => Conferences[conferenceKey].Spaces[i]
                .Equals(Contract.OldValue(Conferences[conferenceKey]
                    .Spaces[i]))));
            #endregion

            #region State of other Conferences

            // Ensure the state of all the other Conferences remains unchanged
            Contract.Ensures(Contract.ForAll(Conferences, c => c.Key == conferenceKey ||
                                                               c.Value.Equals(Contract.OldValue(Conferences)[c.Key])));
            #endregion

            #region Exceptions

            // If we throw in this method, the conference does not exist
            Contract.EnsuresOnThrow<CarParkException>(!Conferences.ContainsKey(conferenceKey));
            #endregion

            #endregion

            return default(int);
        }

        /// <inheritdoc />
        /// <summary>
        /// Get the customer id of the space at the given index
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">Index of the given space</param>
        /// <returns></returns>
        [Pure]
        public override int CheckCustomer(string conferenceKey, int index)
        {
            #region PreConditions
            #endregion

            #region PostConditions

            #region Return value

            // Return the custoemr id of the given space
            Contract.Ensures(Contract.Result<int>() == Conferences[conferenceKey].Spaces[index].CustomerId);
            #endregion

            #region State of the rest of the car park

            // Ensure all spaces remain the same
            Contract.Ensures(Contract.ForAll(0, MaxSize, i => Conferences[conferenceKey].Spaces[i]
                                                                  .Equals(Contract.OldValue(Conferences[conferenceKey]
                                                                      .Spaces[i]))));
            #endregion

            #region State of other Conferences#

            // Ensure the state of all the other Conferences remains unchanged
            Contract.Ensures(Contract.ForAll(Conferences, c => c.Key == conferenceKey ||
                                                               c.Value.Equals(Contract.OldValue(Conferences)[c.Key])));
            #endregion

            #region Exceptions

            // Index not in range or conference does not exist
            Contract.EnsuresOnThrow<CarParkException>(index < 0 ||
                                                      index >= MaxSize ||
                                                      !Conferences.ContainsKey(conferenceKey));

            // State not changed
            Contract.EnsuresOnThrow<CarParkException>(Contract.ForAll(0, MaxSize, i => Conferences[conferenceKey].Spaces[i].State == Contract.OldValue(Conferences[conferenceKey].Spaces[i].State) &&
                                                                                       Conferences[conferenceKey].Spaces[i].CustomerId == Contract.OldValue(Conferences[conferenceKey].Spaces[i].CustomerId) &&
                                                                                       Conferences[conferenceKey].Spaces[i].Premium == Contract.OldValue(Conferences[conferenceKey].Spaces[i].Premium)));

            #endregion

            #endregion

            return default(int);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// A CarParkException class
    /// </summary>
    public class CarParkException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Constructor
        /// Calls the base with a given message
        /// </summary>
        /// <param name="message">Exception message to be shown</param>
        public CarParkException(string message) : base(message)
        {

        }

        /// <inheritdoc />
        /// <summary>
        /// Constructor
        /// Calls the base with no message
        /// </summary>
        public CarParkException() : base()
        {

        }
    }

    /// <summary>
    /// Enumeration of Space State
    /// </summary>
    public enum SpaceEnumeration
    {
        // Space is free
        Free,

        // Space is reserved
        Reserved,

        // Space has been purchased
        Purchased,

        // Space is not in use
        NotInUse
    }

    /// <summary>
    /// A Space class
    /// </summary>
    public class Space
    {
        /// <summary>
        /// Is the space premium?
        /// </summary>
        public bool Premium { get; set; }

        /// <summary>
        /// The state of the space
        /// </summary>
        public SpaceEnumeration State { get; set; }

        /// <summary>
        /// The customer Id associated with this space (if reserved or purchased)
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// The time at which this space has been reserved
        /// </summary>
        public DateTime ReservedAt { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Space()
        {
            ReservedAt = DateTime.MinValue;
        }

        /// <summary>
        /// Overriden equals method (Deep equality)
        /// </summary>
        /// <param name="obj">The other object</param>
        /// <returns>True if the same</returns>
        [Pure]
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // Get the Space object
            var other = (Space)obj;

            if (Premium != other.Premium)
            {
                return false;
            }

            if (State != other.State)
            {
                return false;
            }

            if (CustomerId != other.CustomerId)
            {
                return false;
            }

            if (ReservedAt != other.ReservedAt)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Override GetHashCode method
        /// Required to override .Equals()
        /// </summary>
        /// <returns>base.GetHashCode()</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// A class representing a single conference (day)
    /// No contracts here, CarPark manages them all
    /// </summary>
    public class Conference
    {
        /// <summary>
        /// The number of spaces not in use
        /// </summary>
        private readonly int notInUseSpaces;

        /// <summary>
        /// The number of spaces that are premium
        /// </summary>
        private readonly int premiumSpaces;

        /// <summary>
        /// The spaces
        /// </summary>
        public Space[] Spaces { get; }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="maxSize">Maximum size of the car park</param>
        /// <param name="notInUseSpaces">The number of space that are NotInUse</param>
        /// <param name="premiumSpaces">The number of spaces that are Premium</param>
        public Conference(int maxSize, int notInUseSpaces, int premiumSpaces)
        {
            #region Implementation

            // Set the data members
            this.notInUseSpaces = notInUseSpaces;
            this.premiumSpaces = premiumSpaces;

            // Create the array of spaces
            Spaces = new Space[maxSize];

            // Create the not in use spaces
            for (var i = 0; i < notInUseSpaces; i++)
            {
                Spaces[i] = new Space()
                {
                    State = SpaceEnumeration.NotInUse
                };
            }

            // Create the premium spaces
            for (var i = notInUseSpaces; i < notInUseSpaces + premiumSpaces; i++)
            {
                Spaces[i] = new Space()
                {
                    State = SpaceEnumeration.Free,
                    Premium = true
                };
            }

            // Create the free spaces
            for (var i = notInUseSpaces + premiumSpaces; i < maxSize; i++)
            {
                Spaces[i] = new Space()
                {
                    State = SpaceEnumeration.Free
                };

            }
            #endregion
        }

        /// <summary>
        /// Overriden equals method (Deep equality)
        /// </summary>
        /// <param name="obj">The other object</param>
        /// <returns>True if the same</returns>
        [Pure]
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // Get the Conference object
            var other = (Conference)obj;

            if (notInUseSpaces != other.notInUseSpaces)
            {
                return false;
            }

            if (premiumSpaces != other.premiumSpaces)
            {
                return false;
            }

            if (Spaces.Length != other.Spaces.Length)
            {
                return false;
            }

            // Return if any of the spaces are not the same (Deep equality)
            for (var i = 0; i < Spaces.Length; i++)
            {
                if (!Spaces[i].Equals(other.Spaces[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Override GetHashCode method
        /// Required to override .Equals()
        /// </summary>
        /// <returns>base.GetHashCode()</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Implementation of CarPark class
    /// </summary>
    public class CarPark : ICarPark
    {
        /// <summary>
        /// Configurable maximum size of the carpark
        /// </summary>
        private readonly int maxSize;

        /// <inheritdoc />
        /// <summary>
        /// MaxSize property
        /// </summary>
        public override int MaxSize => maxSize;

        /// <summary>
        /// Number of spaces that cannot be reserved, for use on the last day
        /// </summary>
        private readonly int cannotBeReserved;

        /// <inheritdoc />
        /// <summary>
        /// Number of spaces that cannot be reserved property
        /// </summary>
        public override int CannotBeReserved => cannotBeReserved;

        /// <summary>
        /// Lookup table of conferences
        /// </summary>
        private readonly Dictionary<string, Conference> conferences;

        /// <inheritdoc />
        /// <summary>
        /// Conferences property
        /// </summary>
        public override Dictionary<string, Conference> Conferences => conferences;

        /// <summary>
        /// Invariant method
        /// Only works here, not in the contracts class for some bizzare contracts reason
        /// </summary>
        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            // The spaces arrays must always keep the same length
            Contract.Invariant(Conferences.Values.All(c => c.Spaces.Length == MaxSize));

            // There must be at least 3 spaces free at all times in each conference instance
            Contract.Invariant(Conferences.Values.All(c => c.Spaces.Count(s => s.State == SpaceEnumeration.Free) >= CannotBeReserved));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSize">The amount of spaces in the car park</param>
        /// <param name="cannotBeReserved">The number of spaces that cannot be reserved, use on the last day</param>
        /// <param name="conferenceNames">Enumerable conference names</param>
        /// <param name="notInUseSpaces">The number of spaces not in use for each conference</param>
        /// <param name="premiumSpaces">The number of premium spaces for each conference in the car park</param>
        public CarPark(int maxSize, int cannotBeReserved, IReadOnlyList<string> conferenceNames, IReadOnlyList<int> notInUseSpaces, IReadOnlyList<int> premiumSpaces)
        {
            // Can't pull constructor contracts out as the contract class needs to have a different name
            #region PreConditions

            // Max size must be less than max integer representation
            Contract.Requires(maxSize < int.MaxValue);

            // Max size must be greater than 0
            Contract.Requires(maxSize > 0);

            // Conference names must not be null
            Contract.Requires(conferenceNames != null);

            // Conference names must not be empty
            Contract.Requires(conferenceNames.Any());

            // Conference names and notInUseSpaces must be equal length
            Contract.Requires(conferenceNames.Count() == notInUseSpaces.Count());

            // Conference names and premiumSpaces must be equal length
            Contract.Requires(conferenceNames.Count() == premiumSpaces.Count());

            // notInUseSpaces and premiumSpaces must be equal length
            Contract.Requires(notInUseSpaces.Count() == premiumSpaces.Count());

            // All the notInUseSpaces entries must be greater than 0
            Contract.Requires(notInUseSpaces.All(i => i >= 0));

            // All the premiumSpaces entries must be greater than 0
            Contract.Requires(premiumSpaces.All(i => i >= 0));

            // Number of permanently reserved spaces + the number of premium spaces must be less than the max size for each conference day
            Contract.Requires(Contract.ForAll(0, notInUseSpaces.Count(), i =>
                notInUseSpaces[i] + premiumSpaces[i] <= maxSize - cannotBeReserved));

            #endregion

            #region PostConditions

            // Conferences is correct length
            Contract.Ensures(conferences.Count == conferenceNames.Count());

            // Size of each conference array is correct
            Contract.Ensures(conferences.Values.All(c => c.Spaces.Length == maxSize));

            // The number of not in use spaces for each conference must be correct
            Contract.Ensures(Contract.ForAll(0, notInUseSpaces.Count, i =>
                                                                      conferences[conferenceNames[i]].Spaces.Count(s => s.State == SpaceEnumeration.NotInUse) == notInUseSpaces[i]));

            // The number of premium spaces for each conference must be correct
            Contract.Ensures(Contract.ForAll(0, premiumSpaces.Count, i =>
                                                                     conferences[conferenceNames[i]].Spaces.Count(s => s.Premium) == premiumSpaces[i]));
            #endregion

            #region Implementation

            // Initialise the members
            this.maxSize = maxSize;
            this.cannotBeReserved = cannotBeReserved;

            // Create the map of conferences
            conferences = new Dictionary<string, Conference>();

            // Conert to string array to avoid multiple enumeration inconsistencies
            var enumerable = conferenceNames as string[] ?? conferenceNames.ToArray();
            for (var i = 0; i < enumerable.Length; i++)
            {
                conferences.Add(enumerable[i], new Conference(maxSize, notInUseSpaces[i], premiumSpaces[i]));
            }
            #endregion
        }


        /// <inheritdoc />
        /// <summary>
        /// Method containg the post condition logic for the cancel reservations method
        /// Separated due to issue with code contracts
        /// Ideally should be a private method in the contracts class
        /// Required here due to the fact that code contracts calls this class and not its own, and won't allow methods not defined in interface/top level abstract class
        /// </summary>
        /// <param name="current">The dictionary of conferences after method completion</param>
        /// <param name="old">The dictionary of conferences before original invocation</param>
        /// <returns></returns>
        [Pure]
        public override bool CancelReservationPostConditions(IReadOnlyDictionary<string, Conference> current, IReadOnlyDictionary<string, Conference> old)
        {
            // All space states that are free, bought, or not in use before implies the state does not change

            // Contract should be analagous to below loop, if it worked correctly
            //Contract.Ensures(Contract.ForAll(0, conferences.Count, i =>
            //                                                 Contract.ForAll(0, conferences.Values.ToList()[i].Spaces.Length, j =>
            //                                                                                            !(Contract.OldValue(conferences.Values.ToList()[i].Spaces[j].State) == SpaceEnumeration.Free ||
            //                                                                                              Contract.OldValue(conferences.Values.ToList()[i].Spaces[j].State) == SpaceEnumeration.Purchased ||
            //                                                                                              Contract.OldValue(conferences.Values.ToList()[i].Spaces[j].State) == SpaceEnumeration.NotInUse) ||
            //                                                                                            conferences.Values.ToList()[i].Spaces[j].Equals(Contract.OldValue(conferences.Values.ToList()[i].Spaces[j])))));

            foreach (var c in current)
            {
                for (var i = 0; i < current[c.Key].Spaces.Length; i++)
                {
                    if (!(old[c.Key].Spaces[i].State == SpaceEnumeration.Free ||
                          old[c.Key].Spaces[i].State == SpaceEnumeration.Purchased ||
                          old[c.Key].Spaces[i].State == SpaceEnumeration.NotInUse) ||
                        c.Value.Spaces[i].Equals(old[c.Key].Spaces[i]))
                    {

                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // All spaces that are previously reserved more than 11 seconds ago were made unreserved, premium state was not changed
            // Contract should be analagous to below loop, if it worked correctly
            //Contract.Ensures(Contract.ForAll(conferences, c => Contract.ForAll(0, c.Value.Spaces.Length, i => !(Contract.OldValue(conferences[c.Key].Spaces[i].State) == SpaceEnumeration.Reserved &&
            //                                                                                                    Contract.OldValue(conferences[c.Key].Spaces[i].ReservedAt) < DateTime.Now.AddSeconds(-10)) ||
            //                                                                                                  (c.Value.Spaces[i].State == SpaceEnumeration.Free &&
            //                                                                                                   c.Value.Spaces[i].Premium == Contract.OldValue(conferences[c.Key].Spaces[i].Premium) &&
            //                                                                                                   c.Value.Spaces[i].CustomerId == 0 && 
            //                                                                                                   c.Value.Spaces[i].ReservedAt == DateTime.MinValue))));

            foreach (var c in current)
            {
                for (var i = 0; i < current[c.Key].Spaces.Length; i++)
                {
                    if (!(old[c.Key].Spaces[i].State == SpaceEnumeration.Reserved &&
                          old[c.Key].Spaces[i].ReservedAt < DateTime.Now.AddSeconds(-10)) ||
                        (c.Value.Spaces[i].State == SpaceEnumeration.Free &&
                         c.Value.Spaces[i].Premium == old[c.Key].Spaces[i].Premium &&
                         c.Value.Spaces[i].CustomerId == 0 &&
                         c.Value.Spaces[i].ReservedAt == DateTime.MinValue))
                    {

                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // All spaces that were previously reserved less than 11 seconds ago implies they were not changed
            // Contract should be analagous to below loop, if it worked correctly
            //Contract.Ensures(Contract.ForAll(conferences, c =>
            //    Contract.ForAll(0, c.Value.Spaces.Length, i =>
            //        !(c.Value.Spaces[i].State == SpaceEnumeration.Reserved &&
            //          c.Value.Spaces[i].ReservedAt >= DateTime.Now.AddSeconds(-10)) ||
            //        c.Value.Spaces[i].Equals(Contract.OldValue(c.Value.Spaces[i])))));

            foreach (var c in current)
            {
                for (var i = 0; i < current[c.Key].Spaces.Length; i++)
                {
                    if (!(c.Value.Spaces[i].State == SpaceEnumeration.Reserved &&
                          c.Value.Spaces[i].ReservedAt >= DateTime.Now.AddSeconds(-10)) ||
                        c.Value.Spaces[i].Equals(old[c.Key].Spaces[i]))
                    {

                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Allow a customer to reserve a specific available space
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">The index of the space to reserve</param>
        /// <param name="customer">The id of the customer reserving the space</param>
        /// <returns>True if the space is reserved/purchased by the customer, False if it is reserved by another</returns>
        public override bool ReserveSpace(string conferenceKey, int index, int customer)
        {
            #region Implementation
            // Check the index is in range
            if (index < 0 || index >= maxSize)
            {
                throw new CarParkException("Space index out of range");
            }

            // Check there are free spaces
            if (CheckAvailability(conferenceKey) <= cannotBeReserved)
            {
                throw new CarParkException("No more spaces can be reserved in this car park");
            }

            // Check the space is not premium
            if (conferences[conferenceKey].Spaces[index].Premium)
            {
                throw new CarParkException("Cannot reserve a premium space");
            }

            switch (conferences[conferenceKey].Spaces[index].State)
            {
                // Reserve the space and return true
                case SpaceEnumeration.Free:
                    conferences[conferenceKey].Spaces[index].State = SpaceEnumeration.Reserved;
                    conferences[conferenceKey].Spaces[index].CustomerId = customer;
                    conferences[conferenceKey].Spaces[index].ReservedAt = DateTime.Now;
                    return true;
                // Return true if the space is already reserved/purchased by the customer, otherwise false
                case SpaceEnumeration.Purchased:
                case SpaceEnumeration.Reserved:
                    if (conferences[conferenceKey].Spaces[index].CustomerId == customer)
                    {
                        return true;
                    }
                    break;
                case SpaceEnumeration.NotInUse:
                    throw new CarParkException("Space not in use");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Allow a customer to buy a specific space that is either free or already reserved by them
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">The index of the space to buy</param>
        /// <param name="customer">The id of the customer buying the space</param>
        /// <returns>True if the space is bought by the customer, False otherwise</returns>
        public override bool BuySpace(string conferenceKey, int index, int customer)
        {
            #region Implementation
            // Check the index is in range
            if (index < 0 || index >= maxSize)
            {
                throw new CarParkException("Space index out of range");
            }

            if (!conferences.ContainsKey(conferenceKey))
            {
                throw new CarParkException("Conference does not exist");
            }

            switch (conferences[conferenceKey].Spaces[index].State)
            {
                // Buy the space, return true
                case SpaceEnumeration.Free:
                    conferences[conferenceKey].Spaces[index].State = SpaceEnumeration.Purchased;
                    conferences[conferenceKey].Spaces[index].CustomerId = customer;
                    return true;
                // Buy the space if reserved by the customer, otherwise return false
                case SpaceEnumeration.Reserved:
                    if (conferences[conferenceKey].Spaces[index].CustomerId == customer)
                    {
                        conferences[conferenceKey].Spaces[index].State = SpaceEnumeration.Purchased;
                        return true;
                    }
                    break;
                // Return true if this space has already been purchased by the customer, otherwise false
                case SpaceEnumeration.Purchased:
                    if (conferences[conferenceKey].Spaces[index].CustomerId == customer)
                    {
                        return true;
                    }
                    break;
                case SpaceEnumeration.NotInUse:
                    throw new CarParkException("Space not in use");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Allow a customer to return a specific space previously reserved by them
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index"></param>
        /// <param name="customer"></param>
        public override void ReturnSpace(string conferenceKey, int index, int customer)
        {
            #region Implementation
            // Check the index is in range
            if (index < 0 || index >= maxSize)
            {
                throw new CarParkException("Space index out of range");
            }

            // Check the conference exists
            if (!conferences.ContainsKey(conferenceKey))
            {
                throw new CarParkException("Conference does not exist");
            }

            switch (conferences[conferenceKey].Spaces[index].State)
            {
                case SpaceEnumeration.Free:
                    break;
                case SpaceEnumeration.Reserved:
                    if (conferences[conferenceKey].Spaces[index].CustomerId == customer)
                    {
                        conferences[conferenceKey].Spaces[index].State = SpaceEnumeration.Free;
                        conferences[conferenceKey].Spaces[index].CustomerId = 0;
                        conferences[conferenceKey].Spaces[index].ReservedAt = DateTime.MinValue;
                    }
                    else
                    {
                        throw new CarParkException("Space reserved by different customer");
                    }
                    break;
                case SpaceEnumeration.Purchased:
                    throw new CarParkException("Cannot return a purchased space");
                case SpaceEnumeration.NotInUse:
                    throw new CarParkException("Space not in use");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Removes all the reservations made more than ten seconds days ago
        /// Ten seconds vs two days to demonstrate behaviour
        /// </summary>
        public override void CancelReservations()
        {
            #region Implementation

            foreach (var conference in conferences)
            {
                foreach (var space in conference.Value.Spaces)
                {
                    if (space.State == SpaceEnumeration.Reserved && space.ReservedAt < DateTime.Now.AddSeconds(-10))
                    {
                        space.State = SpaceEnumeration.Free;
                        space.CustomerId = 0;
                        space.ReservedAt = DateTime.MinValue;
                    }
                }
            }
            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Return the number of spaces available
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <returns>The amount of spaces that are not NotInUse</returns>
        [Pure]
        public override int CheckAvailability(string conferenceKey)
        {
            #region Implementation

            if (!conferences.ContainsKey(conferenceKey))
            {
                throw new CarParkException("Conference does not exist");
            }

            return conferences[conferenceKey].Spaces.Count(s => s.State == SpaceEnumeration.Free);

            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Get the customer id of the space at the given index
        /// </summary>
        /// <param name="conferenceKey">Conference name lookup key</param>
        /// <param name="index">Index of the given space</param>
        /// <returns></returns>
        [Pure]
        public override int CheckCustomer(string conferenceKey, int index)
        {
            #region Implementation

            // Check the index is in range
            if (index < 0 || index >= maxSize)
            {
                throw new CarParkException("Space index out of range");
            }

            if (!conferences.ContainsKey(conferenceKey))
            {
                throw new CarParkException("Conference does not exist");
            }

            return conferences[conferenceKey].Spaces[index].CustomerId;

            #endregion
        }
    }

    /// <summary>
    /// Main class
    /// </summary>
    class Program
    {
        private const int MaxSize = 10;
        private const int CannotBeReserved = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("Testing invalid parameters...");

            CheckInvalidParameterSetups();

            Console.WriteLine("Tests passed");

            Console.WriteLine("Testing invalid car park setups...");

            CheckInvalidCarParkSetups();

            Console.WriteLine("Tests passed");

            Console.WriteLine("Testing valid setups...");

            CheckValidSetups();

            Console.WriteLine("Tests passed");

            Console.WriteLine("Tests completed");
            Console.ReadLine();
        }

        private static void CheckInvalidParameterSetups()
        {
            var conferences = new List<string>()
            {
            };

            var notInUseSpaces = new List<int>()
            {
            };

            var premiumSpaces = new List<int>()
            {   
            };

            CarPark carPark;

            // Catch no conferences setup
            try
            {
                carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Precondition failed: conferenceNames.Any()")
                {
                    Console.WriteLine("Contract did not throw error when conference names blank");
                }
            }

            conferences.Add("Conference 1");
            
            // Catch conference names length != notInUseSpaces length
            try
            {
                carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Precondition failed: conferenceNames.Count() == notInUseSpaces.Count()")
                {
                    Console.WriteLine("Contract did not throw error when conference names length not equal to not in use spaces length");
                }
            }

            notInUseSpaces.Add(0);

            // Catch conference names length != premiumSpaces length
            try
            {
                carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Precondition failed: conferenceNames.Count() == premiumSpaces.Count()")
                {
                    Console.WriteLine("Contract did not throw error when conference names length not equal to premium spaces length");
                }
            }

            premiumSpaces.Add(0);

            try
            {
                carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CarPark constructor threw unexpected exception " + ex.Message);
            }
        }

        private static void CheckInvalidCarParkSetups()
        {
            var conferences = new List<string>()
            {
                "Conference 1"
            };

            var notInUseSpaces = new List<int>()
            {
                0
            };

            var premiumSpaces = new List<int>()
            {
                10
            };

            CarPark carPark;

            // Catch not enough free spaces
            try
            {
                carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Precondition failed: Contract.ForAll(0, notInUseSpaces.Count(), i => notInUseSpaces[i] + premiumSpaces[i] <= maxSize - cannotBeReserved)")
                {
                    Console.WriteLine("Contract did not throw error when not enough spaces are free (too many premium)");
                }
            }

            conferences = new List<string>()
            {
                "Conference 1"
            };

            notInUseSpaces = new List<int>()
            {
                10
            };

            premiumSpaces = new List<int>()
            {
                0
            };

            // Catch not enough free spaces
            try
            {
                carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Precondition failed: Contract.ForAll(0, notInUseSpaces.Count(), i => notInUseSpaces[i] + premiumSpaces[i] <= maxSize - cannotBeReserved)")
                {
                    Console.WriteLine("Contract did not throw error when not enough spaces are free (too many not in use)");
                }
            }
        }

        private static void CheckValidSetups()
        {
            IReadOnlyList<string> conferences = new List<string>()
            {
                "Conference 1",
                "Conference 2",
                "Conference 3"
            };

            IReadOnlyList<int> notInUseSpaces = new List<int>()
            {
                1,
                2,
                3
            };

            IReadOnlyList<int> premiumSpaces = new List<int>()
            {
                1,
                2,
                3
            };

            var carPark = new CarPark(MaxSize, CannotBeReserved, conferences, notInUseSpaces, premiumSpaces);

            try
            {
                #region ReserveSpace tests

                // Customer should be able to book the space
                if (!carPark.ReserveSpace("Conference 1", 2, 1))
                {
                    Console.WriteLine("Test failed, customer could not reserve space");
                }

                // Should return true as customer has already reserved the space
                if (!carPark.ReserveSpace("Conference 1", 2, 1))
                {
                    Console.WriteLine("Test failed, returned false when customer reserved space twice");
                }

                // Should return false as a different customer cannot reserve the same space
                if (carPark.ReserveSpace("Conference 1", 2, 2))
                {
                    Console.WriteLine("Test failed, customer reserved space that another has already reserved");
                }

                // Customer should be able to book a different space
                if (!carPark.ReserveSpace("Conference 1", 3, 2))
                {
                    Console.WriteLine("Test failed, customer could not reserve a different space");
                }

                // Customer should not be able to reserve a space that is not in use
                try
                {
                    carPark.ReserveSpace("Conference 1", 0, 1);
                    Console.WriteLine("Test failed, customer could reserve a not in use space");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Customer should not be able to reserve a space that is premium
                try
                {
                    carPark.ReserveSpace("Conference 1", 1, 1);
                    Console.WriteLine("Test failed, customer could reserve a premium space");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to reserve a space for a conference that does not exist
                try
                {
                    carPark.ReserveSpace("abc", 0, 1);
                    Console.WriteLine("Test failed, did not throw when conference does not exist");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to reserve a space with index negative
                try
                {
                    carPark.ReserveSpace("Conference 2", -1, 1);
                    Console.WriteLine("Test failed, should throw when reserving space with negative index");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to reserve a space with index boundary out of range
                try
                {
                    carPark.ReserveSpace("Conference 2", 10, 1);
                    Console.WriteLine("Test failed, should throw when reserving space with boundary out of range");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to reserve a space with index out of range
                try
                {
                    carPark.ReserveSpace("Conference 2", 1000000, 1);
                    Console.WriteLine("Test failed, should throw when reserving space out of range");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }
                #endregion

                #region BuySpace tests

                // Customer should be able to buy a space they have reserved
                if (!carPark.BuySpace("Conference 1", 2, 1))
                {
                    Console.WriteLine("Test failed, customer could not buy space they have previously reserved");
                }

                // Customer should be able to buy a space they have already bought
                if (!carPark.BuySpace("Conference 1", 2, 1))
                {
                    Console.WriteLine("Test failed, returned false when customer bought a space they already own");
                }

                // Should return false as a different customer cannot buy an already bought space
                if (carPark.BuySpace("Conference 1", 2, 2))
                {
                    Console.WriteLine("Test failed, customer bought a space already bought by another");
                }

                // Should return false as a different customer cannot buy a space reserved by another
                if (carPark.BuySpace("Conference 1", 3, 1))
                {
                    Console.WriteLine("Test failed, customer bought a space already reserved by another");
                }

                // Customer should not be able to buy a not in use space
                try
                {
                    carPark.BuySpace("Conference 1", 0, 1);
                    Console.WriteLine("Test failed, customer could buy a space that was not in use");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Customer should be able to buy a space that is currently free
                if (!carPark.BuySpace("Conference 1", 4, 1))
                {
                    Console.WriteLine("Test failed, customer could not buy a space that is currently free");
                }

                // Customer should be able to buy a premium space
                if (!carPark.BuySpace("Conference 1", 1, 1))
                {
                    Console.WriteLine("Test failed, customer could not buy a premium space");
                }

                // Failure case - should not be able to buy a space for a conference that does not exist
                try
                {
                    carPark.BuySpace("abc", 0, 1);
                    Console.WriteLine("Test failed, did not throw when conference does not exist");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to buy a space with index negative
                try
                {
                    carPark.BuySpace("Conference 2", -1, 1);
                    Console.WriteLine("Test failed, should throw when buying space with negative index");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to buy a space with index boundary out of range
                try
                {
                    carPark.BuySpace("Conference 2", 10, 1);
                    Console.WriteLine("Test failed, should throw when buying space with boundary out of range");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to buy a space with index out of range
                try
                {
                    carPark.BuySpace("Conference 2", 1000000, 1);
                    Console.WriteLine("Test failed, should throw when buying space out of range");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }
                #endregion

                #region ReturnSpace tests

                // Customer should not be able to return a space bought by another customer
                try
                {
                    carPark.ReturnSpace("Conference 1", 3, 1);
                    Console.WriteLine("Test failed, customer could return a space bought by another");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Customer should be able to return a space reserved by themselves
                try
                {
                    carPark.ReturnSpace("Conference 1", 3, 2);
                }
                catch (CarParkException ex)
                {
                    Console.WriteLine("Test failed, customer could not return a space that they have reserved");
                }

                // Customer should not be able to return a space that is not in use
                try
                {
                    carPark.ReturnSpace("Conference 1", 0, 1);
                    Console.WriteLine("Test failed, customer should not be able to return a space that is not in use");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Customer should not be able to return a space that has been bought by them
                try
                {
                    carPark.ReturnSpace("Conference 1", 2, 1);
                    Console.WriteLine("Test failed, customer should not be able to return a space that they have purchased");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to return a space for a conference that does not exist
                try
                {
                    carPark.ReturnSpace("abc", 0, 1);
                    Console.WriteLine("Test failed, did not throw when conference does not exist");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to return a space with index negative
                try
                {
                    carPark.ReturnSpace("Conference 2", -1, 1);
                    Console.WriteLine("Test failed, should throw when returning space with negative index");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to return a space with index boundary out of range
                try
                {
                    carPark.ReturnSpace("Conference 2", 10, 1);
                    Console.WriteLine("Test failed, should throw when returning space with boundary out of range");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                // Failure case - should not be able to return a space with index out of range
                try
                {
                    carPark.ReturnSpace("Conference 2", 1000000, 1);
                    Console.WriteLine("Test failed, should throw when returning space out of range");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }
                #endregion
                
                #region CheckAvailability tests

                // Expect there to be x spaces available in Conference 1
                if (carPark.CheckAvailability("Conference 1") != 6)
                {
                    Console.WriteLine("Test failed, number of free spaces should be 6");
                }

                // Expect there to be y spaces available in Conference 2
                if (carPark.CheckAvailability("Conference 2") != 8)
                {
                    Console.WriteLine("Test failed, number of free spaces should be 8");
                }

                // Expect there to be z spaces available in Conference 3
                if (carPark.CheckAvailability("Conference 3") != 7)
                {
                    Console.WriteLine("Test failed, number of free spaces should be 7");
                }

                // Failure case - conference does not exist
                try
                {
                    carPark.CheckAvailability("abc");
                    Console.WriteLine("Test Failed, returned value when conference does not exist");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                #endregion

                #region CheckCustomer tests

                // Conference 1 - Space 0 - no customer
                if (carPark.CheckCustomer("Conference 1", 0) != 0)
                {
                    Console.WriteLine("Test failed, space 0 should not be assigned to any customer");
                }

                // Conference 1 - Space 1 - customer 1
                if (carPark.CheckCustomer("Conference 1", 1) != 1)
                {
                    Console.WriteLine("Test failed, space 1 should be assigned to customer 1");
                }

                // Conference 1 - Space 2 - customer 1
                if (carPark.CheckCustomer("Conference 1", 2) != 1)
                {
                    Console.WriteLine("Test failed, space 2 should be assigned to customer 1");
                }

                // Conference 1 - Space 3 - no customer
                if (carPark.CheckCustomer("Conference 1", 3) != 0)
                {
                    Console.WriteLine("Test failed, space 3 should not be assigned to any customer");
                }

                // Conference 1 - Space 4 - no customer
                if (carPark.CheckCustomer("Conference 1", 4) != 1)
                {
                    Console.WriteLine("Test failed, space 4 should be assigned to customer 1");
                }

                // Conference 1 - Space 5 - no customer
                if (carPark.CheckCustomer("Conference 1", 5) != 0)
                {
                    Console.WriteLine("Test failed, space 5 should not be assigned to any customer");
                }

                // Conference 1 - Space 6 - no customer
                if (carPark.CheckCustomer("Conference 1", 6) != 0)
                {
                    Console.WriteLine("Test failed, space 6 should not be assigned to any customer");
                }

                // Conference 1 - Space 7 - no customer
                if (carPark.CheckCustomer("Conference 1", 7) != 0)
                {
                    Console.WriteLine("Test failed, space 7 should not be assigned to any customer");
                }

                // Conference 1 - Space 8 - no customer
                if (carPark.CheckCustomer("Conference 1", 8) != 0)
                {
                    Console.WriteLine("Test failed, space 8 should not be assigned to any customer");
                }
                
                // Conference 1 - Space 9 - no customer
                if (carPark.CheckCustomer("Conference 1", 9) != 0)
                {
                    Console.WriteLine("Test failed, space 9 should not be assigned to any customer");
                }

                // Conference 2 - Space 0 - no customer
                if (carPark.CheckCustomer("Conference 2", 0) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 0 should not be assigned to any customer");
                }

                // Conference 2 - Space 1 - customer 1
                if (carPark.CheckCustomer("Conference 2", 1) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 1 should not be assigned to any customer");
                }

                // Conference 2 - Space 2 - customer 1
                if (carPark.CheckCustomer("Conference 2", 2) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 2 should not be assigned to any customer");
                }

                // Conference 2 - Space 3 - no customer
                if (carPark.CheckCustomer("Conference 2", 3) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 3 should not be assigned to any customer");
                }

                // Conference 2 - Space 4 - no customer
                if (carPark.CheckCustomer("Conference 2", 4) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 4 should not be assigned to any customer");
                }

                // Conference 2 - Space 5 - no customer
                if (carPark.CheckCustomer("Conference 2", 5) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 5 should not be assigned to any customer");
                }

                // Conference 2 - Space 6 - no customer
                if (carPark.CheckCustomer("Conference 2", 6) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 6 should not be assigned to any customer");
                }

                // Conference 2 - Space 7 - no customer
                if (carPark.CheckCustomer("Conference 2", 7) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 7 should not be assigned to any customer");
                }

                // Conference 2 - Space 8 - no customer
                if (carPark.CheckCustomer("Conference 2", 8) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 8 should not be assigned to any customer");
                }

                // Conference 2 - Space 9 - no customer
                if (carPark.CheckCustomer("Conference 2", 9) != 0)
                {
                    Console.WriteLine("Test failed, conference 2, space 9 should not be assigned to any customer");
                }

                // Conference 3 - Space 0 - no customer
                if (carPark.CheckCustomer("Conference 3", 0) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 0 should not be assigned to any customer");
                }

                // Conference 3 - Space 1 - customer 1
                if (carPark.CheckCustomer("Conference 3", 1) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 1 should not be assigned to any customer");
                }

                // Conference 3 - Space 2 - customer 1
                if (carPark.CheckCustomer("Conference 3", 2) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 2 should not be assigned to any customer");
                }

                // Conference 3 - Space 3 - no customer
                if (carPark.CheckCustomer("Conference 3", 3) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 3 should not be assigned to any customer");
                }

                // Conference 3 - Space 4 - no customer
                if (carPark.CheckCustomer("Conference 3", 4) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 4 should not be assigned to any customer");
                }

                // Conference 3 - Space 5 - no customer
                if (carPark.CheckCustomer("Conference 3", 5) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 5 should not be assigned to any customer");
                }

                // Conference 3 - Space 6 - no customer
                if (carPark.CheckCustomer("Conference 3", 6) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 6 should not be assigned to any customer");
                }

                // Conference 3 - Space 7 - no customer
                if (carPark.CheckCustomer("Conference 3", 7) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 7 should not be assigned to any customer");
                }

                // Conference 3 - Space 8 - no customer
                if (carPark.CheckCustomer("Conference 3", 8) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 8 should not be assigned to any customer");
                }

                // Conference 3 - Space 9 - no customer
                if (carPark.CheckCustomer("Conference 3", 9) != 0)
                {
                    Console.WriteLine("Test failed, Conference 3, space 9 should not be assigned to any customer");
                }

                // Failure case - conference not found
                try
                {
                    carPark.CheckCustomer("abc", 0);
                    Console.WriteLine("Test failed, returned value when conference does not exist");
                }
                catch (CarParkException ex)
                {
                    // Expect exception
                }

                // Failure case - index out of range negative
                try
                {
                    carPark.CheckCustomer("Conference 1", -1);
                    Console.WriteLine("Test failed, returned value for negative index");
                }
                catch (CarParkException ex)
                {
                    // Expect exception
                }

                // Failure case - index out of range boundary greater than
                try
                {
                    carPark.CheckCustomer("Conferece 1", 10);
                    Console.WriteLine("Test failed, returned value for boundary greater than index");
                }
                catch (CarParkException ex)
                {
                    // Expect exception
                }

                // Failure case - index out of range greater than
                try
                {
                    carPark.CheckCustomer("Conferece 1", 100000);
                    Console.WriteLine("Test failed, returned value for greater than index");
                }
                catch (CarParkException ex)
                {
                    // Expect exception
                }
                #endregion

                #region Trying to reserve too many spaces test

                // Reserve all of the free spaces in conference 2 (add 2 for number of premium spaces)
                for (var i = 4; i < MaxSize - CannotBeReserved + 2; i++)
                {
                    carPark.ReserveSpace("Conference 2", i, 1);
                }

                // Attempt to reserve one more space
                try
                {
                    carPark.ReserveSpace("Conference 2", 9, 1);
                    Console.WriteLine("Test failed, could reserve too many spaces");
                }
                catch (CarParkException ex)
                {
                    // Expect the exception
                }

                #endregion

                #region CancelReservations tests

                // Cancel the reservations
                carPark.CancelReservations();

                // Expect the space to still be reserved (less than 10 seconds ago)
                if (carPark.CheckCustomer("Conference 2", 4) != 1)
                {
                    Console.WriteLine("Test failed, space reservation was cancelled prematurely");
                }

                // Sleep for 11 seconds then retry
                Thread.Sleep(11000);
                carPark.CancelReservations();

                if (carPark.CheckCustomer("Conference 2", 4) == 1)
                {
                    Console.WriteLine("Test failed, space reservation was not cancelled");
                }

                #endregion
            }
            catch (CarParkException ex)
            {
                Console.WriteLine("Tests aborted");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}