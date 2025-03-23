using StardewValley.GameData.Locations;
using System;
using StardewValley.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using StardewValley.Extensions;
using StardewValley;
using StardewValley.Tools;
using HarmonyLib;
using xTile.Layers;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;


namespace SDV_Realty_Core.Framework.Patches
{
    internal class DebugPatches
    {
        private static ILoggerService logger;
        private static IGameEventsService _eventsService;
        private static IPatchingService _patchingService;
        private static IModHelperService _modHelperService;
        public static void Initialize(ILoggerService ologger, IGameEventsService eventsService, IPatchingService patchingService, IModHelperService modHelperService)
        {
            logger = ologger;
            _eventsService = eventsService;
            _patchingService = patchingService;
            _modHelperService = modHelperService;

            patchingService.patches.AddPatch(true, typeof(Grass), "TryDropItemsOnCut",
                new Type[] { typeof(Tool), typeof(bool) }, typeof(DebugPatches), "TryDropItemsOnCut", "", "");

            patchingService.patches.AddPatch(true, typeof(Grass), "performToolAction",
                new Type[] { typeof(Tool), typeof(int), typeof(Vector2) }, typeof(DebugPatches), "performToolAction", "", "");
            //
            //  passive logging for DayUpdate profiling
            //
            patchingService.patches.AddPatch(true, typeof(GameLocation), "DayUpdate",
                new Type[] { typeof(int) }, typeof(GameLocationPatches),
                nameof(GameLocationPatches.DayUpdate), "Capture DayUpdate calls.",
                "Debug");

            patchingService.patches.AddPatch(false, typeof(GameLocation), "DayUpdate",
                new Type[] { typeof(int) }, typeof(GameLocationPatches),
                nameof(GameLocationPatches.DayUpdate_Post), "Capture DayUpdate calls.",
                "Debug");
        }

        public static bool performToolAction(Tool t, int explosion, Vector2 tileLocation, Grass __instance, ref bool __result)
        {
            GameLocation location = __instance.Location ?? Game1.currentLocation;
            MeleeWeapon weapon = t as MeleeWeapon;
            if ((weapon != null && (int)weapon.type.Value != 2) || explosion > 0)
            {
                if (weapon != null && (int)weapon.type.Value != 1)
                {
                    DelayedAction.playSoundAfterDelay("daggerswipe", 50, location, tileLocation);
                }
                else
                {
                    location.playSound("swordswipe", tileLocation);
                }
                //__instance.shake((float)Math.PI * 3f / 32f, (float)Math.PI / 40f, Game1.random.NextBool());
                int numberOfWeedsToDestroy = ((explosion <= 0) ? 1 : Math.Max(1, explosion + 2 - Game1.recentMultiplayerRandom.Next(2)));
                if (weapon != null && t.ItemId == "53")
                {
                    numberOfWeedsToDestroy = 2;
                }
                if (__instance.grassType.Value == 6 && Game1.random.NextBool())
                {
                    numberOfWeedsToDestroy = 0;
                }
                __instance.numberOfWeeds.Value = (int)__instance.numberOfWeeds.Value - numberOfWeedsToDestroy;
                Color c = __instance.grassType.Value switch
                {
                    1 => location.GetSeason() switch
                    {
                        Season.Spring => new Color(60, 180, 58),
                        Season.Summer => new Color(110, 190, 24),
                        Season.Fall => new Color(219, 102, 58),
                        _ => Color.Green,
                    },
                    2 => new Color(148, 146, 71),
                    3 => new Color(216, 240, 255),
                    4 => new Color(165, 93, 58),
                    6 => Color.White * 0.6f,
                    _ => Color.Green,
                };
                var mPlayer = _modHelperService.modHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                mPlayer.broadcastSprites(location, new TemporaryAnimatedSprite(28, tileLocation * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), c, 8, Game1.random.NextBool(), Game1.random.Next(60, 100)));
                __result = __instance.TryDropItemsOnCut(t);
                return false;
            }
            __result = false;
            return false;
        }


        public static bool TryDropItemsOnCut(Tool tool, Grass __instance, ref bool __result, bool addAnimation = true)
        {
            Vector2 tile = __instance.Tile;
            GameLocation location = __instance.Location;

            if (location == null)
            {
                location = Game1.player.currentLocation;
                __instance.Location = location;
            }

            if (__instance.numberOfWeeds.Value > 0)
            {
                int numberOfWeedsToDestroy = 4;//  Math.Max(1,  2 - Game1.recentMultiplayerRandom.Next(2));
                if (tool.ItemId == "53")
                {
                    numberOfWeedsToDestroy = 2;
                }
                if (__instance.grassType.Value == 6 && Game1.random.NextBool())
                {
                    numberOfWeedsToDestroy = 0;
                }
                __instance.numberOfWeeds.Value = __instance.numberOfWeeds.Value - numberOfWeedsToDestroy;
            }


            if (__instance.numberOfWeeds.Value <= 0)
            {
                if (__instance.grassType.Value != 1)
                {
                    Random random = (Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tile.X * 1000.0, (double)tile.Y * 11.0, Game1.CurrentMineLevel, Game1.player.timesReachedMineBottom));
                    if (random.NextDouble() < 0.005)
                    {
                        Game1.createObjectDebris("(O)114", (int)tile.X, (int)tile.Y, -1, 0, 1f, location);
                    }
                    else if (random.NextDouble() < 0.01)
                    {
                        Game1.createDebris(4, (int)tile.X, (int)tile.Y, random.Next(1, 2), location);
                    }
                    else if (random.NextDouble() < 0.02)
                    {
                        Game1.createObjectDebris("(O)92", (int)tile.X, (int)tile.Y, random.Next(2, 4), location);
                    }
                }
                else if (tool != null && tool.isScythe())
                {
                    Farmer farmer = tool.getLastFarmerToUse() ?? Game1.player;
                    Random obj = (Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tile.X * 1000.0, (double)tile.Y * 11.0));
                    double num = ((tool.ItemId == "53") ? 0.75 : 0.5);
                    if (obj.NextDouble() < num && GameLocation.StoreHayInAnySilo(1, __instance.Location) == 0)
                    {
                        if (addAnimation)
                        {
                            TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, farmer.Position - new Vector2(0f, 128f), flicker: false, flipped: false, farmer.Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0f, 0f);
                            temporaryAnimatedSprite.motion.Y = -1f;
                            temporaryAnimatedSprite.layerDepth = 1f - (float)Game1.random.Next(100) / 10000f;
                            temporaryAnimatedSprite.delayBeforeAnimationStart = Game1.random.Next(350);
                            var mPlayer = _modHelperService.modHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

                            mPlayer.broadcastSprites(__instance.Location, temporaryAnimatedSprite);
                        }

                        Game1.addHUDMessage(HUDMessage.ForItemGained(ItemRegistry.Create("(O)178"), 1));
                    }
                }

                __result = true;
            }

            __result = false;

            return false;
        }
        public static bool CreateGameLocation(string id, CreateLocationData createData, Game1 __instance, ref GameLocation __result)
        {
            if (createData == null || id != "MovieTheater")
            {
                return true;
            }
            try
            {
                MovieTheater movieTheater = new MovieTheater();
                string theater = movieTheater.GetType().AssemblyQualifiedName;
                Type tType = Type.GetType(theater);
                Type tType2 = Type.GetType(createData.Type);
                Type tType3 = Type.GetType("StardewValley.Locations.Mine, Stardew Valley");
                //GameLocation location = ((createData.Type == null) ? new GameLocation(createData.MapPath, id) : ((GameLocation)Activator.CreateInstance(Type.GetType(createData.Type) ?? throw new Exception("Invalid type for location " + id + ": " + createData.Type), createData.MapPath, id)));
                __result = ((createData.Type == null) ? new GameLocation(createData.MapPath, id) : ((GameLocation)Activator.CreateInstance(Type.GetType(theater), createData.MapPath, id)));
                __result.isAlwaysActive.Value = createData.AlwaysActive;
                return false;// location;
            }
            catch (Exception e)
            {
                string err = e.ToString();
            }

            return true;
        }
    }
}
