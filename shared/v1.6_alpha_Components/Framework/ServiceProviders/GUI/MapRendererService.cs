using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using xTile;
using SkiaSharp;
using StardewModHelpers;
using SDV_MapRenderer;
using Prism99_Core.Utilities;
using StardewValley.GameData.Locations;

namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class MapRendererService : IMapRendererService
    {
        private IGameEnvironmentService _gameEnvironmentService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEnvironmentService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }
       
        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _gameEnvironmentService= (IGameEnvironmentService)args[0];
        }

        internal override StardewBitmap RenderMap(string tmxPath,  SDVMapRenderer.MapOptions mapOptions)
        {
            return SDVMapRenderer.RenderMap(tmxPath,"", (SDVLogger)logger.CustomLogger, mapOptions);
        }
        internal override StardewBitmap RenderMap(GameLocation location,  List<string> layers = null)
        {
            SDVMapRenderer.MapOptions mapOptions = new SDVMapRenderer.MapOptions();
            return SDVMapRenderer.RenderMap(location, location.GetData(), (SDVLogger)logger.CustomLogger, mapOptions, layers);
        }
        internal override StardewBitmap RenderMap(GameLocation location, SDVMapRenderer.MapOptions mapOptions, List<string> layers = null)
        {
            return SDVMapRenderer.RenderMap(location, location.GetData(), (SDVLogger)logger.CustomLogger, mapOptions, layers);
        }
        internal override StardewBitmap RenderMap(Map map,LocationData locationData, SDVMapRenderer.MapOptions mapOptions, List<string> layers = null)
        {
            return SDVMapRenderer.RenderMap(map, locationData,null,(SDVLogger)logger.CustomLogger, mapOptions, layers);
        }

        internal override StardewBitmap RenderLocationMap(string locationName, SDVMapRenderer.MapOptions mapOptions)
        {
            GameLocation gl = Game1.getLocationFromName(locationName);

            if (gl != null)
            {
                return RenderMap(gl, mapOptions);
            }

            return null;
        }

        internal override StardewBitmap RenderIndoorLocation(string buildingName)
        {
            if (Game1.buildingData.TryGetValue(buildingName, out var building))
            {
                if (!string.IsNullOrEmpty(building.IndoorMap))
                {
                    SDVMapRenderer.MapOptions mapOptions = new SDVMapRenderer.MapOptions();
                    return RenderMap(Game1.content.Load<Map>(building.IndoorMap), null, mapOptions);
                }
            }

            return null;
        }

        internal override StardewBitmap RenderLocationMap(string locationName, SDVMapRenderer.MapOptions mapOptions, List<string> layers)
        {
            GameLocation gl = Game1.getLocationFromName(locationName);

            if (gl != null)
            {
                return RenderMap(gl.map,gl.GetData(), mapOptions, layers);
            }

            return null;
        }

        internal override StardewBitmap RenderMap(string assetPat, List<string> layers = null)
        {
            throw new NotImplementedException();
        }
    }
}
