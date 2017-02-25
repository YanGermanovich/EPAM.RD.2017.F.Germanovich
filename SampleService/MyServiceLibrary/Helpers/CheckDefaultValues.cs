using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Helpers
{
    /// <summary>
    /// Class checks all properties of item on default value
    /// </summary>
    public static class CheckDefaultValues
    {
        /// <summary>
        /// Mehod checks all properties of item on default value
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="item">Item to check</param>
        /// <returns>True if item have one or more property with default value</returns>
        public static bool Check<T>(T item)
        {
            TypeInfo metaData = typeof(T).GetTypeInfo();
            bool isDefault = false;
            IEnumerator enumerator = metaData.GetProperties().GetEnumerator();
            try
            {
                while (!isDefault && enumerator.MoveNext())
                {
                    PropertyInfo element = (PropertyInfo)enumerator.Current;
                    if (element.GetCustomAttribute<CheckDefaultValueAttribute>() != null)
                    {
                        Type propType = element.PropertyType;
                        var defaultValue = propType.IsValueType ? Activator.CreateInstance(propType) : null;
                        isDefault = object.Equals(element.GetValue(item), defaultValue);
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            return isDefault;
        }
    }
}
