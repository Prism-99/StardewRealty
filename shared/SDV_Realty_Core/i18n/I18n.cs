using StardewValley.GameData;
using System;


namespace SDV_Realty_Core
{
    internal static class I18n
    {
        private static ITranslationHelper Translations;
        public static void Init(ITranslationHelper translations)
        {
            Translations = translations;
        }
        public static string GMCM_Monsters()
        {
            return GetByKey("gmcm.expan.monsters");
        }
        public static string WF_Total(int totalWeeds, int price)
        {
            return GetByKey("wf.total", new {weeds=$"{totalWeeds:N0}",total=$"{price:N0}"});
        }
        public static string WF_Quote(int quote)
        {
            return GetByKey("wf.quote", new {cost=$"{quote:N0}"});
        }
        public static string WF_Label()
        {
            return GetByKey("wf.label");
        }
        public static string WF_Label_TT()
        {
            return GetByKey("wf.label.tt");
        }
        public static string WF_Header()
        {
            return GetByKey("wf.header");
        }
        public static string Train_BTL()
        {
            return GetByKey("train.btl");
        }
        public static string WM_ScrollText()
        {
            return GetByKey("wm.scrolltext");
        }
        public static string WM_Requirements()
        {
            return GetByKey("wm.requirements");
        }
        public static string WM_AddPacks()
        {
            return GetByKey("wm.add.packs");
        }
        public static string NoMoney()
        {
            return GetByKey("fs.no.money");
        }
        public static string Size()
        {
            return GetByKey("size");
        }
        public static string PC_NoDirt()
        {
            return GetByKey("pc.no.dirt");
        }
        public static string PC_LineQuote(int cost)
        {
            return GetByKey("pc.line.quote", new { price = $"{cost:N0}" });
        }
        public static string PC_Quote(string cropName, int seedCost, int totalPrice)
        {
            return GetByKey("pc.quote", new { crop = cropName, price = $"{seedCost:N0}", total = $"{totalPrice:N0}" });
        }
        public static string PC_SeedHeader()
        {
            return GetByKey("pc.seed.header");
        }
        public static string PC_Label()
        {
            return GetByKey("pc.label");
        }
        public static string PC_Label_TT()
        {
            return GetByKey("pc.label.tt");
        }
        public static string PC_ExpHeader()
        {
            return GetByKey("pc.exp.header");
        }
        public static string PC_Total(int totalSeeded, int seedPrice, int totalCost)
        {
            return GetByKey("pc.total", new { totalseeded = $"{totalSeeded:N0}", price = $"{seedPrice:N0}", totalcost = $"{totalCost:N0}" });
        }
        public static string CF_Label()
        {
            return GetByKey("cf.label");
        }
        public static string CF_Label_TT()
        {
            return GetByKey("cf.label.tt");
        }
        public static string CF_Header()
        {
            return GetByKey("cf.header");
        }
        public static string CF_Quote( int quote)
        {
            return GetByKey("cf.quote", new {  cost = $"{quote:N0}" });
        }
        public static string CF_Total(string displayName, int cost)
        {
            return GetByKey("cf.total", new { expansionname = displayName, totalCost = $"{cost:N0}" });
        }
        public static string HF_Label()
        {
            return GetByKey("hf.label");
        }
        public static string HF_Label_TT()
        {
            return GetByKey("hf.label.tt");
        }
        public static string HF_Header()
        {
            return GetByKey("hf.header");
        }
        public static string HF_Quote( int quote)
        {
            return GetByKey("hf.quote", new {  cost = $"{quote:N0}" });
        }
        public static string HF_Total(int totalTiles, int totalCost)
        {
            return GetByKey("hf.total", new { tiles = $"{totalTiles:N0}", cost = $"{totalCost:N0}" });
        }
        public static string AF_NoDirt()
        {
            return GetByKey("af.no.dirt");
        }
        public static string AF_Label()
        {
            return GetByKey("af.label");
        }
        public static string AF_Label_TT()
        {
            return GetByKey("af.label.tt");
        }
        public static string AF_Header()
        {
            return GetByKey("af.header");
        }
        public static string AF_Quote(int quote)
        {
            return GetByKey("af.quote", new { cost = $"{quote:N0}" });
        }
        public static string AF_FertHeader()
        {
            return GetByKey("af.fertil.header");
        }
        public static string AF_Total(int totalFertilized, int cost, int total)
        {
            return GetByKey("af.total", new { totalfert = $"{totalFertilized:N0}", fertilizercost = $"{cost:N0}", totalcost = $"{total:N0}" });
        }
        public static string TrainArriving()
        {
            return GetByKey("train.arriving");
        }
        public static string MM_Set()
        {
            return GetByKey("mini.map.set");
        }
        public static string MM_Clear()
        {
            return GetByKey("mini.map.clear");
        }

        public static string MM_Open()
        {
            return GetByKey("mini.map.open");
        }
        public static string GMCM_UseWarps()
        {
            return GetByKey("gmcm.use.map.warps");
        }
        public static string GMCM_UseWarps_TT()
        {
            return GetByKey("gmcm.use.map.warps.tt");
        }

        public static string GMCM_Additional()
        {
            return GetByKey("gmcm.use.additional");
        }
        public static string GMCM_Additional_TT()
        {
            return GetByKey("gmcm.use.additional.tt");
        }

        public static string LocationWatered()
        {
            return GetByKey("location.watered");
        }
        public static string LandManagerLandSold(string locationSold, int sellingPrice)
        {
            return GetByKey("lm.land.sold", new { location = locationSold, price = sellingPrice.ToString("N0") });
        }
        public static string LandManagerNone()
        {
            return GetByKey("lm.none.for.sale");
        }
        public static string LandManagerBuying()
        {
            return GetByKey("lm.buying");
        }
        public static string LandManagerSelling()
        {
            return GetByKey("lm.selling");
        }
        public static string LandManagerMaxExp()
        {
            return GetByKey("lm.max.expansions");
        }
        public static string LandManagerBuyingSelling()
        {
            return GetByKey("lm.buying.or.selling");
        }
        public static string GridManagerSelectDestination()
        {
            return GetByKey("gm.select.destintaion");
        }
        public static string JunimoLetterSubject()
        {
            return GetByKey("junimo.letter.subject");
        }
        public static string JunimoLetterFooter()
        {
            return GetByKey("junimo.letter.footer");
        }
        public static string JunimoLetterSeedDiscount(int discount)
        {
            return GetByKey("junimo.letter.seeddiscount", new { discount = discount.ToString("N0") });
        }
        public static string JunimoLetterReseedService()
        {
            return GetByKey("junimo.letter.reseed");
        }
        public static string JunimoReseedServiceCost(int cost)
        {
            return GetByKey("junimo.letter.reseedcost", new { cost = cost.ToString("N0") });
        }
        public static string JunimoLetterWinterServiceCost(int cost)
        {
            return GetByKey("junimo.letter.wintercost", new { cost = cost.ToString("N0") });
        }
        public static string JunimoLetterWinterService()
        {
            return GetByKey("junimo.letter.winterservice");
        }
        public static string JunimoLetterRainServiceCost(int cost)
        {
            return GetByKey("junimo.letter.raincost", new { cost = cost.ToString("N0") });
        }
        public static string JunimoLetterRainService()
        {
            return GetByKey("junimo.letter.rainservice");
        }
        public static string JunimoLetterHeader()
        {
            return GetByKey("junimo.letter.header");
        }
        public static string JunimoLetterIntro()
        {
            return GetByKey("junimo.letter.introduction");
        }
        public static string GotoRanch()
        {
            return GetByKey("goto.ranch");
        }
        public static string GotoCarpenter()
        {
            return GetByKey("goto.carpenter");
        }

        public static string GMCM_GameFixes()
        {
            return GetByKey("gmcm.game.fixes");
        }
        public static string GMCM_FixHoppers()
        {
            return GetByKey("gmcm.game.hopper");            
        }
        public static string GMCM_FixHoppers_TT()
        {
            return GetByKey("gmcm.game.hopper.tt");
        }
        public static string GMCM_GlobalPrice_TT()
        {
            return GetByKey("gmcm.realty.globalprice.tt");
        }

        public static string GMCM_GlobalPrice()
        {
            return GetByKey("gmcm.realty.globalprice");
        }
        public static string GMCM_UseGlobalPrice_TT()
        {
            return GetByKey("gmcm.realty.useglobalprice.tt");
        }

        public static string GMCM_UseGlobalPrice()
        {
            return GetByKey("gmcm.realty.useglobalprice");
        }
        public static string GMCM_GlobalCond_TT()
        {
            return GetByKey("gmcm.realty.globalcondition.tt");
        }
        public static string GMCM_GlobalCond()
        {
            return GetByKey("gmcm.realty.globalcondition");
        }
        public static string GMCM_UseGlobalCond_TT()
        {
            return GetByKey("gmcm.realty.useglobalcondition.tt");
        }
        public static string GMCM_UseGlobalCond()
        {
            return GetByKey("gmcm.realty.useglobalcondition");
        }
        public static string GMCM_Globals()
        {
            return GetByKey("gmcm.realty.globals");
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
        //
        //  premium junimo related
        //
        public static string GMCM_JO_Enabled()
        {
            return GetByKey("gmcm.junimos.premium");
        }
        public static string GMCM_JO_Enabled_TT()
        {
            return GetByKey("gmcm.junimos.premium.tt");
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
        public static string GMCM_MeadowsWarp_Keybind()
        {
            return GetByKey("gmcm.realty.Keybind");
        }
        public static string GMCM_MeadowsWarp_Keybind_TT()
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
        public static string GMCM_ForSale()
        {
            return GetByKey("gmcm.forsale");
        }
        public static string GMCM_ForSale_TT()
        {
            return GetByKey("gmcm.forsale.tt");
        }
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

        public static string GMCM_MeadowsTitle()
        {
            return GetByKey("gmcm.meadows.title");
        }
        public static string GMCM_MeadowsTrainEnable()
        {
            return GetByKey("gmcm.meadows.train.enable");
        }
        public static string GMCM_MeadowsTrainEnable_TT()
        {
            return GetByKey("gmcm.meadows.train.enable.tt");
        }
        public static string GMCM_MeadowsTrainTime()
        {
            return GetByKey("gmcm.meadows.train.arrival");
        }
        public static string GMCM_MeadowsTrainTime_TT()
        {
            return GetByKey("gmcm.meadows.train.arrival.tt");
        }
        public static string GMCM_MeadowsRing()
        {
            return GetByKey("gmcm.meadows.warp.ring");
        }
        public static string GMCM_MeadowsRing_TT()
        {
            return GetByKey("gmcm.meadows.warp.ring.tt");
        }

        public static string TrainStation()
        {
            return GetByKey("train.station");
        }
        public static string BoatDock()
        {
            return GetByKey("boat.dock");
        }
        public static string BusStop()
        {
            return GetByKey("bus.stop");
        }
        public static string Cost(int cost)
        {
            return GetByKey("cost", new { price = cost.ToString("N0") });
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
        public static Translation GetByKey(string key, object tokens = null)
        {
            if (Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(Init)} from the mod's entry method before reading translations.");
            return Translations.Get(key, tokens);
        }

    }
}
