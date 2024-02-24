using System.Collections.Generic;

namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    internal class PatchingDetails
    {
        public List<MapEdit> MapEdits { get; set; }
         public Dictionary<EntranceDirection, EntrancePatch> EntrancePatches { get; set; }
    }
}
