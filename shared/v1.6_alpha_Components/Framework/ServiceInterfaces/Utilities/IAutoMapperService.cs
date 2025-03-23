using System;
using ContentPackFramework.MapUtililities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using xTile;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IAutoMapperService:IService
{
        public override Type ServiceType => typeof(IAutoMapperService);
        internal abstract void AddMapToken(string token, MapTokenHandlerDelegate fnc);
        internal abstract ExpansionPack ParseMap(Map sourceMap);

    }
}
