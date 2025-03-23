using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using StardewRealty.SDV_Realty_Interface;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class FishStockService : IFishStockService
    {
        private IExpansionManager _expansionManager;
        private IUtilitiesService _utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IExpansionManager)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            _utilitiesService = (IUtilitiesService)args[0];
            _expansionManager= (IExpansionManager)args[1];

            _utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), DayStarted);
            _utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), SaveLoaded);
        }
        internal void SaveLoaded(EventArgs e)
        {
            SetFishAreaStocks();
        }
        private void DayStarted(EventArgs e)
        {
            if (!Game1.IsMasterGame && Game1.dayOfMonth % 7 == 0)
            {
                //
                //  reset fish stocks at the beginning of the week
                //  for the current season
                //
                logger.Log("   resetting fish stocks", LogLevel.Debug);

                ResetFishSeasonStock(Game1.currentSeason);
                SetFishAreaStocks();

            }
        }
        internal override void ResetFishSeasonStock(string season = null)
        {
            //
            //  clears out previously auto added fish stock
            //  from Expansion fishareas
            //
            foreach (KeyValuePair<string, ExpansionDetails> exp in _expansionManager.expansionManager.ExpDetails)
            {
                foreach (KeyValuePair<string, FishAreaDetails> area in exp.Value.FishAreas.Where(p => p.Value.AutoFill))
                {
                    if (string.IsNullOrEmpty(season))
                    {
                        area.Value.StockData.RemoveAll(p => p.AutoAdded);
                    }
                    else
                    {
                        area.Value.StockData.RemoveAll(p => p.AutoAdded && p.Season.Equals(season, StringComparison.CurrentCultureIgnoreCase));
                    }
                }
            }
        }
        internal override void SetSeasonalStock(string season, FishAreaDetails area, int maxPrice, List<string> newFish, Dictionary<string, int> prices)
        {
            if (area.AutoFill)
            {
                int fishcount = area.StockData.Where(p => p.Season.Equals(season, StringComparison.CurrentCultureIgnoreCase)).Count();
                if (fishcount < area.MaxFishTypes)
                {
                    Random rnd = new Random();
                    for (int i = 0; i < area.MaxFishTypes - fishcount; i++)
                    {
                        string fishId;

                        if (newFish.Count() == 0)
                        {
                            fishId = prices.Keys.ToArray()[rnd.Next(0, prices.Keys.Count() - 1)];
                        }
                        else
                        {
                            fishId = newFish[rnd.Next(0, newFish.Count() - 1)];
                        }
                        area.StockData.Add(new FishStockData
                        {
                            Chance = (float)Math.Abs((maxPrice * .8 - prices[fishId.ToString()])) / maxPrice,
                            FishId = fishId,
                            Season = season,
                            AutoAdded = true
                        });
                        newFish.Remove(fishId);
                    }
                }
            }
        }

        internal override void SetFishAreaStocks()
        {
            //
            //  fill fish stock in areas with autofill enabled
            //
            //  get all possible fish

            Dictionary<string, int> allFish = Game1.objectData.Where(p => p.Value.Category == -4).ToDictionary(p => "(O)" + p.Key, p => p.Value.Price);

            logger.Log($"FillFishAreas: allFish={allFish.Count}", LogLevel.Debug);
            //  get all fish currently defined in Expansions
            List<string> stockedFish = _expansionManager.expansionManager.ExpDetails.SelectMany(p => p.Value.FishAreas.SelectMany(p => p.Value.StockData.Select(p => p.FishId))).Distinct().ToList();
            logger.Log($"FillFishAreas: stockedFish={stockedFish.Count}", LogLevel.Debug);
            //  get max fish price
            int maxPrice = allFish.Select(p => p.Value).Max();

            //  remove the already stocked fish from the big list to make the candidate list
            List<string> newFish = allFish.Keys.Except(stockedFish).ToList();
            logger.Log($"FillFishAreas: newFish={newFish.Count()}", LogLevel.Debug);
            Random rnd = new Random(Game1.dayOfMonth + Game1.ticks);
            foreach (KeyValuePair<string, ExpansionDetails> exp in _expansionManager.expansionManager.ExpDetails)
            {
                foreach (KeyValuePair<string, FishAreaDetails> area in exp.Value.FishAreas.Where(p => p.Value.AutoFill))
                {
                    SetSeasonalStock("Spring", area.Value, maxPrice, newFish, allFish);
                    SetSeasonalStock("Summer", area.Value, maxPrice, newFish, allFish);
                    SetSeasonalStock("Fall", area.Value, maxPrice, newFish, allFish);
                    SetSeasonalStock("Winter", area.Value, maxPrice, newFish, allFish);
                }
            }
            _utilitiesService.InvalidateCache("Data/Locations");
        }

    }
}
