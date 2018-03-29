using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ASETAssignment.CarParkNamespace
{
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
}
