using StardewValley.GameData.Machines;
using StardewValley.GameData.BigCraftables;
using System.Collections.Generic;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using Newtonsoft.Json;
using System.IO;

namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    /// <summary>
    /// Content pack for custom Machine
    /// </summary>
    public class CustomMachine : ISDRContentPack
    {
        public string MachineId { get; set; }
        public bool IsMachineDefinition { get; set; } = true;
        /// <summary>
        /// List of MachineData Output rules that are only
        /// available if Automate is installed
        /// </summary>
        public List<string> AutomateOnlyRecipes { get; set; }
        public MachineData MachineData { get; set; }
        public Dictionary<string, int> Materials { get; set; }
        public BigCraftableData BigCraftableData { get; set; }
        public override string PackFileName => "machine.json";

        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<CustomMachine>(fileContent);
        }
    }
}
