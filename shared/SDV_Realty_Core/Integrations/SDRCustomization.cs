using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace StardewRealty.SDV_Realty_Interface
{
    internal partial class SDRCustomization
    {
 

        public SDRCustomization(ExpansionPack oPack)
        {
            AllowGiantCrops = oPack.AllowGiantCrops;
            AlwaysRaining = oPack.AlwaysRaining;
            AlwaysSnowing = oPack.AlwaysSnowing;
            AlwaysSunny = oPack.AlwaysSunny;
            Season = oPack.SeasonOverride;
        }
        public SDRCustomization(CustomizationDetails details)
        {
            Artifacts = details.ArtifactList;
            Season = details.SeasonOverride;
            SpringFish = details.GetFishData("spring");
            SummerFish = details.GetFishData("summer");
            FallFish = details.GetFishData("fall");
            WinterFish = details.GetFishData("winter");
            SpringForage = details.GetForageData("spring");
            SummerForage = details.GetForageData("summer");
            FallForage = details.GetForageData("fall");
            WinterForage = details.GetForageData("winter");
            AllowGiantCrops = details.AllowGiantCrops;
            AlwaysRaining = details.AlwaysRaining;
            AlwaysSnowing = details.AlwaysSnowing;
            AlwaysSunny = details.AlwaysSunny;
        }

 
    }
}
