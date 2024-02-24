using System;
using System.Collections.Generic;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace ContentPackFramework.MapUtililities
{
    internal delegate Tuple<bool, string> MapTokenHandlerDelegate(MapToken token, ExpansionPack cPac, List<MapToken> tokens);
}
