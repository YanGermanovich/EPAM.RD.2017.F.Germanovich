using MyServiceLibrary.CustomEventArgs;
using MyServiceLibrary.CustomExceptions;
using MyServiceLibrary.CustomSection;
using MyServiceLibrary.InterfacesAndAbstract;
using Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyServiceLibrary.CustomAttribute;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Class of communication using TCP service .
    /// </summary>
    [Serializable]
    public class UserTcpService :  TcpService<User>
    {
        public List<IPEndPoint> slaves;
        private IPEndPoint endPoint;
        private Task listner;
        private bool disposed = false;

        #region Public Methods

        /// <summary>
        /// Constructor with enteriing user service and its end point 
        /// </summary>
        /// <param name="service">User service</param>
        /// <param name="endPoint">Service endPoint</param>
        public UserTcpService(UserService service, IPEndPoint endPoint) : base(service)
        {
            this.endPoint = endPoint;
            slaves = new List<IPEndPoint>();
            CreateHelper();
        }

        /// <summary>
        /// Method adds set of some items to service.
        /// </summary>
        /// <param name="items">Set of some items</param>
        [Logging]
        public override void  Add(IEnumerable<User> items)
        {
            if (CheckPermission())
            {
                this.service.Add(items);
            }
            this.OnAddUser(new AddItemEventArgs<User>(items.ToList()));
        }

        /// <summary>
        /// Method adds some item to service.
        /// </summary>
        /// <param name="item">Item</param>
        [Logging]
        public override void Add(User item)
        {
            if (CheckPermission())
            {
                this.service.Add(item);
                this.OnAddUser(new AddItemEventArgs<User>(new List<User>() { item }));
            }
        }

        /// <summary>
        /// Method removes items which matches the predicate. 
        /// </summary>
        /// <param name="predicate">Predicate</param>
        [Logging]
        public override void Delete(Predicate<User> predicate)
        {
            if (CheckPermission())
            {
                OnDeleteUser(new DeleteItemEventArgs<User>(
                                                            this.service.Search(u => predicate(u))
                                                          ));
                this.service.Delete(predicate);

            }
        }

        /// <summary>
        /// Method dispose service
        /// </summary>
        public override void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Method returns items which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items which matches the predicate</returns>
        [Logging]
        public override List<User> Search(Func<User, bool> predicate)
        {
            return this.service.SearchDeferred(predicate).ToList();
        }

        /// <summary>
        /// Method deferred returns items which matches the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items which matches the predicate</returns>
        [Logging]
        public override IEnumerable<User> SearchDeferred(Func<User, bool> predicate)
        {
            return this.service.SearchDeferred(predicate);
        }

        /// <summary>
        /// Method serialize actual state using current serializer
        /// </summary>
        /// <param name="serializer"></param>
        [Logging]
        public override void SerializeState(ISerializerProvider<User[]> serializer)
        {
            if(CheckPermission())
            {
                this.service.SerializeState(serializer);
            }
        }

        #endregion

        #region Protected Method
        /// <summary>
        /// Method dispose all resources
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
                Master master = RegisterServicesConfig.GetConfig().Master;
                IPEndPoint masterEndPoint = (IPEndPoint)master[0];
                this.SendMessageFromSocket(masterEndPoint, Serialize(endPoint).GetAwaiter().GetResult());
                this.SendMessageFromSocket(endPoint, Encoding.UTF8.GetBytes("<End>"));
                disposed = true;
            }
        }
        #endregion

        #region Private Methods
        private void CreateHelper()
        {
            listner = Task.Run(() => Listen(endPoint));

            if (TryPermission())
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

        private bool TryPermission()
        {
            return this.service.Role == ServiceRoles.Master;
        }

        private bool CheckPermission()
        {
            if (!TryPermission())
            {
                throw new AccesPermissionException($"{this.service.Role} service tried get access as master");
            }

            return true;
        }

        private void OnAddUser(AddItemEventArgs<User> e)
        {
            NotifySlaves(Serialize(e).GetAwaiter().GetResult()).GetAwaiter().GetResult();
        }

        private void OnDeleteUser(DeleteItemEventArgs<User> e)
        {
            NotifySlaves(Serialize(e).GetAwaiter().GetResult()).GetAwaiter().GetResult();
        }

        private async Task NotifySlaves(byte[] data)
        {
            await Task.Run(() =>
            {
                foreach (var slaveEndPoint in slaves)
                    SendMessageFromSocket(slaveEndPoint, data);
            });
        }

        #region TCP communication
        private void SendMessageFromSocket(IPEndPoint ipEndPoint, byte[] data)
        {
            Socket sender = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(ipEndPoint);

            int bytesSent = sender.Send(data, data.Length, SocketFlags.Partial);

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
                if (Encoding.UTF8.GetString(bytes, 0, length) == "<End>")
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    break;
                }
                if (TryPermission())
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

        [Logging]
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
                    SendMessageFromSocket(slaveEndPoint, Serialize(new AddItemEventArgs<User>(this.service.Search(u => true))).GetAwaiter().GetResult());
                }

                return;
            }
        }

        private void UpdateUsers(byte[] data)
        {
            AddItemEventArgs<User> users = DeserializeClass<AddItemEventArgs<User>>(data).GetAwaiter().GetResult();
            if (users != null)
            {
                this.service.Add(users.UsersToAdd);
                return;
            }
            DeleteItemEventArgs<User> usersToDelete = DeserializeClass<DeleteItemEventArgs<User>>(data).GetAwaiter().GetResult();
            this.service.Delete((u) => usersToDelete.UsersToRemove.Contains(u));
        }

        #endregion

        #region Serialization

        private async Task<byte[]> Serialize<T>(T obj)
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

        #endregion
    }
}
