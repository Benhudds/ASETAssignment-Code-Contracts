
using System.Diagnostics.Contracts;

namespace ASETAssignment
{
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
            var other = (Conference) obj;

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
}