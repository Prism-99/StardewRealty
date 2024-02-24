
namespace Prism99_Core.Extensions
{
    internal static class Warp_Ext
    {
        public static string GetWarpString(this Warp warp)
        {
            return $"{warp.X} {warp.Y} {warp.TargetName} {warp.TargetX} {warp.TargetY}";
        }
    }
}
