using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomExceptions
{
    /// <summary>
    /// Custom exception. Thrown when try to get access 
    /// to application functions of other application working mode
    /// </summary>
    public class InvalidApplicationMode : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidApplicationMode()
        {
        }

        /// <summary>
        /// Constructor with exception message
        /// </summary>
        /// <param name="message">Exception message</param>
        public InvalidApplicationMode(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with exception message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public InvalidApplicationMode(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
