using SDV_xTile;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using xTile;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IMapLoaderService:IService
    {
        public MapLoader Loader;
        internal abstract Map LoadMap( string sMapRelativePath, string sLocationName, bool bLoadTileSheets, bool removeExt, bool fixTileSheets = true);
        internal abstract Map LoadMap( string sMapRelativePath, string sLocationName, bool bLoadTileSheets);

    }
}
