using HarmonyLib;
using Newtonsoft.Json;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Text;
using xTile.Dimensions;

namespace SDV_Realty_Core.Framework.Objects
{
    internal class GameLocationCustomizations
    {
        public const string ModDataKey = "sdr.customizations";
        public GameLocationCustomizations() { LocationName = ""; }
        public GameLocationCustomizations(string locationName)
        {
            LocationName = locationName;
            GameLocation? source = Game1.getLocationFromName(locationName);
            if (source != null)
            {
                if (source.modData.TryGetValue(ModDataKey, out string values))
                {
                    try
                    {
                        GameLocationCustomizations? current = JsonConvert.DeserializeObject<GameLocationCustomizations>(values);
                        if (current != null)
                        {
                            SeasonOverride = current.SeasonOverride;
                            CanHaveGreenRainSpawns=current.CanHaveGreenRainSpawns;
                        }
                    }
                    catch { }
                }
            }
        }
        public string GetLocationName()
        {
            if (LocationName == "Farm")
                return "Farm_" + Game1.GetFarmTypeKey();
            else
                return LocationName;
        }
        public void ApplyCustomizations()
        {
            GameLocation? source = Game1.getLocationFromName(GetLocationName());
            if (source != null)
            {
                if (SeasonOverride == "none")
                    Traverse.Create(source).Field("seasonOverride").SetValue(new Lazy<Season?>());
                else
                {
                    if (Enum.TryParse(typeof(Season), SeasonOverride, true, out object ov))
                    {
                        Traverse.Create(source).Field("seasonOverride").SetValue(new Lazy<Season?>((Season)ov));
                    }
                }
            }
        }
        public void SaveCustomizations()
        {
            GameLocation? source = Game1.getLocationFromName(LocationName);
            if (source != null)
            {
                string data = JsonConvert.SerializeObject(this);
                source.modData[ModDataKey] = data;
                ApplyCustomizations();
            }
        }
        public string LocationName { get; set; }
        public string SeasonOverride { get; set; } = "none";
        public bool? CanHaveGreenRainSpawns { get; set; } = null;
    }
}
