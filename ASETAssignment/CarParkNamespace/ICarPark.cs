using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ASETAssignment.CarParkNamespace
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
}
