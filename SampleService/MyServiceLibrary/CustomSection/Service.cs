using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomSection
{
    public class Service : ConfigurationElement
    {

        [ConfigurationProperty("Id", IsRequired = true)]
        public int Id
        {
            get
            {
                return int.Parse(this["Id"].ToString());
            }
        }

        [ConfigurationProperty("IpAddr", IsRequired = true)]
        public string IpAddr
        {
            get
            {
                return this["IpAddr"] as string;
            }
        }
        [ConfigurationProperty("Port", IsRequired = true)]
        public int Port
        {
            get
            {
                return int.Parse(this["Port"].ToString());
            }
        }

        public static explicit operator IPEndPoint(Service service)
        {
            return new IPEndPoint(IPAddress.Parse(service.IpAddr), service.Port);
        }
    }
}
