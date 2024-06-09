using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.ModFixes
{
    internal class SmallerFishPonds : IModFix
    {
        private ILoggerService logger;
        public override void ApplyFixes(IModHelper helper,IMonitor monitor, ILoggerService logger)
        {
            this.logger = logger;
            try
            {
                //
                //  brought routines local to fix bug in
                //  original code
                //
                RecreateAllPonds(false);               
            }
            catch (Exception ex)
            {
                logger.Log($"Error applying SmallerFishPond pre-save fixes", LogLevel.Error);
                logger.LogError(ex);
            }
        }
        private void RecreateAllPonds(bool smallSize)
        {
            foreach (GameLocation location in Game1.locations)
            {
                RecreatePondsAt(location, location.buildings, smallSize);
            }
        }
        private void RecreatePondsAt(GameLocation location, IEnumerable<Building> buildings, bool smallSize)
        {
            List<Vector2> tilesWithPonds = new List<Vector2>();
            Type smallerFishPondType = Type.GetType("SmallerFishPondsSpace.SmallerFishPond, SmallerFishPonds");
            foreach (Building building in buildings)
            {
                // bug fix: last OR condition was incorrect, did not produce the required logic
                if (building.buildingType.Value == "Fish Pond" && (!smallSize || !( building.GetType().Name== smallerFishPondType.Name)) && (smallSize || (building is FishPond)))
                {
                    tilesWithPonds.Add(new Vector2(building.tileX.Value, building.tileY.Value));
                }
            }
            foreach (Vector2 tile in tilesWithPonds)
            {
                if (smallSize)
                {
                    RecreateAsSmallerPond(location, tile);
                }
                else
                {
                    RecreateAsNormalPond(location, tile);
                }
            }
        }
        private void RecreateAsSmallerPond(GameLocation location, Vector2 pondTile)
        {
            //base.Monitor.Log($"Converting Pond at {pondTile} in {location} to smaller 3x3 size.");
            //Building oldBuilding = location.getBuildingAt(pondTile);
            //SmallerFishPond newPond = new SmallerFishPond(Vector2.Zero);
            //ReplacePondData(oldBuilding, newPond);
            //newPond.tilesWide.Value = 3;
            //newPond.tilesHigh.Value = 3;
            //location.destroyStructure(oldBuilding);
            //location.buildStructure(newPond, pondTile, Game1.player, skipSafetyChecks: true);
            //newPond.performActionOnBuildingPlacement();
            //newPond.resetTexture();
        }
        private void ReplacePondData(Building fromBuilding, FishPond toPond)
        {
            logger.Log("Copying basic Building data.",LogLevel.Debug);
            toPond.daysOfConstructionLeft.Value = fromBuilding.daysOfConstructionLeft.Value;
            toPond.daysUntilUpgrade.Value = fromBuilding.daysUntilUpgrade.Value;
            toPond.owner.Value = fromBuilding.owner.Value;
            toPond.currentOccupants.Value = fromBuilding.currentOccupants.Value;
            toPond.maxOccupants.Value = fromBuilding.maxOccupants.Value;
            if (fromBuilding is FishPond fromPond)
            {
                logger.Log("Copying detailed Fish Pond data.",LogLevel.Debug);
                toPond.fishType.Value = fromPond.fishType.Value;
                toPond.lastUnlockedPopulationGate.Value = fromPond.lastUnlockedPopulationGate.Value;
                toPond.hasCompletedRequest.Value = fromPond.hasCompletedRequest.Value;
                toPond.goldenAnimalCracker.Value = fromPond.goldenAnimalCracker.Value;
                toPond.sign.Value = fromPond.sign.Value;
                toPond.overrideWaterColor.Value = fromPond.overrideWaterColor.Value;
                toPond.output.Value = fromPond.output.Value;
                toPond.neededItemCount.Value = fromPond.neededItemCount.Value;
                toPond.neededItem.Value = fromPond.neededItem.Value;
                toPond.daysSinceSpawn.Value = fromPond.daysSinceSpawn.Value;
                toPond.nettingStyle.Value = fromPond.nettingStyle.Value;
                toPond.seedOffset.Value = fromPond.seedOffset.Value;
                toPond.hasSpawnedFish.Value = fromPond.hasSpawnedFish.Value;
                //  bug fix: codpy modData over
                toPond.modData.Clear();
                foreach(var mData in fromPond.modData.Keys)
                {
                    toPond.modData.Add(mData, fromPond.modData[mData]);
                }
            }
        }
        private void RecreateAsNormalPond(GameLocation location, Vector2 pondTile)
        {
            logger.Log($"Converting Pond at {pondTile} in {location} to normal 5x5 size.",LogLevel.Debug);
            Building oldBuilding = location.getBuildingAt(pondTile);
            FishPond newPond = new FishPond(Vector2.Zero);
            ReplacePondData(oldBuilding, newPond);
            //if (Config.KeepSmallSizeOnSave)
            //{
            //    newPond.tilesWide.Value = 3;
            //    newPond.tilesHigh.Value = 3;
            //}
            //else
            //{
                newPond.tilesWide.Value = 5;
                newPond.tilesHigh.Value = 5;
            //}
            location.destroyStructure(oldBuilding);
            location.buildStructure(newPond, pondTile, Game1.player, skipSafetyChecks: true);
            newPond.performActionOnBuildingPlacement();
            newPond.UpdateMaximumOccupancy();
        }
        public override bool ShouldApply(IModHelper helper)
        {
            return helper.ModRegistry.IsLoaded("platinummyr.SmallerFishPonds");
        }
    }
}
