using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomBigCraftableService:IService
    {
        public override Type ServiceType => typeof(ICustomBigCraftableService);
        public CustomBigCraftableManager customBigCraftableManager;
        public abstract Dictionary<string, object> ExternalReferences { get; }
        public abstract Dictionary<string, CustomBigCraftableData> BigCraftables { get; }
        public abstract void LoadDefinitions();
    }
}
