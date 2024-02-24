using Netcode;
using StardewValley.Network;


namespace Prism99_Core.Extensions
{
    internal static class NetVector2Dictionary_Ext
    {
        public static void ReplaceWith<T, TField>(this NetVector2Dictionary<T, TField> collection, NetVector2Dictionary<T, TField> source)
         where TField : NetField<T, TField>, new()
        {
            collection.Clear();
            foreach (var kvp in source.Pairs)
            {
                collection.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
