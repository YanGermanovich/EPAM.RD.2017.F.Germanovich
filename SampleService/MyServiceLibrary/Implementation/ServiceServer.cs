using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LoggerSingleton;
using MyServiceLibrary.Interfaces;
using System.Reflection;
using System.Globalization;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Server of user service
    /// </summary>
    public class ServiceServer<T> : IServiceServer<IService<User>>, IDisposable
           where T : IService<User>
    {
        #region Private Fields
        private bool disposed = false;

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
        public ServiceServer()
        {
            this.Master = (T)this.masterDomain.CreateInstanceAndUnwrap(
                                                                        Assembly.GetExecutingAssembly().FullName,
                                                                        typeof(User).FullName,
                                                                        false,
                                                                        BindingFlags.Default,
                                                                        null,
                                                                        new object[] { ServiceRoles.Master },
                                                                        CultureInfo.CurrentCulture,
                                                                        null);
            this.CreateHelper();
        }

        /// <summary>
        /// Constructor with entering\ id generator
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        public ServiceServer(Func<int> idGenerator)
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
                                                                        new object[] { idGenerator, ServiceRoles.Master },
                                                                        CultureInfo.CurrentCulture,
                                                                        null);
            this.CreateHelper();
        }

        /// <summary>
        /// Constructor with entering serializer and id generator
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        /// <param name="serializerProvider">Serialization provider</param>
        public ServiceServer(Func<int> idGenerator, ISerializerProvider<User[]> serializerProvider) : this(idGenerator)
        {
            this.InitializeCollection(serializerProvider);
        }

        /// <summary>
        /// Constructor with entering serializer
        /// </summary>
        /// <param name="serializerProvider">Serialization provider</param>
        public ServiceServer(ISerializerProvider<User[]> serializerProvider) : this()
        {
            this.InitializeCollection(serializerProvider);
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
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("SlavesCount"))
            {
                NlogLogger.Logger.Error($"No slaves count in App.config");
                throw new ArgumentNullException("Please add slaves count into App.config");
            }

            int count = Convert.ToInt32(ConfigurationManager.AppSettings["SlavesCount"]);
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            List<IService<User>> slaves = new List<IService<User>>();
            for (int i = 0; i < count; i++)
            {
                slaves.Add((T)this.masterDomain.CreateInstanceAndUnwrap(
                                                                        Assembly.GetExecutingAssembly().FullName,
                                                                        typeof(T).FullName,
                                                                        false,
                                                                        BindingFlags.Default,
                                                                        null,
                                                                        new object[] { this.Master},
                                                                        CultureInfo.CurrentCulture,
                                                                        null));
            }

            this.Slaves = slaves;
        }

        private void InitializeCollection(ISerializerProvider<User[]> serializerProvider)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("FileName"))
            {
                NlogLogger.Logger.Error("No file name");
                throw new ArgumentNullException("Please add file name into App.config");
            }

            string fileName = ConfigurationManager.AppSettings["FileName"].ToString();

            if (fileName == string.Empty)
            {
                NlogLogger.Logger.Error("Empry file name");
                throw new ArgumentException("File name must not be empty!");
            }

            User[] users = serializerProvider.Deserialize(fileName, null);
            if (users != null)
            {
                foreach (User us in users)
                {
                    this.Master.Add(us);
                }
            }
        }
        #endregion

        #region Finilizer
        /// <summary>
        /// Finilazire for server
        /// </summary>
        ~ServiceServer()
        {
            Dispose(false);
        }

        #endregion

    }
}
