using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class StringsFromMaps : IGameDataProvider
    {
        public StringsFromMaps(Dictionary<string, string> StringFromMaps)
        {
            stringFromMaps = StringFromMaps;
        }
        public override string Name => "Strings/StringsFromMaps";
        private readonly Dictionary<string, string> stringFromMaps;

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (string stringKey in stringFromMaps.Keys)
                {
                    if (asset.AsDictionary<string, string>().Data.ContainsKey(stringKey))
                    {
                        asset.AsDictionary<string, string>().Data[stringKey] = stringFromMaps[stringKey];
                    }
                    else
                    {
                        asset.AsDictionary<string, string>().Data.Add(stringKey, stringFromMaps[stringKey]);
                    }
                }
            });
        }

        public override void CheckForActivations()
        {
            
        }

        public override void OnGameLaunched()
        {
           
        }
    }
}
