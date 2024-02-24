using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.CustomEntities.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomBuildingService:IService
{
        public override Type ServiceType => typeof(ICustomBuildingService);
        public CustomBuildingManager customBuildingManager;
        public abstract Dictionary<string, ICustomBuilding> CustomBuildings { get; }
        public abstract Dictionary<string, object> ExternalReferences { get; }

        public abstract void LoadDefinitions();

    }
}
