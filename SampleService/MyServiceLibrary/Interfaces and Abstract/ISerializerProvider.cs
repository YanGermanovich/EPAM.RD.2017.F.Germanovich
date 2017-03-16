using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.InterfacesAndAbstract
{
    /// <summary>
    /// Interface provide function of serializer
    /// </summary>
    public interface ISerializerProvider<T>
    {
        /// <summary>
        /// Methods serialize object in the file
        /// </summary>
        /// <param name="fileName">Object will be serialize in this file</param>
        /// <param name="obj">Object to serialize</param>
        void Serialize(string fileName, T obj);

        /// <summary>
        /// Methods deserialize object from the file
        /// </summary>
        /// <param name="fileName">Object will be serialize in this file</param>
        /// <param name="extraTypes">Extra Types</param>
        T Deserialize(string fileName, Type[] extraTypes);
    }
}
