using System;
using System.Collections.Generic;
using System.Linq;

namespace MyServiceLibrary
{
    /// <summary>
    /// Users service
    /// </summary>
    public class UserService : IService<User>
    {
        private List<User> users = new List<User>();

        private Func<int> idGenerator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserService()
        {
            int id = 0;
            this.idGenerator += () => { return id++; };
        }

        /// <summary>
        /// Constructor with entering of identifier generator 
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        public UserService(Func<int> idGenerator)
        {
            if (idGenerator == null)
            {
                throw new ArgumentNullException(nameof(idGenerator));
            }

            this.idGenerator = (Func<int>)idGenerator.Clone();
        }

        /// <summary>
        /// Method insert users into set
        /// </summary>
        /// <param name="users">Users to add</param>
        public void Add(IEnumerable<User> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            foreach (var user in users)
            {
                this.Add(user);
            }
        }

        /// <summary>
        /// Method insert user into set
        /// </summary>
        /// <param name="user">User to add</param>
        public void Add(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var u = new User();
            if (user.Equals(u))
            {
                throw new DefaultUserException(nameof(user));
            }

            if (this.users.Contains(user))
            {
                throw new ExistUserException();
            }

            user.Id = this.idGenerator();
            this.users.Add(user);
        }

        /// <summary>
        /// Method remove all user which matches the predicate.  
        /// </summary>
        /// <param name="predicate">Predicate</param>
        public void Delete(Predicate<User> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            this.users.RemoveAll(predicate);
        }

        /// <summary>
        /// Method deferred returns users which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Users which matches the predicate</returns>
        public IEnumerable<User> SearchDeferred(Func<User, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return this.users.Where(predicate);
        }

        /// <summary>
        /// Method returns users which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Users which matches the predicate</returns>
        public List<User> Search(Func<User, bool> predicate)
        {
            return this.SearchDeferred(predicate).ToList();
        }
    }
}
