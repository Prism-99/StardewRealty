using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using SDObject = StardewValley.Object;
namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    public class CustomFlavouredOutput
    {
        private static Random randomMultiplier = new Random();
        private static Random randomQuantity = new Random();
        private static Random randomRule = new Random();
        private static string key_preserve_id = "prism99.sdr.preserveid";
        /// <summary>
        /// Get item price with any modifiers applied
        /// </summary>
        /// <param name="basePrice"></param>
        /// <param name="inputItem"></param>
        /// <param name="outputData"></param>
        /// <returns></returns>
        public static int GetPrice(int basePrice, Item inputItem, MachineItemOutput outputData)
        {
            //
            //  set pricing
            //
            int price = basePrice;

            if (outputData.CopyPrice)
                price = inputItem.salePrice();
            //
            //  check pricemodifiers
            //
            if (outputData.PriceModifiers != null)
            {
                QuantityModifier modifier = null;

                if (outputData.PriceModifiers.Count == 1)
                {
                    //
                    //  single multiplier
                    //
                    modifier = outputData.PriceModifiers.First();
                }
                else
                {
                    //
                    //  get a list of potential modifiers
                    //
                    List<QuantityModifier> candadites = outputData.PriceModifiers.Where(
                        p => string.IsNullOrEmpty(p.Condition) || GameStateQuery.CheckConditions(p.Condition)).ToList();

                    if (candadites.Count == 1)
                    {
                        modifier = candadites.First();
                    }
                    else
                    {
                        //
                        //  pick random multiplier
                        //
                        int mulitplier = randomMultiplier.Next(0, candadites.Count);
                        modifier = candadites[mulitplier];
                    }
                }
                //
                //  apply multiplier
                //
                price = (int)QuantityModifier.Apply(price, modifier.Modification, modifier.Amount);
            }

            return price;
        }

        public static SDObject GetCustomFlavouredOutputItem(Item inputItem, MachineItemOutput outputData)
        {
            string baseQualifiedId;
            bool isFlavoured = false;
            if (outputData.CustomData !=null && outputData.CustomData.TryGetValue(key_preserve_id, out baseQualifiedId))
                isFlavoured = true;
            else
                baseQualifiedId = outputData.ItemId;

            //string baseQualifiedId = outputData.PreserveType;

            int quantity = outputData.MinStack;

            if (outputData.MaxStack > outputData.MinStack)
                quantity = randomQuantity.Next(outputData.MinStack, outputData.MaxStack + 1);

            SDObject retItem = ItemRegistry.Create<SDObject>(baseQualifiedId, quantity);

            if (isFlavoured)
            {
                retItem.displayNameFormat = "%PRESERVED_DISPLAY_NAME " + retItem.DisplayName;
                if (inputItem is SDObject preserverObject && !string.IsNullOrEmpty(preserverObject.preservedParentSheetIndex.Value))
                {
                    retItem.preservedParentSheetIndex.Value = preserverObject.preservedParentSheetIndex.Value;
                }
                else
                {
                    retItem.preservedParentSheetIndex.Value = inputItem.itemId.Value;
                }
            }
            // the name is used by game to differentiate flavours
            retItem.Name = inputItem.DisplayName + retItem.DisplayName;
            if (outputData.CopyQuality)
                retItem.Quality = inputItem.Quality;

            retItem.Price = GetPrice(inputItem.salePrice(), inputItem, outputData);

            return retItem;
        }
    }
}
