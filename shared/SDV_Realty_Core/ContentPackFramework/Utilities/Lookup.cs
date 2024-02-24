using System;
using System.Collections.Generic;
using Prism99_Core.Utilities;
using StardewValley;

namespace SDV_Realty_Core.ContentPackFramework.Utilities
{
    internal  class Lookup
    {
        //
        //  Provides objectId to Name lookups
        //
        public static string GetObjectName(int iObjectId)
        {
#if v16
            if (Game1.objectData.ContainsKey(iObjectId.ToString()))
            {
                return SDVUtilities.GetText( Game1.objectData[iObjectId.ToString()].DisplayName);
            }
            else
            {
                return "????";
            }
#else
            if (Game1.objectInformation.ContainsKey(iObjectId))
            {
                return Game1.objectInformation[iObjectId].Split('/')[0];
            }
            else
            {
                return "????";
            }
#endif
        }
        public static string GetObjectName(int iObjectType, int iObjectId, string sDefault)
        {
            string sName = "????";

            switch (iObjectType)
            {
                case 7: //fruit Trees
                    var oTrees = Game1.content.Load<Dictionary<int, string>>("Data/fruitTrees");
                    if (oTrees.ContainsKey(iObjectId))
                    {
                        sName = GetObjectName(Convert.ToInt32(oTrees[iObjectId].Split('/')[2]));
                    }
                    else
                    {
                        sName = sDefault;
                    }
                    break;
                default:
                    sName = GetObjectName(iObjectId);
                    if (sName == "????") sName = sDefault;
                    break;
            }

            return sName;
        }
    }
}
