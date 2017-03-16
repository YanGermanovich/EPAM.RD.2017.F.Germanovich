using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.InterfacesAndAbstract
{
    /// <summary>
    /// Generic interface. It provides functions of service which communicate using TCP.
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    /// <typeparam name="S">Service of items</typeparam>
    [Serializable]
    public abstract class TcpService<T> : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Service of items
        /// </summary>
        protected readonly IService<T> service;

        /// <summary>
        /// Constructor with enteriing service with items of T type
        /// </summary>
        /// <param name="service">Item service</param>
        public TcpService(IService<T> service)
        {
            this.service = service;
        }

        /// <summary>
        /// Method adds set of some items to service.
        /// </summary>
        /// <param name="items">Set of some items</param>
        public abstract void Add(IEnumerable<T> items);

        /// <summary>
        /// Method adds some item to service.
        /// </summary>
        /// <param name="item">Item</param>
        public abstract void Add(T item);

        /// <summary>
        /// Method removes items which matches the predicate. 
        /// </summary>
        /// <param name="predicate">Predicate</param>
        public abstract void Delete(Predicate<T> predicate);

        /// <summary>
        /// Method deferred returns items which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items which matches the predicate</returns>
        public abstract IEnumerable<T> SearchDeferred(Func<T, bool> predicate);

        /// <summary>
        /// Method returns items which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items which matches the predicate</returns>
        public abstract List<T> Search(Func<T, bool> predicate);

        /// <summary>
        /// Method serialize actual state using current serializer
        /// </summary>
        /// <param name="serializer"></param>
        public abstract void SerializeState(ISerializerProvider<T[]> serializer);

        /// <summary>
        /// Method disposes all resources
        /// </summary>
        public abstract void Dispose();
        
    }
}
