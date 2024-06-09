using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IGridManager : IService
    {
        internal const string northSidePatch = "0";
        internal const string eastSidePatch = "1";
        internal const string southSidePatch = "2";
        internal const string westSidePatch = "3";
        //internal List<FarmDetails> GameFarms = null;
        internal const int MaxFarms = 10;
        public Dictionary<int, string> MapGrid ;
        protected IModDataService modDataService;

        internal abstract void FixGrid();
        internal abstract string SwapGridLocations(int GridIdA, int GridIdB);
        internal abstract void PatchInMap(int iGridId);
        internal abstract int AddMapToGrid(string sExpName,int gridId);
        internal abstract string GetNeighbourExpansionName(int iGridId, EntranceDirection side);
        internal abstract Rectangle GetExpansionWorldMapLocation(int iGridId);
        internal abstract Rectangle GetExpansionWorldMapLocation(string sExpansionName);
        internal abstract Rectangle GetExpansionMainMapLocation(int iGridId, int left, int top);

    }
}
