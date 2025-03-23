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

namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class GameFixesService : IGameFixesService
    {
        private static IGameEventsService _gameEventsService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IPatchingService)
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

            //
            //  convert new buildng chests from objects to chest
            _gameEventsService.AddSubscription(typeof(BuildingListChangedEventArgs).Name, FixBuildingChests);

            // stop weeds from spawning on path tiles
            patchingService.patches.AddPatch(true, typeof(GameLocation), "loadWeeds",
    null, typeof(GameFixesService), nameof(loadWeeds_prefix),
    "Stop weeds at start of year of spawning on paths", "Gamelocation");

            //
            //  change ChestAnywhere from using Name to DisplayName, when possible
            //
            Type chestFactory = Type.GetType("Pathoschild.Stardew.ChestsAnywhere.ChestFactory,ChestsAnywhere");
            patchingService.patches.AddPatch(true, chestFactory, "GetCategory",
              new Type[] { typeof(GameLocation) }, typeof(GameFixesService)
              , nameof(GetCategory), "Fix default ChestsAnywhere category",
              "Mechanics");

        }
        private void FixBuildingChests(EventArgs ep)
        {
            if (Game1.IsMasterGame)
            {
                BuildingListChangedEventArgs e = (BuildingListChangedEventArgs)ep;
                if (e.Added != null)
                {
                    foreach (Building buildingAdded in e.Added)
                    {
                        if (buildingAdded.indoors.Value != null)
                        {
                            var chests = buildingAdded.indoors.Value.Objects.Values.Where(p => p.ItemId == "130" && p is not Chest).ToList();

                            foreach (SDObject chest in chests)
                            {
                                buildingAdded.indoors.Value.objects.Remove(chest.TileLocation);
                                buildingAdded.indoors.Value.objects.Add(chest.TileLocation, new Chest(true, chest.TileLocation));
                            }
                        }
                    }
                }
            }
        }
        public static bool loadWeeds_prefix(GameLocation __instance)
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
