using StardewValley.GameData.Crops;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SDV_Realty_Core.Framework.Utilities
{
    static class CropDataExt
    {
        public static string GetCleanHarvestId(this CropData data)
        {
            return data.HarvestItemId.Replace("(O)", "");
        }
    }
}
