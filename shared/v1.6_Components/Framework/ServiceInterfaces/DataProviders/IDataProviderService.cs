using SDV_Realty_Core.Framework.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders
{
    internal abstract class IDataProviderService:IService 
    {
        public DataProviderManager dataProviderManager;
    }
}
