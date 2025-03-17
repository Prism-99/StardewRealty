using System;
using System.Linq;
using StardewValley.TerrainFeatures;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceProviders.ModData;

namespace SDV_Realty_Core.Framework.Objects
{
    class FEForSale
    {
        public bool isShowingForSaleBoard;
        private string forSaleSignLocation = "Town";
        private ILoggerService logger;
        private IUtilitiesService _utilitiesService;
        private IModDataService _modDataService;
        private ILandManager _landManager;

        public void Initialize(ILoggerService olog, IUtilitiesService utilitiesService, IModDataService modDataService, ILandManager landManager)
        {
            logger = olog;
            _modDataService = modDataService;
            _utilitiesService = utilitiesService;
            _landManager = landManager;

            _utilitiesService.GameEventsService.AddSubscription(typeof(WarpedEventArgs).Name, Player_Warped);

        }

        public void Player_Warped(EventArgs ep)
        {
            WarpedEventArgs e = ep as WarpedEventArgs;
            if (e.NewLocation.NameOrUniqueName == forSaleSignLocation)
            {
                AddForSaleSign(e.NewLocation.NameOrUniqueName);
            }
            else if (isShowingForSaleBoard)
            {
                RemoveForSaleSign();
            }
        }
        private LargeTerrainFeature getLargeTerrainFeatureAt(int tileX, int tileY)
        {
            GameLocation gl = Game1.getLocationFromName(forSaleSignLocation);

            foreach (LargeTerrainFeature ltf in gl.largeTerrainFeatures)
            {
                if (ltf.getBoundingBox().Contains(tileX * 64 + 32, tileY * 64 + 32))
                {
                    return ltf;
                }
            }
            return null;
        }
        public void RemoveForSaleSign()
        {
            int x1 = 8;
            int y1 = 53;

            if (!isShowingForSaleBoard) return;

            logger.Log("Removing For Sale sign", LogLevel.Debug);

            GameLocation gl = Game1.getLocationFromName(forSaleSignLocation);

            gl.removeTile(x1, y1, "Buildings");
            gl.removeTile(x1, y1, "Buildings");
            gl.removeTile(x1 + 1, y1, "Buildings");
            gl.removeTile(x1 + 2, y1, "Buildings");
            gl.removeTileProperty(x1, y1, "Buildings", "Action");
            gl.removeTileProperty(x1 + 1, y1, "Buildings", "Action");
            gl.removeTileProperty(x1 + 2, y1 - 1, "Buildings", "Action");
            gl.removeTile(x1, y1 - 1, "Front");
            gl.removeTile(x1 + 1, y1 - 1, "Front");
            gl.removeTile(x1 + 2, y1 - 1, "Front");

            isShowingForSaleBoard = false;
        }
        public void AddForSaleSign(string sLocation)
        {
            if (_modDataService.Config.UseTownForSaleSign)
            {
                logger.Log("Check For Sale sign, location: " + sLocation, LogLevel.Debug);

                if (_modDataService.farmExpansions.Where(p => p.Value.Active).Count() >= _modDataService.MaximumExpansions)
                {
                    isShowingForSaleBoard = false;
                    logger.Log("All expansion slots full, no for sale sign added.", LogLevel.Debug);
                }
                else
                {
                    if (!isShowingForSaleBoard && _modDataService.LandForSale.Count > 0)
                    {
                        try
                        {
                            int x1 = 8;
                            int y1 = 53;

                            logger.Log("Adding sign", LogLevel.Debug);

                            GameLocation gl = Game1.getLocationFromName(forSaleSignLocation);

                            isShowingForSaleBoard = true;
                            LargeTerrainFeature bush = null;
                            do
                            {
                                bush = getLargeTerrainFeatureAt(21, 23);
                                if (bush != null)
                                {
                                    gl.largeTerrainFeatures.Remove(bush);
                                }
                            }
                            while (bush != null);
#if v169
                            string tileSheetId = gl.Map.TileSheets[2].Id;
                            gl.setMapTile(x1, y1, 2045, "Buildings", tileSheetId);
                            gl.setMapTile(x1 + 1, y1, 2046, "Buildings", tileSheetId);
                            gl.setMapTile(x1 + 2, y1, 2047, "Buildings", tileSheetId);
                            gl.setMapTile(x1, y1 - 1, 2013, "Front", tileSheetId);
                            gl.setMapTile(x1 + 1, y1 - 1, 2014, "Front", tileSheetId);
                            gl.setMapTile(x1 + 2, y1 - 1, 2015, "Front", tileSheetId);
#else
                        gl.setMapTileIndex(x1, y1, 2045, "Buildings", 2);
                        gl.setMapTileIndex(x1 + 1, y1, 2046, "Buildings", 2);
                        gl.setMapTileIndex(x1 + 2, y1, 2047, "Buildings", 2);
                        gl.setMapTileIndex(x1, y1 - 1, 2013, "Front", 2);
                        gl.setMapTileIndex(x1 + 1, y1 - 1, 2014, "Front", 2);
                        gl.setMapTileIndex(x1 + 2, y1 - 1, 2015, "Front", 2);
#endif
                            gl.setTileProperty(x1, y1, "Buildings", "Action", "prism99.sdr.ForSale");
                            gl.setTileProperty(x1 + 1, y1, "Buildings", "Action", "prism99.sdr.ForSale");
                            gl.setTileProperty(x1 + 2, y1 - 1, "Buildings", "Action", "prism99.sdr.ForSale");

                        }
                        catch (Exception ex)
                        {
                            logger.LogError("FEForSale.AddForSaleSign", ex);
                        }
                    }
                }
            }
        }
    }
}
