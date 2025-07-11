using SDV_Realty_Core.Framework.CustomEntities.Objects;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomObjectService:IService
{
        public override Type ServiceType => typeof(ICustomObjectService);
        public abstract void LoadDefinitions();
        public abstract void AddObjectDefinition(CustomObjectData nObject);
        public abstract Dictionary<string, CustomObjectData> objects { get; }
        public abstract Dictionary<string, object> ExternalReferences { get; }

    }
}
