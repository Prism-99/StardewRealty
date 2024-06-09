using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.IO;
using Prism99_Core.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using Newtonsoft.Json;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.Buildings
{
    internal class GenericBuilding : ICustomBuilding
    {
        public GenericBuilding() { }
        public GenericBuilding(string modDir, ILoggerService olog) : base(modDir, olog)
        {
        }
        public override GenericBuilding ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<GenericBuilding>(fileContent);

        }
        public override string ID { get; set; }

        public override string Name { get; set; }

        public override string DisplayName { get; set; }

        public override string Description { get; set; }

        public override string Upgrades { get; set; }

        public override bool DrawShadow { get; set; }

        public override Point Size { get; set; }

        public override Point HumanDoor { get; set; }

        public override Point AnimalDoor { get; set; }

        public override Rectangle SourceRectangle { get; set; }

        public override string BaseBuildingType { get; set; }

        public override int BuildDays { get; set; }

        public override int Cost { get; set; }

        public override int MaxOccupancy { get; set; }

        public override string Builder { get; set; }

        public override List<string> BuildLocations { get; set; }

        public override string CollisionMap { get; set; }
        public override Dictionary< string, int> Material { get; set; }
        public override string MachineName { get; set; }
        public override string InteriorMapName { get; set; }
        public override string BuildingTextureName { get; set; }

        //public override string BuildingTexture()
        //{
        //    return Path.Combine(modpath, BuildingTextureName);
        //}

        public override Dictionary<string, object> ExternalReferences { get; set; }

        public override Dictionary<int, SDObject> GetBuildingProduction(GameLocation indoors)
        {
            return new Dictionary<int, SDObject> { };
        }

        public override GameLocation GetIndoorLocation(Building b, string LocationName)
        {
            GameLocation interior = new GameLocation($"Maps{FEConstants.AssetDelimiter}{Name}{FEConstants.AssetDelimiter}{InteriorMapName}", ID);
            interior.uniqueName.Value = $"{Name}_{DateTime.Now.Ticks}";
            interior.IsFarm = false;
            interior.IsGreenhouse = IsGreenhouse;
            interior.isStructure.Value = true;
            interior.IsOutdoors = IsOutdoors;
            if (b.indoors.Value != null)
            {
                interior.characters.Set(b.indoors.Value.characters);
                interior.netObjects.MoveFrom(b.indoors.Value.netObjects);
                interior.terrainFeatures.MoveFrom(b.indoors.Value.terrainFeatures);
                interior.miniJukeboxCount.Set(b.indoors.Value.miniJukeboxCount.Value);
                interior.miniJukeboxTrack.Set(b.indoors.Value.miniJukeboxTrack.Value);

                interior.modData.Clear();
                interior.modData.AddMyRange(b.indoors.Value.modData);

                if (!string.IsNullOrEmpty(b.indoors.Value.uniqueName.Value))
                    interior.uniqueName.Value = b.indoors.Value.uniqueName.Value;

                if (b.indoors.Value is DecoratableLocation)
                {
                    interior.furniture.Set(b.indoors.Value.furniture);
                    foreach (Furniture item in interior.furniture)
                    {
                        item.updateDrawPosition();
                    }
                }
                interior.TransferDataFromSavedLocation(b.indoors.Value);
                if (!interior.modData.ContainsKey(IModDataKeysService.FELocationName))
                    interior.modData.Add(IModDataKeysService.FELocationName, LocationName);
                if (!interior.modData.ContainsKey(IModDataKeysService.FELargeGreenhouse))
                    interior.modData.Add(IModDataKeysService.FELargeGreenhouse, "Y");

                if (b.indoors.Value.warps.Count > 0)
                {
                    interior.warps.Clear();
                    foreach (var warp in b.indoors.Value.warps)
                    {
                        interior.warps.Add(warp);
                    }
                }
            }
            if (dllLoaded)
            {
                InvokeDLLMethod(b.buildingType.Value,"Register", new object[] { b });
            }

            return interior;
        }

        public override string MapAssetPath()
        {
            return Path.Combine(ModPath, InteriorMapName);
        }
    }
}
