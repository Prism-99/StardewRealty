using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.CustomEntities.Objects;
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
        public void Intialize(ITranslationHelper translations)
        {
            Translations = translations;
            TranslationDict = new Dictionary<string, string>();
        }
        public override string Name => "SDR/Strings";
        /// <summary>
        /// Parse CustomObject data for any translated fields and
        /// adds them to the translation dictionary
        /// </summary>
        /// <param name="customObjects"></param>
        public void AddStrings(Dictionary<string, CustomObjectData> customObjects)
        {
            foreach (var customObject in customObjects)
            {
                if (customObject.Value.ObjectData.DisplayName.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customObject.Value.ObjectData.DisplayName, out string displayNameKey))
                        TryAddStringToDictionary(displayNameKey, customObject.Value.translations);
                }
                if (customObject.Value.ObjectData.Description.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customObject.Value.ObjectData.Description, out string descriptionKey))
                        TryAddStringToDictionary(descriptionKey, customObject.Value.translations);
                }
            }
        }
        public void AddStrings(Dictionary<string, ICustomBuilding> customBuildings)
        {
            foreach (var customBuilding in customBuildings)
            {
                if (customBuilding.Value.DisplayName.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customBuilding.Value.DisplayName, out string displayNameKey))
                        TryAddStringToDictionary(displayNameKey, customBuilding.Value.translations);
                }
                if (customBuilding.Value.Description.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(customBuilding.Value.Description, out string descriptionKey))
                        TryAddStringToDictionary(descriptionKey, customBuilding.Value.translations);
                }
            }
        }
      
        public void AddStrings(Dictionary<string,CustomBigCraftableData> customBC)
        {
            foreach (var bigCraftable in customBC.Values)
            {
                if (bigCraftable.BigCraftableData.DisplayName.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(bigCraftable.BigCraftableData.DisplayName, out string displayNameKey))
                        TryAddStringToDictionary(displayNameKey, bigCraftable.translations);
                }
                if (bigCraftable.BigCraftableData.Description.StartsWith("[LocalizedText"))
                {
                    if (TryGetTranslationKey(bigCraftable.BigCraftableData.Description, out string descriptionKey))
                        TryAddStringToDictionary(descriptionKey, bigCraftable.translations);
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
        /// <summary>
        /// Lookup all requested translations
        /// </summary>
        public void GenerateDictionary()
        {
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
