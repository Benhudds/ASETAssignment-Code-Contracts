using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ASETAssignment.CarParkNamespace
{
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
}