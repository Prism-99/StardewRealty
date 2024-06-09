using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using StardewRealty.SDV_Realty_Interface;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using xTile;
using xTile.Layers;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class TreasureManagerService : ITreasureManager
    {
        private List<string> expansionsWithTreasures = new List<string>();
        private List<string> activeTreasureExpansions = new List<string>();
        private IModDataService modDataService;
        private Random rnd = new Random(DateTime.Now.Millisecond);
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService)
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

            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            modDataService = (IModDataService)args[1];

            //  add save loaded hook to cache expansions
            utilitiesService.GameEventsService.AddSubscription("SaveLoadedEventArgs", SaveLoaded);
            // add day started hook to look for seeding days (1,8,15,22)
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), DayStarted);
            // add expansion activated hook
            utilitiesService.CustomEventsService.AddModEventSubscription(ICustomEventsService.ModEvents.ExpansionActivated, ExpansionActivated);
        }
        #region "Event Handlers"
        /// <summary>
        /// Check if activated Expansion needs to be added to the cache
        /// </summary>
        /// <param name="args"></param>
        private void ExpansionActivated(object[] args)
        {
            if (Game1.IsMasterGame)
            {
                if (!activeTreasureExpansions.Contains(args[0].ToString()))
                {
                    activeTreasureExpansions.Add(args[0].ToString());
                    AddLocationTreasures(args[0].ToString());
                }
            }
        }
        /// <summary>
        /// Check for the beginning of the week to seed treasures
        /// </summary>
        /// <param name="e"></param>
        private void DayStarted(EventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                if (Game1.dayOfMonth == 1 || (Game1.dayOfMonth - 1) % 7 == 0)
                {
                    foreach (string expansionName in expansionsWithTreasures)
                    {
                        AddLocationTreasures(expansionName);
                    }
                }
            }
        }
        /// <summary>
        /// Cache up expansions with Treasure locations
        /// </summary>
        /// <param name="e"></param>
        private void SaveLoaded(EventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                expansionsWithTreasures = new();
                activeTreasureExpansions = new();

                foreach (ExpansionDetails expansion in modDataService.expDetails.Values)
                {
                    if (expansion.TreasureSpots != null && expansion.TreasureSpots.Count > 0)
                    {
                        expansionsWithTreasures.Add(expansion.LocationName);
                    }
                }
            }
        }

        #endregion

        #region "Internal Methods"
        /// <summary>
        /// Refresh TreasureSpots
        /// </summary>
        /// <param name="expansionName">Name of expansion refresh treasures</param>
        private void AddLocationTreasures(string expansionName)
        {
#if DEBUG
            logger.LogDebug($"AddLocationTreasures called for {expansionName}");
#endif
            foreach (var treasurespot in modDataService.expDetails[expansionName].TreasureSpots)
            {
                //
                //  check conditions of items
                //
                List<ExpansionDetails.TreasureAreaItem> potentailItems = new();
                foreach (ExpansionDetails.TreasureAreaItem treasureItem in treasurespot.Value.Items)
                {
                    if (string.IsNullOrEmpty(treasureItem.Condition))
                    {
                        potentailItems.Add(treasureItem);
                    }
                    else
                    {
                        if (GameStateQuery.CheckConditions(treasureItem.Condition))
                        {
                            potentailItems.Add(treasureItem);
                        }
                    }
                }


                if (potentailItems.Count > 0)
                {
                    ExpansionDetails.TreasureAreaItem selectedItem;
                    int currentCount = GetTreasureSpotCount(modDataService.ExpansionMaps[expansionName], treasurespot.Value.Area);

                    if (currentCount > -1 && currentCount < treasurespot.Value.MaxItems)
                    {
                        int maxTries = treasurespot.Value.MaxItems * 2;
                        int tries = 0;
                        Layer backLayer = modDataService.ExpansionMaps[expansionName].GetLayer("Back");
                        if (backLayer != null)
                        {
                            while (currentCount < treasurespot.Value.MaxItems && tries < maxTries)
                            {
                                switch (potentailItems.Count)
                                {
                                    case 1:
                                        selectedItem = potentailItems[0];
                                        break;
                                    default:
                                        selectedItem = potentailItems[rnd.Next(potentailItems.Count)];
                                        break;
                                }

                                int x = rnd.Next(treasurespot.Value.Area.Width);
                                int y = rnd.Next(treasurespot.Value.Area.Height);
                                if (backLayer.Tiles[treasurespot.Value.Area.X + x, treasurespot.Value.Area.Y + y] != null)
                                {
                                    //if(backLayer.Tiles[treasurespot.Key.X + x, treasurespot.Key.Y + y].Properties.ContainsKey("Treasure"))
                                    int quantity=rnd.Next(selectedItem.Quantity)+1;
                                    string treasureId = selectedItem.ItemId + " " + quantity.ToString();
                                    backLayer.Tiles[treasurespot.Value.Area.X + x, treasurespot.Value.Area.Y + y].Properties["Treasure"] = treasureId;
#if DEBUG
                                    logger.LogDebug($"Treasure added: {expansionName} ({treasurespot.Value.Area.X + x},{treasurespot.Value.Area.Y + y}) {treasureId}");
#endif
                                    currentCount++;
                                }

                                tries++;
                            }
                        }
                    }
                }
            }

        }

        private int GetTreasureSpotCount(Map locationMap, Rectangle searchArea)
        {
            int count = -1;

            Layer backLayer = locationMap.GetLayer("Back");

            if (backLayer != null)
            {
                count = 0;
                for (int x = searchArea.X; x <= searchArea.Width; x++)
                {
                    for (int y = searchArea.Y; y <= searchArea.Height; y++)
                    {
                        if (backLayer.Tiles[x, y] != null)
                        {
                            if (backLayer.Tiles[x, y].Properties.ContainsKey("Treasure") && backLayer.Tiles[x, y].Properties["Treasure"] != null)
                                count++;
                        }
                    }
                }
            }

            return count;
        }

        #endregion

    }
}
