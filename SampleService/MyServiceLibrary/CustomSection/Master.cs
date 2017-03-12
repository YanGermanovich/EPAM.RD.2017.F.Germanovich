using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.CustomSection
{
    public class Master
        : ConfigurationElementCollection
    {
        public Service this[int index]
        {
            get
            {
                return base.BaseGet((object)index) as Service;
            }
            set
            {
                if (base.BaseGet((object)index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new Service this[string responseString]
        {
            get { return (Service)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new Service();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((Service)element).Id;
        }
    }
}
