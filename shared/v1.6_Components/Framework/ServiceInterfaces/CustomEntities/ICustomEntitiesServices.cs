using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    /// <summary>
    /// Container for custom entity managers
    /// </summary>
    internal abstract class ICustomEntitiesServices:IService
    {
        public ICustomBuildingService customBuildingService;
        public ICustomBigCraftableService customBigCraftableService;
        public ICustomMachineService customMachineService;
        public ICustomCropService customCropService;
        public SDRContentManager contentManager;
        public ICustomObjectService customObjectService;
        public ICustomMachineDataService customMachineDataService;
        public ICustomLocationContextService customLocationContextService;
        public ICustomMovieService customMovieService;
    }
}
