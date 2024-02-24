using SDV_Realty_Core.Framework.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;
using SDV_Realty_Core.Framework.Objects;
//using SDObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using SDV_xTile;
using StardewValley.Buildings;
using SDV_Realty_Core.Framework.Patches;
using StardewValley.Locations;

namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    public class LargeGreenhouseLocation : GameLocation
    {
        public LargeGreenhouseLocation() : base()
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        public LargeGreenhouseLocation(string path, string id) : base(path, id)
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        //
        //  returning true invokes vanilla code
        //  the code looks for tiles
        public new bool IsGreenhouse { get { return true; } set { } }
#if v16
        //public override bool CanPlantSeedsHere(string itemId, int tile_x, int tile_y)
        //{
        //    return true;// base.CanPlantSeedsHere(itemId, tile_x, tile_y);
        //}
        //public override bool CanPlantTreesHere(string itemId, int tile_x, int tile_y)
        //{
        //    return true;// base.CanPlantTreesHere(itemId, tile_x, tile_y);
        //}
#else
        public override bool CanPlantSeedsHere(int crop_index, int tile_x, int tile_y)
        {
            return true;
        }
        public override bool CanPlantTreesHere(int sapling_index, int tile_x, int tile_y)
        {
            return true;
        }
#endif
        public override void updateWarps()
        {
            base.updateWarps();
            if (modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                //FEFramework.FixWarps(this, modData[FEModDataKeys.FELocationName]);
            }
        }
        //public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
        //{
        //    base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
        //    if (modData.ContainsKey(FEModDataKeys.FELocationName))
        //    {

        //        GameLocation bgl = Game1.getLocationFromName(modData[FEModDataKeys.FELocationName]);

        //        if (bgl!=null && Game1.timeOfDay == 2000)
        //        {
        //            Building bld = FEUtility.GetBuildingByName(Name);
        //            if (bld != null)
        //            {
        //                int identifier = (int)(bld.tileX.Value * 2000f + bld.tileY.Value);
        //                LightSource lightSource = new LightSource(4, new Vector2(bld.tileX.Value * 64f + 32f, bld.tileY.Value * 64f - 64f), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.WindowLight, 0L);
        //                bgl.sharedLights.Add(identifier, lightSource);
        //            }
        //        }
        //    }
        //}
        //public override void DayUpdate(int dayOfMonth)
        //{

        //    base.DayUpdate(dayOfMonth);
        //}
        public new bool SeedsIgnoreSeasonsHere()
        {
            return true;
        }
    }
}
