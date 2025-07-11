using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;
using StardewModdingAPI.Events;
using StardewModdingAPI.Enums;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using StardewModdingAPI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.CustomEntities.Objects;
using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class StringService : IStringService
    {
        private IContentManagerService contentManager;
        private ICustomEntitiesServices customEntitiesServices;
        //
        private static ITranslationHelper Translations;
        public Dictionary<string, string> TranslationDict;
        private Dictionary<string, ITranslationHelper> _toTranslate;
        private IUtilitiesService _services;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IContentManagerService),
            typeof(ICustomEntitiesServices),typeof(IModHelperService),
            typeof(IModDataService)
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
            IGameEventsService eventsService = (IGameEventsService)args[0];
            contentManager = (IContentManagerService)args[1];
            customEntitiesServices = (ICustomEntitiesServices)args[2];
            IModHelperService modHelperService = (IModHelperService)args[3];
            IModDataService modDataService = (IModDataService)args[4];

            // added as merge of LocalizationStrings
            Translations = modHelperService.Translation;
            TranslationDict = modDataService.TranslationDict;
            _toTranslate = new Dictionary<string, ITranslationHelper>();
            //_services = utilitiesService;
            //
            eventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, PostContentLoaded);
            //eventsService.AddSubscription(new GameLaunchedEventArgs(), HandleGameLaunched);
            //eventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), HandleLoadStage);
        }
        internal void HandleLoadStage(EventArgs e)
        {
            LoadStageChangedEventArgs ev = (LoadStageChangedEventArgs)e;

            if (ev.NewStage == LoadStage.SaveLoadedBasicInfo || ev.NewStage == LoadStage.CreatedBasicInfo)
            {
                //contentManager.contentManager.localizationStrings.GenerateDictionary();
                GenerateDictionary();
            }
        }
        internal void HandleGameLaunched(EventArgs e)
        {
            AddStrings(customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables);
            AddStrings(customEntitiesServices.customBuildingService.customBuildingManager.CustomBuildings);
            AddStrings(customEntitiesServices.customObjectService.objects);
        }
        internal void PostContentLoaded()
        {
            //
            //  add custom entity strings
            AddStrings(customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables);
            AddStrings(customEntitiesServices.customBuildingService.customBuildingManager.CustomBuildings);
            AddStrings(customEntitiesServices.customObjectService.objects);
            GenerateDictionary();

        }
        //  code from LocalizationString
        //
        public void PopulateDictionary()
        {
            foreach (var translateItem in _toTranslate)
            {
                TryAddStringToDictionary(translateItem.Key, translateItem.Value);
            }
        }
        public void GenerateDictionary()
        {
            logger.Log($"Generating String Dictionary", LogLevel.Debug);
            PopulateDictionary();
            foreach (string key in TranslationDict.Where(p => string.IsNullOrEmpty(p.Value)).Select(p => p.Key))
            {
                if (Translations.Get(key) == null)
                {
                    TranslationDict[key] = key;
                }
                else
                {
                    TranslationDict[key] = Translations.Get(key);
                }
            }
        }
        private void TryAddStringToDictionary(string translationKey, ITranslationHelper translations)
        {
            try
            {
                TranslationDict.Add(translationKey, translations.Get(translationKey).ToString());
            }
            catch (Exception ex)
            {
                logger.LogError("LocalizationStrings.TryAddStringToDictionary", ex);
            }
        }
        public void AddStrings(Dictionary<string, CustomObjectData> customObjects)
        {
            foreach (KeyValuePair<string, CustomObjectData> customObject in customObjects)
            {
                if (customObject.Value.ObjectData.DisplayName.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customObject.Value.ObjectData.DisplayName, out string displayNameKey))
                        AddTranslationToQueue(displayNameKey, customObject.Value.translations);
                }
                if (customObject.Value.ObjectData.Description.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customObject.Value.ObjectData.Description, out string descriptionKey))
                        AddTranslationToQueue(descriptionKey, customObject.Value.translations);
                }
            }
        }
        public void AddStrings(Dictionary<string, ICustomBuilding> customBuildings)
        {
            foreach (KeyValuePair<string, ICustomBuilding> customBuilding in customBuildings)
            {
                if (customBuilding.Value.DisplayName.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customBuilding.Value.DisplayName, out string displayNameKey))
                        AddTranslationToQueue(displayNameKey, customBuilding.Value.translations);
                }
                if (customBuilding.Value.Description.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customBuilding.Value.Description, out string descriptionKey))
                        AddTranslationToQueue(descriptionKey, customBuilding.Value.translations);
                }
            }
        }
        public void AddStrings(Dictionary<string, CustomBigCraftableData> customBC)
        {
            foreach (CustomBigCraftableData bigCraftable in customBC.Values)
            {
                if (bigCraftable.BigCraftableData.DisplayName.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(bigCraftable.BigCraftableData.DisplayName, out string displayNameKey))
                        AddTranslationToQueue(displayNameKey, bigCraftable.translations);
                }
                if (bigCraftable.BigCraftableData.Description.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(bigCraftable.BigCraftableData.Description, out string descriptionKey))
                        AddTranslationToQueue(descriptionKey, bigCraftable.translations);
                }
            }
            //foreach (var bigCraftable in customBC)
            //{
            //    if (bigCraftable.BigCraftableData.DisplayName.StartsWith("[LocalizedText"))
            //    {
            //        TranslationDict.Add($"{bigCraftable.Id}.displayname", "");
            //    }
            //    if (bigCraftable.BigCraftableData.Description.StartsWith("[LocalizedText"))
            //    {
            //        TranslationDict.Add($"{bigCraftable.Id}.description", "");
            //    }
            //}
        }

        private void AddTranslationToQueue(string translationKey, ITranslationHelper translations)
        {
            if (!_toTranslate.ContainsKey(translationKey))
            {
                _toTranslate.Add(translationKey, translations);
            }
        }
        private bool TryGetTranslationKey(string fieldText, out string translationKey)
        {
            translationKey = string.Empty;

            int fieldSeparator = fieldText.IndexOf(":");
            if (fieldSeparator > -1)
            {
                translationKey = fieldText.Substring(fieldSeparator + 1).Replace("]", "").Trim();
                return true;
            }

            return false;
        }

    }
}
