using CustomMenuFramework.Menus;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;

namespace SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders
{
    /// <summary>
    /// FarmServiceProvider to seed an entire expansion
    /// in a single crop type
    /// </summary>
    internal class PlantCrops : IFarmServiceProvider
    {
        public override string Key => "plant_crops";
        public override string DisplayValue => I18n.PC_Label();
        public override string TooltTip => I18n.PC_Label_TT();
        public PlantCrops(IModDataService modDataService, IUtilitiesService utilities, ILoggerService loggerService)
        {
            this.modDataService = modDataService;
            this.loggerService = loggerService;
            utilitiesService = utilities;
            serviceCharge = 0.1f;
        }

        internal override bool PerformAction(GameLocation loctaion, string[] args, Farmer who, Point pos)
        {
            Picklocation(I18n.PC_ExpHeader(), PickCrop, GetQuote);

            return true;
        }
        public string GetQuote(string expansion, int tiles)
        {
            return I18n.PC_LineQuote((int)(tiles * serviceCharge));
        }
        private void PickCrop(string expansionNaame)
        {
            IEnumerable<KeyValuePair<string, StardewValley.GameData.Crops.CropData>> crops = Game1.cropData.Where(p => p.Value.Seasons.Contains(Game1.getLocationFromName(expansionNaame).GetSeason()));
            GenericPickListMenu pickCropMenu = new GenericPickListMenu();
            List<KeyValuePair<string, string>> cropList = new();

            GameLocation? location = null;

            if (expansionNaame == "Farm")
                location = Game1.getFarm();
            else if (modDataService.farmExpansions.TryGetValue(expansionNaame, out FarmExpansionLocation? expansion))
                location = expansion;

            if (location != null)
            {
                foreach (var crop in crops.OrderBy(p => TokenParser.ParseText(Game1.objectData[p.Value.HarvestItemId].DisplayName)))
                {
                    cropList.Add(new KeyValuePair<string, string>(crop.Key, I18n.PC_Quote(TokenParser.ParseText(Game1.objectData[crop.Value.HarvestItemId].DisplayName), utilitiesService.GetSeedPrice(crop.Key), Game1.objectData[crop.Value.HarvestItemId].Price * location.Map.DisplayWidth / Game1.tileSize * location.Map.DisplayHeight / Game1.tileSize)));
                }
                pickCropMenu.ShowPagedResponses(I18n.PC_SeedHeader(), cropList, delegate (string value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        PlantTheCrops(expansionNaame, value);
                    }
                }, auto_select_single_choice: false);
            }
            else
            {
                loggerService.Log($"Unknown location to plant ({expansionNaame})", LogLevel.Error);
            }
        }
        private int GetToPlantCount(GameLocation expansion)
        {
            Layer back = expansion.Map.GetLayer("Back");
            int totalPlanted = 0;
            if (back != null)
            {
                for (int x = 0; x < back.DisplayHeight; x++)
                {
                    for (int y = 0; y < back.DisplayWidth; y++)
                    {
                        if (!expansion.objects.ContainsKey(new Vector2(x, y)) && expansion.terrainFeatures.TryGetValue(new Vector2(x, y), out var feature) && (feature is HoeDirt { crop: null }))
                        {
                            totalPlanted++;
                        }
                    }
                }
            }
            else
            {
                totalPlanted = -1;
                loggerService.Log($"Location: {expansion.Name} has no 'Back' layer.", LogLevel.Error);
            }
            return totalPlanted;
        }
        private void PlantTheCrops(string expansionName, string seedId)
        {
            GameLocation? selectedFarm = null;
            if (expansionName == "Farm")
                selectedFarm = Game1.getFarm();
            else if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation expansion))
                selectedFarm = expansion;

            if (selectedFarm != null)
            {
                int totalSeeded = 0;
                int seedCost = utilitiesService.GetSeedPrice(seedId);
                int toSeed = GetToPlantCount(selectedFarm);

                if (toSeed == 0)
                {
                    utilitiesService.PopMessage(I18n.PC_NoDirt(), IUtilitiesService.PopupType.Dialogue);
                }
                else if ((int)(totalSeeded * seedCost + totalSeeded * serviceCharge) > Game1.player.Money)
                {
                    utilitiesService.PopMessage(I18n.NoMoney(), IUtilitiesService.PopupType.Dialogue);
                }
                else
                {
                    Layer back = selectedFarm.Map.GetLayer("Back");

                    for (int x = 0; x < back.DisplayHeight; x++)
                    {
                        for (int y = 0; y < back.DisplayWidth; y++)
                        {
                            if (!selectedFarm.objects.ContainsKey(new Vector2(x, y)) && selectedFarm.terrainFeatures.TryGetValue(new Vector2(x, y), out var feature) && (feature is HoeDirt { crop: null }))
                            {
                                HoeDirt dirt = (HoeDirt)feature;
                                dirt.crop = new Crop(seedId, x, y, selectedFarm);
                                dirt.state.Value = 1;
                                totalSeeded++;
                            }
                        }
                    }
                    int totalCost = (int)(totalSeeded * seedCost + totalSeeded * serviceCharge);
                    utilitiesService.PopMessage(I18n.PC_Total(totalSeeded, seedCost, totalCost), IUtilitiesService.PopupType.Dialogue);
                    Game1.player.Money -= totalCost;
                }
            }
        }
    }
}
