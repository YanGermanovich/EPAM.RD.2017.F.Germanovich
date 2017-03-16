using System;
using LoggerSingleton;
using PostSharp.Aspects;
using System.Text;
using System.Reflection;
using MyServiceLibrary.Helpers;

namespace MyServiceLibrary.CustomAttribute
{

    [Serializable]
    public class LoggingAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (Convert.ToBoolean(GetValueFromConfig.Get("EnableLogging")))
                NlogLogger.Logger.Info($"Method {args.Method.Name}");
        }
        public override void OnException(MethodExecutionArgs args)
        {
            if (Convert.ToBoolean(GetValueFromConfig.Get("EnableLogging")))
                NlogLogger.Logger.Fatal(args.Exception);
        }
    }

    [Serializable]
    public class LoggingWithArgsAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (Convert.ToBoolean(GetValueFromConfig.Get("EnableLogging")))
                NlogLogger.Logger.Info(DisplayObjectInfo(args));
        }
        public override void OnException(MethodExecutionArgs args)
        {
            if (Convert.ToBoolean(GetValueFromConfig.Get("EnableLogging")))
                NlogLogger.Logger.Fatal(args.Exception);
        }

        static string DisplayObjectInfo(MethodExecutionArgs args)
        {
            StringBuilder sb = new StringBuilder();
            Type type = args.Arguments.GetType();
            sb.Append("Method " + args.Method.Name);
            sb.Append("\r\nArguments:");
            FieldInfo[] fi = type.GetFields();
            if (fi.Length > 0)
            {
                foreach (FieldInfo f in fi)
                {
                    sb.Append("\r\n " + f + " = " + f.GetValue(args.Arguments));
                }
            }
            else
                sb.Append("\r\n None");

            return sb.ToString();
        }
    }


}
