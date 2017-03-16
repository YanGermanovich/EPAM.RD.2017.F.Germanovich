using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.InterfacesAndAbstract
{
    /// <summary>
    /// Interface provide function of id generator
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Method returns new unique id
        /// </summary>
        /// <returns>Id</returns>
        int GetId();
        
        /// <summary>
        /// Id start value
        /// </summary>
        int NextValue { get; set; }
    }
}
