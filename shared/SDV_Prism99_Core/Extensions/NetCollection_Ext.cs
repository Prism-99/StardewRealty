using Netcode;


namespace Prism99_Core.Extensions
{
    internal static class NetCollection_Ext
    {
        public static void ReplaceWith<T>(this NetCollection<T> collection, NetCollection<T> source)
            where T : class, Netcode.INetObject<Netcode.INetSerializable>
        {
            collection.Clear();
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }
        public static void AddTo<T>(this NetCollection<T> collection, NetCollection<T> source)
           where T : class, Netcode.INetObject<Netcode.INetSerializable>
        {
            //collection.Clear();
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }
    }
}
