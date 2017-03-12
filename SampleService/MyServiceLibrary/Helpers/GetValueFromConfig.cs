using LoggerSingleton;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Helpers
{
    public static class GetValueFromConfig
    {
        public static string Get(string name)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(name))
            {
                NlogLogger.Logger.Error($"No {name}");
                throw new ArgumentNullException($"Please add {name} into App.config");
            }

            string item = ConfigurationManager.AppSettings[name].ToString();

            if (item == string.Empty)
            {
                NlogLogger.Logger.Error($"Empty {name}");
                throw new ArgumentException($"{name} must not be empty!");
            }
            return item;
        }
    }
}
