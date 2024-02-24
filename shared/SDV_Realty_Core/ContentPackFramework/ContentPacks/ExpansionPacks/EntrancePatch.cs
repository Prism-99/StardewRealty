using System.Collections.Generic;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    public enum EntranceDirection
    {
        North,
        East,
        South,
        West
    }
    public enum WarpOrientations
    {
        Vertical,
        Horizontal
    }
    public class EntrancePatch
    {
        public EntranceDirection PatchSide { get; set; }
        public EntranceWarp WarpIn { get; set; }
        public EntranceWarp WarpOut { get; set; }
        public WarpOrientations WarpOrientation { get; set; }
        public int PathBlockX { get; set; }
        public int PathBlockY { get; set; }
        public string PatchMapName { get; set; }
        public int MapPatchPointX { get; set; }
        public int MapPatchPointY { get; set; }
        public SignDetails Sign { get; set; } = null;
        public List<MapWarp> GetWarpInList(string endlocation)
        {
            List<MapWarp> list = new List<MapWarp>();

            for (int warps = 0; warps < WarpIn.NumberofPoints; warps++)
            {
                if (WarpOrientation == WarpOrientations.Horizontal)
                {
                    list.Add(new MapWarp { ToMap = endlocation, ToX = WarpIn.X + warps, ToY = WarpIn.Y });
                }
                else
                {
                    list.Add(new MapWarp { ToMap = endlocation, ToX = WarpIn.X, ToY = WarpIn.Y + warps });
                }
            }

            return list;
        }
        public List<MapWarp> GetWarpOutList(string destlocation)
        {
            List<MapWarp> list = new List<MapWarp>();

            for (int warps = 0; warps < WarpOut.NumberofPoints; warps++)
            {
                if (WarpOrientation == WarpOrientations.Horizontal)
                {
                    list.Add(new MapWarp { ToMap = destlocation, FromX = WarpOut.X + warps, FromY = WarpOut.Y });
                }
                else
                {
                    list.Add(new MapWarp { ToMap = destlocation, FromX = WarpOut.X, FromY = WarpOut.Y + warps });
                }
            }

            return list;
        }
    }
}
