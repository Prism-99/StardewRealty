using StardewValley.GameData.Objects;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace SDV_Realty_Core.Framework.CustomEntities.Objects
{
    /// <summary>
    /// Content pack for custom Object data
    /// </summary>
    internal class CustomObjectData:ISDRContentPack
    {
        public string ObjectId { get; set; }
        public Dictionary<string,List<string>> GiftTastes { get; set; }
        public ObjectData ObjectData { get; set; }
        public override string PackFileName => "object.json";
        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            CustomObjectData newObject= JsonConvert.DeserializeObject<CustomObjectData>(fileContent);
            newObject.ModPath=Path.GetDirectoryName(fileName); 

            return newObject;
        }
    }
}
