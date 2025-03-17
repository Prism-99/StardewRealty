
using StardewModdingAPI.Utilities;


namespace SDV_Realty_Core.Framework.Objects
{
    /// <summary>The JSON model for global settings read from the mod's config file.</summary>
    public class FEConfig
    {
        /// <summary>
        /// Enable lightning strikes in Expansions
        /// </summary>
        public bool EnableExpansionLightning { get; set; } = true;
        public KeybindList MiniMapKey { get; set; } = new KeybindList(new Keybind(SButton.F3, SButton.LeftControl), new Keybind(SButton.F3, SButton.RightControl));
        public bool UseWarpRing { get; set; } = true;
        public bool UseSignPosts { get; set; } = true;
        public bool UseMapWarps { get; set; } = true;
        public bool UseAdditionalFarms { get; set; } = true;
        /// <summary>Determines whether crows can spawn on the farm expansion.</summary>
        public bool enableCrows { get; set; } = true;
        public int MaxNumberOfCrows { get; set; } = 4;
        public bool enableNightMonsters { get; set; } = true;
        public bool enableDebug { get; set; } = false;
        /// <summary>Determines whether to patch an alternate entrance to the expansion in the backwoods.</summary>
        public bool useBackwoodsEntrance { get; set; } = true;
        public bool AlwaysShowMagicMenu { get; set; } = true;
        public bool UseTownForSaleSign { get; set; } = true;
        public bool AddMushroomBoxRecipe { get; set; } = true;
        // skip expansion purchase requirements
        public bool SkipRequirements { get; set; } = false;
        // use a global condition for all expansions
        public bool useGlobalCondition { get; set; } = false;
        // global condition for all expansions
        public string globalCondition { get; set; }
        // use a global price for all expansions
        public bool useGlobalPrice { get; set; } = false;
        // global price for all expansions
        public int globalPrice { get; set; }
        public bool AddBridgeSeat { get; set; } = true;
        public bool UseLore { get; set; } = true;
        public bool SkipBuildingConditions { get; set; } = false;
        public bool UseCustomWeather { get; set; } = false;
        public bool ApplyWeatherFixes { get; set; } = true;
        public bool BuildCustomOnHomeFarm { get; set; } = true;
        public KeybindList WarpRoomKey { get; set; } = new KeybindList(new Keybind(SButton.Z, SButton.LeftControl), new Keybind(SButton.Z, SButton.RightControl));
        public bool UseParallel = false;
        public bool useNorthWestEntrance { get; set; } = true;
        public bool useSouthWestEntrance { get; set; } = true;
        //
        //  game fixes
        //
        public bool FixHoppers { get; set; } = true;
        //
        //  stardew meadows trian
        //
        public bool TrainEnabled  { get; set; } = true;
        public int TrainArrivalTime { get; set; } = 1200;
        //
        //  lighting parameters
        //
        public bool AddFishPondLight { get; set; } = true;
        public float LightLevel { get; set; } = 1f;
        public bool EnableBuildingLights { get; set; } = true;
        //
        //  grace period
        //
        public bool UseGracePeriod { get; set; } = true;
        public bool GraceForCrops { get; set; } = true;
        public bool GraceForFruit { get; set; } = false;
        public int SpringGracePeriod { get; set; } = 3;
        public int SummerGracePeriod { get; set; } = 3;
        public int FallGracePeriod { get; set; } = 3;
        public int WinterGracePeriod { get; set; } = 3;
        //
        //  autograbber parameters
        //
        public bool DoHarvestCrops { get; set; } = true;
        public bool DoHarvestFlowers { get; set; } = true;
        public int GrabberRange { get; set; } = 30;
        public bool DoGainExperience { get; set; } = true;
        public bool DoHarvestFruitTrees { get; set; } = true;
        public bool DoGlobalForage { get; set; }
        public string GlobalForageMap { get; set; } = "";
        public int GlobalForageTileX { get; set; }
        public int GlobalForageTileY { get; set; }
        public bool DoHarvestFarmCave { get; set; }
        public bool DoHarvestTruffles { get; set; }
        public bool EnableDeluxeAutoGrabberOnExpansions { get; set; } = true;
        public bool EnableDeluxeAutoGrabberOnHomeFarm { get; set; } = false;
        //
        //  custom junimo parameters
        //
        public bool EnablePremiumJunimos { get; set; } = true;
        public bool RealTimeMoney { get; set; } = true;
        public int MaxNumberJunimos { get; set; } = 3;
        public int JunimoMaxRadius { get; set; } = 8;
        public bool JunimosWorkInRain { get; set; } = true;
        public int JunimoRainFee { get; set; } = 1;
        public bool JunimosWorkInWinter { get; set; } = true;
        public int JunimoWinterFee { get; set; } = 2;
        public bool JunimoReseedCrop { get; set; } = true;
        public bool JunimosChargeForSeeds { get; set; } = true;
        public int JunimosFeeForSeeding { get; set; } = 1;
        public int GlobalMaxJunimos { get; set; } = 40;
        public float JunimoSeedDiscount { get; set; } = .25F;
    }
}
