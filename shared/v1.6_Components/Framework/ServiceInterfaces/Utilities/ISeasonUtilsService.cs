using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using StardewValley.Mods;
using System;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class ISeasonUtilsService:IService
{
        public SeasonalUtils SeasonalUtils;
        public override Type ServiceType => typeof(ISeasonUtilsService);
        public abstract bool isWinter(ModDataDictionary modData);
    }
}
