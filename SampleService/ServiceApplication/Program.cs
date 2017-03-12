using System;
using System.Collections.Generic;
using System.Linq;
using MyServiceLibrary.Implementation;
using LoggerSingleton;
using MyServiceLibrary.CustomSection;

using static System.Console;

namespace ServiceApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var generator = new IdGenerator();
            Func<int> idGenerator = generator.Generate;
            var server = new MasterSlaveService<UserService>(
                                                        idGenerator,
                                                        new XmlSerializeProvider<User[]>());
            var all = server.Slaves.ElementAt(0).Search((u) => true);
            Output(all, "All users");
            //var generator = new IdGenerator();
            //Func<int> idGenerator = generator.Generate;
            //var server = new MasterSlaveService<UserService>(
            //                                            idGenerator,
            //                                            new XmlSerializeProvider<User[]>());


            ////var search_byFirstName_deferred = server.Slaves.ElementAt(0).SearchDeferred((user) => user.FirstName == "Ivan");
            //var search_byFirstName = server.Slaves.ElementAt(2).Search((user) => user.FirstName == "Ivan");

            //server.Master.Delete((user) => user.FirstName == "Ivan");

            ////var search_byLastName_deferred = server.Slaves.ElementAt(1).SearchDeferred((user) => user.LastName == "Germanovich");
            //var search_byLastName = server.Slaves.ElementAt(2).Search((user) => user.LastName == "Germanovich");

            ////Output(search_byFirstName_deferred, "Deferred search users by first name ");
            //Output(search_byFirstName, "Search users by first name");
            //// Output(search_byLastName_deferred, "Deferred search users by last name");
            //Output(search_byLastName, "Search users by last name");

            //server.Master.SerializeState(new XmlSerializeProvider<User[]>());

            ////server.Master.Add(server.Slaves.ElementAt(0).Search(us => true).ElementAt(0));

            //server.Dispose();

            ReadKey();

            // 1. Add a new user to the storage.
            // 2. Remove an user from the storage.
            // 3. Search for an user by the first name.
            // 4. Search for an user by the last name.
        }

        private static void Output(IEnumerable<User> search_result, string name)
        {
            WriteLine(name);
            if (search_result.Count() == 0)
            {
                WriteLine("result is empty");
            }
            else
            {
                foreach (var us in search_result)
                {
                    WriteLine(us);
                }
            }

            int i = 0;
            while (i < 20)
            {
                i++;
                Write("-");
            }

            WriteLine();
        }
    }

    [Serializable]
    public class IdGenerator
    {
        private int id=0;
        public int Generate()
        {
            return id++;
        }
    }
}
