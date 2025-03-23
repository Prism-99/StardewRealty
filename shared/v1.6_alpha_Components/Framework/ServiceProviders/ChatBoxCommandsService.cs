using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.Objects;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class ChatBoxCommandsService : IChatBoxCommandsService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IGridManager),
            typeof(IContentManagerService)
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
            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            IGridManager gridManager = (IGridManager)args[1];
            IContentManagerService contentManager = (IContentManagerService)args[2];

            chatBoxCommands = new SDRChatboxCommands(logger, utilitiesService.ModHelperService, gridManager, contentManager);
               
        }
    }
}
