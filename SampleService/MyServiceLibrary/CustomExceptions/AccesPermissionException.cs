using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomExceptions
{
    /// <summary>
    /// Custom exception. Thrown when try to add or delete user in slave service.
    /// </summary>
    public class AccesPermissionException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AccesPermissionException()
        {
        }

        /// <summary>
        /// Constructor with exception message
        /// </summary>
        /// <param name="message">Exception message</param>
        public AccesPermissionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with exception message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public AccesPermissionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
