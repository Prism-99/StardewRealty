using System.Collections.Generic;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    public class FarmDetails
    {
        public int WorldMapPatchPointX { get; set; }
        public int WorldMapPatchPointY { get; set; }
        public int FarmType { get; set; }
        public string MapName { get; set; }
        public Dictionary<string, EntrancePatch> PathPatches { get; set; }
    }
}
