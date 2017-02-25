using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceLibrary.CustomExceptions;
using MyServiceLibrary.EventArguments;
using MyServiceLibrary.Helpers;
using MyServiceLibrary.Interfaces;
using System.Configuration;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Users service
    /// </summary>
    public class UserService : IService<User>
    {
        private List<User> users = new List<User>();

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserService() : this(ServiceRoles.Slave)
        {
        }

        /// <summary>
        /// Constructor with entering of identifier generator and service role
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        /// <param name="role">Service role</param>
        public UserService(Func<int> idGenerator, ServiceRoles role = ServiceRoles.Slave)
        {
            if (idGenerator == null)
            {
                throw new ArgumentNullException(nameof(idGenerator));
            }

            this.Role = role;
            this.IdGenerator = (Func<int>)idGenerator.Clone();
        }

        /// <summary>
        /// Constructor with entering of service role
        /// </summary>
        /// <param name="role">Service role</param>
        public UserService(ServiceRoles role)
        {
            this.InitializeGenerator();
            this.Role = role;
        }

        /// <summary>
        /// Constructor with entering master service
        /// </summary>
        /// <param name="master">Master service</param>
        public UserService(UserService master)
        {
            if (master == null)
            {
                throw new ArgumentNullException(nameof(master));
            }

            if (!master.CheckPermission())
            {
                throw new AccesPermissionException();
            }

            master.AddUser += this.AddItems;
            master.DeleteUser += this.DeleteItems;
            this.IdGenerator = (Func<int>)master.IdGenerator.Clone();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Add user event
        /// </summary>
        public event EventHandler<AddItemEventArgs<User>> AddUser = delegate { };

        /// <summary>
        /// Delete user event
        /// </summary>
        public event EventHandler<DeleteItemEventArgs<User>> DeleteUser = delegate { };

        #endregion

        #region Public Properties

        /// <summary>
        /// Generator of user id
        /// </summary>
        public Func<int> IdGenerator { get; private set; }

        /// <summary>
        /// Property of service roles
        /// </summary>
        public ServiceRoles Role { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method insert users into set
        /// </summary>
        /// <param name="users">Users to add</param>
        public void Add(IEnumerable<User> users)
        {
            if (!this.CheckPermission())
            {
                throw new AccesPermissionException();
            }

            if (this.users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }
        }

        /// <summary>
        /// Method insert user into set
        /// </summary>
        /// <param name="user">User to add</param>
        public void Add(User user)
        {
            if (!this.CheckPermission())
            {
                throw new AccesPermissionException();
            }

            this.AddHelper(new User[] { user });
        }

        /// <summary>
        /// Method remove all user which matches the predicate.  
        /// </summary>
        /// <param name="predicate">Predicate</param>
        public void Delete(Predicate<User> predicate)
        {
            this.DeleteHelper(predicate);
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

        /// <summary>
        /// Method serialize actual state using current serializer
        /// </summary>
        /// <param name="serializer"></param>
        public void SerializeState(ISerializerProvider<User[]> serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            string fileName = ConfigurationManager.AppSettings["FileName"].ToString(); 

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileName == string.Empty)
            {
                throw new ArgumentException("File name must not be empty!");
            }

            if (!this.CheckPermission())
            {
                throw new AccesPermissionException();
            }

            serializer.Serialize(fileName, this.users.ToArray());
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Method is called when new user was added
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnAddUser(AddItemEventArgs<User> e)
        {
            if (this.AddUser != null)
            {
                this.AddUser(this, e);
            }
        }

        /// <summary>
        /// Method is called when new user was removed
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDeleteUser(DeleteItemEventArgs<User> e)
        {
            if (this.DeleteUser != null)
            {
                this.DeleteUser(this, e);
            }
        }

        #endregion

        #region Private Methods

        private void InitializeGenerator()
        {
            int id = 0;
            this.IdGenerator += () => { return id++; };
        }

        private bool CheckPermission()
        {
            return this.Role == ServiceRoles.Master;
        }

        /// <summary>
        /// Method is called when master adds users
        /// </summary>
        /// <param name="sender">Master</param>
        /// <param name="e">Event arguments</param>
        private void AddItems(object sender, AddItemEventArgs<User> e)
        {
            foreach (var user in e.ItemsToAdd)
            {
                this.AddHelper(user);
            }
        }

        /// <summary>
        /// Method is called when master deletes users
        /// </summary>
        /// <param name="sender">Master</param>
        /// <param name="e">Event arguments</param>
        private void DeleteItems(object sender, DeleteItemEventArgs<User> e)
        {
            this.DeleteHelper(e.Predicate);
        }

        private void AddHelper(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                this.AddHelper(user);
            }

            if (this.CheckPermission())
            {
                this.OnAddUser(new AddItemEventArgs<User>(users));
            }
        }

        private void AddHelper(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (CheckDefaultValues.Check(user))
            {
                throw new DefaultUserException(nameof(user));
            }

            if (this.users.Contains(user))
            {
                throw new ExistUserException();
            }

            user.Id = this.IdGenerator();
            this.users.Add(user);
        }

        private void DeleteHelper(Predicate<User> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            this.users.RemoveAll(predicate);

            if (this.CheckPermission())
            {
                this.OnDeleteUser(new DeleteItemEventArgs<User>(predicate));
            }
        }
        #endregion
    }
}
