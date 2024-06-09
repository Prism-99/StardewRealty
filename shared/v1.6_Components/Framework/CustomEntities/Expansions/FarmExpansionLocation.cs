using System.Collections.Generic;
using Prism99_Core.Utilities;
using StardewValley.Buildings;
using xTile;
using System.Xml.Serialization;
using StardewRealty.SDV_Realty_Interface;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;
using System.Linq;
using SDV_Realty_Core.Framework.Buildings.CheeseFactory;
using SDV_Realty_Core.Framework.Buildings.Greenhouses;

namespace SDV_Realty_Core.Framework.Expansions
{
    [XmlInclude(typeof(FromagerieLocation))]
    //[XmlInclude(typeof(FromagerieBuilding))]
    //[XmlInclude(typeof(WineryBuilding))]
    //[XmlInclude(typeof(SDRGreenhouse))]
    [XmlInclude(typeof(GreenhouseLocation))]
    [XmlInclude(typeof(GreenhouseInterior))]
    [XmlInclude(typeof(LargeGreenhouseLocation))]
    [XmlInclude(typeof(LargeGreenhouseInterior))]
    //[XmlInclude(typeof(BreadFactoryBuilding))]
    //[XmlInclude(typeof(BreadFactoryLocation))]
    public class FarmExpansionLocation : GameLocation
    {
        //
        //  version 1.6
        //
        private static SDVLogger logger;
        private ExpansionManager expansionManager;
        public FarmExpansionLocation() { }
        //
        //  cannot use a game loader always appends .xnb to filename
        //
        //internal FarmExpansionLocation(string mapPath, string a, SDVLogger olog) : base(mapPath,a)
        //{
        //    logger = olog;
        //    //this.Map = omap;
        //    //this.name.Value = name;
        //    AutoAdd = true;
        //    GridId = -1;
        //}
        internal FarmExpansionLocation(Map omap, string mapPathValue, string locationName, SDVLogger olog, ExpansionManager expansionManager) : base()
        {
            logger = olog;
            Map = omap;
            mapPath.Value = mapPathValue;
            name.Value = locationName;
            AutoAdd = true;
            GridId = -1;
            this.expansionManager=expansionManager;
        }

        [XmlIgnoreAttribute]
        public Dictionary<string, FishAreaDetails> FishAreas;
        public bool Active { get; set; }
        public bool BaseBuildingsAdded { get; set; }
        public long PurchasedBy { get; set; }
        public int GridId { get; set; } = -1;
        public bool AutoAdd { get; set; }
        public bool OfferLetterRead { get; set; }
        public string SeasonOverride { get; set; }
        public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

        public bool StockedPonds { get; set; }
        public bool CrowsEnabled { get; set; }

        public override void seasonUpdate(bool onLoad = false)
        {
            Season season = GetSeason();
            terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value.seasonUpdate(onLoad));
            largeTerrainFeatures?.RemoveWhere((LargeTerrainFeature feature) => feature.seasonUpdate(onLoad));
            foreach (NPC character in characters)
            {
                if (!character.IsMonster)
                {
                    character.resetSeasonalDialogue();
                }
            }

            if (IsOutdoors && !onLoad)
            {
                KeyValuePair<Vector2, SDObject>[] array = objects.Pairs.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    KeyValuePair<Vector2, SDObject> keyValuePair = array[i];
                    Vector2 key = keyValuePair.Key;
                    SDObject value = keyValuePair.Value;
                    if (value.IsSpawnedObject && !value.IsBreakableStone())
                    {
                        objects.Remove(key);
                    }
                    else if (value.QualifiedItemId == "(O)590" && doesTileHavePropertyNoNull((int)key.X, (int)key.Y, "Diggable", "Back") == "")
                    {
                        objects.Remove(key);
                    }
                }

                numberOfSpawnedObjectsOnMap = 0;
            }

            switch (season)
            {
                case Season.Spring:
                    waterColor.Value = new Color(120, 200, 255) * 0.5f;
                    break;
                case Season.Summer:
                    waterColor.Value = new Color(60, 240, 255) * 0.5f;
                    break;
                case Season.Fall:
                    waterColor.Value = new Color(255, 130, 200) * 0.5f;
                    break;
                case Season.Winter:
                    waterColor.Value = new Color(130, 80, 255) * 0.5f;
                    break;
            }

            if (!onLoad && season == Season.Spring && Game1.stats.DaysPlayed > 1)
            {
                loadWeeds();
            }
        }
              
        private string GetLocContextId()
        {
            if (locationContextId == null)
            {
                if (map == null)
                {
                    reloadMap();
                }
                if (map != null && map.Properties.TryGetValue("LocationContext", out var contextId))
                {
                    if (Game1.locationContextData.ContainsKey(contextId))
                    {
                        locationContextId = contextId;
                    }
                    else
                    {
                        logger.Log($"Could not find context: {contextId}", StardewModdingAPI.LogLevel.Debug);
                    }
                }
                if (locationContextId == null)
                {
                    locationContextId = GetParentLocation()?.GetLocationContextId() ?? "Default";
                }
            }
            return locationContextId;
        }
        public override bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
        {
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                if (suspensionBridge.CheckPlacementPrevention(tileLocation))
                {
                    return true;
                }
            }
            return base.IsLocationSpecificPlacementRestriction(tileLocation);
        }
        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                suspensionBridge.Update(time);
            }            
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                suspensionBridge.Draw(b);
            }
        }
      
        public void FixAnimals()
        {
            foreach (Building building in buildings)
            {
                FixBuildingAnimals(building);
            }
        }

        public void FixBuildingAnimals(Building building)
        {

            if (building.indoors?.Value == null || building.indoors.Value is not AnimalHouse)
            {
                return;
            }

            foreach (long item in (building.indoors.Value as AnimalHouse).animalsThatLiveHere)
            {
                FarmAnimal animal = getAnimal(item);
                if (animal != null)
                {
                    animal.home = building;

                    animal.reload(building);
                    animal.setTileLocation(new Vector2(building.tileX.Value, building.tileY.Value));
                    if (!animal.IsHome)
                    {
                        animal.warpHome();
                    }

                }
            }

        }
        public FarmAnimal getAnimal(long id)
        {
            if (animals.ContainsKey(id))
            {
                return animals[id];
            }

            foreach (Building building in buildings)
            {
                if (building.indoors.Value is AnimalHouse && (building?.indoors.Value as AnimalHouse).animals.ContainsKey(id))
                {
                    return (building?.indoors.Value as AnimalHouse).animals[id];
                }
            }

            return null;
        }
  
        public void LoadBuildings(bool addToBaseFarm)
        {
            foreach (Building building in buildings)
            {
                //if load is not called, building interior map
                //is not loaded
                building.load();
                expansionManager.FixBuildingWarps(building, Name);
                //
                // 2024-01-10 should not be needed
                //
                //FixBuildingAnimals(building);

                if (building.indoors.Value != null)
                {
                    //
                    //  2024-01-10 fix to move seasonOverride to animal house indoors
                    //
                    //if (!string.IsNullOrEmpty(SeasonOverride))
                    //{
                    //    if (!building.indoors.Value.map.Properties.ContainsKey("SeasonOverride"))
                    //    {
                    //        building.indoors.Value.map.Properties.Add("SeasonOverride", SeasonOverride);
                    //    }
                    //}
                }
            }
        }
    }
}
