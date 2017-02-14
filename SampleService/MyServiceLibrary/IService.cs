using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary
{
    /// <summary>
    /// Generic interface. It provides functions of service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IService<T>
    {
        /// <summary>
        /// Method adds set of some items to service.
        /// </summary>
        /// <param name="items">Set of some items</param>
        void Add(IEnumerable<T> items);

        /// <summary>
        /// Method adds some item to service.
        /// </summary>
        /// <param name="item">Item</param>
        void Add(T item);

        /// <summary>
        /// Method removes items which matches the predicate. 
        /// </summary>
        /// <param name="predicate">Predicate</param>
        void Delete(Predicate<T> predicate);

        /// <summary>
        /// Method deferred returns items which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items which matches the predicate</returns>
        IEnumerable<T> SearchDeferred(Func<T, bool> predicate);

        /// <summary>
        /// Method returns items which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items which matches the predicate</returns>
        List<T> Search(Func<T, bool> predicate);
    }
}
