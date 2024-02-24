namespace SDV_Realty_Core.Framework.Utilities
{
    /*
     * 
     *  Central storage of any ModData keys used.
     * 
     * 
     * 
     * 
     */
    internal static class FEModDataKeys
    {
        public static string ModKey;


        public static void Initialize(string sModKey)
        {
            ModKey = sModKey;
        }

        // expansion name
        public static string FELocationName => ModKey + ".FEExpName";
        public static string FEExpansionType => ModKey + ".FEExpansion";
        public static string FEExpansionDisplayName => ModKey + ".FEDisplayName";
        public static string FEWinery => ModKey + ".FEWinery";
        public static string FEGridId => ModKey + ".FEGridId";
        public static string FEFromagerie => ModKey + ".FEFromagerie";
        public static string FEGreenhouse => ModKey + ".FEGreenhouse";
        public static string FELargeGreenhouse => ModKey + ".FELargeGreenhouse";
        public static string FEBreadFactory => ModKey + ".FEBreadFactory";
    }
}
