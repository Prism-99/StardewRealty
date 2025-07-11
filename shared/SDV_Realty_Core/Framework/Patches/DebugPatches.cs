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
using SDV_Realty_Core.Framework.ServiceProviders.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;
using StardewValley.Buildings;


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
            patchingService.patches.AddPatch(false, typeof(GameLocation), "GetData",
                new Type[] { typeof(string) }, typeof(DebugPatches),
                nameof(DebugPatches.GetDataPostFix), "Capture GetData calls.",
                "Debug");

            //
            // building patch for 2.0.4 building bug
            //
#if DEBUG
            patchingService.patches.AddPatch(true, typeof(Building), "doAction",
                 new Type[] { typeof(Vector2),typeof(Farmer) }, typeof(DebugPatches),
                 nameof(doAction_Prefix), "Capture doAction calls.",
                 "Debug");

#endif
            //
            //  movies
            //
            //patchingService.patches.AddPatch(true, typeof(MovieTheater), "GetSourceRectForScreen",
            //   new Type[] { typeof(int),typeof(int) }, typeof(DebugPatches),
            //   nameof(GetSourcePrefix), "Capture GetSource calls.",
            //   "Debug");
        }

        public static bool doAction_Prefix(Vector2 tileLocation, Farmer who,Building __instance)
        {
            if (who.isRidingHorse())
            {
                return false;
            }
            if (who.IsLocalPlayer && __instance.occupiesTile(tileLocation) && __instance.daysOfConstructionLeft.Value > 0)
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
            }
            else
            {
                if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
                {
                    return false;
                }
                GameLocation interior = __instance.GetIndoors();
                if (who.IsLocalPlayer && tileLocation.X == (float)(__instance.humanDoor.X + __instance.tileX.Value) && tileLocation.Y == (float)(__instance.humanDoor.Y + __instance.tileY.Value) && interior != null)
                {
                    if (who.mount != null)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
                        return false;
                    }
                    if (who.team.demolishLock.IsLocked())
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
                        return false;
                    }
                    if (__instance.OnUseHumanDoor(who))
                    {
                        who.currentLocation.playSound("doorClose", tileLocation);
                        bool isStructure = __instance.indoors.Value != null;
                        Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
                    }
                    return true;
                }
                BuildingData data = __instance.GetData();
                if (data != null)
                {
                    Microsoft.Xna.Framework.Rectangle door = __instance.getRectForAnimalDoor(data);
                    door.Width /= 64;
                    door.Height /= 64;
                    door.X /= 64;
                    door.Y /= 64;
                    if (__instance.daysOfConstructionLeft.Value <= 0 && door != Microsoft.Xna.Framework.Rectangle.Empty && door.Contains(Utility.Vector2ToPoint(tileLocation)) && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
                    {
                        __instance.ToggleAnimalDoor(who);
                        return true;
                    }
                    if (who.IsLocalPlayer && __instance.occupiesTile(tileLocation, applyTilePropertyRadius: true) && !__instance.isTilePassable(tileLocation))
                    {
                        string tileAction = data.GetActionAtTile((int)tileLocation.X - __instance.tileX.Value, (int)tileLocation.Y - __instance.tileY.Value);
                        if (tileAction != null)
                        {
                            tileAction = TokenParser.ParseText(tileAction);
                            if (who.currentLocation.performAction(tileAction, who, new Location((int)tileLocation.X, (int)tileLocation.Y)))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (who.IsLocalPlayer)
                {
                    if (!__instance.isTilePassable(tileLocation) && Building.TryPerformObeliskWarp(__instance.buildingType.Value, who))
                    {
                        return true;
                    }
                    if (who.ActiveObject != null && !__instance.isTilePassable(tileLocation))
                    {
                        return __instance.performActiveObjectDropInAction(who, probe: false);
                    }
                }
            }
            return false;
        }

        public static bool GetSourcePrefix(int movieIndex, int frame,out Rectangle __result)
        {
            //int yOffset = movieIndex * 128 + frame / 5 * 64;
            int yOffset = movieIndex * 128 + frame / 5 * 128;
            int xOffset = frame % 5 * 96*2;
            __result= new Rectangle(16 + xOffset, yOffset, 90*2, 61*2);
            return false;
        }
        public static void GetDataPostFix(GameLocation __instance, string name, LocationData __result)
        {
            string farmId = Game1.GetFarmTypeID();
            string farmTypeKey = Game1.GetFarmTypeKey();
            int x = 1;
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
