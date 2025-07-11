using System;
using System.Collections.Generic;
using xTile;
using StardewModHelpers;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using StardewValley.GameData.Locations;
using SDV_MapRenderer;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GUI
{
    internal abstract class IMapRendererService:IService
{
        public override Type ServiceType => typeof(IMapRendererService);
        internal abstract StardewBitmap RenderMap(Map map,LocationData locationData,SDVMapRenderer.MapOptions mapOptions, List<string> layers = null);
        internal abstract StardewBitmap RenderMap(GameLocation location, SDVMapRenderer.MapOptions mapOptions, List<string> layers = null);
        internal abstract StardewBitmap RenderMap(GameLocation location,  List<string> layers = null);
        internal abstract StardewBitmap RenderMap(string assetPat,List<string> layers = null);
        internal abstract StardewBitmap RenderMap(string tmxPath, SDVMapRenderer.MapOptions mapOptions);
        internal abstract StardewBitmap RenderLocationMap(string locationName, SDVMapRenderer.MapOptions mapOptions);
        internal abstract StardewBitmap RenderLocationMap(string locationName, SDVMapRenderer.MapOptions mapOptions, List<string> layers);
        internal abstract StardewBitmap RenderIndoorLocation(string buildingName);
    }
}
