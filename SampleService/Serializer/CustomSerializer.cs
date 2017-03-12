using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public static class CustomSerializer
    {
        private static object lockObject = new object();
        public static byte[] Serialize<T>(T anySerializableObject)
        {
            lock (lockObject)
            {
                using (var memoryStream = new MemoryStream())
                {
                    (new BinaryFormatter()).Serialize(memoryStream, anySerializableObject);
                    return memoryStream.ToArray();
                }
            }
        }

        public static T DeserializeClass<T>(byte[] message)
            where T:class
        {
            lock (lockObject)
            {
                using (var memoryStream = new MemoryStream(message))
                {
                    return (new BinaryFormatter()).Deserialize(memoryStream) as T;
                }
            }
        }

        public static T DeserializeStruct<T>(byte[] message)
            where T : struct
        {
            lock (lockObject)
            {
                using (var memoryStream = new MemoryStream(message))
                {
                    return (T)(new BinaryFormatter()).Deserialize(memoryStream);
                }
            }
        }
    }
}
