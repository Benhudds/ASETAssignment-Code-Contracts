using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASETAssignment
{
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
}
