using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;
using System;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class CustomTrainService : ICustomTrainService
    {
        private static IModDataService modDataService;
        private static IUtilitiesService utilitiesService;
        private const string mp_trainApproach = "trainApproach";
        private static IMultiplayerService multiplayerService;
        private static IModHelperService modHelperService;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IPatchingService),typeof(IModDataService),
            typeof(IMultiplayerService),typeof(IUtilitiesService),
            typeof(IChatBoxCommandsService),typeof(IModHelperService)
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
            IPatchingService patchingService = (IPatchingService)args[0];
            modDataService = (IModDataService)args[1];
            multiplayerService = (IMultiplayerService)args[2];
            utilitiesService = (IUtilitiesService)args[3];
            IChatBoxCommandsService chatBoxCommandsService = (IChatBoxCommandsService)args[4];
            modHelperService = (IModHelperService)args[5];

            multiplayerService.Multiplayer.AddMessageReceivedSubscription(HandleClientMessageRecevied);
            chatBoxCommandsService.AddCustomCommand("train", HandleChatBoxCommand);
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), HandleDayStarted);

            patchingService.patches.AddPatch(false, typeof(GameLocation), "updateEvenIfFarmerIsntHere",
                new Type[] { typeof(GameTime), typeof(bool) }, typeof(CustomTrainService),
                nameof(updateEvenIfFarmerIsntHere_Post), "Handle custom Train logic.",
                "CustomTrain");
            patchingService.patches.AddPatch(false, typeof(GameLocation), "draw",
                new Type[] { typeof(SpriteBatch) }, typeof(CustomTrainService),
                nameof(trainDraw_Post), "Draw train.",
                "CustomTrain");
            patchingService.patches.AddPatch(false, typeof(GameLocation), "cleanupBeforePlayerExit",
                new Type[] { }, typeof(CustomTrainService),
                nameof(cleanupBeforePlayerExit_Post), "Handle train cleanup.",
                "CustomTrain");
            patchingService.patches.AddPatch(true, typeof(TrainCar), "draw",
                new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(GameLocation) }, typeof(CustomTrainService),
                nameof(trainCarDraw_Prefix), "Handle train car loot drops.",
                "CustomTrain");
            patchingService.ApplyPatches("CustomTrain");
        }

        private void HandleDayStarted(EventArgs args)
        {
#if DEBUG
            logger.Log($"Clear trains sent", LogLevel.Debug);
#endif
            foreach (var train in Trains)
            {
                train.Value.TrainToday = false;
            }
        }
        private void HandleChatBoxCommand(string[] args)
        {
            string location = WarproomManager.StardewMeadowsLoacationName;
            if (args.Length > 1)
                location = args[1];

            int delay = 2000;
            if (args.Length > 2 && int.TryParse(args[2], out int dly))
                delay = dly;

            //
            //  BTL does not initialize loot weight settings unless the railroad is accessible
            //  spawning a train will cause a red storm in the SMAPI console
            if (!utilitiesService.IsTrainOpen() && modHelperService.ModRegistry.IsLoaded("AairTheGreat.BetterTrainLoot"))
                utilitiesService.PopMessage(I18n.Train_BTL(), IUtilitiesService.PopupType.Dialogue);
            else
                setTrainComing(location, delay, true);

        }
        private bool HandleClientMessageRecevied(bool masterPlayer, ModMessageReceivedEventArgs e)
        {
            SDRMultiplayer.FEMessage msg = e.ReadAs<SDRMultiplayer.FEMessage>();
            switch (e.Type)
            {
                case mp_trainApproach:
                    PlayTrainApproach(msg.TextData);
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Check is TrainCar is in loot dropzone
        /// </summary>
        /// <param name="expansion">GameLocaton to check</param>
        /// <param name="globalPosition">Current TrainCar position</param>
        /// <returns>True if TrainCar is in a dropzone</returns>
        private static bool IsInDropZone(FarmExpansionLocation expansion, Vector2 globalPosition)
        {
            return Trains[expansion.Name].DropZones.Where(p => globalPosition.X >= p.MinPos && globalPosition.X <= p.MaxPos).Any();
        }
        /// <summary>
        /// Overrides draw to provide custom dropzones for loot
        /// </summary>
        /// <param name="__instance">TrainCar being drawn</param>
        /// <param name="b">Current SpriteBatch</param>
        /// <param name="globalPosition">Postion of TrainCaar</param>
        /// <param name="wheelRotation">WheelRotation for Engine</param>
        /// <param name="location">Locaation to draw traian.</param>
        /// <returns></returns>
        public static bool trainCarDraw_Prefix(TrainCar __instance, SpriteBatch b, Vector2 globalPosition, float wheelRotation, GameLocation location)
        {
            bool doBase = true;
            if (__instance.carType.Value == 1 && IsSDRTrainAndActive(location) && location is FarmExpansionLocation expansion)
            {
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle(192 + __instance.carType.Value * 128, 512 - (__instance.alternateCar.Value ? 64 : 0), 128, 57), __instance.color.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 256f) / 10000f);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(0f, 228f)), new Rectangle(192 + __instance.carType.Value * 128, 569, 128, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 256f) / 10000f);

                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle(448 + __instance.resourceType.Value * 128 % 256, 576 + __instance.resourceType.Value / 2 * 32, 128, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
                float chance = 0.02f;
                if (__instance.loaded.Value > 0 && Game1.random.NextDouble() < chance && IsInDropZone(expansion, globalPosition))
                {
                    __instance.loaded.Value--;
                    string debrisId = null;
                    switch (__instance.resourceType.Value)
                    {
                        case 0:
                            debrisId = "(O)382";
                            break;
                        case 1:
                            debrisId = ((__instance.color.R > __instance.color.G) ? "(O)378" : ((__instance.color.G > __instance.color.B) ? "(O)380" : ((__instance.color.B > __instance.color.R) ? "(O)384" : "(O)378")));
                            break;
                        case 7:
                            debrisId = (location.IsWinterHere() ? "(O)536" : ((Game1.stats.DaysPlayed > 120 && __instance.color.R > __instance.color.G) ? "(O)537" : "(O)535"));
                            break;
                        case 2:
                            debrisId = ((Game1.random.NextDouble() < 0.05) ? "(O)709" : "(O)388");
                            break;
                        case 6:
                            debrisId = "(O)390";
                            break;
                        case 9:
                            if (Utility.tryRollMysteryBox(0.02))
                            {
                                debrisId = "(O)MysteryBox";
                            }
                            break;
                    }
                    if (debrisId != null)
                    {
                        Game1.createObjectDebris(debrisId, (int)globalPosition.X / 64 + 2, (int)globalPosition.Y / 64, (int)(globalPosition.Y + 320f));
                    }
                    if (Game1.random.NextDouble() < 0.01)
                    {
                        Game1.createItemDebris(ItemRegistry.Create("(B)806"), new Vector2((int)globalPosition.X + 128, (int)globalPosition.Y), (int)(globalPosition.Y + 320f));
                    }
                }
                //__instance.DrawFrontDecal(b, globalPosition);
                if (__instance.frontDecal.Value == 35)
                {
                    b.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(192f, 92f)), new Rectangle(480, 480, 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
                }
                else if (__instance.frontDecal.Value != -1 && __instance.frontDecal.Value < 35)
                {
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(192f, 92f)), new Rectangle(224 + __instance.frontDecal.Value * 32 % 224, 576 + __instance.frontDecal.Value * 32 / 224 * 32, 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
                }
                doBase = false;
            }
            return doBase;
        }
        /// <summary>
        /// Handles invoking drawing of the train
        /// </summary>
        /// <param name="__instance">GameLocation being drawn.</param>
        /// <param name="b"></param>
        public static void trainDraw_Post(GameLocation __instance, SpriteBatch b)
        {
            if (IsSDRTrainAndActive(__instance) && __instance is FarmExpansionLocation expansion && expansion.train.Value != null && !Game1.eventUp && Game1.activeClickableMenu==null)
            {
                expansion.train.Value.draw(b, __instance);
            }
        }
        /// <summary>
        /// Public method to request a train to be spawned
        /// </summary>
        /// <param name="locationName">GameLocation to spawn the train</param>
        /// <param name="delay">How many milliseconds to delay before train</param>
        /// <param name="forceActive">Ignore train enabling rules</param>
        public override void setTrainComing(string locationName, int delay, bool forceActive = false)
        {
            setTrainDelay(locationName, delay, forceActive);
        }
        /// <summary>
        /// Request a train to be spawned
        /// </summary>
        /// <param name="locationName">GameLocation to spawn the train</param>
        /// <param name="delay">How many milliseconds to delay before train</param>
        /// <param name="forceActive">Ignore train enabling rules</param>
        private static void setTrainDelay(string locationName, int delay, bool forceActive = false)
        {
            if ((utilitiesService.IsTrainOpen() || forceActive) && modDataService.Config.TrainEnabled && Trains.ContainsKey(locationName))
            {
                GameLocation expansion = Game1.getLocationFromName(locationName);
                if (expansion != null)
                {
                    ((FarmExpansionLocation)expansion).trainTimer.Value = delay;
                    if (Game1.IsMasterGame)
                    {
                        Trains[locationName].TrainToday = Trains[locationName].TrainToday || !forceActive;
                        PlayTrainApproach(expansion.Name);
                        if (Game1.IsMultiplayer)
                            multiplayerService.Multiplayer.SendMessageToClient(new SDRMultiplayer.FEMessage { TextData = expansion.Name }, mp_trainApproach);
                    }
                }
            }
        }
        /// <summary>
        /// Plays train approaching sounds
        /// </summary>
        /// <param name="locationName">GameLocation to play sound in</param>
        public static void PlayTrainApproach(string locationName)
        {
            if (Trains.TryGetValue(locationName, out TrainDetails details))
            {
                bool? flag = Game1.currentLocation?.IsOutdoors;
                if (flag.HasValue && flag.GetValueOrDefault() && !Game1.isFestival() && Game1.currentLocation.InValleyContext())
                {
                    Game1.showGlobalMessage(I18n.GetByKey(details.ApproachingMessage));
                    Game1.playSound("distantTrain", out var whistle);
                    whistle.SetVariable("Volume", 100f);
                }
            }
        }
        /// <summary>
        /// Checks to see if location has trains enabled
        /// </summary>
        /// <param name="location">GameLocation to check</param>
        /// <returns>True is train is active for location</returns>
        private static bool IsSDRTrainAndActive(GameLocation location)
        {
            return modDataService.Config.TrainEnabled && Trains.ContainsKey(location.Name);
        }
        /// <summary>
        /// Cleanup train audio
        /// </summary>
        /// <param name="__instance"></param>
        public static void cleanupBeforePlayerExit_Post(GameLocation __instance)
        {
            if (IsSDRTrainAndActive(__instance))
            {
                ((FarmExpansionLocation)__instance).trainLoop?.Stop(AudioStopOptions.Immediate);
                ((FarmExpansionLocation)__instance).trainLoop = null;
            }
        }
        /// <summary>
        /// Override base method to add train support
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="time"></param>
        public static void updateEvenIfFarmerIsntHere_Post(GameLocation __instance, GameTime time)
        {
            if (IsSDRTrainAndActive(__instance))
            {
                TrainDetails details = Trains[__instance.Name];
                FarmExpansionLocation expansion = (FarmExpansionLocation)__instance;
                if (expansion.train.Value != null && expansion.train.Value.Update(time, __instance) && Game1.IsMasterGame)
                {
                    expansion.train.Value = null;
                }
                if (Game1.timeOfDay >= details.TrainTime && !details.TrainToday && expansion.train.Value == null && expansion.trainTimer.Value <= 0)
                {
                    setTrainDelay(expansion.Name, 15000);
                }
                if (expansion.trainTimer.Value > 0)
                {
                    expansion.trainTimer.Value -= time.ElapsedGameTime.Milliseconds;
                    if (expansion.trainTimer.Value <= 0)
                    {
                        expansion.train.Value = new StardewMeadowsTrain(details.CrossingY);
                        __instance.playSound("trainWhistle");
                    }
                    if (expansion.trainTimer.Value < 3500 && Game1.currentLocation == __instance && (expansion.trainLoop == null || !expansion.trainLoop.IsPlaying))
                    {
                        Game1.playSound("trainLoop", out expansion.trainLoop);
                        expansion.trainLoop.SetVariable("Volume", 0f);
                    }
                }
                if (expansion.train.Value != null)
                {
                    if (Game1.currentLocation == __instance && (expansion.trainLoop == null || !expansion.trainLoop.IsPlaying))
                    {
                        Game1.playSound("trainLoop", out expansion.trainLoop);
                        expansion.trainLoop.SetVariable("Volume", 0f);
                    }
                    ICue cue = expansion.trainLoop;
                    if (cue != null && cue.GetVariable("Volume") < 100f)
                    {
                        expansion.trainLoop.SetVariable("Volume", expansion.trainLoop.GetVariable("Volume") + 0.5f);
                    }
                }
                else if (expansion.trainLoop != null && expansion.trainTimer.Value <= 0)
                {
                    expansion.trainLoop.SetVariable("Volume", expansion.trainLoop.GetVariable("Volume") - 0.15f);
                    if (expansion.trainLoop.GetVariable("Volume") <= 0f)
                    {
                        expansion.trainLoop.Stop(AudioStopOptions.Immediate);
                        expansion.trainLoop = null;
                    }
                }
                else if (expansion.trainTimer.Value > 0 && expansion.trainLoop != null)
                {
                    expansion.trainLoop.SetVariable("Volume", expansion.trainLoop.GetVariable("Volume") + 0.15f);
                }
            }
        }
    }
}
