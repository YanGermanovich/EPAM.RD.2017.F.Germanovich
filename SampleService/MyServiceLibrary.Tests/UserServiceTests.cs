using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyServiceLibrary.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        #region Add one user
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullUser_ExceptionThrown()
        {
            var service = new UserService();
            User us = null;
            service.Add(us);
        }

        [TestMethod]
        [ExpectedException(typeof(ExistUserException))]
        public void Add_ExistUser_ExceptionThrown()
        {
            var service = new UserService();
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
            var service = new UserService();
            var us = new User();
            service.Add(us);
        }

        [TestMethod]
        public void Add_ValidtUsersWithValidGenerator_ExceptionThrown()
        {
            int i = 0;
            var service = new UserService(() => i++);
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
            var service = new UserService(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullUsersSet_ExceptionThrown()
        {
            var service = new UserService();
            IEnumerable<User> us = null;
            service.Add(us);
        }

        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Delete_NullPredicate_ExceptionThrown()
        {
            var service = new UserService();
            service.Delete(null);
        }

        [TestMethod]
        public void Delete_ExistUser_ExceptionThrown()
        {
            var service = new UserService();
            var us = new User()
            {
                DateOfBirth = new DateTime(1998, 7, 4),
                FirstName = "Yan",
                LastName = "Germanovich"
            };

            service.Add(us);
            service.Delete((user) =>
            {
                int YearsPassed = DateTime.Now.Year - user.DateOfBirth.Year;
                if (DateTime.Now.Month < user.DateOfBirth.Month || (DateTime.Now.Month == user.DateOfBirth.Month &&
                        DateTime.Now.Day < user.DateOfBirth.Day))
                {
                    YearsPassed--;
                }

                return YearsPassed >= 18;
            });
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
            var service = new UserService();
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
            var service = new UserService();
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
    }
}
