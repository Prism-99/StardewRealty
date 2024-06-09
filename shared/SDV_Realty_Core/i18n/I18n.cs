using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;

namespace SDV_Realty_Core
{
    internal static class I18n
    {
        private static ITranslationHelper Translations;
        public static void Init(ITranslationHelper translations)
        {
            Translations = translations;
        }
        public static string GotoRanch()
        {
            return GetByKey("goto.ranch");
        }
        public static string GotoCarpenter()
        {
            return GetByKey("goto.carpenter");
        }
   
        public static string GMCM_BuildingLights_TT()
        {
            return GetByKey("gmcm.realty.buildinglights.tt");
        }
        public static string GMCM_BuildingLights()
        {
            return GetByKey("gmcm.realty.buildinglights");
        }
        public static string GMCM_Extras()
        {
            return GetByKey("gmcm.realty.extras");
        }
        public static string GMCM_LightLevel()
        {
            return GetByKey("gmcm.realty.lightlevel");
        }
        public static string GMCM_LightLevel_TT()
        {
            return GetByKey("gmcm.realty.lightlevel.tt");
        }

        public static string GMCM_UseCustomWeather()
        {
            return GetByKey("gmcm.realty.useCustomWeather");
        }
        public static string GMCM_UseCustomWeather_TT()
        {
            return GetByKey("gmcm.realty.useCustomWeather.tt");
        }
        public static string GMCM_FishPondLight()
        {
            return GetByKey("gmcm.realty.AddFishPondLights");
        }
        public static string GMCM_FishPondLight_TT()
        {
            return GetByKey("gmcm.realty.AddFishPondLights.tt");
        }

        public static string GMCM_SkipBuildConditions()
        {
            return GetByKey("gmcm.realty.SkipBuildConditions");
        }
        public static string GMCM_SkipBuildConditions_TT()
        {
            return GetByKey("gmcm.realty.SkipBuildConditions.tt");
        }

        public static string GMCM_AddBridge()
        {
            return GetByKey("gmcm.realty.AddBridgeSeat");
        }
        public static string GMCM_AddBridge_TT()
        {
            return GetByKey("gmcm.realty.AddBridgeSeat.tt");
        }

        public static string GMCM_UseNWExit()
        {
            return GetByKey("gmcm.realty.usenwexit");
        }
        public static string GMCM_UseNWExit_TT()
        {
            return GetByKey("gmcm.realty.usenwexit.tt");
        }
        public static string GMCM_UseSWExit()
        {
            return GetByKey("gmcm.realty.useswexit");
        }
        public static string GMCM_UseSWExit_TT()
        {
            return GetByKey("gmcm.realty.useswexit.tt");
        }
        public static string WarpInUse()
        {
            return GetByKey("warproom.inuse");
        }
        public static string NeedPacks()
        {
            return GetByKey("need_packs");
        }
        public static string GMCM_JO_RealTime()
        {
            return GetByKey("gmcm.jo.realtime");
        }
        public static string GMCM_JO_RealTime_TT()
        {
            return GetByKey("gmcm.jo.realtime.tt");
        }
        public static string GMCM_JO_Work_Winter_Fee_TT()
        {
            return GetByKey("gmcm.jo.winter.fee.tt");
        }
        public static string GMCM_JO_Work_Winter_Fee()
        {
            return GetByKey("gmcm.jo.winter.fee");
        }
        public static string GMCM_JO_Work_Winter_TT()
        {
            return GetByKey("gmcm.jo.work.winter.tt");
        }
        public static string GMCM_JO_Work_Winter()
        {
            return GetByKey("gmcm.jo.work.winter");
        }
        public static string GMCM_JO_Rain_Fee_TT()
        {
            return GetByKey("gmcm.jo.rain.fee.tt");
        }
        public static string GMCM_JO_Rain_Fee()
        {
            return GetByKey("gmcm.jo.rain.fee");
        }
        public static string GMCM_JO_Work_Rain_TT()
        {
            return GetByKey("gmcm.jo.work.rain.tt");
        }
        public static string GMCM_JO_Work_Rain()
        {
            return GetByKey("gmcm.jo.work.rain");
        }
        public static string GMCM_JO_Charge_Seeding_TT()
        {
            return GetByKey("gmcm.jo.charge.seeding.tt");
        }
        public static string GMCM_JO_Charge_Seeding()
        {
            return GetByKey("gmcm.jo.charge.seeding");
        }
        public static string GMCM_JO_Seeds_Charge_TT()
        {
            return GetByKey("gmcm.jo.charge.seeds.tt");
        }
        public static string GMCM_JO_Seeds_Charge()
        {
            return GetByKey("gmcm.jo.charge.seeds");
        }
        public static string GMCM_JO_Reseed()
        {
            return GetByKey("gmcm.jo.reseed");
        }
        public static string GMCM_JO_Reseed_TT()
        {
            return GetByKey("gmcm.jo.reseed.tt");
        }
        public static string GMCM_JO_Title()
        {
            return GetByKey("gmcm.jo.Title");
        }
        public static string GMCM_JO_GlobalMaxJos_TT()
        {
            return GetByKey("gmcm.jo.globalmax.tt");
        }
        public static string GMCM_JO_GlobalMaxJos()
        {
            return GetByKey("gmcm.jo.globalmax");
        }
        public static string GMCM_JO_MaxJos()
        {
            return GetByKey("gmcm.jo.maxjos");
        }
        public static string GMCM_JO_MaxJos_TT()
        {
            return GetByKey("gmcm.jo.maxjos.tt");
        }
        public static string GMCM_JO_MaxRad()
        {
            return GetByKey("gmcm.jo.maxrad");
        }
        public static string GMCM_JO_MaxRad_TT()
        {
            return GetByKey("gmcm.jo.maxrad.tt");
        }
        public static string GMCM_AutoGrab_DoHarvestFarmCave()
        {
            return GetByKey("gmcm.autograb.DoHarvestFarmCave");
        }
        public static string GMCM_AutoGrab_DoHarvestFarmCave_TT()
        {
            return GetByKey("gmcm.autograb.DoHarvestFarmCave.tt");
        }

        public static string GMCM_AutoGrab_GlobalForageX()
        {
            return GetByKey("gmcm.autograb.GlobalForageX");
        }
        public static string GMCM_AutoGrab_GlobalForageX_TT()
        {
            return GetByKey("gmcm.autograb.GlobalForageX.tt");
        }
        public static string GMCM_AutoGrab_GlobalForageY()
        {
            return GetByKey("gmcm.autograb.GlobalForageY");
        }
        public static string GMCM_AutoGrab_GlobalForageY_TT()
        {
            return GetByKey("gmcm.autograb.GlobalForageY.tt");
        }
        public static string GMCM_AutoGrab_GlobalForageMap()
        {
            return GetByKey("gmcm.autograb.GlobalForageMap");
        }
        public static string GMCM_AutoGrab_GlobalForageMap_TT()
        {
            return GetByKey("gmcm.autograb.GlobalForageMap.tt");
        }
        public static string GMCM_AutoGrab_DoGlobalForage()
        {
            return GetByKey("gmcm.autograb.DoGlobalForage");
        }
        public static string GMCM_AutoGrab_DoGlobalForage_TT()
        {
            return GetByKey("gmcm.autograb.DoGlobalForage.tt");
        }
        public static string GMCM_AutoGrab_Global()
        {
            return GetByKey("gmcm.autograb.Global");
        }
        public static string GMCM_AutoGrab_Title()
        {
            return GetByKey("gmcm.autograb.title");
        }
        public static string GMCM_AutoGrab_EnableAG_TT()
        {
            return GetByKey("gmcm.autograb.EnableAG.tt");
        }
        public static string GMCM_AutoGrab_Keybind()
        {
            return GetByKey("gmcm.realty.Keybind");
        }
        public static string GMCM_AutoGrab_Keybind_TT()
        {
            return GetByKey("gmcm.realty.Keybind.tt");
        }
        public static string GMCM_Realty_SkipRequirements()
        {
            return GetByKey("gmcm.realty.SkipRequirements");
        }
        public static string GMCM_Realty_SkipRequirements_TT()
        {
            return GetByKey("gmcm.realty.SkipRequirements.tt");
        }
        public static string GMCM_Realty_Options()
        {
            return GetByKey("gmcm.realty.Options");
        }
        public static string GMCM_Realty_ShowMagical()
        {
            return GetByKey("gmcm.realty.ShowMagical");
        }
        public static string GMCM_Realty_ShowMagical_TT()
        {
            return GetByKey("gmcm.realty.ShowMagical.tt");
        }
        public static string GMCM_Realty_CustomOnHome()
        {
            return GetByKey("gmcm.realty.CustomOnHome");
        }
        public static string GMCM_Realty_CustomOnHome_TT()
        {
            return GetByKey("gmcm.realty.CustomOnHome.tt");
        }
        public static string GCMC_AutoGrab_EnableAG_HomeFarm()
        {
            return GetByKey("gmcm.autograb.EnableAG.HomeFarm");
        }
        public static string GMCM_AutoGrab_EnableAG()
        {
            return GetByKey("gmcm.autograb.EnableAG");
        }
        public static string GMCM_AutoGrab_EnableAG_HomeFarm_TT()
        {
            return GetByKey("gmcm.autograb.EnableAG.HomeFarm.tt");
        }
        public static string GMCM_AutoGrab_DoHarvestCrops()
        {
            return GetByKey("gmcm.autograb.DoHarvestCrops");
        }
        public static string GMCM_AutoGrab_DoHarvestCrops_TT()
        {
            return GetByKey("gmcm.autograb.DoHarvestCrops.tt");
        }
        public static string GMCM_AutoGrab_DoHarvestFruitTrees()
        {
            return GetByKey("gmcm.autograb.DoHarvestFruitTrees");
        }
        public static string GMCM_AutoGrab_DoHarvestFruitTrees_TT()
        {
            return GetByKey("gmcm.autograb.DoHarvestFruitTrees.tt");
        }
        public static string GMCM_AutoGrab_DoHarvestFlowers()
        {
            return GetByKey("gmcm.autograb.DoHarvestFlowers");
        }
        public static string GMCM_AutoGrab_DoHarvestFlowers_TT()
        {
            return GetByKey("gmcm.autograb.DoHarvestFlowers.tt");
        }
        public static string GMCM_AutoGrab_DoGainExperience()
        {
            return GetByKey("gmcm.autograb.DoGainExperience");
        }
        public static string GMCM_AutoGrab_DoGainExperience_TT()
        {
            return GetByKey("gmcm.autograb.DoGainExperience.tt");
        }
        public static string GMCM_AutoGrab_GrabberRange()
        {
            return GetByKey("gmcm.autograb.GrabberRange");
        }
        public static string GMCM_AutoGrab_GrabberRange_TT()
        {
            return GetByKey("gmcm.autograb.GrabberRange.tt");
        }
        public static string GMCM_AutoGrab_DoHarvestTruffles()
        {
            return GetByKey("gmcm.autograb.DoHarvestTruffles");
        }
        public static string GMCM_AutoGrab_DoHarvestTruffles_TT()
        {
            return GetByKey("gmcm.autograb.DoHarvestTruffles.tt");
        }
        public static string RobinBusy()
        {
            return GetByKey("robin.busy");
        }
        //
        //  debug
        //
        public static string GMCM_Debug()
        {
            return GetByKey("gmcm.debug");
        }
        public static string GMCM_Debug_TT()
        {
            return GetByKey("gmcm.debug.tt");
        }
        public static string GMCM_Debug_Title()
        {
            return GetByKey("gmcm.debug.title");
        }
        //
        //  grace period options
        //
        public static string GMCM_GP_Title()
        {
            return GetByKey("gmcm.gp.Title");
        }
        public static string GMCM_GP_Enabled()
        {
            return GetByKey("gmcm.gp.Enabled");
        }
        public static string GMCM_GP_Enabled_TT()
        {
            return GetByKey("gmcm.gp.Enabled.tt");
        }
        public static string GMCM_GP_Spring()
        {
            return GetByKey("gmcm.gp.Spring");
        }
        public static string GMCM_GP_Summer()
        {
            return GetByKey("gmcm.gp.Summer");
        }
        public static string GMCM_GP_Fall()
        {
            return GetByKey("gmcm.gp.Fall");
        }
        public static string GMCM_GP_Winter()
        {
            return GetByKey("gmcm.gp.Winter");
        }
        public static string GMCM_GP_ForCrops()
        {
            return GetByKey("gmcm.gp.ForCrops");
        }
        public static string GMCM_GP_ForCrops_TT()
        {
            return GetByKey("gmcm.gp.ForCrops.tt");
        }
        public static string GMCM_GP_ForFruits()
        {
            return GetByKey("gmcm.gp.ForFruits");
        }
        public static string GMCM_GP_ForFruits_TT()
        {
            return GetByKey("gmcm.gp.ForFruits.tt");
        }



        public static string Cost(int cost)
        {
            return GetByKey("cost", new { price=cost.ToString("N0") });
        }
        public static string Purchase()
        {
            return GetByKey("purchase");
        }
        public static string CheckMsgBd()
        {
            return GetByKey("check_msgbd");
        }
        public static string LandForSale()
        {
            return GetByKey("land_for_sale");
        }
        public static string DeedTomorrow()
        {
            return GetByKey("deed_tomorrow");
        }
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(Init)} from the mod's entry method before reading translations.");
            return Translations.Get(key, tokens);
        }
    }
}
