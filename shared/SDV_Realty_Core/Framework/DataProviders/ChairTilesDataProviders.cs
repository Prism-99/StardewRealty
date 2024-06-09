using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using System.Collections.Generic;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class ChairTilesDataProviders : IGameDataProvider
    {
        /// <summary>
        /// Handles adding custom ChairTiles
        /// </summary>
        /// 
        /// Note: chairs must be on the Building layer
        ///
        public override string Name => "Data/ChairTiles";
        private Dictionary<string, string> Tiles;
        private IUtilitiesService utilitiesService;
        public ChairTilesDataProviders(IUtilitiesService utilitiesService, SDRContentManager conMan)
        {
            this.utilitiesService = utilitiesService;
            Tiles = conMan.ChairTiles;
            //
            //  Add swing tiles so shadow is not needed
            //
            AddSeatTile("spring_town/8/38", "1/1/down/custom 0 0.25 0/-1/-1/true");
            AddSeatTile("spring_town/10/38", "1/1/down/custom 0 0.25 0/-1/-1/true");
        }
        public override void ConfigChanged()
        {
            utilitiesService.InvalidateCache(Name);
        }
        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                IDictionary<string, string> chairTiles = asset.AsDictionary<string, string>().Data;
                // add game bridge as seat
                if (utilitiesService.ConfigService.config.AddBridgeSeat && !chairTiles.ContainsKey("spring_town/18/29"))
                {
                    // this tile is usually on the Front layer
                    //chairTiles.Add("spring_town/13/30", "1/1/up/bench/-1/-1/false");
                    chairTiles.Add("spring_town/18/29", "1/1/down/bench/-1/-1/false");
                }

                foreach (string tile in Tiles.Keys)
                {
                    if (!chairTiles.ContainsKey(tile))
                    {
                        chairTiles.Add(tile, Tiles[tile]);
                    }
                    else
                    {
                        logger.Log($"Duplicate chair tile {tile}", LogLevel.Warn);
                    }
                }
            });
        }

        public void AddSeatTile(string tileSheetDetails, string seatDetails)
        {
            if (!Tiles.ContainsKey(tileSheetDetails))
                Tiles.Add(tileSheetDetails, seatDetails);

        }

        public override void OnGameLaunched()
        {

        }
    }
}
