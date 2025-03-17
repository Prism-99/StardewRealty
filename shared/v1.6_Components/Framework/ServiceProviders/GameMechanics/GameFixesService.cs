using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using StardewModdingAPI.Events;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Buildings;
using System;
using System.Linq;
using xTile.Layers;
using xTile.Dimensions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class GameFixesService : IGameFixesService
    {
        private static IGameEventsService _gameEventsService;
        private static IModDataService _modDataService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IPatchingService),
            typeof(IUtilitiesService),typeof(IModDataService)
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

            _gameEventsService = (IGameEventsService)args[0];
            IPatchingService patchingService = (IPatchingService)args[1];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[2];
            _modDataService = (IModDataService)args[3];
            //
            //  convert new building custom objects from objects to custom objects
            //  Chests and Casks
            //
            _gameEventsService.AddSubscription(typeof(BuildingListChangedEventArgs).Name, FixBuildingAutoAddedObjects);

            // stop weeds from spawning on path tiles
            patchingService.patches.AddPatch(true, typeof(GameLocation), "loadWeeds",
    null, typeof(GameFixesService), nameof(LoadWeeds_Prefix),
    "Stop weeds at start of year of spawning on paths", "Gamelocation");
            //
            //  fixes to allow getting hay from hoppers
            //
            patchingService.patches.AddPatch(false, typeof(SDObject), "actionOnPlayerEntry",
                null, typeof(GameFixesService), nameof(SDObject_actionOnPlayerEntry_Post),
                "Fix showing hay icon upon entry to animal house", "GameFixes");

            patchingService.patches.AddPatch(true, typeof(SDObject), "CheckForActionOnFeedHopper",
                new Type[] { typeof(Farmer), typeof(bool) }, typeof(GameFixesService), nameof(SDObject_CheckForActionOnFeedHopper_Prefix),
                "Fix taking hay out of hopper", "GameFixes");

            patchingService.patches.AddPatch(false, typeof(SDObject), "performDropDownAction",
                new Type[] { typeof(Farmer) }, typeof(GameFixesService), nameof(SDObject_performDropDownAction_Post),
                "Fix to set hay icon visible after adding hay", "GameFixes");


            patchingService.patches.AddPatch(true, typeof(SDObject), "performObjectDropInAction",
                new Type[] {typeof(Item),typeof(bool),typeof(Farmer),typeof(bool) }, typeof(GameFixesService), nameof(SDObject_performObjectDropInAction_Prefix),
                "Fix to add hopper hay to any silo", "GameFixes");

            patchingService.ApplyPatches("GameFixes");
            //
            //  Check to see if Chests Anywhere is installed
            //  -added in 1.4.3
            //
            //if (utilitiesService.ModHelperService.modHelper.ModRegistry.IsLoaded("Pathoschild.ChestsAnywhere"))
            //{
            //    //
            //    //  change ChestAnywhere from using Name to DisplayName, when possible
            //    //
            //    //Type chestFactory = Type.GetType("Pathoschild.Stardew.ChestsAnywhere.ChestFactory,ChestsAnywhere");
            //    //patchingService.patches.AddPatch(true, chestFactory, "GetCategory",
            //    //  new Type[] { typeof(GameLocation) }, typeof(GameFixesService)
            //    //  , nameof(GetCategory), "Fix default ChestsAnywhere category",
            //    //  "Mechanics");
            //}
        }
        private static bool IsThereHay()
        {
            if (Game1.getFarm().piecesOfHay.Value > 0)
                return true;

            return Game1.locations.Where(p => p.IsBuildableLocation() && p.piecesOfHay.Value > 0).Any();
        }
        private static bool IsThereAnyHayCapacity()
        {
            if (Game1.getFarm().GetHayCapacity() > 0)
                return true;

            return Game1.locations.Where(p => p.IsBuildableLocation() && p.GetHayCapacity() > 0).Any();
        }
        private static bool SDObject_performObjectDropInAction_Prefix(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, SDObject __instance, ref bool __result)
        {
            if (!_modDataService.Config.FixHoppers)
                return true;

            if (__instance.QualifiedItemId == "(BC)99" && dropInItem.QualifiedItemId == "(O)178")
            {
                if (__instance.isTemporarilyInvisible)
                {
                    __result = false;
                    return false;
                }
                if (!(dropInItem is SDObject dropIn))
                {
                    __result = false;
                    return false;
                }
                if (!IsThereAnyHayCapacity())
                {
                    if (SDObject.autoLoadFrom == null && !probe)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
                        __result = false;
                    }
                }
                else if (probe)
                {
                    __result = true;
                }
                else
                {
                    __instance.Location.playSound("Ship");
                    DelayedAction.playSoundAfterDelay("grassyStep", 100);
                    if (dropIn.Stack == 0)
                    {
                        dropIn.Stack = 1;
                    }
                    dropIn.Stack = GameLocation.StoreHayInAnySilo(dropIn.Stack, __instance.Location);
                    __instance.showNextIndex.Value = IsThereHay();
                    __result = dropIn.Stack <= 0;
                }
                return false;
            }

            return true;
        }
        private static void SDObject_performDropDownAction_Post(Farmer who, SDObject __instance)
        {
            if (_modDataService.Config.FixHoppers && __instance.QualifiedItemId == "(BC)99")
            {
                __instance.showNextIndex.Value = IsThereHay();
            }
        }
        private static bool SDObject_CheckForActionOnFeedHopper_Prefix(Farmer who, bool justCheckingForActivity, SDObject __instance, ref bool __result)
        {
            if (!_modDataService.Config.FixHoppers)
                return true;

            if (justCheckingForActivity)
            {
                __result = true;
                return false;
            }
            if (who.ActiveObject != null)
            {
                __result = false;
                return false;
            }
            if (who.freeSpotsInInventory() > 0)
            {
                if (IsThereHay())
                {
                    if (__instance.Location is AnimalHouse i)
                    {
                        int hayNeeded = i.animalsThatLiveHere.Count - i.numberOfObjectsWithName("Hay");
                        while (IsThereHay() && hayNeeded > 0)
                        {
                            if (Game1.player.couldInventoryAcceptThisItem("(O)178", 1))
                            {
                                GameLocation.GetHayFromAnySilo(__instance.Location);
                                who.addItemToInventoryBool(ItemRegistry.Create("(O)178", 1));
                                Game1.playSound("shwip");
                                hayNeeded--;
                            }
                            else
                            {
                                hayNeeded = 0;
                            }
                        }
                    }
                    else if (Game1.player.couldInventoryAcceptThisItem("(O)178", 1))
                    {
                        SDObject hay = GameLocation.GetHayFromAnySilo(__instance.Location);
                        if (hay != null)
                        {
                            who.addItemToInventoryBool(ItemRegistry.Create("(O)178"));
                            Game1.playSound("shwip");
                        }
                    }
                    __instance.showNextIndex.Value = IsThereHay();
                    __result = true;
                }
                else
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                    __result = true;
                }
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                __result = true;
            }
            return false;
        }
        private static void SDObject_actionOnPlayerEntry_Post(SDObject __instance)
        {
            if (_modDataService.Config.FixHoppers && __instance.QualifiedItemId == "(BC)99")
            {
                __instance.showNextIndex.Value = IsThereHay();
            }
        }
        private void FixBuildingAutoAddedObjects(EventArgs ep)
        {
            //
            //  Auto Added building objects are added as SDObject
            //  some need to be added as custom objects
            //      * Casks
            //      * Chests
            //
            if (Game1.IsMasterGame)
            {
                BuildingListChangedEventArgs e = (BuildingListChangedEventArgs)ep;
                if (e.Added != null)
                {
                    foreach (Building buildingAdded in e.Added)
                    {
                        if (buildingAdded.indoors.Value != null)
                        {
                            //
                            //  check for chests added as objects
                            //
                            var chests = buildingAdded.indoors.Value.Objects.Values.Where(p => p.ItemId == "130" && p is not Chest).ToList();

                            foreach (SDObject chest in chests)
                            {
                                buildingAdded.indoors.Value.objects.Remove(chest.TileLocation);
                                buildingAdded.indoors.Value.objects.Add(chest.TileLocation, new Chest(true, chest.TileLocation));
                            }
                            //
                            //  check for casks added as objects
                            //
                            var casks = buildingAdded.indoors.Value.Objects.Values.Where(p => p.ItemId == "163" && p.bigCraftable.Value && p is not Cask).ToList();

                            foreach (var cask in casks)
                            {
                                buildingAdded.indoors.Value.objects.Remove(cask.TileLocation);
                                buildingAdded.indoors.Value.objects.Add(cask.TileLocation, new Cask(cask.TileLocation));
                            }
                        }
                    }
                }
            }
        }
        public static bool LoadWeeds_Prefix(GameLocation __instance)
        {
            if (!Game1.IsMasterGame)
                return true;

            //logger.Log($"loadWeeds called.", LogLevel.Debug);
            if ((bool)_gameEventsService.GetProxyValue("IsExpansion", new object[] { __instance.Name }))
            {
                Layer pathsLayer = __instance.map?.GetLayer("Paths");
                if (pathsLayer == null)
                    return false;

                for (int x = 0; x < __instance.map.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < __instance.map.Layers[0].LayerHeight; y++)
                    {
                        int tileIndex = pathsLayer.GetTileIndexAt(x, y);
                        if (tileIndex == -1)
                        {
                            continue;
                        }
                        Vector2 tile = new Vector2(x, y);
                        if (__instance.terrainFeatures.ContainsKey(tile))
                        {
                            switch (tileIndex)
                            {
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                case 18:
                                    continue;

                            }
                        }
                        switch (tileIndex)
                        {
                            case 13:
                            case 14:
                            case 15:
                                if (__instance.CanLoadPathObjectHere(tile))
                                {
                                    __instance.objects.Add(tile, ItemRegistry.Create<SDObject>(GameLocation.getWeedForSeason(Game1.random, __instance.GetSeason())));
                                }
                                break;
                            case 16:
                                if (__instance.CanLoadPathObjectHere(tile))
                                {
                                    __instance.objects.Add(tile, ItemRegistry.Create<SDObject>(Game1.random.Choose("(O)343", "(O)450")));
                                }
                                break;
                            case 17:
                                if (__instance.CanLoadPathObjectHere(tile))
                                {
                                    __instance.objects.Add(tile, ItemRegistry.Create<SDObject>(Game1.random.Choose("(O)343", "(O)450")));
                                }
                                break;
                            case 18:
                                if (__instance.CanLoadPathObjectHere(tile))
                                {
                                    __instance.objects.Add(tile, ItemRegistry.Create<SDObject>(Game1.random.Choose("(O)294", "(O)295")));
                                }
                                break;
                        }
                    }
                }

                return false;
            }
            return true;
        }
        /// <summary>
        /// Fixes game bug that spawns debris on path tiles
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        public static bool GetCategory(GameLocation location, ref string __result)
        {
            Cabin cabin = location as Cabin;
            if (cabin != null)
            {
                return true;
            }

            __result = location.GetDisplayName() ?? location.Name;


            return false;
        }
    }
}
