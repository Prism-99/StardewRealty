using SDV_Realty_Core.Framework.CustomEntities.Audio;
using StardewModdingAPI.Events;
using StardewValley.GameData;
using System.Collections.Generic;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/AudioChanges to add custom sounds
    /// </summary>
    internal class AudioChangesDataProviders : IGameDataProvider
    {
        public override string Name => "Data/AudioChanges";

        public override void CheckForActivations()
        {
            
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (KeyValuePair<string, AudioCueData> cue in AudioCueManager.CueData)
                {
                    asset.AsDictionary<string, AudioCueData>().Data.Add(cue.Key, cue.Value);
                }
            });
        }

        public override void OnGameLaunched()
        {
         }
    }
}
