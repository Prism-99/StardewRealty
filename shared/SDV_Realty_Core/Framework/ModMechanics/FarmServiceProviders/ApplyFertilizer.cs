using CustomMenuFramework.Menus;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using System.Collections.Generic;
using xTile.Layers;

namespace SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders
{
    /// <summary>
    /// FarmServiceProvider service to apply fertilizer to
    /// an expansion
    /// </summary>
    internal class ApplyFertilizer : IFarmServiceProvider
    {        
        public override string Key => "apply_fertilizer";
        public override string DisplayValue => I18n.AF_Label();
        public override string TooltTip => I18n.AF_Label_TT();

        public ApplyFertilizer(IModDataService modDataService, IUtilitiesService utilitiesService) : base(modDataService, utilitiesService)
        {
            serviceCharge = 0.1f;
        }

        internal override bool PerformAction(GameLocation loctaion, string[] args, Farmer who, Point pos)
        {
            Picklocation(I18n.AF_Header(), PickFertilizer, GetQuote);

            return true;
        }
        private string GetQuote(string expansion, int tiles)
        {
            return I18n.AF_Quote((int)(tiles * serviceCharge));
        }
        private void PickFertilizer(string expansionName)
        {
            GenericPickListMenu pickFertilizerMenu = new GenericPickListMenu();
            List<KeyValuePair<string, string>> fertilizerList = GetFertilizers(false);

            pickFertilizerMenu.ShowPagedResponses(I18n.AF_FertHeader(), fertilizerList, delegate (string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    FertilizeDirt(expansionName, value);
                }
            }, auto_select_single_choice: false);
        }
        private int GetToFertilizeCount(GameLocation expansion)
        {
            Layer back = expansion.Map.GetLayer("Back");
            int totalFertilized = 0;
            if (back != null)
            {
                for (int x = 0; x < back.DisplayHeight; x++)
                {
                    for (int y = 0; y < back.DisplayWidth; y++)
                    {
                        if (!expansion.objects.ContainsKey(new Vector2(x, y)) && expansion.terrainFeatures.TryGetValue(new Vector2(x, y), out var feature) && (feature is HoeDirt { crop: null }))
                        {
                            totalFertilized++;
                        }
                    }
                }
            }
            else
            {
                totalFertilized = -1;
                loggerService.Log($"Location: {expansion.Name} has no 'Back' layer.", LogLevel.Error);
            }

            return totalFertilized;
        }
        private void FertilizeDirt(string expansionName, string fertilizerId)
        {
            GameLocation? location = null;
            if (expansionName == "Farm")
                location = Game1.getFarm();
            else if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation? expansion))
                location = expansion;

            if(location!=null)
            {
                int fertilizerCost = 0;
                if (Game1.objectData.TryGetValue(fertilizerId, out var objectData))
                {
                    fertilizerCost = GetFertilizerCost(fertilizerId);
                }
                int toFertilize = GetToFertilizeCount(location);
                if (toFertilize == 0)
                {
                    utilitiesService.PopMessage(I18n.AF_NoDirt(), IUtilitiesService.PopupType.Dialogue);
                }
                else if (toFertilize * serviceCharge + toFertilize * fertilizerCost > Game1.player.Money)
                {
                    utilitiesService.PopMessage(I18n.NoMoney(), IUtilitiesService.PopupType.Dialogue);
                }
                else
                {
                    int totalFertilized = 0;
                    Layer back = location.Map.GetLayer("Back");
                    if (back != null)
                    {
                        for (int x = 0; x < back.DisplayHeight; x++)
                        {
                            for (int y = 0; y < back.DisplayWidth; y++)
                            {
                                if (!location.objects.ContainsKey(new Vector2(x, y)) && location.terrainFeatures.TryGetValue(new Vector2(x, y), out var feature) && (feature is HoeDirt { crop: null }))
                                {
                                    HoeDirt dirt = (HoeDirt)feature;
                                    dirt.fertilizer.Value = fertilizerId;
                                    totalFertilized++;
                                }
                            }
                        }
                        int totalCost = (int)(totalFertilized * fertilizerCost + totalFertilized * serviceCharge);
                        utilitiesService.PopMessage(I18n.AF_Total(totalFertilized, fertilizerCost, totalCost), IUtilitiesService.PopupType.Dialogue);
                        Game1.player.Money -= totalCost;
                    }
                    else
                    {
                        loggerService.Log($"Location: {expansionName} has no 'Back' layer.", LogLevel.Error);
                    }
                }
            }
            else
            {
                loggerService.Log($"Unknown location to fertilize ({expansionName})", LogLevel.Error);
            }
        }
        private int GetFertilizerCost(string fertilizerId)
        {
            return fertilizerId.Replace("(O)", "") switch
            {
                "369" => 150,
                "371" => 150,
                "466" => 150,
                "918" => 200,
                "919" => 200,
                "920" => 200,
                _ => 100
            };
        }
        private string GetDisplayName(string fertilizerId)
        {
            if (Game1.objectData.TryGetValue(fertilizerId, out var objectData))
            {
                return TokenParser.ParseText(objectData.DisplayName);
            }

            return fertilizerId;
        }
        private List<KeyValuePair<string, string>> GetFertilizers(bool ignoreRequirements = false)
        {
            List<KeyValuePair<string, string>> fertilizerList = new()
            {
               new KeyValuePair<string, string>("368",$"{GetDisplayName("368")} (100g)" )
            };

            if (ignoreRequirements || Game1.player.FarmingLevel >= 4)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("370", $"{GetDisplayName("370")} (100g)"));

            if (ignoreRequirements || Game1.player.FarmingLevel >= 3)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("465", $"{GetDisplayName("465")} (100g)"));


            if (ignoreRequirements || Game1.player.FarmingLevel >= 9)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("369", $"{GetDisplayName("369")} (150g)"));

            if (ignoreRequirements || Game1.player.FarmingLevel >= 7)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("371", $"{GetDisplayName("371")} (150g)"));

            if (ignoreRequirements || Game1.player.FarmingLevel >= 8)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("466", $"{GetDisplayName("466")} (150g)"));

            if (ignoreRequirements || Game1.player.FarmingLevel > 9)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("919", $"{GetDisplayName("919")} (200g)"));

            if (ignoreRequirements || Game1.player.FarmingLevel > 9)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("920", $"{GetDisplayName("920")} (200g)"));

            if (ignoreRequirements || Game1.player.FarmingLevel > 9)
                fertilizerList.Add(new KeyValuePair<string, string>
                    ("918", $"{GetDisplayName("918")} (200g)"));

            return fertilizerList;
        }
    }
}
