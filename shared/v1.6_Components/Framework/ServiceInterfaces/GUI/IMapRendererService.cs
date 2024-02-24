using System;
using System.Collections.Generic;
using xTile;
using StardewModHelpers;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GUI
{
    internal abstract class IMapRendererService:IService
{
        public override Type ServiceType => typeof(IMapRendererService);
        internal abstract StardewBitmap RenderMap(Map map, List<string> layers = null);
        internal abstract StardewBitmap RenderMap(string assetPat,List<string> layers = null);
        internal abstract StardewBitmap RenderLocationMap(string locationName);
        internal abstract StardewBitmap RenderLocationMap(string locationName,List<string> layers);
        internal abstract StardewBitmap RenderIndoorLocation(string buildingName);
    }
}
