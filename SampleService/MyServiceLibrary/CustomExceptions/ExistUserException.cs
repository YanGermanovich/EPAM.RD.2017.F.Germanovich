using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary
{
    /// <summary>
    /// Custom exception. Thrown when try to add user that have already added 
    /// </summary>
    public class ExistUserException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExistUserException()
        {
        }

        /// <summary>
        /// Constructor with exception message
        /// </summary>
        /// <param name="message">Exception message</param>
        public ExistUserException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor with exception message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public ExistUserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
