using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_xTile;
using System;
using xTile;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class MapLoaderService : IMapLoaderService
    {
        public override Type ServiceType => typeof(IMapLoaderService);
        private IGameEnvironmentService _gameEnvironmentService;
        public override Type[] Dependants => new Type[]
        {
          typeof(IModHelperService)
        };

        public override Type[] InitArgs => new Type[] 
        {
            typeof(IModHelperService), typeof(IGameEnvironmentService)
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
            IModHelperService modHelper = (IModHelperService)args[0];
            _gameEnvironmentService = (IGameEnvironmentService)args[1];

            Loader = new MapLoader();
            Loader.Initialize(modHelper, logger);
        }

        internal override Map LoadMap(string sMapRelativePath, string sLocationName, bool bLoadTileSheets, bool removeExt, bool fixTileSheets = true)
        {

            return Loader.LoadMap(_gameEnvironmentService.GamePath, sMapRelativePath.Replace(_gameEnvironmentService.GamePath, ""), sLocationName, bLoadTileSheets, removeExt, fixTileSheets);
        }

        internal override Map LoadMap(string sMapRelativePath, string sLocationName, bool bLoadTileSheets)
        {
            return Loader.LoadMap(_gameEnvironmentService.GamePath, sMapRelativePath.Replace(_gameEnvironmentService.GamePath, ""), sLocationName, bLoadTileSheets);
        }
    }
}
