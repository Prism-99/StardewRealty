using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using StardewValley.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    internal abstract class ILocationDataProvider : IService
    {
        public override Type ServiceType => typeof(ILocationDataProvider);
        public abstract LocationData GetData(string locationName);
    }
}
