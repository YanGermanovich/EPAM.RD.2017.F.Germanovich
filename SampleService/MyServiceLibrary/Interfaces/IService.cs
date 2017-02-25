using System;
using System.Collections.Generic;
using MyServiceLibrary.Implementation;

namespace MyServiceLibrary.Interfaces
{
    /// <summary>
    /// Generic interface. It provides functions of service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IService<T>
    {
        #region Property
        /// <summary>
        /// Property of service roles
        /// </summary>
        ServiceRoles Role { get; }

        #endregion

        #region Mehods
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

        /// <summary>
        /// Method serialize actual state using current serializer
        /// </summary>
        /// <param name="serializer"></param>
        void SerializeState(ISerializerProvider<T[]> serializer);

        #endregion
    }
}
