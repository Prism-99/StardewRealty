

namespace StardewRealty.SDV_Realty_Interface
{
    public class FishStockData
    {
        public string FishId { get; set; }
        public float Chance { get; set; }
        public string Season { get; set; }
        public bool AutoAdded { get; set; } = false;
    }
}
