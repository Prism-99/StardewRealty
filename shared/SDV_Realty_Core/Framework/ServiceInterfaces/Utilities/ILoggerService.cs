using System;
using System.Diagnostics;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class ILoggerService
    {
        public abstract void Log(string message);
        public abstract void Log(string message, object logLevel, bool withCaller = true);
        public abstract void Log( string message, object logLevel, int indentLevel, bool withCaller = true);
        public abstract void LogDebug(string caller, string message);
        public abstract void LogDebug(string message);
        public abstract void LogError(string message, Exception err);
        public abstract void LogError(Exception err);
        public abstract void LogOnce(string message, object logLevel);
        public abstract bool Debug { get; set; }
        public virtual object CustomLogger { get; }
        public virtual IMonitor Monitor { get; }
        public string GetCaller()
        {
            StackTrace stackTrace = new StackTrace();
            // Get calling method name
            return $"{stackTrace.GetFrame(2).GetMethod().DeclaringType.Name}.{stackTrace.GetFrame(2).GetMethod().Name}";
        }
    }
}
