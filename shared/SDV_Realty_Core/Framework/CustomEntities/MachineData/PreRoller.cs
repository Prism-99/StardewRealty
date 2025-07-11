using StardewValley.GameData.Machines;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;
//using SDObject = StardewValley.Object;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.GameData;
using System.Linq;

namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    public class PreRoller
    {

        public static Item GetOutput(SDObject machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;

            //SDObject retItem = ItemRegistry.Create<SDObject>("(O)Joint", quantity);

            return CustomFlavouredOutput.GetCustomFlavouredOutputItem(  inputItem, outputData);

            //retItem.displayNameFormat = "%PRESERVED_DISPLAY_NAME " + retItem.DisplayName;
            //// the name is used by game to differentiate flavours
            //retItem.Name = inputItem.DisplayName + retItem.DisplayName;
            //retItem.preservedParentSheetIndex.Value = inputItem.itemId.Value;
            //if (outputData.CopyQuality)
            //    retItem.Quality = inputItem.Quality;

            //return retItem;
        }
    }
}
