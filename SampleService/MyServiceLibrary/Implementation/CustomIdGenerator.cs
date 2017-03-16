using MyServiceLibrary.InterfacesAndAbstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// Class provide function of id generator
    /// </summary>
    [Serializable]
    public class CustomIdGenerator : IIdGenerator
    {
        private int id;
        private int firstId;
        /// <summary>
        /// Id start value
        /// </summary>
        public int NextValue {get;set;}

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomIdGenerator():this(0)
        {

        }

        /// <summary>
        /// Constructor with entering id start value
        /// </summary>
        /// <param name="idStartValue">id start value</param>
        public CustomIdGenerator(int idStartValue)
        {
            this.NextValue = idStartValue;
            this.id = idStartValue;
            this.firstId = idStartValue;
        }


        /// <summary>
        /// Method returns new unique id
        /// </summary>
        /// <returns>Id</returns>
        public int GetId()
        {
            if (this.id == this.firstId)
            {
                this.id = this.NextValue;
            }
            return id++;
        }
    }
}
