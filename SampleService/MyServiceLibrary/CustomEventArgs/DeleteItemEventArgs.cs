using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomEventArgs
{
    [Serializable]
    public class DeleteItemEventArgs<T>
    {
        private readonly List<T> removeUsers;

        public DeleteItemEventArgs(List<T> removeUsers)
        {
            this.removeUsers = removeUsers;
        }
        public List<T> UsersToRemove
        {
            get
            {
                return removeUsers;
            }
        }
    }
}
