using Microsoft.Xna.Framework.Content;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using StardewModdingAPI.Events;
using StardewModHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xTile.Tiles;
using xTile;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceProviders.ModData;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class ExpansionBridgingService : IExpansionBridgingService
    {
        private IUtilitiesService _utilitiesService;
        public const string BridgingLocationName = "sdr.expansion.bridge";
        private string bridgeMapName = "bridgearea.tmx";
        public GameLocation ExpansionBridgeLocation = null;
        private IExpansionManager _expansionManager;
        private IContentManagerService contentManagerService;
        private IGridManager gridManager;
        public static EntrancePatch WesternExit;
        public static EntrancePatch NorthernExit;
        public static EntrancePatch SouthernExit;
        Map bridgeMap;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IExpansionManager),
            typeof(IContentManagerService),typeof(IGridManager),
            typeof(IAutoMapperService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;
            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _utilitiesService = (IUtilitiesService)args[0];
            _expansionManager = (IExpansionManager)args[1];
            contentManagerService = (IContentManagerService)args[2];
            gridManager = (IGridManager)args[3];

            _utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, HandleContentLoaded);
            _utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), GameLoop_SaveLoaded);
            _utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.PreSave, HandlePreSave);
            LoadAreaMap((IAutoMapperService)args[4]);
        }
        private void LoadAreaMap(IAutoMapperService autoMapperService)
        {
            bridgeMap = _utilitiesService.MapLoaderService.LoadMap(Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", BridgingLocationName, bridgeMapName).Replace(_utilitiesService.GameEnvironment.GamePath, ""), BridgingLocationName, false, false);

            ExpansionPack autoParse = autoMapperService.ParseMap(bridgeMap);

            autoParse.EntrancePatches.TryGetValue(((int)EntranceDirection.North).ToString(), out NorthernExit);
            autoParse.EntrancePatches.TryGetValue(((int)EntranceDirection.South).ToString(), out SouthernExit);
            autoParse.EntrancePatches.TryGetValue(((int)EntranceDirection.West).ToString(), out WesternExit);
        }
        private void HandlePreSave()
        {
            RemoveBridgeArea();
        }
        public void RemoveBridgeArea()
        {
            if (Game1.getLocationFromName(BridgingLocationName) != null)
            {
                logger.LogDebug($"   Warproom removed.");
                if (ExpansionBridgeLocation == null)
                {
                    ExpansionBridgeLocation = Game1.getLocationFromName(BridgingLocationName);
                }
                Game1.locations.Remove(Game1.getLocationFromName(BridgingLocationName));
                Game1._locationLookup.Remove(BridgingLocationName);
                //SetWarpRoomStatus(false);
            }
        }
        private void GameLoop_SaveLoaded(EventArgs e)
        {
            AddBridge();
        }
        private void HandleContentLoaded()
        {
            //
            //  only add warproom if any expansion loaded
            //
            if (_utilitiesService.HaveExpansions)
            {
                _utilitiesService.GameEventsService.AddSubscription("WarpedEventArgs", HandlePlayerWarped);
                //_utilitiesService.GameEventsService.AddSubscription(new UpdateTickingEventArgs(), GameLoop_UpdateTicking);
                //_utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), GameLoop_SaveLoaded);
                //_utilitiesService.GameEventsService.AddSubscription(new SavedEventArgs(), GameLoop_SaveLoaded);
                //_utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), LoadStageChanged);

                //_utilitiesService.CustomEventsService.AddCustomSubscription("ClientDisconnected", ClientDisconnected);
                //
                // warp room patch
                //
                //_utilitiesService.PatchingService.patches.AddPatch(true, typeof(GameLocation), "isCollidingWithWarp",
                //    new Type[] { typeof(Rectangle), typeof(Character) }, typeof(WarproomManager), nameof(isCollidingWithWarp_Prefix),
                //    "Check warproom availability.",
                //    "Warps");

                //_utilitiesService.PatchingService.ApplyPatches("Warps");
            }
        }
        public void AddBridge()
        {
#if DEBUG
            logger.LogDebug("Adding Adding Expansion Bridge");
            logger.LogDebug($"  Game Location loaded: {Game1.getLocationFromName(BridgingLocationName) != null}");
            logger.LogDebug($"  Expansion Bridge cached: {ExpansionBridgeLocation != null}");
            logger.LogDebug($"  Master game: {Game1.IsMasterGame}");
            logger.LogDebug($"  Split screen: {Context.IsSplitScreen}");
#endif
            //
            //  added isplitscreen for mplayer support
            //
            if (!Game1.locations.Where(p => p.Name == BridgingLocationName).Any())// && (Game1.IsMasterGame || Context.IsSplitScreen))
            {
                //
                //  add bridge area map
                //
                if (ExpansionBridgeLocation != null)
                {
                    _expansionManager.expansionManager.AddGameLocation(ExpansionBridgeLocation, "AddBridge");
                }
                else
                {
                    string assetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", BridgingLocationName, bridgeMapName));

                    //
                    //  this is all loaded by AddWarproomDefinition
                    //
                    //ContentManager.ExternalReferences.Add(assetPath, warpmap);
                    contentManagerService.ExternalReferences[assetPath] = bridgeMap;
                    //if (contentManagerService.contentManager.ExternalReferences.ContainsKey(assetPath))
                    //{
                    //    contentManagerService.contentManager.ExternalReferences.Remove(assetPath);
                    //}
                    //contentManagerService.contentManager.ExternalReferences.Add(assetPath, warpmap);
                    ////
                    ////  add warproom tilesheet sources
                    ////
                    foreach (TileSheet tileSheet in bridgeMap.TileSheets)
                    {
                        tileSheet.ImageSource = tileSheet.ImageSource.Replace("\\", FEConstants.AssetDelimiter);
                        if (!string.IsNullOrEmpty(tileSheet.ImageSource) && tileSheet.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}{BridgingLocationName}"))
                        {
                            string sFilename = Path.GetFileName(tileSheet.ImageSource);
                            string sFullname = Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", BridgingLocationName, sFilename);

                            if (!contentManagerService.ExternalReferences.ContainsKey(tileSheet.ImageSource))
                            {
                                contentManagerService.ExternalReferences.Add(tileSheet.ImageSource, new StardewBitmap(sFullname).Texture());
                            }
                        }
                    }

                    //WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
                    //
                    //  still need to re-add bridge location
                    //  LocationsData setting only loads the bridge location at the
                    //  start of the game
                    //
                    ExpansionBridgeLocation = new GameLocation(assetPath, BridgingLocationName);
                    ExpansionBridgeLocation.modData.Add(IModDataKeysService.FEExpansionDisplayName, "Expansion Bridge");
                    ExpansionBridgeLocation.mapPath.Set($"SDR\\{BridgingLocationName}\\{bridgeMapName}");
                    ExpansionBridgeLocation.uniqueName.Value = BridgingLocationName;
                    ExpansionBridgeLocation.name.Value = BridgingLocationName;

                    _expansionManager.expansionManager.AddGameLocation(ExpansionBridgeLocation, "AddBridge");
                    //ExpansionBridgeLocation = bridgeLocation;
                }
            }

        }

        private void HandlePlayerWarped(EventArgs eParam)
        {
            WarpedEventArgs e = (WarpedEventArgs)eParam;

            if (e.NewLocation.Name == BridgingLocationName)
            {
                if (_expansionManager.expansionManager.farmExpansions.TryGetValue(e.OldLocation.Name, out var farmExpansion))
                {
                    ExpansionBridgeLocation.warps.Clear();
                    // need to get where they warped from
                    if (gridManager.TryGetLocationNeighbours(e.OldLocation.Name, out var neighbours))
                    {

                        if (farmExpansion.GridId == 0 || (farmExpansion.GridId + 2) % 3 == 0)
                        {
                            // top row warp straight to the east
                            // should never happen, should be
                            // handled whe expansion is mapped in
                        }
                        else
                        {
                            // get north neighbour
                            if (neighbours.North != null)
                            {
                                if (neighbours.North.EntrancePatches.TryGetValue("2", out var entrancePatches))
                                {
                                    for (int index = 0; index < entrancePatches.WarpIn.NumberofPoints; index++)
                                    {
                                        if (entrancePatches.WarpOrientation == WarpOrientations.Vertical)
                                        {
                                            ExpansionBridgeLocation.warps.Add(new Warp(
                                       NorthernExit.WarpOut.X, NorthernExit.WarpOut.Y + index, neighbours.North.LocationName,
                                               entrancePatches.WarpIn.X,
                                               entrancePatches.WarpIn.Y + index,
                                               true
                                            ));
                                        }
                                        else
                                        {
                                            ExpansionBridgeLocation.warps.Add(new Warp(
                                              NorthernExit.WarpOut.X + index, NorthernExit.WarpOut.Y, neighbours.North.LocationName,
                                              entrancePatches.WarpIn.X + index,
                                              entrancePatches.WarpIn.Y,
                                              true
                                           ));
                                        }

                                    }
                                }
                            }
                            // get west neighbour
                            if (neighbours.West != null)
                            {
                                if (neighbours.West.EntrancePatches.TryGetValue("1", out var entrancePatches))
                                {
                                    for (int index = 0; index < entrancePatches.WarpIn.NumberofPoints; index++)
                                    {
                                        if (entrancePatches.WarpOrientation == WarpOrientations.Vertical)
                                        {
                                            ExpansionBridgeLocation.warps.Add(new Warp(
                                               WesternExit.WarpOut.X, WesternExit.WarpOut.Y + index, neighbours.West.LocationName,
                                               entrancePatches.WarpIn.X,
                                               entrancePatches.WarpIn.Y + index,
                                               true
                                            ));
                                        }
                                        else
                                        {
                                            ExpansionBridgeLocation.warps.Add(new Warp(
                                              WesternExit.WarpOut.X + index, WesternExit.WarpOut.Y, neighbours.West.LocationName,
                                              entrancePatches.WarpIn.X + index,
                                              entrancePatches.WarpIn.Y,
                                              true
                                           ));
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
