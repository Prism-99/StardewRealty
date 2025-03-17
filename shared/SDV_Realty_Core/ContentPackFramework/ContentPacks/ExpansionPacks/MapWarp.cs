

namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
   public class MapWarp
    {
        public MapWarp() { }
        public MapWarp(int X,int Y,string  TargetName,int TargetX, int TargetY,bool flip)
        {
            ToMap=TargetName;
            FromX=X;
            FromY=Y;
            ToX=TargetX;
            ToY=TargetY;
            FlipFarmer = flip;
        }
        public string FromMap { get; set; }
        public string ToMap { get; set; }
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public bool  FlipFarmer { get; set; }
    }
}
