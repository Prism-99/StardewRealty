using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Characters;
using StardewValley.Pathfinding;
using StardewValley.GameData.Crops;
using StardewValley.TerrainFeatures;
using HarmonyLib;
using StardewValley.Buildings;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;


namespace SDV_Realty_Core.Framework.Patches.Characters
{
    internal  class FEJuminoHarvester
    {
        //
        //  version 1.6
        //
        private static ILoggerService logger;
        //Tuple< int,int> seed index, cost
        private  static  Dictionary<string, Tuple<string, int>> CropToSeeds = new Dictionary<string, Tuple<string, int>>();
        private static  FEConfig config;
        private static ExpansionManager expansionManager;
        private static IValleyStatsService _valleyStatsService;
        public  void Initialize(ILoggerService olog, FEConfig oconfig, IExpansionManager  expansionMan, IValleyStatsService valleyStatsService)
        {
            config = oconfig;
            logger = olog;
            expansionManager = expansionMan.expansionManager;
            _valleyStatsService=valleyStatsService;
        }
        public  void DumpSeeds()
        {
            logger.Log("$--------- Harvestor Crop Catalogue ----------", LogLevel.Info);
            logger.Log("$==============================================", LogLevel.Info);
            foreach (string key in CropToSeeds.Keys)
            {
                logger.Log($"{key} <- {CropToSeeds[key].Item1} [{CropToSeeds[key].Item2}g]", LogLevel.Info);
            }

        }

        public  void LoadSeeds(float discount)
        {
            try
            {
                CropToSeeds.Clear();
                Dictionary<string, CropData> game_data = Game1.content.Load<Dictionary<string, CropData>>("Data/Crops");
                foreach (string key in game_data.Keys)
                {
                     if (!string.IsNullOrEmpty(game_data[key].HarvestItemId) && !CropToSeeds.ContainsKey(game_data[key].HarvestItemId))
                    {
                        //
                        //  TODO: need to find where to get prices
                        //
                        CropToSeeds.Add(game_data[key].HarvestItemId,  Tuple.Create(key, (int)(5 * (1 - discount))));
                    }
                }
                //  add hay
                CropToSeeds.Add("178",  Tuple.Create("483", 5));
            }
            catch (Exception ex)
            {
                logger.LogError("LoadSeeds", ex);
            }
#if DEBUG
            DumpSeeds();
#endif
}

        public static bool update_pre(JunimoHarvester __instance, int __state, GameTime time, GameLocation location)
        {
            __state = -1;
            Traverse.Create(__instance).Field("lastItemHarvested").SetValue(null);
            //if (config.JunimoReseedCrop)
            //{
            //    if (__instance.currentLocation != null && __instance.currentLocation.terrainFeatures.ContainsKey(__instance.getTileLocation()))
            //    {
            //        var tf = __instance.currentLocation.terrainFeatures[__instance.getTileLocation()];

            //        if (tf != null && tf is HoeDirt hd)
            //        {
            //            if (hd.crop != null)
            //            {
            //                __state = hd.crop.netSeedIndex?.Value ?? -1;
            //            }
            //        }
            //    }
            //}
            //Item hitem = (Item)Traverse.Create(__instance).Field("lastItemHarvested").GetValue();
            //__state = hitem == null;
            //logger.Log($"   pre harvested {(hitem == null ? "none." : hitem.Name)}", LogLevel.Debug);

            return true;
        }
        public static void update_post(JunimoHarvester __instance, int __state, GameTime time, GameLocation location)
        {
            if (!Game1.IsMasterGame)
                return;
            //
            //  apply fees and re-seed
            //
            if (__instance.currentLocation != null && __instance.currentLocation.terrainFeatures.ContainsKey(__instance.Tile))
            {
                Item hitem = (Item)Traverse.Create(__instance).Field("lastItemHarvested").GetValue();
                if (hitem != null)
                {
                    int totalharvestfee = 0;
                    //
                    // item harvested
                    //
                    //
                    //  apply rain fee
                    //
                    if (Game1.isRaining)
                    {
                        totalharvestfee += config.JunimoRainFee;
                        if (config.RealTimeMoney)
                            Game1.MasterPlayer.Money -= config.JunimoRainFee;
                    }
                    //
                    //  check for winter fee
                    //
                    string season=null;
                    if (expansionManager.farmExpansions.ContainsKey(location.Name))
                    {
                        season = expansionManager.GetExpansionSeasonalOverride(location.Name);
                    }
                    if (string.IsNullOrEmpty(season))
                    {
                        season = Game1.currentSeason;
                    }
                    if (season.ToLower() == "winter")
                    {
                        //
                        //  deduct winter fee
                        //
                        totalharvestfee += config.JunimoWinterFee;
                        if (config.RealTimeMoney)
                            Game1.MasterPlayer.Money -= config.JunimoWinterFee;
                    }

                    _valleyStatsService.CropHarvested(1, totalharvestfee);

                    if (__instance.currentLocation.terrainFeatures.ContainsKey(__instance.Tile))
                    {
                        var tf = __instance.currentLocation.terrainFeatures[__instance.Tile];

                        if (tf != null && tf is HoeDirt hd)
                        {
                            if (hd.crop == null || hd.crop.dead.Value)
                            {
                                //
                                //  non-regrowing crop, find seed
                                //
                                if (config.JunimoReseedCrop && CropToSeeds.ContainsKey(hitem.ParentSheetIndex.ToString()))
                                {

                                    totalharvestfee = 0;
                                    string seedId = CropToSeeds[hitem.ParentSheetIndex.ToString()].Item1;
                                    if (config.JunimosChargeForSeeds)
                                    {
                                        //  deduct cost of the seed
                                        totalharvestfee += CropToSeeds[hitem.ParentSheetIndex.ToString()].Item2;
                                        if (config.RealTimeMoney)
                                            Game1.MasterPlayer.Money -= CropToSeeds[hitem.ParentSheetIndex.ToString()].Item2;
                                    }
                                    //  apply seeding fee
                                    totalharvestfee += config.JunimosFeeForSeeding;
                                    if (config.RealTimeMoney)
                                        Game1.MasterPlayer.Money -= config.JunimosFeeForSeeding;
                                    _valleyStatsService.CropSeeded(1, totalharvestfee);
                                    //  create crop

                                    hd.crop = new Crop(seedId,(int) __instance.Tile.X,(int) __instance.Tile.Y, __instance.currentLocation);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool foundCropEndFunction(JunimoHarvester __instance, PathNode currentNode, Point endPoint, GameLocation location, Character c, ref bool __result)
        {
            if (location.isCropAtTile(currentNode.x, currentNode.y) && (location.terrainFeatures[new Vector2(currentNode.x, currentNode.y)] as HoeDirt).readyForHarvest())
            {
                __result = true;
            }
            else if (location.terrainFeatures.ContainsKey(new Vector2(currentNode.x, currentNode.y)) && location.terrainFeatures[new Vector2(currentNode.x, currentNode.y)] is Bush && (int)(location.terrainFeatures[new Vector2(currentNode.x, currentNode.y)] as Bush).tileSheetOffset.Value == 1)
            {
                __result = true;
            }
            else
            {
                __result = false;
            }

            return false;
        }
        public static bool JunimoHarvester_Gethome(JunimoHarvester __instance, ref JunimoHut __result)
        {
            Guid home = (Guid)Traverse.Create(__instance).Field("netHome").Field("value").GetValue();// .SetValue(2000);

            __result = GetBuidlingFromGUID(home);

            return false;
        }
        private static JunimoHut GetBuidlingFromGUID(Guid bGuild)
        {
            if (Game1.getFarm().buildings.ContainsGuid(bGuild))
            {
                return (JunimoHut)Game1.getFarm().buildings[bGuild];
            }
            var expansions = expansionManager.farmExpansions.Where(p => p.Value.Active).Select(p => p.Value).ToList();

            foreach (var exp in expansions)
            {
                if (exp.buildings.ContainsGuid(bGuild))
                {
                    return (JunimoHut)exp.buildings[bGuild];
                }
            }

            return null;
        }
        private static Guid GetBuildingGuid(JunimoHut __instance)
        {
            Guid buildGuid = Game1.getFarm().buildings.GuidOf(__instance);
            if (buildGuid != Guid.Empty)
                return buildGuid;

            var expansions = expansionManager.farmExpansions.Where(p => p.Value.Active).Select(p => p.Value).ToList();

            foreach (var exp in expansions)
            {
                buildGuid = exp.buildings.GuidOf(__instance);
                if (buildGuid != Guid.Empty)
                    return buildGuid;
            }

            return Guid.Empty;
        }
        public static bool JunimoHarvester_SetHome(JunimoHarvester __instance, JunimoHut value)
        {
            Guid buildingGuid = GetBuildingGuid(value);

            Traverse.Create(__instance).Field("netHome").Field("value").SetValue(buildingGuid);

            return false;
        }
        //    get
        //    {
        //        return Game1.getFarm().buildings[netHome.Value] as JunimoHut;
        //    }
        //    set
        //    {
        //        netHome.Value = Game1.getFarm().buildings.GuidOf(value);
        //    }
        //}
        public static bool tryToHarvestHere(JunimoHarvester __instance)
        {
            //if (!__instance.modData.ContainsKey(IModDataKeysService.FELocationName))
            //{
            //    return true;
            //}

            if (__instance.currentLocation != null)
            {
                if (isHarvestable(__instance))
                {
                    Traverse.Create(__instance).Field("harvestTimer").SetValue(2000);
                }
                else
                {
                    __instance.pokeToHarvest();
                }
            }

            return false;
        }
        private static bool isHarvestable(JunimoHarvester __instance)
        {
            if (__instance.currentLocation.terrainFeatures.ContainsKey(__instance.Tile) && __instance.currentLocation.terrainFeatures[__instance.Tile] is HoeDirt && (__instance.currentLocation.terrainFeatures[__instance.Tile] as HoeDirt).readyForHarvest())
            {
                return true;
            }

            if (__instance.currentLocation.terrainFeatures.ContainsKey(__instance.Tile) && __instance.currentLocation.terrainFeatures[__instance.Tile] is Bush && (int)(__instance.currentLocation.terrainFeatures[__instance.Tile] as Bush).tileSheetOffset.Value == 1)
            {
                return true;
            }

            return false;
        }
        public static bool JunimoHarvestor_update_Prefix(GameTime time, ref GameLocation location, ref JunimoHarvester __instance)
        {
            try
            {
                // monitor.Log($"Harvertor_update, loc {location.name}", LogLevel.Info);
                Farm farmexp = Game1.getLocationFromName("FarmExpansion") as Farm;

                //farmexp.

                if (Game1.currentLocation.Name == "FarmExpansion")
                {
                    location = Game1.currentLocation;
                    return true;
                }
                else
                {
                    return true;
                }
                // int xx = 1;
                //    if (magnetRangeMult <= 0)
                //        __result = true;
                //    else __result = Math.Abs(position.X + 32f - (float)farmer.getStandingX()) <= (float)farmer.MagneticRadius * magnetRangeMult && Math.Abs(position.Y + 32f - (float)farmer.getStandingY()) <= (float)farmer.MagneticRadius * magnetRangeMult;
                //    return false;
            }
            catch (Exception ex)
            {
                logger.Log($"Failed in {JunimoHarvestor_update_Prefix}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
        public static void JunimoHarvester_JunimoHarvester_Prefix()
        {
        }
    }
}
