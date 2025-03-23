using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Services
{
    internal abstract class IServicesManager
    {
        internal Dictionary<Type, IService> services= new Dictionary<Type, IService>();
        internal Dictionary<Type, IService> cachedServices=new Dictionary<Type, IService>();
        internal ILoggerService logger;
        internal abstract void AddService(Type serviceType, IService service);
        internal abstract void AddService( IService service);
        internal abstract void RemoveService(Type serviceType);
        internal abstract T GetService<T>(Type serviceType);  
        internal abstract bool IsServiceRegistered(Type serviceType);
        internal abstract void VerifyServices();
        internal abstract void DumpLoadTimes();
        public abstract void DumpConfiguration();

    }
}
