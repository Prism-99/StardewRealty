using Prism99_Core.Utilities;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class SDVServiceLogger : ILoggerService
    {
        SDVLogger logger;
        public SDVServiceLogger(SDVLogger logger)
        {
            this.logger = logger;
        }
        public SDVServiceLogger(IMonitor monitor, IModHelper helper)
        {
            logger = new SDVLogger(monitor, helper.DirectoryPath, helper, false);
        }
        public override object CustomLogger => logger;
        public override bool Debug { get => logger.Debug; set => logger.Debug=value; }
        public override void Log(string message, object logLevel,bool withCaller=true)
        {
            if ((Debug || withCaller) && (LogLevel)logLevel !=LogLevel.Info )
            {
                logger.Log($"[{GetCaller()}] {message}", (LogLevel)logLevel);
            }
            else
            {
                logger.Log(message, (LogLevel)logLevel);
            }
        }

        public override void Log(string message)
        {
            logger.Log(message);
        }
        public override void LogDebug(string caller, string message)
        {
            logger.LogDebug(caller, message);
        }
        public override void LogDebug(string message)
        {
            logger.Log(message, LogLevel.Debug);
        }
        public override void LogError(string message, Exception err)
        {
            logger.LogError(message,err);
        }
        public override void LogError( Exception err)
        {
            logger.LogError(GetCaller(), err);
        }
        public override void LogOnce(string message, object logLevel)
        {
            logger.LogOnce(message,(LogLevel)logLevel);
        }
    }
}
