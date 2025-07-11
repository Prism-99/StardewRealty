using System.IO;
using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using StardewValley.GameData.Crops;

namespace SDV_Realty_Core.Framework.CustomEntities.Crops
{
    internal class CustomCropData : ISDRContentPack
    {
        public string CropId { get; set; }
        public override string PackFileName => "crop.json";

        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            CustomCropData newCrop = JsonConvert.DeserializeObject<CustomCropData>(fileContent);
            newCrop.ModPath = Path.GetDirectoryName(fileName);

            return newCrop;
        }
        public CropData CropData { get; set; }
    }
}
