using System;
using System.Collections.Generic;
using MyServiceLibrary.Interfaces;
using System.Configuration;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Server of user service
    /// </summary>
    public class ServiceServer<T> : IServiceServer<IService<User>>
           where T : IService<User>
    {
        /// <summary>
        /// Default constructor. Default count of slaves is 5
        /// </summary>
        public ServiceServer()
        {
            this.Master = (T)Activator.CreateInstance(typeof(T), ServiceRoles.Master);
            this.CreateHelper();
        }

        /// <summary>
        /// Constructor with enering count of saves and id generator
        /// </summary>
        /// <param name="idGenerator">Identifier generator</param>
        public ServiceServer(Func<int> idGenerator)
        {
            if (idGenerator == null)
            {
                throw new ArgumentNullException(nameof(idGenerator));
            }

            this.Master = (T)Activator.CreateInstance(typeof(T), idGenerator, ServiceRoles.Master);
            this.CreateHelper();
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
    }
}
