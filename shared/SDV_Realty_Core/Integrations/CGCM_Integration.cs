using GenericModConfigMenu;
using SDV_Realty_Core.Framework.Objects;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;


namespace SDV_Realty_Core.Framework.Integrations
{
    internal  class CGCM_Integration
    {
        private static IModHelperService helper;
        private static IManifest manifest;
        private static FEConfig config;
        private static ILoggerService logger;

        private static void ResetGuiVars()
        {
            //
            //  reset the exposed config items to their
            //  default values.
            //
            config.GlobalForageMap = "";
            config.GlobalForageTileX = 0;
            config.GlobalForageTileY = 0;
            config.UseGracePeriod = false;
            config.GraceForCrops = true;
            config.GraceForFruit = false;
            config.SpringGracePeriod = 3;
            config.SummerGracePeriod = 3;
            config.FallGracePeriod = 3;
            config.WinterGracePeriod = 3;
            config.DoGainExperience = false;
            config.DoHarvestCrops = false;
            config.DoHarvestFruitTrees = false;
            config.EnableDeluxeAutoGrabberOnExpansions = false;
            config.EnableDeluxeAutoGrabberOnExpansions = true;
            config.DoGlobalForage = false;
            config.DoHarvestTruffles = false;
            config.GrabberRange = 30;
            config.AlwaysShowMagicMenu = false;
            config.WarpRoomKey = new KeybindList(new Keybind(SButton.Z, SButton.LeftControl), new Keybind(SButton.Z, SButton.RightControl));
            config.MaxNumberJunimos = 3;
            config.JunimoMaxRadius = 8;
            config.JunimoReseedCrop = true;
            config.JunimosChargeForSeeds = true;
            config.JunimosFeeForSeeding = 1;
            config.JunimosWorkInWinter = false;
            config.JunimoWinterFee = 2;
            config.RealTimeMoney = true;
            config.GlobalMaxJunimos = 40;
            config.useSouthWestEntrance = true;
            config.useNorthWestEntrance = true;
            config.enableDebug = false;
            config.AddBridgeSeat = true;
            config.AddFishPondLight = true;
            config.UseCustomWeather = true;
            config.LightLevel = 1;
        }
        public CGCM_Integration(IModHelperService modHelperService, IManifest omanifest, FEConfig oconfig, ILoggerService olog)
        {
            helper = modHelperService;
            manifest = omanifest;
            config = oconfig;
            logger = olog;
        }
        internal  void RegisterMenu()
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            logger.Log($"Registering with GMCM", LogLevel.Debug);

            // register mod
            configMenu.Register(
                mod: manifest,
                reset: ResetGuiVars,
                save: () => helper.WriteConfig(config),
                 titleScreenOnly: false
            );
            //
            //  create config GUI
            //
            //  Realty Options
            //
            configMenu.AddSectionTitle(
                mod: manifest,
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
                  mod: manifest,
                  name: () => I18n.GMCM_Realty_SkipRequirements(),
                  tooltip: () => I18n.GMCM_Realty_SkipRequirements_TT(),
                  getValue: () => config.SkipRequirements,
                  setValue: value => config.SkipRequirements = value
              );

            configMenu.AddBoolOption(
                  mod: manifest,
                  name: () => I18n.GMCM_SkipBuildConditions(),
                  tooltip: () => I18n.GMCM_SkipBuildConditions_TT(),
                  getValue: () => config.SkipBuildingConditions,
                  setValue: value => config.SkipBuildingConditions = value
              );

            configMenu.AddBoolOption(
                   mod: manifest,
                   name: () => I18n.GMCM_AddBridge(),
                   tooltip: () => I18n.GMCM_AddBridge_TT(),
                   getValue: () => config.AddBridgeSeat,
                   setValue: value =>  config.AddBridgeSeat = value 
               );

            configMenu.AddBoolOption(
                    mod: manifest,
                    name: () => I18n.GMCM_FishPondLight(),
                    tooltip: () => I18n.GMCM_FishPondLight_TT(),
                    getValue: () => config.AddFishPondLight,
                    setValue: value => config.AddFishPondLight = value
                );

            configMenu.AddBoolOption(
                    mod: manifest,
                    name: () => I18n.GMCM_UseCustomWeather(),
                    tooltip: () => I18n.GMCM_UseCustomWeather_TT(),
                    getValue: () => config.UseCustomWeather,
                    setValue: value => config.UseCustomWeather = value
                );

            configMenu.AddBoolOption(
                  mod: manifest,
                  name: () => I18n.GMCM_UseNWExit(),
                  tooltip: () => I18n.GMCM_UseNWExit_TT(),
                  getValue: () => config.useNorthWestEntrance,
                  setValue: value => config.useNorthWestEntrance = value
              );

            configMenu.AddBoolOption(
               mod: manifest,
               name: () => I18n.GMCM_UseSWExit(),
               tooltip: () => I18n.GMCM_UseSWExit_TT(),
               getValue: () => config.useSouthWestEntrance,
               setValue: value => config.useSouthWestEntrance = value
           );

            configMenu.AddKeybindList(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_Keybind(),
                tooltip: () => I18n.GMCM_AutoGrab_Keybind_TT(),
                 getValue: () => config.WarpRoomKey,
                 setValue: value => config.WarpRoomKey = value

             );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => I18n.GMCM_LightLevel(),
                tooltip: () => I18n.GMCM_LightLevel_TT(),
                getValue: () => (int)(config.LightLevel * 100),
                setValue: value => config.LightLevel = value / 100f
            ); ;
            //
            //  Custom Junimo Options
            //
            configMenu.AddSectionTitle(
                 mod: manifest,
                 text: () => I18n.GMCM_JO_Title(),
                 tooltip: () => ""
             );
            configMenu.AddNumberOption(
                 mod: manifest,
                 name: () => I18n.GMCM_JO_MaxJos(),
                 tooltip: () => I18n.GMCM_JO_MaxJos_TT(),
                 getValue: () => config.MaxNumberJunimos,
                 setValue: value => config.MaxNumberJunimos = value
             );
            configMenu.AddNumberOption(
                mod: manifest,
                name: () => I18n.GMCM_JO_GlobalMaxJos(),
                tooltip: () => I18n.GMCM_JO_GlobalMaxJos_TT(),
                getValue: () => config.GlobalMaxJunimos,
                setValue: value => config.GlobalMaxJunimos = value
            );
            configMenu.AddNumberOption(
                mod: manifest,
                name: () => I18n.GMCM_JO_MaxRad(),
                tooltip: () => I18n.GMCM_JO_MaxRad_TT(),
                getValue: () => config.JunimoMaxRadius,
                setValue: value => config.JunimoMaxRadius = value
            );
            configMenu.AddBoolOption(
                 mod: manifest,
                 name: () => I18n.GMCM_JO_RealTime(),
                 tooltip: () => I18n.GMCM_JO_RealTime_TT(),
                 getValue: () => config.RealTimeMoney,
                 setValue: value => config.RealTimeMoney = value
             );

            configMenu.AddBoolOption(
                 mod: manifest,
                 name: () => I18n.GMCM_JO_Reseed(),
                 tooltip: () => I18n.GMCM_JO_Reseed_TT(),
                 getValue: () => config.JunimoReseedCrop,
                 setValue: value => config.JunimoReseedCrop = value
             );
            configMenu.AddBoolOption(
                 mod: manifest,
                 name: () => I18n.GMCM_JO_Seeds_Charge(),
                 tooltip: () => I18n.GMCM_JO_Seeds_Charge_TT(),
                 getValue: () => config.JunimosChargeForSeeds,
                 setValue: value => config.JunimosChargeForSeeds = value
             );
            configMenu.AddNumberOption(
                  mod: manifest,
                  name: () => I18n.GMCM_JO_Charge_Seeding(),
                  tooltip: () => I18n.GMCM_JO_Charge_Seeding_TT(),
                  getValue: () => config.JunimosFeeForSeeding,
                  setValue: value => config.JunimosFeeForSeeding = value
              );
            configMenu.AddBoolOption(
                 mod: manifest,
                 name: () => I18n.GMCM_JO_Work_Rain(),
                 tooltip: () => I18n.GMCM_JO_Work_Rain_TT(),
                 getValue: () => config.JunimosWorkInRain,
                 setValue: value => config.JunimosWorkInRain = value
             );
            configMenu.AddNumberOption(
                  mod: manifest,
                  name: () => I18n.GMCM_JO_Rain_Fee(),
                  tooltip: () => I18n.GMCM_JO_Rain_Fee_TT(),
                  getValue: () => config.JunimoRainFee,
                  setValue: value => config.JunimoRainFee = value
              );
            configMenu.AddBoolOption(
                 mod: manifest,
                 name: () => I18n.GMCM_JO_Work_Winter(),
                 tooltip: () => I18n.GMCM_JO_Work_Winter_TT(),
                 getValue: () => config.JunimosWorkInWinter,
                 setValue: value => config.JunimosWorkInWinter = value
             );
            configMenu.AddNumberOption(
                  mod: manifest,
                  name: () => I18n.GMCM_JO_Work_Winter_Fee(),
                  tooltip: () => I18n.GMCM_JO_Work_Winter_Fee_TT(),
                  getValue: () => config.JunimoWinterFee,
                  setValue: value => config.JunimoWinterFee = value
              );



            //
            //  Deluxe Autograbber Options
            //
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => I18n.GMCM_AutoGrab_Title(),
                tooltip: () => ""
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_EnableAG(),
                tooltip: () => I18n.GMCM_AutoGrab_EnableAG_TT(),
                getValue: () => config.EnableDeluxeAutoGrabberOnExpansions,
                setValue: value => config.EnableDeluxeAutoGrabberOnExpansions = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GCMC_AutoGrab_EnableAG_HomeFarm(),
                tooltip: () => I18n.GMCM_AutoGrab_EnableAG_HomeFarm_TT(),
                getValue: () => config.EnableDeluxeAutoGrabberOnHomeFarm,
                setValue: value => config.EnableDeluxeAutoGrabberOnHomeFarm = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestCrops(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestCrops_TT(),
                getValue: () => config.DoHarvestCrops,
                setValue: value => config.DoHarvestCrops = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestFruitTrees(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFruitTrees_TT(),
                getValue: () => config.DoHarvestFruitTrees,
                setValue: value => config.DoHarvestFruitTrees = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestFlowers(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFlowers_TT(),
                getValue: () => config.DoHarvestFlowers,
                setValue: value => config.DoHarvestFlowers = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_DoGainExperience(),
                tooltip: () => I18n.GMCM_AutoGrab_DoGainExperience_TT(),
                getValue: () => config.DoGainExperience,
                setValue: value => config.DoGainExperience = value
            );

            configMenu.AddNumberOption(
                 mod: manifest,
                 name: () => I18n.GMCM_AutoGrab_GrabberRange(),
                 tooltip: () => I18n.GMCM_AutoGrab_GrabberRange_TT(),
                 getValue: () => config.GrabberRange,
                 setValue: value => config.GrabberRange = value
             );
            //
            //  Global Autograbber Options
            //
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => I18n.GMCM_AutoGrab_Title(),
                tooltip: () => ""
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_DoGlobalForage(),
                tooltip: () => I18n.GMCM_AutoGrab_DoGlobalForage_TT(),
                getValue: () => config.DoGlobalForage,
                setValue: value => config.DoGlobalForage = value
            );

            configMenu.AddTextOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_GlobalForageMap(),
                tooltip: () => I18n.GMCM_AutoGrab_GlobalForageMap_TT(),
                getValue: () => config.GlobalForageMap ?? "",
                setValue: value => config.GlobalForageMap = value
            );


            configMenu.AddNumberOption(
                  mod: manifest,
                  name: () => I18n.GMCM_AutoGrab_GlobalForageX(),
                  tooltip: () => I18n.GMCM_AutoGrab_GlobalForageX_TT(),
                  getValue: () => config.GlobalForageTileX,
                  setValue: value => config.GlobalForageTileX = value
              );

            configMenu.AddNumberOption(
                   mod: manifest,
                   name: () => I18n.GMCM_AutoGrab_GlobalForageY(),
                   tooltip: () => I18n.GMCM_AutoGrab_GlobalForageY_TT(),
                   getValue: () => config.GlobalForageTileY,
                   setValue: value => config.GlobalForageTileY = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_AutoGrab_DoHarvestTruffles(),
                tooltip: () => I18n.GMCM_AutoGrab_DoHarvestTruffles_TT(),
                getValue: () => config.DoHarvestTruffles,
                setValue: value => config.DoHarvestTruffles = value
            );

            configMenu.AddBoolOption(
                 mod: manifest,
                 name: () => I18n.GMCM_AutoGrab_DoHarvestFarmCave(),
                 tooltip: () => I18n.GMCM_AutoGrab_DoHarvestFarmCave_TT(),
                 getValue: () => config.DoHarvestFarmCave,
                 setValue: value => config.DoHarvestFarmCave = value
             );
            //
            //  Grace Period Options
            //
            configMenu.AddSectionTitle(
               mod: manifest,
               text: () => I18n.GMCM_GP_Title(),
               tooltip: () => ""
           );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_GP_Enabled(),
                tooltip: () => I18n.GMCM_GP_Enabled_TT(),
                getValue: () => config.UseGracePeriod,
                setValue: value => config.UseGracePeriod = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_GP_ForCrops(),
                tooltip: () => I18n.GMCM_GP_ForCrops_TT(),
                getValue: () => config.GraceForCrops,
                setValue: value => config.GraceForCrops = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_GP_ForFruits(),
                tooltip: () => I18n.GMCM_GP_ForFruits_TT(),
                getValue: () => config.GraceForFruit,
                setValue: value => config.GraceForFruit = value
            );

            configMenu.AddNumberOption(
                    mod: manifest,
                    name: () => I18n.GMCM_GP_Spring(),
                    tooltip: () => "",
                    getValue: () => config.SpringGracePeriod,
                    setValue: value => config.SpringGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: manifest,
                    name: () => I18n.GMCM_GP_Summer(),
                    tooltip: () => "",
                    getValue: () => config.SummerGracePeriod,
                    setValue: value => config.SummerGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: manifest,
                    name: () => I18n.GMCM_GP_Fall(),
                    tooltip: () => "",
                    getValue: () => config.FallGracePeriod,
                    setValue: value => config.FallGracePeriod = value
             );

            configMenu.AddNumberOption(
                    mod: manifest,
                    name: () => I18n.GMCM_GP_Winter(),
                    tooltip: () => "",
                    getValue: () => config.WinterGracePeriod,
                    setValue: value => config.WinterGracePeriod = value
             );
            //
            //  Debug Option
            //
            configMenu.AddSectionTitle(
               mod: manifest,
               text: () => I18n.GMCM_Debug_Title(),
               tooltip: () => ""
           );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.GMCM_Debug(),
                tooltip: () => I18n.GMCM_Debug_TT(),
                getValue: () => config.enableDebug,
                setValue: value =>
                {
                    config.enableDebug = value;
                    logger.Debug = value;
                }
            );
            //
            //
            //
            //
            //
            //
            //

            //configMenu.AddTextOption(
            //    mod: manifest,
            //    name: () => "Example string",
            //    getValue: () => this.Config.ExampleString,
            //    setValue: value => this.Config.ExampleString = value
            //);
            //configMenu.AddTextOption(
            //    mod: manifest,
            //    name: () => "Example dropdown",
            //    getValue: () => this.Config.ExampleDropdown,
            //    setValue: value => this.Config.ExampleDropdown = value,
            //    allowedValues: new string[] { "choice A", "choice B", "choice C" }
            //);
        }
    }
}
