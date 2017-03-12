using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomEventArgs
{
    public class DeleteItemEventArgs<T>
    {
        private readonly Predicate<T> removeUsers;

        public DeleteItemEventArgs(Predicate<T> removeUsers)
        {
            this.removeUsers = removeUsers;
        }
        public Predicate<T> UsersToRemove
        {
            get
            {
                return removeUsers;
            }
        }
    }
}
