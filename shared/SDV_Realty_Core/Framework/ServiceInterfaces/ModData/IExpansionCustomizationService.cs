using SDV_Realty_Core.Framework.Objects;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    internal abstract class IExpansionCustomizationService:IService
{
        public override Type ServiceType => typeof(IExpansionCustomizationService);
        public abstract Dictionary<string, CustomizationDetails> CustomDefinitions { get; }
        public abstract void LoadDefinitions();
        public abstract void SaveDefinitions();
    }
}
