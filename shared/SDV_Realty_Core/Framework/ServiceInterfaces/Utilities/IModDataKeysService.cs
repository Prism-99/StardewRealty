using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IModDataKeysService : IService
    {
        public override Type ServiceType => typeof(IModDataKeysService);

        public static string ModKey;
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
