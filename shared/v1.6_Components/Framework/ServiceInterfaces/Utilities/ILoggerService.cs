using System;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class ILoggerService
    {
        public abstract void Log(string message);
        public abstract void Log(string message, object logLevel);
        public abstract void LogError(string message, Exception err);
        public abstract void LogOnce(string message, object logLevel);
        public abstract bool Debug { get; set; }
        public virtual object CustomLogger { get; }
    }
}
