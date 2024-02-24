using System;
using System.Reflection;
using System.Linq;



namespace SDV_Realty_Core.Framework.Patches.Integrations
{
    internal static class FEAnimalsNeedWater
    {
        public static string TileSheetPath = null;
        public static void AddNewTileSheetPostfix(Object __instance)
        {
            //
            //  get path to trough tilesheet
            //
            try
            {
                Type type = Type.GetType("AnimalsNeedWater.ModEntry, AnimalsNeedWater");
                PropertyInfo propInfo = type.GetProperties().Where(m => m.Name == "Helper").First();
                MethodInfo methodInfo = propInfo.GetMethod;

                IModHelper oHelper = (IModHelper)methodInfo.Invoke(__instance, new object[] { });
                //TileSheetPath = oHelper.Content.GetActualAssetKey($"assets/waterTroughTilesheet.xnb");
                TileSheetPath = oHelper.GameContent.ParseAssetName($"assets/waterTroughTilesheet.xnb").Name;
            }
            catch { }
        }
    }
}
