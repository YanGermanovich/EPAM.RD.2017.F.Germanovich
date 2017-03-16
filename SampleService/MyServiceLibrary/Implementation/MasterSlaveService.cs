using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LoggerSingleton;
using MyServiceLibrary.InterfacesAndAbstract;
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
    public class MasterSlavesService<ItemType,TcpServiceType,ItemServiceType> : IMasterSlaveService<TcpService<ItemType>>, IDisposable
        where TcpServiceType : TcpService<ItemType>
        where ItemServiceType : IService<ItemType>
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

        private TcpService<ItemType> master;
        private IEnumerable<TcpService<ItemType>> slaves; 

        #endregion

        #region Public Methods

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public MasterSlavesService()
        {
            if (!TryPermission("slave"))
            {
                var master = (IPEndPoint)RegisterServicesConfig.GetConfig().Master[0];
                this.master = CreateTcpServiceInstanceInDomain(this.masterDomain, master, ServiceRoles.Master);
            }
            if (!TryPermission("master"))
            {
                this.CreateHelper();
            }
        }

        /// <summary>
        /// Constructor with entering id generator
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        public MasterSlavesService(IIdGenerator idGenerator)
        {
            if (!TryPermission("slave"))
            {
                if (idGenerator == null)
                {
                    NlogLogger.Logger.Fatal($"Value cannot be null. Parameter name: {nameof(idGenerator)}");
                    throw new ArgumentNullException(nameof(idGenerator));
                }
                var master = (IPEndPoint)RegisterServicesConfig.GetConfig().Master[0];
                this.master = CreateTcpServiceInstanceInDomain(this.masterDomain, master, ServiceRoles.Master);
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
        public MasterSlavesService(IIdGenerator idGenerator, ISerializerProvider<ItemType[]> serializerProvider) : this(idGenerator)
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
        public MasterSlavesService(ISerializerProvider<ItemType[]> serializerProvider) : this()
        {
            if (!TryPermission("slave"))
            {
                this.InitializeCollection(serializerProvider);
            }
        }

        #endregion

        

        /// <summary>
        /// Method unload all domains
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Public Fields 
        /// <summary>
        /// Property returns all slaves
        /// </summary>
        public TcpService<ItemType> Master
        {
            get
            {
                if (master == null)
                    throw new InvalidApplicationMode("Application mode is slave. To get access to mater change it to master or server");
                return master;
            }
        }

        /// <summary>
        /// Property returns master entity
        /// </summary>
        public IEnumerable<TcpService<ItemType>> Slaves
        {
            get
            {
                if (slaves == null)
                    throw new InvalidApplicationMode("Application mode is master. To get access to slaves change it to slaves or server");
                return slaves;
            }
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

                if (this.slaves != null)
                {

                    foreach (var service in this.slaves)
                    {
                        service.Dispose();
                    }
                }

                if (this.master != null)
                {
                    this.master.Dispose();
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

            List<TcpService<ItemType>> slaves = new List<TcpService<ItemType>>();
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
                    slaves.Add(CreateTcpServiceInstanceInDomain(slavesDomians.Last(), endPoint, ServiceRoles.Slave));
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
                slaves.Add(CreateTcpServiceInstanceInDomain(slavesDomians[0], (IPEndPoint)slavesConfig[0],ServiceRoles.Slave));
            }

            this.slaves = slaves;

        }

        private void InitializeCollection(ISerializerProvider<ItemType[]> serializerProvider)
        {

            string fileName = GetValueFromConfig.Get("FileName");

            ItemType[] users = serializerProvider.Deserialize(fileName, null);
            if (users != null)
            {
                foreach (ItemType us in users)
                {
                    this.master.Add(us);
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

        private TcpService<ItemType> CreateTcpServiceInstanceInDomain(AppDomain domain, IPEndPoint seviceEndPoint, params object[] serviceParams)
        {
            IService<ItemType> service = (IService<ItemType>)domain.CreateInstanceAndUnwrap(
                                                                           Assembly.GetExecutingAssembly().FullName,
                                                                           typeof(ItemServiceType).FullName,
                                                                           false,
                                                                           BindingFlags.Default,
                                                                           null,
                                                                           serviceParams,
                                                                           CultureInfo.CurrentCulture,
                                                                           null);
            return (TcpService<ItemType>)domain.CreateInstanceAndUnwrap(
                                                                        Assembly.GetExecutingAssembly().FullName,
                                                                        typeof(TcpServiceType).FullName,
                                                                        false,
                                                                        BindingFlags.Default,
                                                                        null,
                                                                        new object[] { service, seviceEndPoint },
                                                                        CultureInfo.CurrentCulture,
                                                                        null);
        }

        #endregion


        #region Finilizer
        /// <summary>
        /// Finilazire for server
        /// </summary>
        ~MasterSlavesService()
        {
            Dispose(false);
        }

        #endregion

    }
}
