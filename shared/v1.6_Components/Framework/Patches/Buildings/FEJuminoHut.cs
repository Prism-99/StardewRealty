using System;
using System.Collections.Generic;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.Objects;
using HarmonyLib;
using StardewValley.TerrainFeatures;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SkiaSharp;

namespace SDV_Realty_Core.Framework.Patches.Buildings
{
    /// <summary>
    /// Provides additional abilities to Junimos
    /// </summary>
    internal class FEJuminoHut
    {
        //
        //  version 1.6
        //
        private static ILoggerService logger;
        //private static FEConfig config;
        private static ISeasonUtilsService _seasonUtils;
        private static IUtilitiesService _Utilities;
        private static IModDataService _modDataService;
        public static string unionLetterId = "junimoUnion";
        public FEJuminoHut(ILoggerService olog, IModDataService modDataService, IUtilitiesService utilitiesService, ISeasonUtilsService seasonUtils)
        {
            logger = olog;
            //config = utilitiesService.ConfigService.config;
            _seasonUtils = seasonUtils;
            _Utilities = utilitiesService;
            _modDataService = modDataService;

            utilitiesService.GameEventsService.AddSubscription("BuildingListChangedEventArgs", HandleBuildingListChanged);
        }
        public static string GenerateJunimoFeeLetter()
        {
            string mailContent = I18n.JunimoLetterIntro() + I18n.JunimoLetterHeader();

            if (_modDataService.Config.JunimosWorkInRain)
            {
                mailContent += I18n.JunimoLetterRainService();
            }
            if (_modDataService.Config.JunimoRainFee > 0)
            {
                mailContent += I18n.JunimoLetterRainServiceCost(_modDataService.Config.JunimoRainFee);
            }
            if (!_modDataService.Config.JunimosWorkInWinter)
            {
                mailContent += I18n.JunimoLetterWinterService();

                if (_modDataService.Config.JunimoWinterFee > 0)
                {
                    mailContent += I18n.JunimoLetterWinterServiceCost(_modDataService.Config.JunimoWinterFee);
                }
            }

            if (_modDataService.Config.JunimoReseedCrop)
            {
                mailContent += I18n.JunimoLetterReseedService();

                if (_modDataService.Config.JunimosFeeForSeeding > 0)
                {
                    mailContent += I18n.JunimoReseedServiceCost(_modDataService.Config.JunimosFeeForSeeding);
                }
                if (_modDataService.Config.JunimoSeedDiscount > 0)
                {
                    mailContent += I18n.JunimoLetterSeedDiscount((int)(_modDataService.Config.JunimoSeedDiscount * 100));
                }
            }
            mailContent += $"{I18n.JunimoLetterFooter()}[#]{I18n.JunimoLetterSubject()}";

            return mailContent;
        }
        private void HandleBuildingListChanged(EventArgs e)
        {
            if (!_modDataService.ModState.JunimoLetterSent)
            {
                BuildingListChangedEventArgs changedArgs = (BuildingListChangedEventArgs)e;
                var hutBuild = changedArgs.Added.Where(p => p.buildingType.Value == "Junimo Hut");
                if (hutBuild.Any())
                {
                    //
                    //  add mail to data/mail
                    //
                    if (_modDataService.Config.EnablePremiumJunimos)
                    {      
                        string mailContent= GenerateJunimoFeeLetter();
                        _modDataService.CustomMail.Add(unionLetterId, mailContent);
                        _modDataService.ModState.JunimoLetter = mailContent;

                        _Utilities.InvalidateCache("Data/Mail");
                        _Utilities.PlayerComms.SendPlayerMail(unionLetterId, false, true);
                        logger.Log("Junimo Union mail sent", LogLevel.Debug);
                        _modDataService.ModState.JunimoLetterSent = true;
                        _modDataService.SaveModState();
                    }
                }
            }
        }
        //public static void dayUpdate(int dayOfMonth, JunimoHut __instance)
        //{
        //    logger.Log($"JuminoHut.dayUpdate called for {__instance.buildingType}", LogLevel.Debug);
        //    if ((int)Traverse.Create(__instance).Field("junimoSendOutTimer").GetValue() == 0)
        //    {
        //        Traverse.Create(__instance).Field("junimoSendOutTimer").SetValue(1000);
        //    }
        //    //FEFramework.monitor.Log("FEJunimoHut.dayUpdate called", StardewModdingAPI.LogLevel.Info);
        //    //base.dayUpdate(dayOfMonth);
        //    //_ = (int)daysOfConstructionLeft;
        //    //_ = 0;
        //    //sourceRect = getSourceRectForMenu();
        //    //myJunimos.Clear();
        //    //wasLit.Value = false;
        //    //shouldSendOutJunimos.Value = true;
        //    //foreach (Farmer allFarmer in Game1.getAllFarmers())
        //    //{
        //    //    if (allFarmer.isActive() && allFarmer.currentLocation != null && (allFarmer.currentLocation is FarmHouse || allFarmer.currentLocation.isStructure.Value))
        //    //    {
        //    //        shouldSendOutJunimos.Value = false;
        //    //    }
        //    //}
        //}
        private static void SendOutJunimo(JunimoHut __instance)
        {
            GameLocation glFarm = __instance.GetParentLocation();
            int unusedJunimoNumber = __instance.getUnusedJunimoNumber();
            bool isPrismatic = false;

            Color? gemColor = getGemColor(ref isPrismatic, __instance);
            JunimoHarvester junimoHarvester = new JunimoHarvester(glFarm, new Vector2((int)__instance.tileX.Value + 1, (int)__instance.tileY.Value + 1) * 64f + new Vector2(0f, 32f), __instance, unusedJunimoNumber, gemColor);
            //CustomJunimoHarvester junimoHarvester = new CustomJunimoHarvester(logger, new Vector2(__instance.tileX.Value + 1, __instance.tileY.Value + 1) * 64f + new Vector2(0f, 32f), __instance, glFarm, unusedJunimoNumber, gemColor);
            //junimoHarvester.modData.Add(IModDataKeysService.FELocationName, glFarm.Name);
            //junimoHarvester.modData.Add(IModDataKeysService.FEExpansionType, "FarmExpansion");

            junimoHarvester.isPrismatic.Value = isPrismatic;
            glFarm.characters.Add(junimoHarvester);
            __instance.myJunimos.Add(junimoHarvester);
            Traverse.Create(__instance).Field("junimoSendOutTimer").SetValue(1000);
            if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2(__instance.tileX.Value + 1, __instance.tileY.Value + 1)), 64, glFarm))
            {
                try
                {
                    glFarm.playSound("junimoMeep1");
                }
                catch (Exception)
                {
                }
            }

        }
        //private static void SendOutLumberJack(JunimoHut __instance)
        //{
        //    GameLocation glFarm = __instance.GetParentLocation();
        //    int unusedJunimoNumber = __instance.getUnusedJunimoNumber();
        //    bool isPrismatic = false;

        //    Color? gemColor = getGemColor(ref isPrismatic, __instance);
        //    LumberJackWorker lumberJack = new LumberJackWorker(glFarm, new Vector2((int)__instance.tileX.Value + 1, (int)__instance.tileY.Value + 1) * 64f + new Vector2(0f, 32f), __instance, unusedJunimoNumber, gemColor);
        //    //CustomJunimoHarvester junimoHarvester = new CustomJunimoHarvester(logger, new Vector2(__instance.tileX.Value + 1, __instance.tileY.Value + 1) * 64f + new Vector2(0f, 32f), __instance, glFarm, unusedJunimoNumber, gemColor);
        //    //junimoHarvester.modData.Add(IModDataKeysService.FELocationName, glFarm.Name);
        //    //junimoHarvester.modData.Add(IModDataKeysService.FEExpansionType, "FarmExpansion");

        //    //lumberJack.isPrismatic.Value = isPrismatic;
        //    glFarm.characters.Add(lumberJack);
        //    //__instance.myJunimos.Add(lumberJack);
        //    Traverse.Create(__instance).Field("junimoSendOutTimer").SetValue(1000);
        //    if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2(__instance.tileX.Value + 1, __instance.tileY.Value + 1)), 64, glFarm))
        //    {
        //        try
        //        {
        //            glFarm.playSound("junimoMeep1");
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        //}
        public static bool updateWhenFarmNotCurrentLocation(GameTime time, JunimoHut __instance)
        {
            if (!_modDataService.Config.EnablePremiumJunimos || !Game1.IsMasterGame)
                return true;

            GameLocation glFarm = __instance.GetParentLocation();
            __instance.GetOutputChest().mutex.Update(glFarm);
            if (__instance.GetOutputChest().mutex.IsLockHeld() && Game1.activeClickableMenu == null)
            {
                __instance.GetOutputChest().mutex.ReleaseLock();
            }

            int junimoSendOutTimer = (int)Traverse.Create(__instance).Field("junimoSendOutTimer").GetValue();
            if (junimoSendOutTimer <= 0 || !__instance.shouldSendOutJunimos.Value)
            {
                return false;
            }

            junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
            Traverse.Create(__instance).Field("junimoSendOutTimer").SetValue(junimoSendOutTimer);
            if (junimoSendOutTimer > 0 || Game1.farmEvent != null)
            {
                return false;
            }
#if DEBUG_LOG
            //logger.Log($"     sending out Junimos", LogLevel.Debug);
#endif
            if (__instance.myJunimos.Count < _modDataService.Config.MaxNumberJunimos && (!_seasonUtils.isWinter(__instance.modData) || _modDataService.Config.JunimosWorkInWinter) && (!Game1.isRaining || _modDataService.Config.JunimosWorkInRain) && areThereMatureCropsWithinRadius_int(glFarm, __instance))
            {
                SendOutJunimo(__instance);
            }
            //int lbj_count = 0;

            //if (__instance.modData.ContainsKey("lbj_count"))
            //{
            //    lbj_count = Convert.ToInt32(__instance.modData["lbj_count"]);
            //    if (lbj_count < 4)
            //    {
            //        if(areThereTreesWithinRadius(glFarm, __instance))
            //        {

            //        }
            //    }
            //}
            return false;
        }
        //public static bool update(GameTime time, JunimoHut __instance)
        //{
        //    GameLocation currentFarm = null;

        //    //if (__instance.modData.ContainsKey(IModDataKeysService.FELocationName))
        //    //{
        //    //currentFarm = Game1.getLocationFromName(__instance.modData[IModDataKeysService.FELocationName]);
        //    currentFarm = __instance.GetParentLocation();
        //    //}
        //    //else
        //    //{
        //    //    //
        //    //    //  let the code fall through to the base
        //    //    //  for Farm base Junimo Huts
        //    //    //
        //    //    return true;
        //    //    //currentFarm = Game1.getLocationFromName("Farm") as Farm;
        //    //}
        //    __instance.output.Value.mutex.Update(currentFarm);
        //    if (__instance.output.Value.mutex.IsLockHeld() && Game1.activeClickableMenu == null)
        //    {
        //        __instance.output.Value.mutex.ReleaseLock();
        //    }
        //    object ojunimoSendOutTimer = Traverse.Create(__instance).Field("junimoSendOutTimer").GetValue();

        //    int junimoSendOutTimer = 0;
        //    if (ojunimoSendOutTimer != null)
        //    {
        //        junimoSendOutTimer = (int)ojunimoSendOutTimer;
        //    }

        //    if (Game1.IsMasterGame && junimoSendOutTimer > 0)
        //    {
        //        junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
        //        if (junimoSendOutTimer <= 0 && __instance.myJunimos.Count() < 3 && !FEFramework.isWinter(__instance.modData) && !Game1.isRaining && areThereMatureCropsWithinRadius_int(currentFarm, __instance) && Game1.farmEvent == null)
        //        {
        //            int junimoNumber = __instance.getUnusedJunimoNumber();
        //            bool isPrismatic = false;
        //            Color? gemColor = getGemColor(ref isPrismatic, __instance);
        //            JunimoHarvester i = new JunimoHarvester(currentFarm, new Vector2(__instance.tileX.Value + 1, __instance.tileY.Value + 1) * 64f + new Vector2(0f, 32f), __instance, junimoNumber, gemColor);
        //            i.isPrismatic.Value = isPrismatic;
        //            currentFarm.characters.Add(i);
        //            __instance.myJunimos.Add(i);
        //            Traverse.Create(__instance).Field("junimoSendOutTimer").SetValue(1000);
        //            if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2(__instance.tileX.Value + 1, __instance.tileY.Value + 1)), 64, currentFarm))
        //            {
        //                try
        //                {
        //                    currentFarm.playSound("junimoMeep1");
        //                }
        //                catch (Exception)
        //                {
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}
        //public static bool areThereMatureCropsWithinRadius(JunimoHut __instance, ref bool __result)
        //{
        //    GameLocation currentFarm = null;

        //    currentFarm = __instance.GetParentLocation();// Game1.getLocationFromName(__instance.modData[IModDataKeysService.FELocationName]);
        //    __result = areThereMatureCropsWithinRadius_int(currentFarm, __instance);

        //    return false;
        //}
        //private static bool areThereTreesWithinRadius(GameLocation farm, JunimoHut oHut)
        //{
        //    for (int i = oHut.tileX.Value + 1 - config.JunimoMaxRadius; i < oHut.tileX.Value + 2 + config.JunimoMaxRadius; i++)
        //    {
        //        for (int j = oHut.tileY.Value - config.JunimoMaxRadius + 1; j < oHut.tileY.Value + 2 + config.JunimoMaxRadius; j++)
        //        {
        //            if (farm.isCropAtTile(i, j) && (farm.terrainFeatures[new Vector2(i, j)] as HoeDirt).readyForHarvest())
        //            {
        //                oHut.lastKnownCropLocation = new Point(i, j);
        //                return true;
        //            }

        //            if (farm.terrainFeatures.ContainsKey(new Vector2(i, j)) && farm.terrainFeatures[new Vector2(i, j)] is Bush && (farm.terrainFeatures[new Vector2(i, j)] as Bush).tileSheetOffset.Value == 1)
        //            {
        //                oHut.lastKnownCropLocation = new Point(i, j);
        //                return true;
        //            }
        //        }
        //    }

        //    oHut.lastKnownCropLocation = Point.Zero;
        //    return false;
        //}
        private static bool areThereMatureCropsWithinRadius_int(GameLocation farm, JunimoHut junimoHut)
        {
            for (int i = junimoHut.tileX.Value + 1 - _modDataService.Config.JunimoMaxRadius; i < junimoHut.tileX.Value + 2 + _modDataService.Config.JunimoMaxRadius; i++)
            {
                for (int j = junimoHut.tileY.Value - _modDataService.Config.JunimoMaxRadius + 1; j < junimoHut.tileY.Value + 2 + _modDataService.Config.JunimoMaxRadius; j++)
                {
                    if (farm.isCropAtTile(i, j) && (farm.terrainFeatures[new Vector2(i, j)] as HoeDirt).readyForHarvest())
                    {
                        junimoHut.lastKnownCropLocation = new Point(i, j);
                        return true;
                    }

                    if (farm.terrainFeatures.ContainsKey(new Vector2(i, j)) && farm.terrainFeatures[new Vector2(i, j)] is Bush && (farm.terrainFeatures[new Vector2(i, j)] as Bush).tileSheetOffset.Value == 1)
                    {
                        junimoHut.lastKnownCropLocation = new Point(i, j);
                        return true;
                    }
                }
            }

            junimoHut.lastKnownCropLocation = Point.Zero;
            return false;
        }
        //public static bool getSourceRectForMenu(JunimoHut __instance, ref Rectangle __result)
        //{
        //    if (__instance.modData.ContainsKey(IModDataKeysService.FELocationName) && __instance.modData[IModDataKeysService.FELocationName] != "Farm")
        //    {
        //        string sOverride = FEFramework.farmExpansions[__instance.modData[IModDataKeysService.FELocationName]].seasonOverride;
        //        if (!string.IsNullOrEmpty(sOverride))
        //        {
        //            __result = new Rectangle(Utility.getSeasonNumber(sOverride) * 48, 0, 48, 64);
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        private static Color? getGemColor(ref bool isPrismatic, JunimoHut __instance)
        {
            List<Color> gemColors = new List<Color>();
#if v16
            foreach (Item item in __instance.GetOutputChest().Items)
#else
            foreach (Item item in __instance.output.Value.items)
#endif
            {
                if (item != null && (item.Category == -12 || item.Category == -2))
                {
                    Color? gemColor = TailoringMenu.GetDyeColor(item);
                    if (item.Name == "Prismatic Shard")
                    {
                        isPrismatic = true;
                    }
                    if (gemColor.HasValue)
                    {
                        gemColors.Add(gemColor.Value);
                    }
                }
            }
            if (gemColors.Count > 0)
            {
                return gemColors[Game1.random.Next(gemColors.Count)];
            }
            return null;
        }
        //public static bool performTenMinuteAction(int timeElapsed, JunimoHut __instance)
        //{
        //    Farm currentFarm;
        //    if (__instance.modData.ContainsKey(IModDataKeysService.FELocationName))
        //    {
        //        currentFarm = Game1.getLocationFromName(__instance.modData[IModDataKeysService.FELocationName]) as Farm;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //    //base.performTenMinuteAction(timeElapsed);
        //    for (int i = __instance.myJunimos.Count - 1; i >= 0; i--)
        //    {
        //        if (!currentFarm.characters.Contains(__instance.myJunimos[i]))
        //        {
        //            __instance.myJunimos.RemoveAt(i);
        //        }
        //        else
        //        {
        //            __instance.myJunimos[i].pokeToHarvest();
        //        }
        //    }
        //    if (__instance.myJunimos.Count < 3 && Game1.timeOfDay < 1900)
        //    {
        //        Traverse.Create(__instance).Field("junimoSendOutTimer").SetValue(1);
        //    }
        //    if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && !_seasonUtils.isWinter(__instance.modData) && Game1.random.NextDouble() < 0.2)
        //    {
        //        __instance.wasLit.Value = true;
        //    }
        //    else if (Game1.timeOfDay == 2400 && !_seasonUtils.isWinter(__instance.modData))
        //    {
        //        __instance.wasLit.Value = false;
        //    }

        //    return false;
        //}
        ///// <summary>
        /// Override draw to allow Junimo Huts to follow seasonoverride
        /// </summary>
        /// <param name="b"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        //        public static bool draw(SpriteBatch b, JunimoHut __instance)
        //        {
        //            float alpha = 1f;
        //            Rectangle lightInteriorRect = new Rectangle(195, 0, 18, 17);
        //            Rectangle bagRect = new Rectangle(208, 51, 15, 13);

        //            if (__instance.isMoving)
        //            {
        //                return false;
        //            }
        //            if (__instance.daysOfConstructionLeft.Value > 0)
        //            {
        //                __instance.drawInConstruction(b);
        //                return false;
        //            }
        //            __instance.drawShadow(b);
        //            b.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value * 64, __instance.tileY.Value * 64 + __instance.tilesHigh.Value * 64)), __instance.sourceRect, __instance.color.Value * alpha, 0f, new Vector2(0f, __instance.texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)((__instance.tileY.Value + __instance.tilesHigh.Value - 1) * 64) / 10000f);
        //            bool containsOutput = false;
        //#if v16
        //            foreach (Item item in __instance.output.Value.Items)
        //#else
        //            foreach (Item item in __instance.output.Value.items)
        //#endif
        //            {
        //                if (item != null && item.Category != -12 && item.Category != -2)
        //                {
        //                    containsOutput = true;
        //                    break;
        //                }
        //            }


        //            if (containsOutput)
        //            {
        //                b.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value * 64 + 128 + 12, __instance.tileY.Value * 64 + __instance.tilesHigh.Value * 64 - 32)), bagRect, __instance.color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((__instance.tileY.Value + __instance.tilesHigh.Value - 1) * 64 + 1) / 10000f);
        //            }
        //            if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && !FEFramework.isWinter(__instance.modData) && __instance.wasLit.Value)
        //            {
        //                b.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.tileX.Value * 64 + 64, __instance.tileY.Value * 64 + __instance.tilesHigh.Value * 64 - 64)), lightInteriorRect, __instance.color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((__instance.tileY.Value + __instance.tilesHigh.Value - 1) * 64 + 1) / 10000f);
        //            }

        //            return false;
        //        }
    }
}