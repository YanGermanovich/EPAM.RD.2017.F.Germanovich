using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomSection
{
    public class RegisterServicesConfig
        : ConfigurationSection
    {

        public static RegisterServicesConfig GetConfig()
        {
             return (RegisterServicesConfig)ConfigurationManager.GetSection("RegisterServices") ?? new RegisterServicesConfig();
        }

        [System.Configuration.ConfigurationProperty("Slaves")]
        [ConfigurationCollection(typeof(Slaves), AddItemName = "Slave")]
        public Slaves Slaves
        {
            get
            {
                object o = this["Slaves"];
                return o as Slaves;
            }
        }

        [System.Configuration.ConfigurationProperty("Master")]
        [ConfigurationCollection(typeof(Master), AddItemName = "Master")]
        public Master Master
        {
            get
            {
                object o = this["Master"];
                return o as Master;
            }
        }

    }
}
