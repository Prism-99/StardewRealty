using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;
using SDV_Realty_Core.Framework.Objects;
using Netcode;
using System.Linq;
using StardewValley.Objects;
using Prism99_Core.Abstractions.Objects;

namespace SDV_Realty_Core.Framework.Integrations
{
    internal class JSONAssets
    {
        public enum LocationType
        {
            Fromagerie,
            Winery,
            Location
        }
        //public static void ShuffleMe( ref Chest chest, LocationType ltype)
        //{
        //    //
        //    //  verify object list
        //    //
        //    prism_Chest pChest = new prism_Chest(chest);

        //    foreach (Item cItem in pChest.items.ToList())
        //    {
        //        switch (ltype)
        //        {
        //            case LocationType.Fromagerie:
        //                if (!IsObjectValid(cItem.Name, cItem.ParentSheetIndex))
        //                {
        //                    int newId = Convert.ToInt32(GetObjectId(cItem.Name));
        //                    if (newId == -1)
        //                    {
        //                        //
        //                        //  item did not get re-mapped, change it to cheese
        //                        //
        //                        FEFramework.logger.Log($"  re-mapping item: {cItem.Name}", LogLevel.Debug);
        //                        FEFramework.logger.Log($"Item removed {pChest.items.Remove(cItem)}", LogLevel.Debug); ;
        //                        cItem.ParentSheetIndex = 424;
        //                        cItem.Name = "Cheese";
        //                        //cItem..Value = 2;
        //                        pChest.items.Add(cItem);
        //                    }
        //                    else
        //                    {
        //                        //
        //                        //  replace item with new id
        //                        //
        //                        FEFramework.logger.Log($"  replacing item: {cItem.Name}", LogLevel.Debug);
        //                        FEFramework.logger.Log($"Item removed {pChest.items.Remove(cItem)}", LogLevel.Debug); ;
        //                        //chest.items.Remove(cItem);
        //                        cItem.ParentSheetIndex = newId;
        //                        cItem.Name = GetItemName(newId.ToString());
        //                        pChest.items.Add(cItem);

        //                    }
        //                }
        //                break;
        //        }
        //    }

        //}
#if v16
        public static string GetItemName(string itemId)
        {
            if (Game1.objectData.ContainsKey(itemId))
            {
                return Game1.objectData[itemId].Name;
            }

            return null;
        }
#else
        public static string GetItemName(string itemId)
        {
            int id = Convert.ToInt32(itemId);
            if (Game1.objectInformation.ContainsKey(id))
            {
                return Game1.objectInformation[id].Split('/')[0];
            }

            return null;
        }
#endif
//        public static string GetObjectId(string objectName)
//        {
//            //
//            //  lookup id for modded output items
//            //
//            FEFramework.logger.Log($"Getting id for '{objectName}'", LogLevel.Debug);
//#if v16
//            foreach (string key in Game1.objectData.Keys)
//            {
//                if (Game1.objectData[key].Name.StartsWith(objectName))
//                {
//                    FEFramework.logger.Log($"   Found Id: {key}", LogLevel.Debug);
//                    return key.ToString();
//                }
//            }
//#else
//            foreach (int key in Game1.objectInformation.Keys)
//            {
//                if (Game1.objectInformation[key].StartsWith(objectName))
//                {
//                    FEFramework.logger.Log($"   Found Id: {key}", LogLevel.Debug);
//                    return key.ToString();
//                }
//            }
//#endif
//                return "-1";
//        }

#if v16
        public static bool IsObjectValid(string objectName, int objId)
        {
            if (Game1.objectData.ContainsKey(objId.ToString()))
            {
                return Game1.objectData[objId.ToString()].Name.StartsWith(objectName);
            }

            return false;
        }
#else
        public static bool IsObjectValid(string objectName, int objId) 
        {
            if(Game1.objectInformation.ContainsKey(objId))
            {
                return Game1.objectInformation[objId].StartsWith(objectName);
            }

            return false;
        }
#endif
    }
}
