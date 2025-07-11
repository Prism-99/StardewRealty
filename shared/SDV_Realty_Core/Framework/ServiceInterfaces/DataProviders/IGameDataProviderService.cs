using SDV_Realty_Core.Framework.DataProviders;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders
{
    /// <summary>
    /// Container for all Custom GameData providers
    /// </summary>
    internal abstract class IGameDataProviderService:IService
    {
        public List<IGameDataProvider> dataProviders;

    }
}
