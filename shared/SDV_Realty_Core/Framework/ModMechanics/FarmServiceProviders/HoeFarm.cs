using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.TerrainFeatures;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using xTile.Layers;
using System;

namespace SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders
{
    /// <summary>
    /// FarmServiceProvider to hoe all available tiles in
    /// an expansion.
    /// </summary>
    internal class HoeFarm : IFarmServiceProvider
    {
        public override string Key => "hoe_farm";
        public override string DisplayValue => I18n.HF_Label();
        public override string TooltTip => I18n.HF_Label_TT();

        public HoeFarm(IModDataService modDataService, IUtilitiesService utilitiesService, ILoggerService logger) : base(modDataService, utilitiesService, logger)
        {
            serviceCharge = 0.1f;
        }

        internal override bool PerformAction(GameLocation location, string[] args, Farmer who, Point pos)
        {
            Picklocation(I18n.HF_Header(), HoeAllDirt, GetQuote);
            return true;
        }
        private string GetQuote(string expansion, int tiles)
        {
            return I18n.HF_Quote((int)(tiles * serviceCharge));
        }
        private int GetTillableTileCount(GameLocation expansion)
        {
            int tilesHoed = 0;
            Layer back = expansion.Map.GetLayer("Back");
            Stopwatch timer = new();
            // single threading
            //timer.Start();
            //for(int x=0; x< back.DisplayHeight; x++)
            //{
            //    for(int y=0; y< back.DisplayWidth; y++)
            //    {
            //        if (!IsOccupied(expansion, new Vector2(x, y)) && IsTillable(expansion, new Vector2(x, y)))
            //        {
            //            Interlocked.Add(ref tilesHoed, 1);
            //        }
            //    }
            //}
            //timer.Stop();
            //loggerService.Log($"Normal for: {timer.ElapsedMilliseconds:N0}", LogLevel.Debug);
            // parallel processing
            timer.Start();
            //tilesHoed = 0;
            Parallel.For(0, back.DisplayHeight, x =>
            {
                Parallel.For(0, back.DisplayWidth, y =>
                   {
                       if (!IsOccupied(expansion, new Vector2(x, y)) && IsTillable(expansion, new Vector2(x, y)))
                       {
                           Interlocked.Add(ref tilesHoed, 1);
                       }
                   });
            });
            timer.Stop();
            loggerService.Log($"Parallel for: {timer.ElapsedMilliseconds:N0}", LogLevel.Debug);

            return tilesHoed;
        }
        private void HoeAllDirt(string expansionName)
        {
            GameLocation? selectedFarm = null;
            if (expansionName == "Farm")
                selectedFarm = Game1.getFarm();
            else if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation? expansion))
                selectedFarm = expansion;

            if (selectedFarm != null)
            {
                int tillable = GetTillableTileCount(selectedFarm);
                if (tillable * serviceCharge > Game1.player.Money)
                {
                    utilitiesService.PopMessage(I18n.NoMoney(), IUtilitiesService.PopupType.Dialogue);
                }
                else
                {
                    Layer back = selectedFarm.Map.GetLayer("Back");
                    if (back != null)
                    {
                        int tilesHoed = 0;
                        for (int x = 0; x < back.DisplayHeight; x++)
                        {
                            for (int y = 0; y < back.DisplayWidth; y++)
                            {
                                if (!IsOccupied(selectedFarm, new Vector2(x, y)) && IsTillable(selectedFarm, new Vector2(x, y)))
                                {
                                    try
                                    {
                                        selectedFarm.terrainFeatures.Add(new Vector2(x, y), new HoeDirt());
                                        tilesHoed++;
                                    }
                                    catch (Exception ex)
                                    {
                                        loggerService.Log($"Error hoeing dirt {x},{y}, Occupied={IsOccupied(selectedFarm, new Vector2(x, y))}, IsTillable={IsTillable(selectedFarm, new Vector2(x, y))}");
                                        loggerService.LogError(ex);
                                    }
                                }
                            }
                        }
                        int totalCost = (int)(tilesHoed * serviceCharge);
                        utilitiesService.PopMessage(I18n.HF_Total(tilesHoed, totalCost), IUtilitiesService.PopupType.Dialogue);
                        Game1.player.Money -= totalCost;
                    }
                    else
                        loggerService.Log($"Location: {expansionName} has no 'Back' layer.", LogLevel.Error);
                }
            }
            else
            {
                loggerService.Log($"Unknown location to hoe ({expansionName})", LogLevel.Error);
            }
        }
    }
}
