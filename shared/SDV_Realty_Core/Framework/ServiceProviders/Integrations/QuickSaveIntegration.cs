using QuickSave.API;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceProviders.Integrations
{
    internal class QuickSaveIntegration : IQuickSaveIntegration
    {
        private IModHelperService modHelperService;
        private ISaveManagerService saveManagerService;
        public override Type ServiceType => typeof(IQuickSaveIntegration);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(ISaveManagerService)
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
            modHelperService=(IModHelperService)args[0];
            saveManagerService=(ISaveManagerService)args[1];

            modHelperService.modHelper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }
        public void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (!modHelperService.modHelper.ModRegistry.IsLoaded("DLX.QuickSave"))
                return;

            var api = modHelperService.modHelper.ModRegistry.GetApi<IQuickSaveAPI>("DLX.QuickSave");
            api.SavingEvent += Api_SavingEvent;
            api.SavedEvent += Api_SavedEvent;
        }
        private void Api_SavingEvent(object sender, ISavingEventArgs e)
        {
            saveManagerService.PreSave();
        }
        private void Api_SavedEvent(object sender, ISavedEventArgs e)
        {
            saveManagerService.PostSave();
        }
    }
}
