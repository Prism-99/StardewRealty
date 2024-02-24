using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Buildings;
using StardewValley.Menus;
using SDV_Realty_Core.Framework.Utilities;
using StardewValley.TerrainFeatures;

namespace Framework.StardewObjects
{
    class CustomJunimoHut:JunimoHut
    {
        public override void updateWhenFarmNotCurrentLocation(GameTime time)
        {
            //framework.monitor.Log("Running FEJunimoHut update", StardewModdingAPI.LogLevel.Info);

            Farm currentFarm = Game1.getLocationFromName(modData[FEModDataKeys.FELocationName]) as Farm;


            GetOutputChest().mutex.Update(currentFarm);
            if (GetOutputChest().mutex.IsLockHeld() && Game1.activeClickableMenu == null)
            {
                GetOutputChest().mutex.ReleaseLock();
            }

            int junimoSendOutTimer = 1;

            if (Game1.IsMasterGame && junimoSendOutTimer > 0)
            {
                junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
                if (junimoSendOutTimer <= 0 && myJunimos.Count() < 3 && !Game1.IsWinter && !Game1.isRaining && areThereMatureCropsWithinRadius(currentFarm) && Game1.farmEvent == null)
                {
                    int junimoNumber = getUnusedJunimoNumber();
                    bool isPrismatic = false;
                    Color? gemColor = getGemColor(ref isPrismatic);
                     junimoSendOutTimer = 1000;
                    if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2((float)(tileX.Value + 1), (float)(tileY.Value + 1))), 64, currentFarm))
                    {
                        try
                        {
                            currentFarm.playSound("junimoMeep1");
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

           
        }
        private  bool areThereMatureCropsWithinRadius(Farm farm)
        {
            for (int i = tileX.Value + 1 - 8; i < tileX.Value + 2 + 8; i++)
            {
                for (int j = tileY.Value - 8 + 1; j < tileY.Value + 2 + 8; j++)
                {
                    if (farm.isCropAtTile(i, j) && (farm.terrainFeatures[new Vector2(i, j)] as HoeDirt).readyForHarvest())
                    {
                        lastKnownCropLocation = new Point(i, j);
                        return true;
                    }

                    if (farm.terrainFeatures.ContainsKey(new Vector2(i, j)) && farm.terrainFeatures[new Vector2(i, j)] is Bush && (farm.terrainFeatures[new Vector2(i, j)] as Bush).tileSheetOffset.Value == 1)
                    {
                        lastKnownCropLocation = new Point(i, j);
                        return true;
                    }
                }
            }

            lastKnownCropLocation = Point.Zero;
            return false;
        }

        private Color? getGemColor(ref bool isPrismatic)
        {
            List<Color> gemColors = new List<Color>();

            foreach (Item item in GetOutputChest().Items)
            {
                if (item != null && (item.Category == -12 || item.Category == -2))
                {
                    Color? gemColor = TailoringMenu.GetDyeColor(item);
                    if (item.Name == "Prismatic Shard")
                    {
                        isPrismatic = true;
                    }
                    if (gemColor.HasValue)
                    {
                        gemColors.Add(gemColor.Value);
                    }
                }
            }
            if (gemColors.Count > 0)
            {
                return gemColors[Game1.random.Next(gemColors.Count)];
            }
            return null;
        }
    }
}
