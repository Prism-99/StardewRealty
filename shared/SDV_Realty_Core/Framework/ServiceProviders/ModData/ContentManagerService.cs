using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using System.Collections.Generic;
using static SDV_Realty_Core.Framework.ServiceInterfaces.Events.IGameEventsService;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Graphics;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ContentManagerService : IContentManagerService
    {
        private ICustomObjectService customObjectService;
        private IModDataService modDataService;
        public override List<string> CustomServiceEventSubscripitions => new List<string> { };
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService),typeof(IModHelperService),
            typeof(IUtilitiesService),typeof(ICustomObjectService)
        };
        public override Type[] Dependants => new Type[]
        {

        };

        internal override Dictionary<string, object> ExternalReferences => modDataService.ExternalReferences;
        internal override Dictionary<string, string> stringFromMaps => contentManager.stringFromMaps;

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            modDataService = (IModDataService)args[0];
            IModHelperService helper = (IModHelperService)args[1];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[2];
            customObjectService = (ICustomObjectService)args[3];
            contentManager = new AssetUtils.SDRContentManager(utilitiesService, modDataService);
            utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, ContentLoaded);
            //utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), LoadExpansionFiles);
            utilitiesService.GameEventsService.AddSubscription(EventTypes.AllModsLoaded, LoadExpansionFiles, 500);
            //utilitiesService.even )
            utilitiesService.CustomEventsService.AddCustomSubscription("RemoveStringFromMap", RemoveString);
        }
        public override Texture2D? GetExternalTexture2D(string assetName)
        {
            if (TryGetExternalReference(assetName, out var texture))
            {
                if (texture is Texture2D textureOut)
                {
                    return textureOut;
                }
            }
            return null;
        }
        public override bool TryGetExternalReference(string assetName, out object? assValue)
        {
            return ExternalReferences.TryGetValue(assetName, out assValue);
        }
        public override void AddExternalReference(string assetName, object assValue, bool removeExt = true)
        {
            if (ExternalReferences.ContainsKey(assetName))
            {
                logger.Log($"Trying to add duplicate External Reference {assetName}", LogLevel.Warn);
            }
            else
            {
                ExternalReferences.Add(assetName, assValue);
                if (removeExt)
                {                    
                    if (assetName.EndsWith(".png",StringComparison.CurrentCultureIgnoreCase))
                    {
                        int end = assetName.LastIndexOf('.');
                        if (end > -1)
                        {
                            string trimmedName = assetName.Substring(0, end);
                            if (ExternalReferences.ContainsKey(trimmedName))
                            {
                                if (ExternalReferences[trimmedName].GetType().Name != assValue.GetType().Name)
                                    logger.Log($"Trying to add duplicate External Reference '{trimmedName}' Type1={ExternalReferences[trimmedName].GetType().Name},Type2={assValue.GetType().Name}", LogLevel.Error);
                                else
                                    logger.Log($"Trying to add duplicate External Reference '{trimmedName}' (Types Match)", LogLevel.Warn);
                            }
                            else
                            {
                                ExternalReferences.Add(trimmedName, assValue);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Handle AllModsLoaded
        /// </summary>
        /// 
        private void LoadExpansionFiles()
        {
            Task.Run(() => contentManager.AddAllExpansionFiles());
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
