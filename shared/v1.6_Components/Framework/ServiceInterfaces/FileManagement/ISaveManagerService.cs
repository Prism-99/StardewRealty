using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement
{
    internal abstract class ISaveManagerService:IService
    {
        public SaveManagerV2 saveManager;

    }
}
