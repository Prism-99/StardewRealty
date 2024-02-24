using StardewModdingAPI.Events;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using xTile;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceInterfaces;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class MapsDataProvider : IGameDataProvider
    {
        public Dictionary<string,Map> _maps=new ();
        private IUtilitiesService _utilitiesService;
        private IExpansionManager _expansionManager;
        private IModDataService _modDataService;
        public override string Name => "Maps";
       
        public MapsDataProvider(SDRContentManager conMan, IUtilitiesService utilitiesService, IExpansionManager expansionManager, IModDataService modDataService)
        {
            //_maps = conMan.Maps;
            _maps = modDataService.BuildingMaps;
            _utilitiesService = utilitiesService;
            _expansionManager = expansionManager;
            _modDataService = modDataService;
        }
        public override void CheckForActivations()
        {
           
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            if (_maps.ContainsKey(e.NameWithoutLocale.Name))
            {
                e.LoadFrom(() => { return _maps[e.NameWithoutLocale.Name]; }, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.Name == "Maps/Backwoods")
            {
                HandleBackwoods(e);
            }
        }

        private void HandleBackwoods(AssetRequestedEventArgs e)
        {
            if (_expansionManager.expansionManager.GetActiveFarmExpansions().Any() || _utilitiesService.GameEnvironment.ExitsLoaded)
            {
                var mapPatch = _utilitiesService.GameEnvironment.GetFarmDetails(999999).PathPatches.Values.ToArray()[0];
                Map mExitPatchMap = _utilitiesService.MapLoaderService.LoadMap( Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "mappatchs", mapPatch.PatchMapName), "patchmap", false);
                //logger?.Log($"Getting Maps/Backwoods Map name {mExitPatchMap.Id}", LogLevel.Debug);
                //
                //   tried using PatchMap, but it did not remove the old layers
                //
                e.Edit(asset =>
                {
                    Map mapToEdit = asset.AsMap().Data;
                    logger.Log($"Adding Backwoods patch", StardewModdingAPI.LogLevel.Debug);
                    _utilitiesService.MapUtilities.PatchInMap(mapToEdit, mExitPatchMap, new Vector2(mapPatch.MapPatchPointX, mapPatch.MapPatchPointY));

                    if (_modDataService.MapGrid.Any())
                    {
                        //logger?.Log($"Applying backwoods patch", LogLevel.Debug);
                        //
                        //  add warps
                        //
                        var destinationPatch = _modDataService.validContents[_modDataService.MapGrid[0]].EntrancePatches["1"];

                        MapWarp[] backwoodOutWarps = mapPatch.GetWarpOutList(_modDataService.MapGrid[0]).ToArray();
                        MapWarp[] destInWarps = destinationPatch.GetWarpInList("Backwoods").ToArray();

                        string warpText = "";
                        int iPtr = 0;
                        foreach (MapWarp warp in backwoodOutWarps)
                        {
                            warpText += $"{warp.FromX} {warp.FromY} {_modDataService.MapGrid[0]} {destInWarps[iPtr].ToX} {destInWarps[iPtr].ToY} ";
                        }
                        if (mapToEdit.Properties.ContainsKey("Warp"))
                        {

                            mapToEdit.Properties["Warp"] = mapToEdit.Properties["Warp"] + " " + warpText.TrimEnd();
                        }
                        else
                        {
                            mapToEdit.Properties.Add("Warp", warpText.TrimEnd());
                        }
                    }
                    else
                    {
                        //
                        //  add path block
                        //
                        FarmDetails oFarm = _utilitiesService.GameEnvironment.GetFarmDetails(999999);
                        //logger?.Log($"Placing backwoods blocker patch", LogLevel.Debug);

                        EntrancePatch oBackWoodsPatch = oFarm.PathPatches["0"];
                        _utilitiesService.MapUtilities.AddPathBlock(new Vector2(oFarm.PathPatches["0"].PathBlockX, oFarm.PathPatches["0"].PathBlockY), mapToEdit, oBackWoodsPatch.WarpOrientation, oBackWoodsPatch.WarpOut.NumberofPoints);
                    }
                });
            }

        }
        public void AddMap(string mapName,Map map)
        {
            _maps.Add(mapName, map);
        }
        public override bool Handles(string assetName)
        {
            return assetName.StartsWith("Maps/");
        }
        public override void OnGameLaunched()
        {
           
        }
    }
}
