using GenericModConfigMenu;
using SDV_Realty_Core.Framework.Objects;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;


namespace SDV_Realty_Core.Framework.Integrations
{
    internal class CGCM_Integration
    {
        private static IManifest _manifest;
        private static ILoggerService _logger;
        private static IConfigService _configService;
        private static IModHelperService _modHelperService;
        private static void ResetGuiVars()
        {
            //
            //  reset the exposed config items to their
            //  default values.
            //
            _configService.config.GlobalForageMap = "";
            _configService.config.GlobalForageTileX = 0;
            _configService.config.GlobalForageTileY = 0;
            _configService.config.UseGracePeriod = false;
            _configService.config.GraceForCrops = true;
            _configService.config.GraceForFruit = false;
            _configService.config.SpringGracePeriod = 3;
            _configService.config.SummerGracePeriod = 3;
            _configService.config.FallGracePeriod = 3;
            _configService.config.WinterGracePeriod = 3;
            _configService.config.DoGainExperience = false;
            _configService.config.DoHarvestCrops = false;
            _configService.config.DoHarvestFruitTrees = false;
            _configService.config.EnableDeluxeAutoGrabberOnExpansions = false;
            _configService.config.EnableDeluxeAutoGrabberOnExpansions = true;
            _configService.config.DoGlobalForage = false;
            _configService.config.DoHarvestTruffles = false;
            _configService.config.GrabberRange = 30;
            _configService.config.AlwaysShowMagicMenu = false;
            _configService.config.WarpRoomKey = new KeybindList(new Keybind(SButton.Z, SButton.LeftControl), new Keybind(SButton.Z, SButton.RightControl));
            _configService.config.MaxNumberJunimos = 3;
            _configService.config.JunimoMaxRadius = 8;
            _configService.config.JunimoReseedCrop = true;
            _configService.config.JunimosChargeForSeeds = true;
            _configService.config.JunimosFeeForSeeding = 1;
            _configService.config.JunimosWorkInWinter = false;
            _configService.config.JunimoWinterFee = 2;
            _configService.config.RealTimeMoney = true;
            _configService.config.GlobalMaxJunimos = 40;
            _configService.config.useSouthWestEntrance = true;
            _configService.config.useNorthWestEntrance = true;
            _configService.config.enableDebug = false;
            _configService.config.AddBridgeSeat = true;
            _configService.config.AddFishPondLight = true;
            _configService.config.UseCustomWeather = true;
            _configService.config.LightLevel = 1;
        }
        public CGCM_Integration(IModHelperService helperService, IManifest omanifest, IConfigService configService, ILoggerService olog)
        {
            _modHelperService = helperService;
            _manifest = omanifest;
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
            // no OOB method to control buildings by location
            //  maybe a GSQ coud do it
            //
            //configMenu.AddBoolOption(
            //     mod: manifest,
            //     name: () => I18n.GMCM_Realty_CustomOnHome(),
            //     tooltip: () => I18n.GMCM_Realty_CustomOnHome_TT(),
            //     getValue: () => config.BuildCustomOnHomeFarm,
            //     setValue: value => config.BuildCustomOnHomeFarm = value
            // );

            configMenu.AddBoolOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_Realty_SkipRequirements(),
                  tooltip: () => I18n.GMCM_Realty_SkipRequirements_TT(),
                  getValue: () => _configService.config.SkipRequirements,
                  setValue: value => _configService.config.SkipRequirements = value
              );

            configMenu.AddBoolOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_SkipBuildConditions(),
                  tooltip: () => I18n.GMCM_SkipBuildConditions_TT(),
                  getValue: () => _configService.config.SkipBuildingConditions,
                  setValue: value => _configService.config.SkipBuildingConditions = value
              );


        

            configMenu.AddBoolOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_UseNWExit(),
                  tooltip: () => I18n.GMCM_UseNWExit_TT(),
                  getValue: () => _configService.config.useNorthWestEntrance,
                  setValue: value => _configService.config.useNorthWestEntrance = value
              );

            configMenu.AddBoolOption(
               mod: _manifest,
               name: () => I18n.GMCM_UseSWExit(),
               tooltip: () => I18n.GMCM_UseSWExit_TT(),
               getValue: () => _configService.config.useSouthWestEntrance,
               setValue: value => _configService.config.useSouthWestEntrance = value
           );

            configMenu.AddKeybindList(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_Keybind(),
                tooltip: () => I18n.GMCM_AutoGrab_Keybind_TT(),
                 getValue: () => _configService.config.WarpRoomKey,
                 setValue: value => _configService.config.WarpRoomKey = value

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
            configMenu.AddNumberOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_MaxJos(),
                 tooltip: () => I18n.GMCM_JO_MaxJos_TT(),
                 getValue: () => _configService.config.MaxNumberJunimos,
                 setValue: value => _configService.config.MaxNumberJunimos = value
             );
            configMenu.AddNumberOption(
                mod: _manifest,
                name: () => I18n.GMCM_JO_GlobalMaxJos(),
                tooltip: () => I18n.GMCM_JO_GlobalMaxJos_TT(),
                getValue: () => _configService.config.GlobalMaxJunimos,
                setValue: value => _configService.config.GlobalMaxJunimos = value
            );
            configMenu.AddNumberOption(
                mod: _manifest,
                name: () => I18n.GMCM_JO_MaxRad(),
                tooltip: () => I18n.GMCM_JO_MaxRad_TT(),
                getValue: () => _configService.config.JunimoMaxRadius,
                setValue: value => _configService.config.JunimoMaxRadius = value
            );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_RealTime(),
                 tooltip: () => I18n.GMCM_JO_RealTime_TT(),
                 getValue: () => _configService.config.RealTimeMoney,
                 setValue: value => _configService.config.RealTimeMoney = value
             );

            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Reseed(),
                 tooltip: () => I18n.GMCM_JO_Reseed_TT(),
                 getValue: () => _configService.config.JunimoReseedCrop,
                 setValue: value => _configService.config.JunimoReseedCrop = value
             );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Seeds_Charge(),
                 tooltip: () => I18n.GMCM_JO_Seeds_Charge_TT(),
                 getValue: () => _configService.config.JunimosChargeForSeeds,
                 setValue: value => _configService.config.JunimosChargeForSeeds = value
             );
            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_JO_Charge_Seeding(),
                  tooltip: () => I18n.GMCM_JO_Charge_Seeding_TT(),
                  getValue: () => _configService.config.JunimosFeeForSeeding,
                  setValue: value => _configService.config.JunimosFeeForSeeding = value
              );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Work_Rain(),
                 tooltip: () => I18n.GMCM_JO_Work_Rain_TT(),
                 getValue: () => _configService.config.JunimosWorkInRain,
                 setValue: value => _configService.config.JunimosWorkInRain = value
             );
            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_JO_Rain_Fee(),
                  tooltip: () => I18n.GMCM_JO_Rain_Fee_TT(),
                  getValue: () => _configService.config.JunimoRainFee,
                  setValue: value => _configService.config.JunimoRainFee = value
              );
            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_JO_Work_Winter(),
                 tooltip: () => I18n.GMCM_JO_Work_Winter_TT(),
                 getValue: () => _configService.config.JunimosWorkInWinter,
                 setValue: value => _configService.config.JunimosWorkInWinter = value
             );
            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_JO_Work_Winter_Fee(),
                  tooltip: () => I18n.GMCM_JO_Work_Winter_Fee_TT(),
                  getValue: () => _configService.config.JunimoWinterFee,
                  setValue: value => _configService.config.JunimoWinterFee = value
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
                getValue: () => _configService.config.EnableDeluxeAutoGrabberOnExpansions,
                setValue: value => _configService.config.EnableDeluxeAutoGrabberOnExpansions = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GCMC_AutoGrab_EnableAG_HomeFarm(),
                tooltip: () => I18n.GMCM_AutoGrab_EnableAG_HomeFarm_TT(),
                getValue: () => _configService.config.EnableDeluxeAutoGrabberOnHomeFarm,
                setValue: value => _configService.config.EnableDeluxeAutoGrabberOnHomeFarm = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestCrops(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestCrops_TT(),
                getValue: () => _configService.config.DoHarvestCrops,
                setValue: value => _configService.config.DoHarvestCrops = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestFruitTrees(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFruitTrees_TT(),
                getValue: () => _configService.config.DoHarvestFruitTrees,
                setValue: value => _configService.config.DoHarvestFruitTrees = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestFlowers(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFlowers_TT(),
                getValue: () => _configService.config.DoHarvestFlowers,
                setValue: value => _configService.config.DoHarvestFlowers = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoGainExperience(),
                tooltip: () => I18n.GMCM_AutoGrab_DoGainExperience_TT(),
                getValue: () => _configService.config.DoGainExperience,
                setValue: value => _configService.config.DoGainExperience = value
            );

            configMenu.AddNumberOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_AutoGrab_GrabberRange(),
                 tooltip: () => I18n.GMCM_AutoGrab_GrabberRange_TT(),
                 getValue: () => _configService.config.GrabberRange,
                 setValue: value => _configService.config.GrabberRange = value
             );
            #endregion
            //
            //  Global Autograbber Options
            //
            #region "Global Autograbber"
            configMenu.AddSectionTitle(
                mod: _manifest,
                text: () => I18n.GMCM_AutoGrab_Title(),
                tooltip: () => ""
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoGlobalForage(),
                tooltip: () => I18n.GMCM_AutoGrab_DoGlobalForage_TT(),
                getValue: () => _configService.config.DoGlobalForage,
                setValue: value => _configService.config.DoGlobalForage = value
            );

            configMenu.AddTextOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_GlobalForageMap(),
                tooltip: () => I18n.GMCM_AutoGrab_GlobalForageMap_TT(),
                getValue: () => _configService.config.GlobalForageMap ?? "",
                setValue: value => _configService.config.GlobalForageMap = value
            );


            configMenu.AddNumberOption(
                  mod: _manifest,
                  name: () => I18n.GMCM_AutoGrab_GlobalForageX(),
                  tooltip: () => I18n.GMCM_AutoGrab_GlobalForageX_TT(),
                  getValue: () => _configService.config.GlobalForageTileX,
                  setValue: value => _configService.config.GlobalForageTileX = value
              );

            configMenu.AddNumberOption(
                   mod: _manifest,
                   name: () => I18n.GMCM_AutoGrab_GlobalForageY(),
                   tooltip: () => I18n.GMCM_AutoGrab_GlobalForageY_TT(),
                   getValue: () => _configService.config.GlobalForageTileY,
                   setValue: value => _configService.config.GlobalForageTileY = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestTruffles(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestTruffles_TT(),
                getValue: () => _configService.config.DoHarvestTruffles,
                setValue: value => _configService.config.DoHarvestTruffles = value
            );

            configMenu.AddBoolOption(
                 mod: _manifest,
                 name: () => I18n.GMCM_AutoGrab_DoHarvestFarmCave(),
                 tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFarmCave_TT(),
                 getValue: () => _configService.config.DoHarvestFarmCave,
                 setValue: value => _configService.config.DoHarvestFarmCave = value
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
                getValue: () => _configService.config.UseGracePeriod,
                setValue: value => _configService.config.UseGracePeriod = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_GP_ForCrops(),
                tooltip: () => I18n.GMCM_GP_ForCrops_TT(),
                getValue: () => _configService.config.GraceForCrops,
                setValue: value => _configService.config.GraceForCrops = value
            );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_GP_ForFruits(),
                tooltip: () => I18n.GMCM_GP_ForFruits_TT(),
                getValue: () => _configService.config.GraceForFruit,
                setValue: value => _configService.config.GraceForFruit = value
            );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Spring(),
                    tooltip: () => "",
                    getValue: () => _configService.config.SpringGracePeriod,
                    setValue: value => _configService.config.SpringGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Summer(),
                    tooltip: () => "",
                    getValue: () => _configService.config.SummerGracePeriod,
                    setValue: value => _configService.config.SummerGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Fall(),
                    tooltip: () => "",
                    getValue: () => _configService.config.FallGracePeriod,
                    setValue: value => _configService.config.FallGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_GP_Winter(),
                    tooltip: () => "",
                    getValue: () => _configService.config.WinterGracePeriod,
                    setValue: value => _configService.config.WinterGracePeriod = value
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
                   getValue: () => _configService.config.AddBridgeSeat,
                   setValue: value => _configService.config.AddBridgeSeat = value
               );

            configMenu.AddBoolOption(
                mod: _manifest,
                name: () => I18n.GMCM_FishPondLight(),
                tooltip: () => I18n.GMCM_FishPondLight_TT(),
                getValue: () => _configService.config.AddFishPondLight,
                setValue: value => _configService.config.AddFishPondLight = value
            );
            configMenu.AddBoolOption(
              mod: _manifest,
              name: () => I18n.GMCM_BuildingLights(),
              tooltip: () => I18n.GMCM_BuildingLights_TT(),
              getValue: () => _configService.config.EnableBuildingLights,
              setValue: value => _configService.config.EnableBuildingLights = value
          );
            configMenu.AddNumberOption(
                mod: _manifest,
                name: () => I18n.GMCM_LightLevel(),
                tooltip: () => I18n.GMCM_LightLevel_TT(),
                getValue: () => (int)(_configService.config.LightLevel * 100),
                setValue: value => _configService.config.LightLevel = value / 100f
            );

            configMenu.AddBoolOption(
                    mod: _manifest,
                    name: () => I18n.GMCM_UseCustomWeather(),
                    tooltip: () => I18n.GMCM_UseCustomWeather_TT(),
                    getValue: () => _configService.config.UseCustomWeather,
                    setValue: value => _configService.config.UseCustomWeather = value
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
                getValue: () => _configService.config.enableDebug,
                setValue: value =>
                {
                    _configService.config.enableDebug = value;
                    _logger.Debug = value;
                }
            );
            #endregion
        }
    }
}
