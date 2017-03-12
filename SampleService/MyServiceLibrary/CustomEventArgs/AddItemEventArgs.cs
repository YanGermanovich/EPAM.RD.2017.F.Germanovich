using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomEventArgs
{

    [Serializable]
    public class AddItemEventArgs<T>
    {
        private readonly List<T> addUsers;

        public AddItemEventArgs(List<T> addUsers)
        {
            this.addUsers = addUsers;
        }
        public List<T> UsersToAdd
        {
            get
            {
                return addUsers;
            }
        }
    }

    
}
