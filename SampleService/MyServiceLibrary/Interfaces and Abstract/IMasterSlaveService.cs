using System.Collections.Generic;

namespace MyServiceLibrary.InterfacesAndAbstract
{
    /// <summary>
    /// Interface provide function of server 
    /// </summary>
    public interface IMasterSlaveService<T>
    {
        /// <summary>
        /// Property returns all slaves
        /// </summary>
        IEnumerable<T> Slaves { get; }

        /// <summary>
        /// Property returns master entity
        /// </summary>
        T Master { get; }
    }
}
