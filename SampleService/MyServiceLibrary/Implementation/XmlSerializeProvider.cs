using System;
using System.IO;
using System.Xml.Serialization;
using MyServiceLibrary.Interfaces;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Class provides xml serializer
    /// </summary>
    /// <typeparam name="T">type of object to serialize</typeparam>
    public class XmlSerializeProvider<T> : ISerializerProvider<T>
    {
        /// <summary>
        /// Method serializes obj to the file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="obj">Object to serialize</param>
        public void Serialize(string fileName, T obj)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof(User[]), new Type[] { typeof(T) });

            using (Stream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                xmlFormat.Serialize(fileStream, obj);
            }
        }
    }
}
