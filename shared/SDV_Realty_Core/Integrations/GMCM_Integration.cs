using GenericModConfigMenu;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.Objects;



namespace SDV_Realty_Core.Framework.Integrations
{
    internal class GMCM_Integration
    {
        private static IManifest _manifest;
        private static ILoggerService _logger;
        private static IConfigService _configService;
        private static IModHelperService _modHelperService;
        private static IModDataService _modDataService;
        private static void ResetGuiVars()
        {
            //
            //  reset the exposed config items to their
            //  default values.
            //
            _modDataService.Config.GlobalForageMap = "";
            _modDataService.Config.GlobalForageTileX = 0;
            _modDataService.Config.GlobalForageTileY = 0;
            _modDataService.Config.UseGracePeriod = false;
            _modDataService.Config.GraceForCrops = true;
            _modDataService.Config.GraceForFruit = false;
            _modDataService.Config.SpringGracePeriod = 3;
            _modDataService.Config.SummerGracePeriod = 3;
            _modDataService.Config.FallGracePeriod = 3;
            _modDataService.Config.WinterGracePeriod = 3;
            _modDataService.Config.DoGainExperience = false;
            _modDataService.Config.DoHarvestCrops = false;
            _modDataService.Config.DoHarvestFruitTrees = false;
            _modDataService.Config.EnableDeluxeAutoGrabberOnExpansions = false;
            _modDataService.Config.EnableDeluxeAutoGrabberOnExpansions = true;
            _modDataService.Config.DoGlobalForage = false;
            _modDataService.Config.DoHarvestTruffles = false;
            _modDataService.Config.GrabberRange = 30;
            _modDataService.Config.AlwaysShowMagicMenu = false;
            _modDataService.Config.WarpRoomKey = new KeybindList(new Keybind(SButton.Z, SButton.LeftControl), new Keybind(SButton.Z, SButton.RightControl));
            _modDataService.Config.EnablePremiumJunimos = true;
            _modDataService.Config.MaxNumberJunimos = 3;
            _modDataService.Config.JunimoMaxRadius = 8;
            _modDataService.Config.JunimoReseedCrop = true;
            _modDataService.Config.JunimosChargeForSeeds = true;
            _modDataService.Config.JunimosFeeForSeeding = 1;
            _modDataService.Config.JunimosWorkInWinter = false;
            _modDataService.Config.JunimoWinterFee = 2;
            _modDataService.Config.RealTimeMoney = true;
            _modDataService.Config.GlobalMaxJunimos = 40;
            _modDataService.Config.useSouthWestEntrance = true;
            _modDataService.Config.useNorthWestEntrance = true;
            _modDataService.Config.enableDebug = false;
            _modDataService.Config.AddBridgeSeat = true;
            _modDataService.Config.AddFishPondLight = true;
            _modDataService.Config.UseCustomWeather = true;
            _modDataService.Config.LightLevel = 1;
            _modDataService.Config.useGlobalCondition = false;
            _modDataService.Config.globalCondition = "";
            _modDataService.Config.useGlobalPrice = false;
            _modDataService.Config.globalPrice = 0;
            _modDataService.Config.UseMapWarps = true;
            _modDataService.Config.UseAdditionalFarms = true;
            _modDataService.Config.TrainArrivalTime = 1200;
            _modDataService.Config.TrainEnabled = true;
        }
        public GMCM_Integration(IModDataService modDataService, IModHelperService helperService, IManifest manifest, IConfigService configService, ILoggerService olog)
        {
            _modDataService = modDataService;
            _modHelperService = helperService;
            _manifest = manifest;
            _configService = configService;
            _logger = olog;
        }
        internal void RegisterMenu()
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = _modHelperService.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            _logger.Log($"Registering with GMCM", LogLevel.Debug);

            // register mod
            configMenu.Register(
                mod: _manifest,
                reset: ResetGuiVars,
                save: () => _configService.SaveConfig(),
                 titleScreenOnly: false
            );
            //
            //  create config GUI
            //
            //  Realty Options
            //
            #region "Base Options"
            configMenu.AddSectionTitle(
                mod: _manifest,
                text: () => I18n.GMCM_Realty_Options(),
                tooltip: () => ""
            );


            //
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_Additional(),
                 tooltip: () => I18n.GMCM_Additional_TT(),
                 getValue: () => _modDataService.Config.UseAdditionalFarms,
                 setValue: value => _modDataService.Config.UseAdditionalFarms = value
             );

            configMenu.AddBoolOption(
             mod: _manifest,
             name: () => I18n.GMCM_UseWarps(),
             tooltip: () => I18n.GMCM_UseWarps_TT(),
             getValue: () => _modDataService.Config.UseMapWarps,
             setValue: value => _modDataService.Config.UseMapWarps = value
            );

            configMenu.AddBoolOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_SkipBuildConditions(),
                  tooltip: () => I18n.GMCM_SkipBuildConditions_TT(),
                  getValue: () => _modDataService.Config.SkipBuildingConditions,
                  setValue: value => _modDataService.Config.SkipBuildingConditions = value
             );

            configMenu.AddBoolOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_UseNWExit(),
                  tooltip: () => I18n.GMCM_UseNWExit_TT(),
                  getValue: () => _modDataService.Config.useNorthWestEntrance,
                  setValue: value => _modDataService.Config.useNorthWestEntrance = value
             );

            configMenu.AddBoolOption(
               mod: _manifest,
               name: () => I18n.GMCM_UseSWExit(),
               tooltip: () => I18n.GMCM_UseSWExit_TT(),
               getValue: () => _modDataService.Config.useSouthWestEntrance,
               setValue: value => _modDataService.Config.useSouthWestEntrance = value
           );
            configMenu.AddBoolOption(
           mod: _manifest,
           name: () => I18n.GMCM_ForSale(),
           tooltip: () => I18n.GMCM_ForSale_TT(),
           getValue: () => _modDataService.Config.UseTownForSaleSign,
           setValue: value => _modDataService.Config.UseTownForSaleSign = value
       );
            configMenu.AddKeybindList(
                mod: _manifest,
                name: () => I18n.GMCM_MeadowsWarp_Keybind(),
                tooltip: () => I18n.GMCM_MeadowsWarp_Keybind_TT(),
                 getValue: () => _modDataService.Config.WarpRoomKey,
                 setValue: value => _modDataService.Config.WarpRoomKey = value

             );

            #endregion

            #region "Stardew Meadows Options"
            configMenu.AddSectionTitle(
              mod: _manifest,
              text: () => I18n.GMCM_MeadowsTitle(),
              tooltip: () => ""
          );
            configMenu.AddBoolOption(
               mod: _manifest,
               name: () => I18n.GMCM_MeadowsTrainEnable(),
               tooltip: () => I18n.GMCM_MeadowsTrainEnable_TT(),
               getValue: () => _modDataService.Config.TrainEnabled,
               setValue: value => _modDataService.Config.TrainEnabled = value
           );

            configMenu.AddNumberOption(
            mod: _manifest,
            name: () => I18n.GMCM_MeadowsTrainTime(),
            tooltip: () => I18n.GMCM_MeadowsTrainTime_TT(),
            getValue: () => _modDataService.Config.TrainArrivalTime,
            setValue: value => _modDataService.Config.TrainArrivalTime = value
        );

            configMenu.AddBoolOption(
         mod: _manifest,
         name: () => I18n.GMCM_MeadowsRing(),
         tooltip: () => I18n.GMCM_MeadowsRing_TT(),
         getValue: () => _modDataService.Config.UseWarpRing,
         setValue: value => _modDataService.Config.UseWarpRing = value
     );

            #endregion

            #region "globals"
            configMenu.AddSectionTitle(
               mod: _manifest,
               text: () => I18n.GMCM_Globals(),
               tooltip: () => ""
           );

            configMenu.AddBoolOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_Realty_SkipRequirements(),
                  tooltip: () => I18n.GMCM_Realty_SkipRequirements_TT(),
                  getValue: () => _modDataService.Config.SkipRequirements,
                  setValue: value => _modDataService.Config.SkipRequirements = value
              );

            configMenu.AddBoolOption(
             mod: _manifest,
             name: () => I18n.GMCM_UseGlobalCond(),
             tooltip: () => I18n.GMCM_UseGlobalCond_TT(),
             getValue: () => _modDataService.Config.useGlobalCondition,
             setValue: value => _modDataService.Config.useGlobalCondition = value
         );
            configMenu.AddTextOption(
               mod: _manifest,
               name: () => I18n.GMCM_GlobalCond(),
               tooltip: () => I18n.GMCM_GlobalCond_TT(),
               getValue: () => _modDataService.Config.globalCondition ?? "",
               setValue: value => _modDataService.Config.globalCondition = value
            );

            configMenu.AddTextOption(_manifest, () => _modDataService.Config.globalPriceMode
            , delegate (string v)
            {
                _modDataService.Config.globalPriceMode = v;
            }, () =>"Global Price Mode", () => "", _modDataService.Config.globalPriceModes);

       //     configMenu.AddBoolOption(
       //    mod: _manifest,
       //    name: () => I18n.GMCM_UseGlobalPrice(),
       //    tooltip: () => I18n.GMCM_UseGlobalPrice_TT(),
       //    getValue: () => _modDataService.Config.useGlobalPrice,
       //    setValue: value => _modDataService.Config.useGlobalPrice = value
       //);
            configMenu.AddNumberOption(
             mod: _manifest,
             name: () => I18n.GMCM_GlobalPrice(),
             tooltip: () => I18n.GMCM_GlobalPrice_TT(),
             getValue: () => _modDataService.Config.globalPrice,
             setValue: value => _modDataService.Config.globalPrice = value
         );
            #endregion
            //
            //  Custom Junimo Options
            //
            #region "Custom Junimo"
            configMenu.AddSectionTitle(
                 mod: _manifest,
                 text: () => I18n.GMCM_JO_Title(),
                 tooltip: () => ""
             );
            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_JO_Enabled(),
                tooltip: () => I18n.GMCM_JO_Enabled_TT(),
                getValue: () => _modDataService.Config.EnablePremiumJunimos,
                setValue: value => _modDataService.Config.EnablePremiumJunimos = value
            );
            configMenu.AddNumberOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_MaxJos(),
                 tooltip: () => I18n.GMCM_JO_MaxJos_TT(),
                 getValue: () => _modDataService.Config.MaxNumberJunimos,
                 setValue: value => _modDataService.Config.MaxNumberJunimos = value
             );
            configMenu.AddNumberOption(
                mod: _manifest,
                name: () => I18n.GMCM_JO_GlobalMaxJos(),
                tooltip: () => I18n.GMCM_JO_GlobalMaxJos_TT(),
                getValue: () => _modDataService.Config.GlobalMaxJunimos,
                setValue: value => _modDataService.Config.GlobalMaxJunimos = value
            );
            configMenu.AddNumberOption(
                mod: _manifest,
                name: () => I18n.GMCM_JO_MaxRad(),
                tooltip: () => I18n.GMCM_JO_MaxRad_TT(),
                getValue: () => _modDataService.Config.JunimoMaxRadius,
                setValue: value => _modDataService.Config.JunimoMaxRadius = value
            );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_RealTime(),
                 tooltip: () => I18n.GMCM_JO_RealTime_TT(),
                 getValue: () => _modDataService.Config.RealTimeMoney,
                 setValue: value => _modDataService.Config.RealTimeMoney = value
             );

            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Reseed(),
                 tooltip: () => I18n.GMCM_JO_Reseed_TT(),
                 getValue: () => _modDataService.Config.JunimoReseedCrop,
                 setValue: value => _modDataService.Config.JunimoReseedCrop = value
             );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Seeds_Charge(),
                 tooltip: () => I18n.GMCM_JO_Seeds_Charge_TT(),
                 getValue: () => _modDataService.Config.JunimosChargeForSeeds,
                 setValue: value => _modDataService.Config.JunimosChargeForSeeds = value
             );
            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_JO_Charge_Seeding(),
                  tooltip: () => I18n.GMCM_JO_Charge_Seeding_TT(),
                  getValue: () => _modDataService.Config.JunimosFeeForSeeding,
                  setValue: value => _modDataService.Config.JunimosFeeForSeeding = value
              );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Work_Rain(),
                 tooltip: () => I18n.GMCM_JO_Work_Rain_TT(),
                 getValue: () => _modDataService.Config.JunimosWorkInRain,
                 setValue: value => _modDataService.Config.JunimosWorkInRain = value
             );
            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_JO_Rain_Fee(),
                  tooltip: () => I18n.GMCM_JO_Rain_Fee_TT(),
                  getValue: () => _modDataService.Config.JunimoRainFee,
                  setValue: value => _modDataService.Config.JunimoRainFee = value
              );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Work_Winter(),
                 tooltip: () => I18n.GMCM_JO_Work_Winter_TT(),
                 getValue: () => _modDataService.Config.JunimosWorkInWinter,
                 setValue: value => _modDataService.Config.JunimosWorkInWinter = value
             );
            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_JO_Work_Winter_Fee(),
                  tooltip: () => I18n.GMCM_JO_Work_Winter_Fee_TT(),
                  getValue: () => _modDataService.Config.JunimoWinterFee,
                  setValue: value => _modDataService.Config.JunimoWinterFee = value
              );
            #endregion
            //
            //  Deluxe Autograbber Options
            //
            #region "Deluxe Autograbber Options"
            configMenu.AddSectionTitle(
                mod: _manifest,
                text: () => I18n.GMCM_AutoGrab_Title(),
                tooltip: () => ""
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_EnableAG(),
                tooltip: () => I18n.GMCM_AutoGrab_EnableAG_TT(),
                getValue: () => _modDataService.Config.EnableDeluxeAutoGrabberOnExpansions,
                setValue: value => _modDataService.Config.EnableDeluxeAutoGrabberOnExpansions = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GCMC_AutoGrab_EnableAG_HomeFarm(),
                tooltip: () => I18n.GMCM_AutoGrab_EnableAG_HomeFarm_TT(),
                getValue: () => _modDataService.Config.EnableDeluxeAutoGrabberOnHomeFarm,
                setValue: value => _modDataService.Config.EnableDeluxeAutoGrabberOnHomeFarm = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestCrops(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestCrops_TT(),
                getValue: () => _modDataService.Config.DoHarvestCrops,
                setValue: value => _modDataService.Config.DoHarvestCrops = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestFruitTrees(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFruitTrees_TT(),
                getValue: () => _modDataService.Config.DoHarvestFruitTrees,
                setValue: value => _modDataService.Config.DoHarvestFruitTrees = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestFlowers(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFlowers_TT(),
                getValue: () => _modDataService.Config.DoHarvestFlowers,
                setValue: value => _modDataService.Config.DoHarvestFlowers = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoGainExperience(),
                tooltip: () => I18n.GMCM_AutoGrab_DoGainExperience_TT(),
                getValue: () => _modDataService.Config.DoGainExperience,
                setValue: value => _modDataService.Config.DoGainExperience = value
            );

            configMenu.AddNumberOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_AutoGrab_GrabberRange(),
                 tooltip: () => I18n.GMCM_AutoGrab_GrabberRange_TT(),
                 getValue: () => _modDataService.Config.GrabberRange,
                 setValue: value => _modDataService.Config.GrabberRange = value
             );
            #endregion
            //
            //  Global Autograbber Options
            //
            #region "Global Autograbber"
            configMenu.AddSectionTitle(
                mod: _manifest,
                text: () => I18n.GMCM_AutoGrab_Global(),
                tooltip: () => ""
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoGlobalForage(),
                tooltip: () => I18n.GMCM_AutoGrab_DoGlobalForage_TT(),
                getValue: () => _modDataService.Config.DoGlobalForage,
                setValue: value => _modDataService.Config.DoGlobalForage = value
            );

            configMenu.AddTextOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_GlobalForageMap(),
                tooltip: () => I18n.GMCM_AutoGrab_GlobalForageMap_TT(),
                getValue: () => _modDataService.Config.GlobalForageMap ?? "",
                setValue: value => _modDataService.Config.GlobalForageMap = value
            );


            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_AutoGrab_GlobalForageX(),
                  tooltip: () => I18n.GMCM_AutoGrab_GlobalForageX_TT(),
                  getValue: () => _modDataService.Config.GlobalForageTileX,
                  setValue: value => _modDataService.Config.GlobalForageTileX = value
              );

            configMenu.AddNumberOption(
                   mod: _manifest,
                   name: () => I18n.GMCM_AutoGrab_GlobalForageY(),
                   tooltip: () => I18n.GMCM_AutoGrab_GlobalForageY_TT(),
                   getValue: () => _modDataService.Config.GlobalForageTileY,
                   setValue: value => _modDataService.Config.GlobalForageTileY = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestTruffles(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestTruffles_TT(),
                getValue: () => _modDataService.Config.DoHarvestTruffles,
                setValue: value => _modDataService.Config.DoHarvestTruffles = value
            );

            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_AutoGrab_DoHarvestFarmCave(),
                 tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFarmCave_TT(),
                 getValue: () => _modDataService.Config.DoHarvestFarmCave,
                 setValue: value => _modDataService.Config.DoHarvestFarmCave = value
             );
            #endregion
            //
            //  Grace Period Options
            //
            #region "Grace Period"
            configMenu.AddSectionTitle(
               mod: _manifest,
               text: () => I18n.GMCM_GP_Title(),
               tooltip: () => ""
           );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_GP_Enabled(),
                tooltip: () => I18n.GMCM_GP_Enabled_TT(),
                getValue: () => _modDataService.Config.UseGracePeriod,
                setValue: value => _modDataService.Config.UseGracePeriod = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_GP_ForCrops(),
                tooltip: () => I18n.GMCM_GP_ForCrops_TT(),
                getValue: () => _modDataService.Config.GraceForCrops,
                setValue: value => _modDataService.Config.GraceForCrops = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_GP_ForFruits(),
                tooltip: () => I18n.GMCM_GP_ForFruits_TT(),
                getValue: () => _modDataService.Config.GraceForFruit,
                setValue: value => _modDataService.Config.GraceForFruit = value
            );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Spring(),
                    tooltip: () => "",
                    getValue: () => _modDataService.Config.SpringGracePeriod,
                    setValue: value => _modDataService.Config.SpringGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Summer(),
                    tooltip: () => "",
                    getValue: () => _modDataService.Config.SummerGracePeriod,
                    setValue: value => _modDataService.Config.SummerGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Fall(),
                    tooltip: () => "",
                    getValue: () => _modDataService.Config.FallGracePeriod,
                    setValue: value => _modDataService.Config.FallGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Winter(),
                    tooltip: () => "",
                    getValue: () => _modDataService.Config.WinterGracePeriod,
                    setValue: value => _modDataService.Config.WinterGracePeriod = value
             );
            #endregion
            //
            //  extras
            //
            #region Extras
            configMenu.AddSectionTitle(
               mod: _manifest,
               text: () => I18n.GMCM_Extras(),
               tooltip: () => ""
           );


            configMenu.AddBoolOption(
                   mod: _manifest,
                   name: () => I18n.GMCM_AddBridge(),
                   tooltip: () => I18n.GMCM_AddBridge_TT(),
                   getValue: () => _modDataService.Config.AddBridgeSeat,
                   setValue: value => _modDataService.Config.AddBridgeSeat = value
               );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_FishPondLight(),
                tooltip: () => I18n.GMCM_FishPondLight_TT(),
                getValue: () => _modDataService.Config.AddFishPondLight,
                setValue: value => _modDataService.Config.AddFishPondLight = value
            );
            configMenu.AddBoolOption(
              mod: _manifest,
              name: () => I18n.GMCM_BuildingLights(),
              tooltip: () => I18n.GMCM_BuildingLights_TT(),
              getValue: () => _modDataService.Config.EnableBuildingLights,
              setValue: value => _modDataService.Config.EnableBuildingLights = value
          );
            configMenu.AddNumberOption(
                mod: _manifest,
                name: () => I18n.GMCM_LightLevel(),
                tooltip: () => I18n.GMCM_LightLevel_TT(),
                getValue: () => (int)(_modDataService.Config.LightLevel * 100),
                setValue: value => _modDataService.Config.LightLevel = value / 100f
            );

            configMenu.AddBoolOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_UseCustomWeather(),
                    tooltip: () => I18n.GMCM_UseCustomWeather_TT(),
                    getValue: () => _modDataService.Config.UseCustomWeather,
                    setValue: value => _modDataService.Config.UseCustomWeather = value
                );
            #endregion


            #region "Game Fixes"
            configMenu.AddSectionTitle(
              mod: _manifest,
              text: () => I18n.GMCM_GameFixes(),
              tooltip: () => ""
          );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_FixHoppers(),
                 tooltip: () => I18n.GMCM_FixHoppers_TT(),
                 getValue: () => _modDataService.Config.FixHoppers,
                 setValue: value => _modDataService.Config.FixHoppers = value
             );
            #endregion


            #region "Debug Options"
            //
            //  Debug Option
            //
            configMenu.AddSectionTitle(
               mod: _manifest,
               text: () => I18n.GMCM_Debug_Title(),
               tooltip: () => ""
           );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_Debug(),
                tooltip: () => I18n.GMCM_Debug_TT(),
                getValue: () => _modDataService.Config.enableDebug,
                setValue: value =>
                {
                    _modDataService.Config.enableDebug = value;
                    _logger.Debug = value;
                }
            );
            #endregion
        }
    }
}
