using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System.IO;
using System.Linq;
using xTile;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class ExitsService : IExitsService
    {
        private IGridManager _gridManager;
        private IContentPackService _contentPackService;
        private IModDataService _modDataService;
        private IMapUtilities _mapUtilities;
        private ContentPackLoader _contentPackLoader;
        private IUtilitiesService _utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGridManager),typeof(IContentPackService),
            typeof(IModDataService),typeof(IMapUtilities),
            typeof(IUtilitiesService)
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
            _gridManager = (IGridManager)args[0];
            _contentPackService = (IContentPackService)args[1];
            _modDataService = (IModDataService)args[2];
            _mapUtilities = (IMapUtilities)args[3];
            _utilitiesService = (IUtilitiesService)args[4];

            _contentPackLoader = _contentPackService.contentPackLoader;

            _utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), DayStarted);
            _utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0,0), GameLoaded,100);
        }
        private void GameLoaded(EventArgs ep)
        {
            LoadStageChangedEventArgs e=(LoadStageChangedEventArgs)ep;
            if (e.NewStage == LoadStage.SaveAddedLocations || e.NewStage == LoadStage.CreatedInitialLocations)
            {
                AddFarmExits();
            }
        }
        public override void AddMapExitBlockers(int iGridId)
        {
            //
            //  An Expansion should have an exit on each of the 4 map sides
            //  add path blocks to each of the 4 exits
            //
            string sExpansionName = _gridManager.MapGrid[iGridId];
            GameLocation glNewExp = Game1.getLocationFromName(sExpansionName);
            foreach (EntrancePatch oPatch in _contentPackService.contentPackLoader.ValidContents[sExpansionName].EntrancePatches.Values)
            {
                if ((oPatch.PatchSide != EntranceDirection.East) || (iGridId == 2 && !_utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit1) || (iGridId == 3 && !_utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit2))
                {
                    _utilitiesService.MapUtilities.AddPathBlock(new Vector2(oPatch.PathBlockX, oPatch.PathBlockY), glNewExp.map, oPatch.WarpOrientation, oPatch.WarpOut.NumberofPoints);
                }
            }
        }
        private string GetNeighbourSide(EntranceDirection side)
        {
            return side switch
            {
                EntranceDirection.West => IMapUtilities.eastSidePatch,
                EntranceDirection.East => IMapUtilities.westSidePatch,
                EntranceDirection.North => IMapUtilities.southSidePatch,
                EntranceDirection.South => IMapUtilities.northSidePatch,
                _ => ""
            };
        }


        public override void AddFarmExits()
        {
            logger.Log("Adding Farm exits", LogLevel.Debug);

            List<Tuple<string, KeyValuePair<string, EntrancePatch>>> lExitsToMap = new List<Tuple<string, KeyValuePair<string, EntrancePatch>>> { };


            switch (Game1.whichFarm)
            {
                case int iFarm when iFarm < 9:
                    if (_utilitiesService.GameEnvironment.ActiveFarmProfile.PatchFarmExits)
                    {
                        logger.Log("Adding farm side exits.", LogLevel.Debug);
                        //
                        //  add farm map exits
                        //
                        foreach (KeyValuePair<string, EntrancePatch> oPatches in _utilitiesService.GameEnvironment.GetFarmDetails(iFarm).PathPatches.ToList())
                        {
                            lExitsToMap.Add(Tuple.Create(_utilitiesService.GameEnvironment.GetFarmDetails(iFarm).MapName, oPatches));
                        }
                    }

                    break;
            }
            logger.Log($"     # of Exit Maps: {lExitsToMap.Count}", LogLevel.Debug);

            foreach (Tuple<string, KeyValuePair<string, EntrancePatch>> oExitPatch in lExitsToMap)
            {
                if (oExitPatch.Item2.Key == "0" && _utilitiesService.ConfigService.config.useNorthWestEntrance || oExitPatch.Item2.Key == "1" && _utilitiesService.ConfigService.config.useSouthWestEntrance)
                {
                    logger.Log($"Adding exits to '{oExitPatch.Item1}'", LogLevel.Debug);
                    Map mExitPatchMap = _utilitiesService.MapLoaderService.LoadMap( Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "mappatchs", oExitPatch.Item2.Value.PatchMapName), "patchmap", false);
                    GameLocation glExitFarm = Game1.getLocationFromName(oExitPatch.Item1);
                    _mapUtilities.PatchInMap(glExitFarm, mExitPatchMap, new Vector2(oExitPatch.Item2.Value.MapPatchPointX, oExitPatch.Item2.Value.MapPatchPointY));
                    _utilitiesService.MapUtilities.AddPathBlock(new Vector2(oExitPatch.Item2.Value.PathBlockX, oExitPatch.Item2.Value.PathBlockY), glExitFarm.map, oExitPatch.Item2.Value.WarpOrientation, oExitPatch.Item2.Value.WarpOut.NumberofPoints);
                }
            }
            _utilitiesService.GameEnvironment.ExitsLoaded = true;
        }
        internal override string GetNeighbourExpansionTTId(int iGridId, EntranceDirection side)
        {
            string expName = _gridManager.GetNeighbourExpansionName(iGridId, side);
            if (expName == null)
                return null;
            return $"{_contentPackLoader.ValidContents[expName].Owner.Manifest.UniqueID}.{expName}/{_contentPackLoader.ValidContents[expName].Owner.Manifest.UniqueID}.{expName}.tt";
        }
        internal void DayStarted(EventArgs e)
        {
            PatchBackwoods();
        }
        public void PatchBackwoods()
        {
            if (_utilitiesService.GameEnvironment.ActiveFarmProfile.PatchBackwoodExits)
            {
                logger.Log("Patching in Backwood exits", LogLevel.Debug);

                _utilitiesService.InvalidateCache("Maps/Backwoods");

                //if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
                //{
                //    FEUtility.InvalidateCache("Maps/Backwoods");
                //}
                //else
                //{
                //    string langCode = Game1.content.LanguageCodeString(Game1.content.GetCurrentLanguage());
                //    FEUtility.InvalidateCache($"Maps/Backwoods.{langCode}");
                //}

            }
        }

        public void AddAllMapExitBlockers(string sExpansionName)
        {
            //
            //  An Expansion should have an exit on each of the 4 map sides
            //  add path blocks to each of the 4 exits
            //
            GameLocation glNewExp = Game1.getLocationFromName(sExpansionName);
            int iGridId = _modDataService.farmExpansions[sExpansionName].GridId;

            foreach (EntrancePatch oPatch in _contentPackService.contentPackLoader.ValidContents[sExpansionName].EntrancePatches.Values)
            {
                //if ((oPatch.PatchSide != EntranceDirection.East) || (iGridId == 2 && !ActiveFarmProfile.UseFarmExit1) || (iGridId == 3 && !ActiveFarmProfile.UseFarmExit2))
                //{
                _utilitiesService.MapUtilities.AddPathBlock(new Vector2(oPatch.PathBlockX, oPatch.PathBlockY), glNewExp.map, oPatch.WarpOrientation, oPatch.WarpOut.NumberofPoints);

                //
                // check for neighbour
                //
                if ((iGridId == 2 || iGridId == 3) && oPatch.PatchSide == EntranceDirection.East)
                {
                    //
                    //  check for farm patch
                }
                else
                {
                    EntrancePatch patchNeighbour = null;
                    string neighbour = _gridManager.GetNeighbourExpansionName(iGridId, oPatch.PatchSide);
                    GameLocation glNeighbour = null;
                    if (!string.IsNullOrEmpty(neighbour))
                    {
                        string opside = GetNeighbourSide(oPatch.PatchSide);
                        patchNeighbour = _contentPackService.contentPackLoader.ValidContents[neighbour].EntrancePatches[opside];
                        glNeighbour = Game1.getLocationFromName(neighbour);
                    }
                    //
                    //
                    //  remove any sign in patch side
                    //
                    if (oPatch.Sign?.UseSign ?? false)
                    {
                        //
                        //  need to remove local sign post map pieces
                        //  and remove action tile properties
                        _utilitiesService.MapUtilities.RemoveSignPost(glNewExp, oPatch, _utilitiesService.MapUtilities.GetSignPostMessagePrefix(oPatch.PatchSide));
                        //
                        //  remove side neighbours sign
                        //
                        if (patchNeighbour != null)
                        {
                            _utilitiesService.MapUtilities.RemoveSignPost(glNeighbour, patchNeighbour, _utilitiesService.MapUtilities.GetSignPostMessagePrefix(patchNeighbour.PatchSide));
                        }
                    }
                    //
                    //  remove local warps
                    //
                    _utilitiesService.MapUtilities.RemovePatchWarps(oPatch, glNewExp);
                    if (patchNeighbour != null)
                    {
                        //
                        //  need to remove warps
                        //  from neighbouring expansions
                        //
                        _utilitiesService.MapUtilities.RemovePatchWarps(patchNeighbour, glNeighbour);
                    }

                }
            }

        }
    }
}
