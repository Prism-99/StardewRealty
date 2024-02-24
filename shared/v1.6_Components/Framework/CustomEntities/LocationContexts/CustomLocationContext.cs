using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using StardewModdingAPI;
using StardewValley.GameData.LocationContexts;

namespace SDV_Realty_Core.Framework.CustomEntities.LocationContexts
{
    internal class CustomLocationContext : ISDRContentPack
    {
        public override string PackFileName => "locationcontext.json";
        public string ContextName { get; set; }
        public bool CopyDefaultFields { get; set; }
        public LocationContextData LocationContextData { get; set; }
        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            CustomLocationContext newContext = JsonConvert.DeserializeObject<CustomLocationContext>(fileContent);
            newContext.ModPath = Path.GetDirectoryName(fileName);

            return newContext;
        }
    }
}
