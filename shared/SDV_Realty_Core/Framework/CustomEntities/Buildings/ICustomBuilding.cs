using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.GameData;
using SDV_Realty_Core.Framework.CustomEntities.Audio;
using xTile;
using StardewModHelpers;
using System.IO;
using SDV_xTile;
using xTile.Tiles;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using Newtonsoft.Json;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;


namespace SDV_Realty_Core.Framework.Buildings
{
    internal abstract class ICustomBuilding : ISDRContentPack
    {

        public ICustomBuilding() { }
        public ICustomBuilding(ILoggerService olog)
        {
            logger = olog;
        }

        public ICustomBuilding(string modDir, ILoggerService olog) : this(olog)
        {
            ModPath = modDir;
        }
        public override string PackFileName => "building.json";
        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<ICustomBuilding>(fileContent);

        }
        public bool IsGreenhouse { get; set; }
        public bool IsOutdoors { get; set; }
        public bool CanCaskHere { get; set; }
        public bool AllowGiantCrops { get; set; }
        public List<NightLight> NightLights { get; set; }
        public abstract string ID { get; set; }
        public abstract string Name { get; set; }
        public abstract string DisplayName { get; set; }
        public abstract string Description { get; set; }
        public abstract string Upgrades { get; set; }
        public Vector2 UpgradeSignTile { get; set; }
        public int UpgradeSignHeight { get; set; }
        public abstract bool DrawShadow { get; set; }
        public abstract Point Size { get; set; }
        public abstract Point HumanDoor { get; set; }
        public abstract Point AnimalDoor { get; set; }
        public abstract Rectangle SourceRectangle { get; set; }
        public abstract string BaseBuildingType { get; set; }
        public abstract int BuildDays { get; set; }
        public abstract int Cost { get; set; }
        public string IndoorMusic { get; set; }
        public string IndoorMusicTrack { get; set; }
        public abstract int MaxOccupancy { get; set; }
        public abstract string Builder { get; set; }
        public abstract List<string> BuildLocations { get; set; }
        public abstract string CollisionMap { get; set; }

        public abstract Dictionary<string, int> Material { get; set; }
        public List<BuildingChest> Chests { get; set; }

        public string BuildingTexture()
        {
            return Path.Combine(ModPath, BuildingTextureName);
        }
        public abstract string InteriorMapName { get; set; }
        public abstract string BuildingTextureName { get; set; }
        public abstract string MapAssetPath();
        public abstract string MachineName { get; set; }
        public abstract Dictionary<int, SDObject> GetBuildingProduction(GameLocation indoors);
        public string MapPath { get { return ID; } set { ID = value; } }
        public Map InteriorMap { get; set; } = null;
        public List<BuildingItem> BuildingItems { get; set; }

        public void LoadInteriorMap(MapLoader loader,string gamePath)
        {
            InteriorMap = loader.LoadMap(gamePath, Path.Combine(ModPath, InteriorMapName), Name, false, false, false);
            if (!string.IsNullOrEmpty(IndoorMusic))
            {
                InteriorMap.Properties["Music"] = IndoorMusic;

                if (!string.IsNullOrEmpty(IndoorMusicTrack))
                {
                    AudioCueManager.AddCue(IndoorMusic, new AudioCueData
                    {
                        Id=IndoorMusic,
                        Category = "Music",
                        FilePaths = new List<string> { Path.Combine(ModPath, IndoorMusicTrack) },
                        StreamedVorbis = true,
                        Looped = true
                    }); ;
                }
            }
        }
        public void LoadExternalReferences(MapLoader loader,string gamePath)
        {
            LoadInteriorMap( loader, gamePath);
            ExternalReferences = new Dictionary<string, object> { };
            foreach (TileSheet ts in InteriorMap.TileSheets)
            {
                //
                //  only adjust files in the custom directory
                //
                if (ts.ImageSource.StartsWith(Name))
                {
                    //
                    //  append SDR path for asset serving
                    //
                    ts.ImageSource = $"SDR{FEConstants.AssetDelimiter}Buildings{FEConstants.AssetDelimiter}" + ts.ImageSource;
                    string filename = Path.Combine(ModPath, Path.GetFileNameWithoutExtension(ts.ImageSource) + ".png");
                    try
                    {
                        ExternalReferences.Add(ts.ImageSource.Replace("\\", FEConstants.AssetDelimiter), new StardewBitmap(filename).Texture());
#if DEBUG_LOG
                        logger.Log($"Added tilesheet: {ts.ImageSource}", LogLevel.Debug);
#endif
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Error loading building TileSheet: {ex}", LogLevel.Error);
                        logger.Log($"Filename: {filename}", LogLevel.Error);
                    }
                }
                else
                {
                    logger.Log($"Skipping tilesheet: {ts.ImageSource}", LogLevel.Debug);
                }
            }
        }

        public BuildingData BuildingDefinition()
        {
            List<BuildingMaterial> materials = new List<BuildingMaterial>();

            if (Material != null)
            {
                foreach (var item in Material)
                {
                    materials.Add(new BuildingMaterial { Amount = item.Value, ItemId = item.Key });
                }
            }
            string finalBuilder = Builder;
            if (string.IsNullOrEmpty(finalBuilder) || finalBuilder == "Carpenter")
                finalBuilder = "Robin";

            return new BuildingData
            {

                // = ID,
                Name = DisplayName,
                Description = Description,
                Texture = $"SDR/Buildings/{ID}",
                DrawShadow = DrawShadow,
                Size = Size,
                SourceRect = SourceRectangle,
                SortTileOffset = 0,
                CollisionMap = CollisionMap,
                BuildingToUpgrade=Upgrades,
                UpgradeSignTile=UpgradeSignTile,
                UpgradeSignHeight=UpgradeSignHeight,
                Builder = finalBuilder,
                BuildCondition = Conditions,
                BuildDays = BuildDays,
                BuildCost = Cost,
                Chests = Chests,
                //game is hard coded to "Maps\\IndoorMap"
                IndoorMap = $"{ID}/{InteriorMapName}",
                IndoorMapType = BaseBuildingType ?? null,
                BuildMaterials = materials,
                MaxOccupants = MaxOccupancy,
                // = BuildLocations,
                HumanDoor = HumanDoor,
                //  TODO: need to get correct dimensions instead of 4,4
                AnimalDoor = new Rectangle(AnimalDoor.X, AnimalDoor.Y, 4, 4),
                IndoorItems = BuildingItems?.Select(p => new IndoorItemAdd { ItemId = p.ItemId, Tile = p.Position, Id = $"{Name}.{p.Id}", Indestructible = p.Indestructible }).ToList() ?? null,
                //Skins = new List<BuildingSkin>
                //{
                //    new BuildingSkin
                //    {
                //         BuildCost=Cost,
                //          BuildDays=BuildDays,
                //           BuildMaterials=materials,
                //            Id=$"{Name}.skin.1",
                //             Name="Base Skin",
                //              Texture=$"Buildings/{ID}",
                //              ShowAsSeparateConstructionEntry=true
                //    }
                //}

                //AnimalDoor = AnimalDoor,
                //IndoorMap = ID,
                //IndoorMapType = BaseBuildingType

            };
        }
        public abstract Dictionary<string, object> ExternalReferences { get; set; }
        public abstract GameLocation GetIndoorLocation(Building b, string LocationName);
    }
}
