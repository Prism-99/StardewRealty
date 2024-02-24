using StardewValley.Mods;

namespace Prism99_Core.Extensions
{
    internal static class modData_Ext
    {
        public static void AddMyRange(this ModDataDictionary dest, ModDataDictionary toAdd)
        {
            foreach (string key in toAdd.Keys)
            {
                dest.Add(key, toAdd[key]);
            }
        }
    }
}
