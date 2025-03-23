using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ContentManagerService : IContentManagerService
    {
        private ICustomObjectService customObjectService;
        public override List<string> CustomServiceEventSubscripitions => new List<string> {  };
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService),typeof(IModHelperService),
            typeof(IUtilitiesService),typeof(ICustomObjectService)
        };
        public override Type[] Dependants => new Type[]
        {

        };

        internal override Dictionary<string, object> ExternalReferences => contentManager.ExternalReferences;
        internal override Dictionary<string, string> stringFromMaps => contentManager.stringFromMaps;

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            IModDataService modDataService = (IModDataService)args[0];
            IModHelperService helper = (IModHelperService)args[1];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[2];
            customObjectService = (ICustomObjectService)args[3];
            contentManager = new AssetUtils.SDRContentManager(helper, logger, modDataService);
            utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, ContentLoaded);
            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), LoadExpansionFiles);
            utilitiesService.CustomEventsService.AddCustomSubscription("RemoveStringFromMap", RemoveString);
         }
        /// <summary>
        /// Handle GameLaunchedEvent
        /// </summary>
        /// <param name="e">ignored</param>
        private void LoadExpansionFiles(EventArgs e)
        {
            contentManager.AddExpansionFiles();
        }
        /// <summary>
        /// Handle RemoveStringFromMap event
        /// </summary>
        /// <param name="args">[0] - string key</param>
        private void RemoveString(object[] args)
        {
            stringFromMaps.Remove(args[0].ToString());
        }
        private void ContentLoaded()
        {
            //
            //add npc tastes for custom objects
            //
            foreach (string key in customObjectService.objects.Keys)
            {
                if (customObjectService.objects[key].GiftTastes != null)
                {
                    contentManager.AddNPCTastes(key, customObjectService.objects[key].GiftTastes);
                }
            }
        }
        public override void RemoveStringFromMap(string assetName)
        {
            stringFromMaps.Remove(assetName);
        }
        public override void UpsertStringFromMap(string assetName, string assValue)
        {
            if (stringFromMaps.ContainsKey(assetName))
            {
                stringFromMaps[assetName] = assValue;
            }
            else
            {
                stringFromMaps.Add(assetName, assValue);
            }
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;

        }
    }
}
