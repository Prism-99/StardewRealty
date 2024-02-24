using StardewValley.GameData.BigCraftables;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using Newtonsoft.Json;
using System.IO;
using SDV_Realty_Core.Framework.CustomEntities.Objects;
using SDV_Realty_Core.Framework.CustomEntities.Machines;

namespace SDV_Realty_Core.Framework.CustomEntities.BigCraftables
{
    /// <summary>
    /// Custom BigCraftable content pack definition
    /// </summary>
    internal class CustomBigCraftableData : ISDRContentPack
    {
        public CustomBigCraftableData() { }
        public CustomBigCraftableData(CustomMachine machine)
        {
            Id = machine.MachineId;
            BigCraftableData = machine.BigCraftableData;
            Materials = machine.Materials;
        }

        public string Id { get; set; }
        public Dictionary<string, int> Materials { get; set; }
        public string CraftingRecipe()
        {
            string recipe = "";
            if (Materials != null)
            {
                recipe = string.Join(" ", Materials.Select(p => p.Key + " " + p.Value.ToString()));
            }
            //recipe += $"/Home/{Id}/true/{(string.IsNullOrEmpty( Conditions)? "default" :$"G {Conditions}")}/";
            recipe += $"/Home/{Id}/true/default/";

            return recipe;
        }

        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);

            CustomBigCraftableData retObject = JsonConvert.DeserializeObject<CustomBigCraftableData>(fileContent);
            retObject.ModPath = Path.GetDirectoryName(fileName);

            return retObject;
        }

        public BigCraftableData BigCraftableData { get; set; }

        public override string PackFileName => "bigcraftable.json";
    }
}
