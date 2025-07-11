using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.CustomEntities.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class LocalizationStrings : IGameDataProvider
    {
        private static ITranslationHelper Translations;
        public static Dictionary<string, string> TranslationDict;
        private Dictionary<string, ITranslationHelper> _toTranslate;
        private IUtilitiesService _services;
        public void Intialize(ITranslationHelper translations, IUtilitiesService utilitiesService)
        {
            Translations = translations;
            TranslationDict = new Dictionary<string, string>();
            _toTranslate = new Dictionary<string, ITranslationHelper>();
            _services = utilitiesService;
            //_services.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), HandleGameLaunched);
        }
        public override string Name => "SDR/Strings";
        /// <summary>
        /// Parse CustomObject data for any translated fields and
        /// adds them to the translation dictionary
        /// </summary>
        /// <param name="customObjects"></param>
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
        public void HandleGameLaunched(EventArgs e)
        {
            PopulateDictionary();
        }
        public void PopulateDictionary()
        {
            foreach (var translateItem in _toTranslate)
            {
                TryAddStringToDictionary(translateItem.Key, translateItem.Value);
            }
        }
        /// <summary>
        /// Lookup all requested translations
        /// </summary>
        public void GenerateDictionary()
        {
            logger.Log($"Generating String Dictionary",LogLevel.Debug);
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
        public override void CheckForActivations()
        {

        }
        /// <summary>
        /// Returns the SDR translation dictionary
        /// </summary>
        /// <param name="e"></param>
        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.LoadFrom(() => { return TranslationDict; }, AssetLoadPriority.Medium);
        }

        public override void OnGameLaunched()
        {

        }
    }
}
