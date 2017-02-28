using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyServiceLibrary.Implementation;

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
                var server = new ServiceServer<UserService>();
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
            var server = new ServiceServer<UserService>(idGen);
        }

        [TestMethod]
        public void Add()
        {
            var server = new ServiceServer<UserService>();
            var us = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Ivan",
                LastName = "Ivanov"
            };

            server.Master.Add(us);

            Assert.IsTrue(server.Slaves.All(s => s.Search((u) => u.Equals(us)).Count == 1));
        }

        [TestMethod]
        public void Delete()
        {
            var server = new ServiceServer<UserService>();
            var us = new User()
            {
                DateOfBirth = DateTime.Now,
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
            server.Master.Delete(u => u.Equals(us));

            Assert.IsTrue(server.Slaves.All(s => (s.Search(u => u.Equals(us1)).Count == 1) && s.Search(u => true).Count == 1));
        }
    }
}
