using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    internal abstract class IContentPackService:IService
    {
        internal ContentPackLoader contentPackLoader;
        internal abstract void LoadPacks();
        internal abstract void LoadMaps();

    }
}
