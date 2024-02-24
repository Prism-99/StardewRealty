using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
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
        public override List<string> CustomServiceEventSubscripitions => new List<string> { "RemoveStringFromMap" };
        public override Type[] InitArgs => new Type[] 
        { 
            typeof(IModDataService),typeof(IModHelperService),
            typeof(IGameEventsService),typeof(ICustomObjectService)
        };
        public override Type[] Dependants => new Type[]
        {
            //typeof(ICustomEntitiesServices)
        };

        internal override Dictionary<string, object> ExternalReferences => contentManager.ExternalReferences;

        internal override Dictionary<string, string> stringFromMaps => contentManager.stringFromMaps;

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            IModDataService modDataService = (IModDataService)args[0];
            IModHelper helper= ((IModHelperService)args[1]).modHelper;
            // DataProviderService dataProviderManager = (DataProviderService)args[2];
            IGameEventsService eventsService = (IGameEventsService)args[2];
            customObjectService= (ICustomObjectService)args[3];

            contentManager = new AssetUtils.SDRContentManager( helper, logger, modDataService);
            eventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, ContentLoaded);
            eventsService.AddSubscription(new GameLaunchedEventArgs(), LoadExpansionFiles);
            RegisterEventHandler("RemoveStringFromMap", RemoveString);
        }
        private void LoadExpansionFiles(EventArgs e)
        {
            contentManager.AddExpansionFiles();
        }
        private void RemoveString(object[] args)
        {
            stringFromMaps.Remove(args[0].ToString());
            //contentManager.RemoveStringFromMap(args[0].ToString());
        }
        private void ContentLoaded()
        {
            // moved to the StringService
            //contentManager.localizationStrings.AddStrings(CustomObjectManager.objects);
            //contentManager.localizationStrings.AddStrings(CustomBigCraftableManager.BigCraftables);
            //contentManager.localizationStrings.AddStrings(CustomBuildingManager.CustomBuildings);
            //contentManager.localizationStrings.GenerateDictionary();
            //
            //add npc tastes for custom objects
            //
            foreach (var key in customObjectService.objects.Keys)
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
