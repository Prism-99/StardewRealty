using StardewValley.GameData.Machines;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;
//using SDObject = StardewValley.Object;

namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    internal class GenericMachihneOutput
{
        public static Item GetOutput(SDObject machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;

            return CustomFlavouredOutput.GetCustomFlavouredOutputItem( inputItem, outputData);
        }
    }
}
