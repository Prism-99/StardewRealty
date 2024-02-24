using SDV_Realty_Core.Framework.AssetUtils;
using StardewModdingAPI.Events;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class ChairTiles : IGameDataProvider
    {
        public override string Name => "Data/ChairTiles";
        private Dictionary<string, string> Tiles;
        private bool AddBridgeChair = false;
        public ChairTiles(bool addBridgeChair, SDRContentManager conMan)
        {
            AddBridgeChair = addBridgeChair;
            Tiles = conMan.ChairTiles;
        }
        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                //asset.AsDictionary<string, string>().Data.Add("spring_extrasTileSheet/13/9", "1/1/opposite/bench/-1/-1/false");
                // add game bridge as seat
                if (AddBridgeChair && !asset.AsDictionary<string, string>().Data.ContainsKey("spring_town/18/29"))
                    asset.AsDictionary<string, string>().Data.Add("spring_town/18/29", "1/1/opposite/bench/-1/-1/false");

                foreach (string tile in Tiles.Keys)
                {
                    asset.AsDictionary<string, string>().Data.Add(tile, Tiles[tile]);
                }
            });
        }

        public void AddSeatTile(string tileSheetDetails, string seatDetails)
        {
            Tiles.Add(tileSheetDetails, seatDetails);
        }

        public override void OnGameLaunched()
        {

        }
    }
}
