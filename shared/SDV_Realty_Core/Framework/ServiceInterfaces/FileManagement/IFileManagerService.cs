using SDV_Realty_Core.ContentPackFramework.FileManagement;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement
{
    internal abstract class IFileManagerService:IService
    {
        public FileManager LoadGameMenu;

        public override Type ServiceType => typeof(IFileManagerService);
    }
}
