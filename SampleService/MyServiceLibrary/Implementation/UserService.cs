using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LoggerSingleton;
using MyServiceLibrary.CustomExceptions;
using MyServiceLibrary.Helpers;
using MyServiceLibrary.Interfaces;
using System.Net;
using System.Text;
using System.Net.Sockets;
using Serializer;
using MyServiceLibrary.CustomEventArgs;
using System.Threading.Tasks;
using MyServiceLibrary.CustomSection;
using System.Configuration;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Users service
    /// </summary>
    public class UserService : MarshalByRefObject, IService<User>
    {
        private List<User> users = new List<User>();

        private List<IPEndPoint> slaves;

        private IPEndPoint endPoint;

        private Task listner;

        private bool disposed = false;

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
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(idGenerator)}");
                throw new ArgumentNullException(nameof(idGenerator));
            }

            this.Role = role;
            CreateHelper();
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
            CreateHelper();
            
        }

        /// <summary>
        /// Constructor with entering identifier generator, end point and service role
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        /// <param name="role">Service role</param>
        /// <param name="endPoint">End point of service</param>
        public UserService(Func<int> idGenerator, IPEndPoint endPoint, ServiceRoles role = ServiceRoles.Slave)
        {
            if (idGenerator == null)
            {
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(idGenerator)}");
                throw new ArgumentNullException(nameof(idGenerator));
            }

            if(endPoint == null)
            {
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(endPoint)}");
                throw new ArgumentNullException(nameof(endPoint));
            }

            this.endPoint = endPoint;
            this.Role = role;
            CreateHelper();
            this.IdGenerator = (Func<int>)idGenerator.Clone();
        }

        /// <summary>
        /// Constructor with entering service role and end point 
        /// </summary>
        /// <param name="role">Service role</param>
        /// <param name="endPoint">End point of service</param>
        public UserService(ServiceRoles role, IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(endPoint)}");
                throw new ArgumentNullException(nameof(endPoint));
            }

            this.endPoint = endPoint;
            this.InitializeGenerator();
            this.Role = role;
            CreateHelper();

        }
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
            if (this.CheckPermission(ServiceRoles.Master))
            {
                throw new AccesPermissionException();
            }

            if (users == null)
            {
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(users)}");
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
            if (this.CheckPermission(ServiceRoles.Master))
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
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(predicate)}");
                throw new ArgumentNullException(nameof(predicate));
            }

            NlogLogger.Logger.Info($"Users was searched by predicate ");

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
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(serializer)}");
                throw new ArgumentNullException(nameof(serializer));
            }

            string fileName = GetValueFromConfig.Get("FileName");


            if (this.CheckPermission(ServiceRoles.Master))
            {
                throw new AccesPermissionException();
            }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
                Master master = RegisterServicesConfig.GetConfig().Master;
                IPEndPoint masterEndPoint = (IPEndPoint)master[0];
                this.SendMessageFromSocket(masterEndPoint, Serialize(endPoint).GetAwaiter().GetResult());
                this.SendMessageFromSocket(endPoint, Encoding.UTF8.GetBytes("<End>"));


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
        private void InitializeGenerator()
        {
            int id = 0;
            this.IdGenerator += () => { return id++; };
        }

        private void CreateHelper()
        {
            listner = Task.Run(() => Listen(endPoint));

            if (TryPermission(ServiceRoles.Master))
            {
                slaves = new List<IPEndPoint>();
            }
            else
            {
                Master master = RegisterServicesConfig.GetConfig().Master;
                IPEndPoint masterEndPoint = (IPEndPoint)master[0];
                this.SendMessageFromSocket(masterEndPoint, Serialize(endPoint).GetAwaiter().GetResult());
            }

           

        }

        private bool TryPermission(ServiceRoles role)
        {
            return this.Role == role;
        }

        private bool CheckPermission(ServiceRoles role)
        {
            if (TryPermission(role))
            {
                return false;
            }

            NlogLogger.Logger.Warn($"{this.Role} service tried get access as {role}");
            return true;
        }

        private void OnAddUser(AddItemEventArgs<User> e)
        {
            NotifySlaves(Serialize(e).GetAwaiter().GetResult()).GetAwaiter().GetResult();
            NlogLogger.Logger.Info($"Some users added");
        }

        private void OnDeleteUser(DeleteItemEventArgs<User> e)
        {
            NotifySlaves(Serialize(e).GetAwaiter().GetResult()).GetAwaiter().GetResult();
            NlogLogger.Logger.Info("Users deleted by predicate");
        }

        private async Task NotifySlaves(byte[] data)
        {
            await Task.Run(() =>
            {
                foreach (var slaveEndPoint in slaves)
                    SendMessageFromSocket(slaveEndPoint, data);
            });
        }

        private void AddHelper(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                this.AddHelper(user);
            }

            if (TryPermission(ServiceRoles.Master))
            {
                this.OnAddUser(new AddItemEventArgs<User>(users.ToList()));
            }
        }

        private void AddHelper(User user)
        {
            if (user == null)
            {
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(user)}");
                throw new ArgumentNullException(nameof(user));
            }

            if (CheckDefaultValues.Check(user))
            {
                NlogLogger.Logger.Fatal($"User values cannot be default. Usrt : {user.ToString()}");
                throw new DefaultUserException(nameof(user));
            }

            if (this.users.Contains(user))
            {
                NlogLogger.Logger.Fatal($"Exist user error. User {user.ToString()} is already exist");
                throw new ExistUserException();
            }

            if (TryPermission(ServiceRoles.Master))
            {
                user.Id = this.IdGenerator();
            }

            this.users.Add(user);
        }

        private void DeleteHelper(Predicate<User> predicate)
        {
            if (predicate == null)
            {
                NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(predicate)}");
                throw new ArgumentNullException(nameof(predicate));
            }


            if (TryPermission(ServiceRoles.Master))
            {
                this.OnDeleteUser(new DeleteItemEventArgs<User>(users.FindAll(predicate).ToList()));
            }
            this.users.RemoveAll(predicate);
        }

        private void SendMessageFromSocket(IPEndPoint ipEndPoint, byte[] data)
        {
            Socket sender = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(ipEndPoint);

            int bytesSent = sender.Send(data,data.Length,SocketFlags.Partial);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void Listen(IPEndPoint ipEndPoint)
        {

            Socket sListener = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sListener.Bind(ipEndPoint);
            sListener.Listen(10);
            while (true)
            {
                Socket handler = sListener.Accept();
                byte[] bytes = new byte[2000];
                int length = handler.Receive(bytes);
                if (Encoding.UTF8.GetString(bytes, 0,length) == "<End>")
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    break;
                }
                if (TryPermission(ServiceRoles.Master))
                {
                    AddOrRemoveSlave(bytes);
                }
                else
                {
                    UpdateUsers(bytes);
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

        }


        private void AddOrRemoveSlave(byte[] data)
        {
            IPEndPoint slaveEndPoint = DeserializeClass<IPEndPoint>(data).GetAwaiter().GetResult();
            if (slaveEndPoint != null)
            {
                if (slaves.Contains(slaveEndPoint))
                {
                    slaves.Remove(slaveEndPoint);
                }
                else
                {
                    slaves.Add(slaveEndPoint);
                    SendMessageFromSocket(slaveEndPoint, Serialize(new AddItemEventArgs<User>(users)).GetAwaiter().GetResult());
                }

                return;
            }
        }

        private void UpdateUsers(byte[] data)
        {
            AddItemEventArgs<User> users = DeserializeClass<AddItemEventArgs<User>>(data).GetAwaiter().GetResult();
            if (users != null)
            {
                AddHelper(users.UsersToAdd);
                return;
            }
            DeleteItemEventArgs<User> usersToDelete = DeserializeClass<DeleteItemEventArgs<User>>(data).GetAwaiter().GetResult();
            DeleteHelper((u) => usersToDelete.UsersToRemove.Contains(u));
        }

        private async Task<byte[]> Serialize<T> (T obj)
        {
            return await Task.Run(() => CustomSerializer.Serialize(obj));
        }
        private async Task<T> DeserializeClass<T>(byte[] data)
            where T : class
        {
            return await Task.Run(() => CustomSerializer.DeserializeClass<T>(data));
        }

        private async Task<T> DeserializeStruct<T>(byte[] data)
            where T : struct
        {
            return await Task.Run(() => CustomSerializer.DeserializeStruct<T>(data));
        }

        #endregion
    }
}
