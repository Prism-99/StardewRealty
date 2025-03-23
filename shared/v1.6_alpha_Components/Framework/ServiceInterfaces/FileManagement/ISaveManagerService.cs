using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement
{
    /// <summary>
    /// Handles side saving and loading of Expansion data
    /// </summary>
    internal abstract class ISaveManagerService:IService
    {
        public SaveManagerV2 saveManager;
        internal abstract void LoadSaveFile(string loadContext, string saveFilename = null, bool loadOnly=false);

    }
}
