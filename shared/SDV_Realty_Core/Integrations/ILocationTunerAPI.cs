using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Integrations
{
    public interface ILocationTunerAPI
    {
        public Dictionary<string, object?> GetAllValues(string locationName);
        public bool NoCrows(string locationName, out bool value);
        public bool NoBirdies(string locationName, out bool value);
        public bool NoButterflies(string locationName, out bool value);
        public bool NoBunnies(string locationName, out bool value);
        public bool NoSquirrels(string locationName, out bool value);
        public bool NoWoodpeckers(string locationName, out bool value);
        public bool NoOwls(string locationName, out bool value);
        public bool NoOpossums(string locationName, out bool value);
        public bool NoClouds(string locationName, out bool value);
        public bool NoFrogs(string locationName, out bool value);
        public bool NoDebris(string locationName, out bool value);
        public bool NoLightning(string locationName, out bool value);
        public bool SeasonOverride(string locationName, out string value);
        public bool ContextId(string locationName, out string? value);
        public bool CanHaveGreenRainSpawns(string locationName, out bool? value);
        public bool CanPlantHere(string locationName, out bool? value);
        public bool CanBuildHere(string locationName, out bool? value);
        public bool MinDailyWeeds(string locationName, out int? value);
        public bool MaxDailyWeeds(string locationName, out int? value);
        public bool FirstDayWeedMultiplier(string locationName, out int? value);
        public bool MinDailyForageSpawn(string locationName, out int? value);
        public bool MaxDailyForageSpawn(string locationName, out int? value);
        public bool MaxSpawnedForageAtOnce(string locationName, out int? value);
        public bool ChanceForClay(string locationName, out float? value);
        public bool DirtDecayChance(string locationName, out float? value);
        public bool AllowGiantCrops(string locationName, out bool? value);
        public bool AllowGrassSurviveInWinter(string locationName, out bool? value);
        public bool AllowGrassGrowInWinter(string locationName, out bool? value);
        public bool EnableGrassSpread(string locationName, out bool? value);
        public bool AllowTreePlanting(string locationName, out bool? value);
        public bool SkipWeedGrowth(string locationName, out bool? value);
    }
}
