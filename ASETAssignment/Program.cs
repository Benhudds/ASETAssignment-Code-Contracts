using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASETAssignment.CarParkNamespace;

namespace ASETAssignment
{


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
