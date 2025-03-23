using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using System;

namespace SDV_Realty_Core.Framework.ServiceProviders.FileManagement
{
    internal class FileManagerService : IFileManagerService
    {
        public override Type[] InitArgs => new Type[]
        {
           typeof( ISaveManagerService),typeof(IUtilitiesService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            ISaveManagerService saveManagerService = (ISaveManagerService)args[0];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[1];

            LoadGameMenu = new FileManager(logger, utilitiesService.PatchingService, saveManagerService);
        }
    }
}
