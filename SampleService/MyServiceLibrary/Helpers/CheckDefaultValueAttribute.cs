using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Helpers
{
    /// <summary>
    /// Property has attribute when it should be checked on default value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CheckDefaultValueAttribute : Attribute
    {
    }
}
