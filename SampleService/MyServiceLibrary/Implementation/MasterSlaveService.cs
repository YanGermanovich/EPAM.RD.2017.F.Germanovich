using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LoggerSingleton;
using MyServiceLibrary.Interfaces;
using System.Reflection;
using System.Net;
using System.Globalization;
using MyServiceLibrary.Helpers;
using MyServiceLibrary.CustomExceptions;
using MyServiceLibrary.CustomSection;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Server of user service
    /// </summary>
    public class MasterSlaveService<T> : IMasterSlaveService<IService<User>>, IDisposable
           where T : IService<User>
    {
        #region Private Fields
        private bool disposed = false;

        private string serviceMode = GetValueFromConfig.Get("AppMode");

        private AppDomain masterDomain = AppDomain.CreateDomain(
                                                                "MyDomianForMaster",
                                                                 null,
                                                                 new AppDomainSetup
                                                                 {
                                                                     ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                                                                     PrivateBinPath = Path.Combine(
                                                                                                   AppDomain.CurrentDomain.BaseDirectory,
                                                                                                   "MyDomianForMaster")
                                                                 });

        private List<AppDomain> slavesDomians = new List<AppDomain>();

        #endregion

        #region Public Methods

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public MasterSlaveService()
        {
            var master = (IPEndPoint)RegisterServicesConfig.GetConfig().Master[0];
            if (!TryPermission("slave"))
            {
                this.Master = (T)this.masterDomain.CreateInstanceAndUnwrap(
                                                                            Assembly.GetExecutingAssembly().FullName,
                                                                            typeof(UserService).FullName,
                                                                            false,
                                                                            BindingFlags.Default,
                                                                            null,
                                                                            new object[] { ServiceRoles.Master, master },
                                                                            CultureInfo.CurrentCulture,
                                                                            null);
            }
            if (!TryPermission("master"))
            {
                this.CreateHelper();
            }
        }

        /// <summary>
        /// Constructor with entering\ id generator
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        public MasterSlaveService(Func<int> idGenerator)
        {
            if (!TryPermission("slave"))
            {
                if (idGenerator == null)
                {
                    NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(idGenerator)}");
                    throw new ArgumentNullException(nameof(idGenerator));
                }

                this.Master = (T)this.masterDomain.CreateInstanceAndUnwrap(
                                                                            Assembly.GetExecutingAssembly().FullName,
                                                                            typeof(T).FullName,
                                                                            false,
                                                                            BindingFlags.Default,
                                                                            null,
                                                                            new object[] { idGenerator, (IPEndPoint)RegisterServicesConfig.GetConfig().Master[0], ServiceRoles.Master },
                                                                            CultureInfo.CurrentCulture,
                                                                            null);
            }
            if (!TryPermission("master"))
            {
                this.CreateHelper();
            }
        }

        /// <summary>
        /// Constructor with entering serializer and id generator
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        /// <param name="serializerProvider">Serialization provider</param>
        public MasterSlaveService(Func<int> idGenerator, ISerializerProvider<User[]> serializerProvider) : this(idGenerator)
        {
            if (!TryPermission("slave"))
            {
                this.InitializeCollection(serializerProvider);
            }
        }

        /// <summary>
        /// Constructor with entering serializer
        /// </summary>
        /// <param name="serializerProvider">Serialization provider</param>
        public MasterSlaveService(ISerializerProvider<User[]> serializerProvider) : this()
        {
            if (!TryPermission("slave"))
            {
                this.InitializeCollection(serializerProvider);
            }
        }

        #endregion

        /// <summary>
        /// Property returns all slaves
        /// </summary>
        public IService<User> Master { get; private set; }

        /// <summary>
        /// Property returns master entity
        /// </summary>
        public IEnumerable<IService<User>> Slaves { get; private set; }

        /// <summary>
        /// Method unload all domains
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Protected Method

        /// <summary>
        /// Method unload all domins and suppress finalize 
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

                if (this.Slaves != null)
                {

                    foreach (var service in this.Slaves)
                    {
                        service.Dispose();
                    }
                }

                if (this.Master != null)
                {
                    this.Master.Dispose();
                }

                foreach (AppDomain slaveDom in this.slavesDomians)
                {
                    AppDomain.Unload(slaveDom);
                }

                AppDomain.Unload(masterDomain);

                disposed = true;
            }
        }
        #endregion

        #region Private Methods
        private void CreateHelper()
        {

            Slaves slavesConfig = RegisterServicesConfig.GetConfig().Slaves;

            List<IService<User>> slaves = new List<IService<User>>();
            if (serviceMode == "server")
            {

                foreach (Service service in slavesConfig)
                {
                    slavesDomians.Add(AppDomain.CreateDomain(
                                                                $"MyDomianForSlave{slavesDomians.Count}",
                                                                 null,
                                                                 new AppDomainSetup
                                                                 {
                                                                     ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                                                                     PrivateBinPath = Path.Combine(
                                                                                                   AppDomain.CurrentDomain.BaseDirectory,
                                                                                                   $"MyDomianForSlave{slavesDomians.Count}")
                                                                 }));
                    var master = (IPEndPoint)RegisterServicesConfig.GetConfig().Master[0];
                    IPEndPoint endPoint = (IPEndPoint)service;
                    slaves.Add((T)this.slavesDomians.Last().CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                                                                            typeof(UserService).FullName,
                                                                            false,
                                                                            BindingFlags.Default,
                                                                            null,
                                                                            new object[] { ServiceRoles.Slave, endPoint },
                                                                            CultureInfo.CurrentCulture,
                                                                            null));
                }

            }
            else if(serviceMode =="slave")
            {
                slavesDomians.Add(AppDomain.CreateDomain(
                                                                $"MyDomianForSlave{slavesDomians.Count}",
                                                                 null,
                                                                 new AppDomainSetup
                                                                 {
                                                                     ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                                                                     PrivateBinPath = Path.Combine(
                                                                                                   AppDomain.CurrentDomain.BaseDirectory,
                                                                                                   $"MyDomianForSlave{slavesDomians.Count}")
                                                                 }));
                slaves.Add((T)this.slavesDomians[0].CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                                                                            typeof(UserService).FullName,
                                                                            false,
                                                                            BindingFlags.Default,
                                                                            null,
                                                                            new object[] { ServiceRoles.Slave, (IPEndPoint)slavesConfig[0] },
                                                                            CultureInfo.CurrentCulture,
                                                                            null));
            }

            this.Slaves = slaves;

        }

        private void InitializeCollection(ISerializerProvider<User[]> serializerProvider)
        {

            string fileName = GetValueFromConfig.Get("FileName");

            User[] users = serializerProvider.Deserialize(fileName, null);
            if (users != null)
            {
                foreach (User us in users)
                {
                    this.Master.Add(us);
                }
            }
        }

        private bool TryPermission(string mode)
        {
            return (mode == this.serviceMode);
        }

        private bool GetPermission(string mode)
        {
            if (TryPermission(mode))
            {
                return true;
            }
            throw new AccesPermissionException();
        }


        #endregion


        #region Finilizer
        /// <summary>
        /// Finilazire for server
        /// </summary>
        ~MasterSlaveService()
        {
            Dispose(false);
        }

        #endregion

    }
}
