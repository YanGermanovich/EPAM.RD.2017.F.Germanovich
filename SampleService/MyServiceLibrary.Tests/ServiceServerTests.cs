using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyServiceLibrary.Implementation;
using MyServiceLibrary.CustomSection;

namespace MyServiceLibrary.Tests
{
    [TestClass]
    public class ServiceServerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Create_WhithNegativeCount_ExceptionThrown()
        {
            ConfigurationManager.AppSettings.Set("SlavesCount", "-1");
            try
            {
                var server = new MasterSlaveService<UserService>();
            }
            catch (ArgumentOutOfRangeException)
            {
                ConfigurationManager.AppSettings.Set("SlavesCount", "3");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_WhithNullGenerator_ExceptionThrown()
        {
            Func<int> idGen = null;
            var server = new MasterSlaveService<UserService>(idGen);
        }

        [TestMethod]
        public void Add()
        {
            var slaves = RegisterServicesConfig.GetConfig().Master;
            var server = new MasterSlaveService<UserService>();
            var us = new User()
            {
                DateOfBirth = new DateTime(1998,7,4),
                FirstName = "Ivan",
                LastName = "Ivanov"
            };

            server.Master.Add(us);

            foreach(var slave in server.Slaves)
            {
                var users = slave.Search((u) =>
                {
                    User user = new User()
                    {
                        DateOfBirth = new DateTime(1998, 7, 4),
                        FirstName = "Ivan",
                        LastName = "Ivanov"
                    };
                    return u.Equals(user);
                });
                if (users.Count != 1)
                    Assert.Fail();
            }
            //Assert.IsTrue(slaves.All(s => s.Search((u) => u.Equals(us)).Count == 1));
        }

        [TestMethod]
        public void Delete()
        {
            var server = new MasterSlaveService<UserService>();
            var us = new User()
            {
                DateOfBirth = new DateTime(1998, 7, 4),
                FirstName = "Ivan",
                LastName = "Ivanov"
            };

            var us1 = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Pavel",
                LastName = "Ivanov"
            };

            server.Master.Add(us);
            server.Master.Add(us1);
            server.Master.Delete(u => {
                var user = new User()
                {
                    DateOfBirth = new DateTime(1998, 7, 4),
                    FirstName = "Ivan",
                    LastName = "Ivanov"
                };
                return u.Equals(user);
            });

            Assert.IsTrue(server.Slaves.All(s => (s.Search(u => u.FirstName=="Pavel").Count == 1) && s.Search(u => true).Count == 1));
        }
    }
}
