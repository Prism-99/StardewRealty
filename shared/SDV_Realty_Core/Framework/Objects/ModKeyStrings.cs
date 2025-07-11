

namespace SDV_Realty_Core.Framework.Objects
{
    internal class ModKeyStrings
    {
        // Action Strings
        //
        // used in maps (FarmServicesInterior)
        // opens the Farm Services menu
        public const string Action_FarmServices = "prism99.sdr.farm.services";
        // used in maps (ReaEstateOfficeInterior)
        // opens the Buying/Selling menu
        public const string Action_RealEstateOffice = "prism99.sdr.realestate.office";
        // used in maps (Greenhouses)
        // waters crops in location
        public const string Action_WaterMe = "prism99.sdr.WaterMe";
        // used in maps (Town Billboard)
        // opens the For Sale menu
        public const string Action_ForSale = "prism99.sdr.ForSale";
        // used in maps
        // pops SDR minecart menu (going away)
        public const string Action_MineCartOld = "SDR_MineCart";
        // used in maps
        public const string Action_MineCart = "prism99.sdr.MineCart";

        //  TouchAction Strings
        //
        // used in maps (StardewMeadows)
        // warp from Stardew Meadows back to previous location
        public const string TAction_WarpBack = "prism99.sdr.warpback";
        // used in maps (StardewMeadows)
        // warp to warp ring destination location
        public const string TAction_WarpRing = "prism99.sdr.warpring";
        // used in code
        // pick destination from dual exit
        // used by IGridManager
        public const string TAction_PickDestination = "prism99.sdr.pickdestination";
        // used in maps (BackWoods)
        // handle warp out of BackWoods (Stardew Meadows or Grid 0 (if exists))
        public const string TAction_BackWoods = "prism99.sdr.backwoods";
        // used in expansions
        // handle warp from expansion location
        public const string TAction_ToMeadows = "prism99.sdr.meadows";
    }
}
