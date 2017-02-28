using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyServiceLibrary.CustomExceptions;
using MyServiceLibrary.Implementation;

namespace MyServiceLibrary.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        #region Create

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_SlaveWithNullMaster_ExceptionThrown()
        {
            var serive = new UserService(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AccesPermissionException))]
        public void Create_SlaveWithSlaveAsMaster_ExceptionThrown()
        {
            var serive = new UserService(new UserService());
        }

        #endregion

        #region Add one user
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullUser_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            User us = null;
            service.Add(us);
        }

        [TestMethod]
        [ExpectedException(typeof(ExistUserException))]
        public void Add_ExistUser_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            var us = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Ivan",
                LastName = "Ivanov"
            };
            service.Add(us);
            service.Add(us);
        }

        [TestMethod]
        [ExpectedException(typeof(DefaultUserException))]
        public void Add_DefaultUser_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);

            var us = new User()
            {
                FirstName = "Ivan",
                LastName = "Ivanov"
            };
            service.Add(us);
        }

        [TestMethod]
        public void Add_ValidtUsersWithValidGenerator()
        {
            int i = 0;
            var service = new UserService(() => i++, ServiceRoles.Master);
            var us = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Ivan",
                LastName = "Ivanov"
            };
            var us1 = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Ivan1",
                LastName = "Ivanov"
            };
            service.Add(us);
            service.Add(us1);
        }

        [TestMethod]
        [ExpectedException(typeof(AccesPermissionException))]

        public void Add_InSlaveService_ExceptionThrown()
        {
            int i = 0;
            var service = new UserService(() => i++, ServiceRoles.Slave);
            var us = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Ivan",
                LastName = "Ivanov"
            };
            var us1 = new User()
            {
                DateOfBirth = DateTime.Now,
                FirstName = "Ivan1",
                LastName = "Ivanov"
            };
            service.Add(us);
            service.Add(us1);
        }

        #endregion

        #region Add several users

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullIdGenerator_ExceptionThrown()
        {
            Func<int> idGenerator = null;
            var service = new UserService(idGenerator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullUsersSet_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            IEnumerable<User> us = null;
            service.Add(us);
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Delete_NullPredicate_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            service.Delete(null);
        }

        [TestMethod]
        public void Delete_ExistUser()
        {
            var service = new UserService(ServiceRoles.Master);
            var us = new User()
            {
                DateOfBirth = new DateTime(1998, 7, 4),
                FirstName = "Yan",
                LastName = "Germanovich"
            };

            service.Add(us);
            service.Delete((user) => user.DateOfBirth >= new DateTime(1999, 28, 02));
        }

        [TestMethod]
        [ExpectedException(typeof(AccesPermissionException))]
        public void Delete_ExistUserinSlaveService_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Slave);
            var us = new User()
            {
                DateOfBirth = new DateTime(1998, 7, 4),
                FirstName = "Yan",
                LastName = "Germanovich"
            };

            service.Add(us);
            service.Delete((user) => user.DateOfBirth >= new DateTime(1999, 28, 02));
        }

        #endregion

        #region Search

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Search_NullPredicate_ExceptionThrown()
        {
            var service = new UserService();
            service.Search(null);
        }

        [TestMethod]
        public void Search_NoUsersFound_ExceptionThrown()
        {
            var service = new UserService();

            var actual = service.Search((user) => false);

            Assert.AreEqual(actual.Count(), 0);
        }

        [TestMethod]
        public void Search_UserFound_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            var us = new User()
            {
                DateOfBirth = new DateTime(1998, 7, 4),
                FirstName = "Yan",
                LastName = "Germanovich"
            };
            service.Add(us);

            var actual = service.Search((user) => user.FirstName == "Yan");

            service.Delete((user) => true);

            Assert.IsTrue(actual.Contains(us));
        }

        #endregion

        #region Search Deferred

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchDeferred_NullPredicate_ExceptionThrown()
        {
            var service = new UserService();
            service.Search(null);
        }

        [TestMethod]
        public void SearchDeferred_NoUsersFound_ExceptionThrown()
        {
            var service = new UserService();

            var actual = service.Search((user) => false);

            Assert.AreEqual(actual.Count(), 0);
        }

        [TestMethod]
        public void SearchDeferred_UserFound_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            var us = new User()
            {
                DateOfBirth = new DateTime(1998, 7, 4),
                FirstName = "Yan",
                LastName = "Germanovich"
            };
            service.Add(us);

            var actual = service.Search((user) => user.FirstName == "Yan");

            Assert.IsTrue(actual.Contains(us));
        }

        #endregion

        #region Serialize

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serialize_NullProvider_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            service.SerializeState(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AccesPermissionException))]
        public void Serialize_SlaveServicer_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Slave);
            service.SerializeState(new XmlSerializeProvider<User[]>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serialize_NullFileName_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("FileName");
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("appSettings");
            try
            {
                service.SerializeState(new XmlSerializeProvider<User[]>());
            }
            catch (ArgumentNullException)
            {
                config.AppSettings.Settings.Add("FileName", "users.xml");
                config.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Serialize_EmptyFileName_ExceptionThrown()
        {
            var service = new UserService(ServiceRoles.Master);
            ConfigurationManager.AppSettings.Set("FileName", string.Empty);
            try
            {
                service.SerializeState(new XmlSerializeProvider<User[]>());
            }
            catch (ArgumentException)
            {
                ConfigurationManager.AppSettings.Set("FileName", "users.xml");
                throw;
            }
        }

        #endregion
    }
}
