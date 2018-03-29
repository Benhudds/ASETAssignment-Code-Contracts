using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASETAssignment
{
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
            var other = (Space) obj;

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
}
