using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System.Linq;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IGridManager : IService
    {
        public struct FarmProfile
        {
            //
            //  flags for farm Exits
            //
            public bool UseFarmExit1;
            public bool UseFarmExit2;
            public bool PatchFarmExits;
            public bool PatchBackwoodExits;
            public bool SDEInstalled;
        }
        public struct Neighbours
        {
            public ExpansionPack North;
            public ExpansionPack South;
            public ExpansionPack East;
            public ExpansionPack West;
        }
 
        protected IModDataService modDataService;

        internal abstract void FixGrid();
        internal abstract string? GetNeighbourExpansionTTId(int iGridId, EntranceDirection side);

        internal string? GetNeighbourExpansionName(int iGridId, EntranceDirection side)
        {
            //
            //  returns the neighbouring expasion in the direction supplied
            //
            int nGridId;
            switch (side)
            {
                case EntranceDirection.North:
                    //  top row has no northern neighbour
                    if (iGridId == 0 || (iGridId - 1) % 3 == 0) return null;
                    nGridId = iGridId - 1;
                    if (modDataService.MapGrid.ContainsKey(nGridId))
                        return modDataService.MapGrid[nGridId];
                    break;
                case EntranceDirection.East:
                    nGridId = iGridId == 1 ? 0 : iGridId - 3;
                    if (modDataService.MapGrid.ContainsKey(nGridId))
                        return modDataService.MapGrid[nGridId];
                    break;
                case EntranceDirection.South:
                    //  bottom row has no southern neighbour
                    if (iGridId == 0 || iGridId % 3 == 0) return null;
                    nGridId = iGridId + 1;
                    if (modDataService.MapGrid.ContainsKey(nGridId))
                        return modDataService.MapGrid[nGridId];
                    break;
                case EntranceDirection.West:
                    nGridId = iGridId == 0 ? 1 : iGridId + 3;
                    if (modDataService.MapGrid.ContainsKey(nGridId))
                        return modDataService.MapGrid[nGridId];
                    break;

            }

            return null;
        }
        public abstract void AddMapExitBlockers(string expansionName);
        public abstract void AddMapExitBlockers(int iGridId);
        internal abstract string SwapGridLocations(int GridIdA, int GridIdB);
        internal abstract void PatchInMap(int iGridId);
        internal abstract int AddMapToGrid(string sExpName, int gridId);
        internal abstract Rectangle GetExpansionWorldMapLocation(int iGridId);
        internal abstract Rectangle GetExpansionWorldMapLocation(string sExpansionName);
        internal abstract void AddSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix, string signMessage);
        /// <summary>
        /// Returns gridId of the location
        /// </summary>
        /// <param name="locationName">LocationName to lookup</param>
        /// <returns></returns>
        internal int GetLocationGridId(string locationName)
        {
            var location = modDataService.MapGrid.Where(p => p.Value == locationName);
            if (location.Any())
            {
                return location.First().Key;
            }

            return -1;
        }
        /// <summary>
        /// Remove a location from the grid
        /// </summary>
        /// <param name="gridId">GridId to be removed</param>
        internal void RemoveLocationFromGrid(int gridId)
        {
            if (modDataService.MapGrid.ContainsKey(gridId))
            {
                // add exit blocks
                AddMapExitBlockers(gridId);
                RemoveWarpIns(gridId);
                modDataService.MapGrid.Remove(gridId);
                FixGrid();
                //for (int gridToFix = 0; gridToFix < modDataService.MaximumExpansions; gridToFix++)
                //{
                //    // open exits to adjacent expansions
                //    PatchInMap(gridToFix);
                //}
            }
        }
        /// <summary>
        /// Remove Warps to location at gridId
        /// </summary>
        /// <param name="gridId">GridId to remove references to</param>
        private void RemoveWarpIns(int gridId)
        {
            foreach (var location in Game1.locations.Where(p => p.warps.Where(p => p.TargetName == modDataService.MapGrid[gridId]).Any()))             
            {
                location.warps.RemoveWhere(p=>p.TargetName == modDataService.MapGrid[gridId]);
            }
        }
        internal bool TryGetLocationNeighbours(string locationName, out Neighbours neighbours)
        {
            bool result = false;
            neighbours = new Neighbours();

            List<KeyValuePair<int, string>> gridEntry = modDataService.MapGrid.Where(p => p.Value == locationName).ToList();
            if (gridEntry.Any())
            {
                int gridId = gridEntry.First().Key;
                string? neighbourName = GetNeighbourExpansionName(gridId, EntranceDirection.North);
                if (!string.IsNullOrEmpty(neighbourName))
                {
                    neighbours.North = modDataService.validContents[neighbourName];
                }
                neighbourName = GetNeighbourExpansionName(gridId, EntranceDirection.East);
                if (!string.IsNullOrEmpty(neighbourName))
                {
                    neighbours.East = modDataService.validContents[neighbourName];
                }
                neighbourName = GetNeighbourExpansionName(gridId, EntranceDirection.South);
                if (!string.IsNullOrEmpty(neighbourName))
                {
                    neighbours.South = modDataService.validContents[neighbourName];
                }
                neighbourName = GetNeighbourExpansionName(gridId, EntranceDirection.West);
                if (!string.IsNullOrEmpty(neighbourName))
                {
                    neighbours.West = modDataService.validContents[neighbourName];
                }
                result = true;
            }

            return result;
        }
    }
}
