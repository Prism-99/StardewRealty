using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.Objects;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class ChatBoxCommandsService : IChatBoxCommandsService
    {
        private IModDataService modDataService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IGridManager),
            typeof(IContentManagerService),typeof(IModDataService)
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
            modDataService = (IModDataService)args[3];

            chatBoxCommands = new SDRChatboxCommands(logger,  utilitiesService.ModHelperService, gridManager, contentManager);

            AddCustomCommand("requests", HandleRequestsCommand);
            AddCustomCommand("summary", HandleSummaryCommand);
        }
        public override void AddCustomCommand(string command, Action<string[]> action)
        {
            chatBoxCommands.AddCustomCommand(command,action);
        }
        private void HandleSummaryCommand(string[] args)
        {
            logger.Log($"Total requests {modDataService.assetRequests.Count:N0}", LogLevel.Info);
        }
        private void HandleRequestsCommand(string[] args)
        {
            foreach (var entry in modDataService.assetRequests.OrderBy(p => p.Key))
            {
                logger.Log($"{entry.Key}: Request: {entry.Value.Item1:N0}, Ready: {entry.Value.Item2:N0}", LogLevel.Info);
            }
        }
    }
}
