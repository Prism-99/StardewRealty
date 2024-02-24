using SDV_Realty_Core.Framework.Utilities;


namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    public class GreenhouseLocation : GameLocation
    {
        public GreenhouseLocation() : base()
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        public GreenhouseLocation(string path, string id) : base(path, id)
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
        //public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
        //{
        //    //FEFramework.logger.Log($"gh.updateEvenIfFarmerIsntHere", StardewModdingAPI.LogLevel.Debug);
        //    base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
        //    if (modData.ContainsKey(FEModDataKeys.FELocationName))
        //    {
        //       // FEFramework.logger.Log($"  have exp", StardewModdingAPI.LogLevel.Debug);

        //        BuildableGameLocation bgl = (BuildableGameLocation)Game1.getLocationFromName(modData[FEModDataKeys.FELocationName]);
        //        if (bgl != null && Game1.timeOfDay == 2000)
        //        {
        //            //FEFramework.logger.Log($"  looking for {NameOrUniqueName}", LogLevel.Debug);
        //            Building bld = FEUtility.GetBuildingByName(NameOrUniqueName);
        //            if (bld != null)
        //            {
        //                int identifier = (int)(bld.tileX.Value * 2100f + bld.tileY.Value);
        //                LightSource lightSource = new LightSource(6, new Vector2((bld.tileX.Value-1) * 64f + 32f, bld.tileY.Value * 64f - 64f), 5.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.WindowLight, Game1.player.UniqueMultiplayerID);
        //                if (!bgl.sharedLights.ContainsKey(identifier))
        //                {
        //                    FEFramework.logger.Log($"  found greenhouse.", StardewModdingAPI.LogLevel.Debug);
        //                    //bgl.lightGlows.Add(new Vector2(bld.tileX.Value * 64f + 32f, bld.tileY.Value * 64f - 64f));
        //                    bgl.sharedLights.Add(identifier, lightSource);
                            
        //                }
        //            }
        //        }
        //    }
        //}
        public override void updateWarps()
        {
            base.updateWarps();
            if (modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                //FEFramework.FixWarps(this, modData[FEModDataKeys.FELocationName]);
            }
        }
        //public override void DayUpdate(int dayOfMonth)
        //{
          
        //        base.DayUpdate(dayOfMonth);
          

        //}
        public new bool SeedsIgnoreSeasonsHere()
        {
            return true;
        }
    }
}
