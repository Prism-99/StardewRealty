using System.Collections.Generic;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    class MapEdit
    {
        public string TargetMap { get; set; }
        public string SignLocationName { get; set; }
        public List<MapWarp> Warps { get; set; }
        public string PatchMapName { get; set; }
        public Vector2 PatchPoint { get; set; }
        public Point SignPostLocation { get; set; }
        public Vector2 SignVector()
        {
            if (SignPostLocation == Point.Zero)
            {
                return new Vector2(-1, -1);
            }
            else
            {
                return new Vector2(SignPostLocation.X, SignPostLocation.Y);
            }
        }
    }
}
