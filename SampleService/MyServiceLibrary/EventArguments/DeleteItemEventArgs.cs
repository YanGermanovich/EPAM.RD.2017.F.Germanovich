﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.EventArguments
{
    /// <summary>
    /// Arguments of delete item event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeleteItemEventArgs<T> : EventArgs
    {
        private readonly Expression<Predicate<T>> predicate;

        /// <summary>
        /// Constructor with entering predicate for deleting
        /// </summary>
        /// <param name="predicate">predicate for deleting</param>
        public DeleteItemEventArgs(Expression<Predicate<T>> predicate)
        {
            this.predicate = predicate;
        }

        /// <summary>
        /// Property of predicate for deleting
        /// </summary>
        public Expression<Predicate<T>> Predicate
        {
            get { return this.predicate; }
        }
    }
}
