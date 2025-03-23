using StardewValley.GameData.LocationContexts;


namespace SDV_Realty_Core.Framework.Utilities
{
    internal static class LocationContextDataExt
    {
        /// <summary>
        /// Clones the values of the current LocationContextData to a new instance
        /// </summary>
        /// <param name="source">LocationContextData to be cloned</param>
        /// <returns></returns>
        public static LocationContextData Clone(this LocationContextData source)
        {
            return new LocationContextData
            {
                AllowRainTotem = source.AllowRainTotem,
                CopyWeatherFromLocation = source.CopyWeatherFromLocation,
                CustomFields = source.CustomFields,
                DayAmbience = source.DayAmbience,
                DefaultMusic = source.DefaultMusic,
                DefaultMusicCondition = source.DefaultMusicCondition,
                DefaultMusicDelayOneScreen = source.DefaultMusicDelayOneScreen,
                MaxPassOutCost = source.MaxPassOutCost,
                NightAmbience = source.NightAmbience,
                PassOutLocations = source.PassOutLocations,
                PassOutMail = source.PassOutMail,
                //PlantableLocations = source.PlantableLocations,
                PlayRandomAmbientSounds = source.PlayRandomAmbientSounds,
                RainTotemAffectsContext = source.RainTotemAffectsContext,
                ReviveLocations = source.ReviveLocations,
                SeasonOverride = source.SeasonOverride,
                //
                //SpringMusic = source.SpringMusic,
                //SummerMusic = source.SummerMusic,
                //FallMusic = source.FallMusic,
                //WinterMusic = source.WinterMusic
             WeatherConditions = source.WeatherConditions,
             };
        }
    }
}
