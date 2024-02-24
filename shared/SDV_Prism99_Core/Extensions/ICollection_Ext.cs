using System;
using System.Collections.Generic;
using System.Linq;


namespace Prism99_Core.Extensions
{
    internal static class ICollection_Ext
    {
        public static bool ContainsType<T>(this ICollection<T> list, Type oType)
        {
            return list.Any(list => list != null && list.GetType() == oType);
        }
    }
}
