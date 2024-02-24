using System.Collections.Generic;
using Prism99_Core.Utilities;
using StardewValley.GameData.Locations;


namespace StardewRealty.SDV_Realty_Interface
{
    public class FishAreaDetails
    {

        public FishAreaDetails()
        {
            StockData = new List<FishStockData> { };
        }

        public FishAreaDetails(FishAreaData fdata)
        {
            StockData = new List<FishStockData> { };
            Id = SDVUtilities.GetText(fdata.DisplayName);
            DisplayName = SDVUtilities.GetText(fdata.DisplayName);
            Position = fdata.Position ?? Rectangle.Empty;
            CrabPotFishTypes=fdata.CrabPotFishTypes;
            CrabPotJunkChance=fdata.CrabPotJunkChance;
        }
        public static FishAreaData GetData(FishAreaDetails data)
        {
            return new FishAreaData
            {
                DisplayName = data.DisplayName,
                Position = data.Position,
                CrabPotFishTypes = data.CrabPotFishTypes,
                CrabPotJunkChance = data.CrabPotJunkChance
            };
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public Rectangle Position { get; set; }
        public float CrabPotJunkChance { get; set; }
        public List<string> CrabPotFishTypes { get; set; }
        public List<FishStockData> StockData { get; set; }
        public bool AutoFill { get; set; }
        public int MaxFishTypes { get; set; }
    }
}
