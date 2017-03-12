using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Configuration;

namespace LoggerSingleton
{
    public sealed class NlogLogger
    {
        private static readonly Lazy<Logger> Lazy =
            new Lazy<Logger>(() => LogManager.GetCurrentClassLogger());

        private NlogLogger()
        {
        }

        public static Logger Logger
        {
            get
            {
                return Lazy.Value;
            }
        }
    }
}
