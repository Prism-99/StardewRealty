using StardewValley.GameData.Machines;
using System.Collections.Generic;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using Newtonsoft.Json;
using System.IO;

namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    /// <summary>
    /// Content pack for MachineData
    /// </summary>
    public class CustomMachineData : ISDRContentPack
    {
        public CustomMachineData() { }
        public CustomMachineData(CustomMachine machineData) {
            MachineData = machineData.MachineData;
            MachineId = machineData.MachineId;
            IsMachineDefinition = machineData.IsMachineDefinition;
            AutomateOnlyRecipes= machineData.AutomateOnlyRecipes;
        }
        public string MachineId { get; set; }
        public bool IsMachineDefinition { get; set; } = true;
        /// <summary>
        /// List of MachineData Output rules that are only
        /// available if Automate is installed
        /// </summary>
        public List<string> AutomateOnlyRecipes { get; set; }
        public MachineData MachineData { get; set; }

        public override string PackFileName => "machinedata.json";

        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<CustomMachineData>(fileContent);
        }
    }
}
