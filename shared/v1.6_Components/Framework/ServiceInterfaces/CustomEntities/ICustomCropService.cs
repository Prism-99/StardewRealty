using SDV_Realty_Core.Framework.CustomEntities.Crops;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomCropService:IService
{
        public override Type ServiceType => typeof(ICustomCropService);
        public abstract void LoadDefinitions();
        public abstract Dictionary<string, CustomCropData> Crops { get; }
        public abstract void AddCropDefinition(CustomCropData cropData);
        public abstract Dictionary<string, object> ExternalReferences { get; }

    }
}
