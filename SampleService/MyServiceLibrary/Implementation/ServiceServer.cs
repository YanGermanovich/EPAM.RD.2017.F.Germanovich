using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LoggerSingleton;
using MyServiceLibrary.Interfaces;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Server of user service
    /// </summary>
    public class ServiceServer<T> : IServiceServer<IService<User>>
           where T : IService<User>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ServiceServer()
        {
            this.Master = (T)Activator.CreateInstance(typeof(T), ServiceRoles.Master);

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

            this.Master = (T)Activator.CreateInstance(typeof(T), idGenerator, ServiceRoles.Master);
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

        /// <summary>
        /// Property returns all slaves
        /// </summary>
        public IService<User> Master { get; private set; }

        /// <summary>
        /// Property returns master entity
        /// </summary>
        public IEnumerable<IService<User>> Slaves { get; private set; }

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
                slaves.Add(new UserService(this.Master as UserService));
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
    }
}
