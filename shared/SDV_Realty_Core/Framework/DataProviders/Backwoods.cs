using StardewModdingAPI.Events;
using System.IO;
using System.Linq;
using xTile;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class Backwoods : IGameDataProvider
    {
        public override string Name => "Maps/Backwoods";
        private IMapLoaderService _mapLoaderService;
        private IExpansionManager _expansionManager;
        private IGameEnvironmentService _gameEnvironmentService;
        private IMapUtilities _mapUtilities;
        private IModDataService _modDataService;
        public Backwoods(IMapLoaderService mapLoaderService, IExpansionManager expansionManager, IGameEnvironmentService gameEnvironmentService, IMapUtilities mapUtilities, IModDataService modDataService)
        {

            _mapLoaderService = mapLoaderService;
            _expansionManager = expansionManager;
            _gameEnvironmentService = gameEnvironmentService;
            _mapUtilities = mapUtilities;
            _modDataService = modDataService;
        }

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            if (_expansionManager.expansionManager.GetActiveFarmExpansions().Any() || _gameEnvironmentService.ExitsLoaded)
            {
                var mapPatch = _gameEnvironmentService.GetFarmDetails(999999).PathPatches.Values.ToArray()[0];
                Map mExitPatchMap = _mapLoaderService.LoadMap(Path.Combine(_gameEnvironmentService.ModPath, "data", "mappatchs", mapPatch.PatchMapName).Replace(_gameEnvironmentService.GamePath, ""), "patchmap", false);
                //
                //   tried using PatchMap, but it did not remove the old layers
                //
                e.Edit(asset =>
                {
                    Map mapToEdit = asset.AsMap().Data;
                    logger.Log($"Adding Backwoods patch", StardewModdingAPI.LogLevel.Debug);
                    _mapUtilities.PatchInMap(mapToEdit, mExitPatchMap, new Vector2(mapPatch.MapPatchPointX, mapPatch.MapPatchPointY));

                    if (_modDataService.MapGrid.Any())
                    {
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
                        FarmDetails oFarm = _gameEnvironmentService.GetFarmDetails(999999);

                        EntrancePatch oBackWoodsPatch = oFarm.PathPatches["0"];
                        _mapUtilities.AddPathBlock(new Vector2(oFarm.PathPatches["0"].PathBlockX, oFarm.PathPatches["0"].PathBlockY), mapToEdit, oBackWoodsPatch.WarpOrientation, oBackWoodsPatch.WarpOut.NumberofPoints);
                    }
                });
            }

        }

        public override void OnGameLaunched()
        {

        }
    }
}
