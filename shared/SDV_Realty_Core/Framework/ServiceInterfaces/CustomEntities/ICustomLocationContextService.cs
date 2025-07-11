using SDV_Realty_Core.Framework.CustomEntities.LocationContexts;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomLocationContextService : IService
    {
        public override Type ServiceType => typeof(ICustomLocationContextService);
        public abstract Dictionary<string, CustomLocationContext> locationContexts { get; }
        public LocationContextDataManager locationContextDataManager;
        public abstract void LoadDefinitions();
    }
}
