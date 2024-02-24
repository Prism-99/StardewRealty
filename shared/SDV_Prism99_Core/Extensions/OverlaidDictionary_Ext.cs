using StardewValley.Network;


namespace Prism99_Core.Extensions
{
    internal static class OverlaidDictionary_Ext
    {
        public static void ReplaceWith(this OverlaidDictionary collection, OverlaidDictionary source)
        {
            collection.Clear();
            foreach (var kvp in source.Pairs)
            {
                collection.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
