using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralStationIntegration
{
    public class CentralStationAPI
    {
        public CentralStationAPI(IModHelper modhelper)
        {
            modhelper.Events.Content.AssetRequested += Content_AssetRequested;
            modhelper.Events.Content.AssetReady += Content_AssetReady;
        }

        private void Content_AssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.Equals("Mods/Pathoschild.CentralStation/Stops"))
            {
                int xx = 1; 
              
            }
        }

        private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.Equals("Mods/Pathoschild.CentralStation/Stops"))
            {
                e.Edit(asset =>
                {
                    var data = asset.Data;
                    int xx = 1;
                }
                );
            }
        }
    }
}
