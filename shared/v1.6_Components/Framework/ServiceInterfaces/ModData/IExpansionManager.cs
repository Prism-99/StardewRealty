using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.Expansions;
using System;
//using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    internal abstract class IExpansionManager : IService
    {
         public override Type ServiceType => typeof(IExpansionManager);
        public ExpansionManager expansionManager;
        public abstract void ActivateExpansionOnRemote(string expansionName, int gridId);
        public abstract bool ActivateExpansion(string expansionName, int gridId = -1);
    }
}
