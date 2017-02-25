using System;
using System.Collections.Generic;

namespace MyServiceLibrary.EventArguments
{
    /// <summary>
    /// Arguments of add item event
    /// </summary>
    public class AddItemEventArgs<T> : EventArgs
    {
        private readonly IEnumerable<T> itemsToAdd;

        /// <summary>
        /// Constructor with entering items to add
        /// </summary>
        /// <param name="itemsToAdd">items to add </param>
        public AddItemEventArgs(IEnumerable<T> itemsToAdd)
        {
            this.itemsToAdd = itemsToAdd;
        }

        /// <summary>
        /// Property of item to add 
        /// </summary>
        public IEnumerable<T> ItemsToAdd
        {
            get { return this.itemsToAdd; }
        }
    }
}
