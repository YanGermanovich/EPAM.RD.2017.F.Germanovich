using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceLibrary.CustomExceptions;
using MyServiceLibrary.Helpers;
using MyServiceLibrary.InterfacesAndAbstract;
using System.Net;
using System.Text;
using System.Net.Sockets;
using Serializer;
using MyServiceLibrary.CustomEventArgs;
using System.Threading.Tasks;
using MyServiceLibrary.CustomSection;
using System.Threading;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Users service
    /// </summary>
    [Serializable]
    public class UserService : MarshalByRefObject, IService<User>
    {
        private List<User> users = new List<User>();
        private bool disposed = false;
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        private readonly IIdGenerator idGenerator;

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
        public UserService(IIdGenerator idGenerator, ServiceRoles role = ServiceRoles.Slave)
        {
            this.Role = role;
            this.idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        /// <summary>
        /// Constructor with entering of service role
        /// </summary>
        /// <param name="role">Service role</param>
        public UserService(ServiceRoles role)
        {
            this.idGenerator = new CustomIdGenerator();
            this.Role = role;
        }

        #endregion

        #region Public Properties
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
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            AddHelper(users);
        }

        /// <summary>
        /// Method insert user into set
        /// </summary>
        /// <param name="user">User to add</param>
        public void Add(User user)
        {
            this.AddHelper(user);
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

            this.locker.EnterReadLock();
            try
            {
                return this.users.Where(predicate);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
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

            string fileName = GetValueFromConfig.Get("FileName");

            serializer.Serialize(fileName, this.users.ToArray());
        }

        /// <summary>
        /// Method dispose service
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Finilizer and Dispose

        /// <summary>
        /// Method disposes resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finilizer for service
        /// </summary>
        ~UserService()
        {
            Dispose(false);
        }

        #endregion

        #region Private Methods

        private void AddHelper(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                this.AddHelper(user);
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

            if (this.Role == ServiceRoles.Master)
            {
                user.Id = this.idGenerator.GetId();
            }
            this.locker.EnterWriteLock();
            try
            {
                this.users.Add(user);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        private void DeleteHelper(Predicate<User> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            this.locker.EnterWriteLock();
            try{
                this.users.RemoveAll(predicate);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }

        }
        #endregion
    }
}
