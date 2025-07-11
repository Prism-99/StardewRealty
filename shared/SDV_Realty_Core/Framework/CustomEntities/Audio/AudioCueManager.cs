using StardewValley.GameData;
using System.Collections.Generic;

namespace SDV_Realty_Core.Framework.CustomEntities.Audio
{
    /// <summary>
    /// Manager to track any required custom sounds. Provides source
    /// data to DataProviders/AudioChanges
    /// </summary>
    internal static class AudioCueManager
    {
        public static Dictionary<string,AudioCueData> CueData = new Dictionary<string, AudioCueData> { };

        public static void AddCue(string key, AudioCueData data)
        {
            CueData.Add(key,data);
        }
    }
}
