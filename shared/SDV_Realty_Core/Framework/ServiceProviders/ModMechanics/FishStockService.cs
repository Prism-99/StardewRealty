using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using StardewRealty.SDV_Realty_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.MessageBox;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Locations;
using StardewValley.TokenizableStrings;
using StardewValley.ItemTypeDefinitions;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class FishStockService : IFishStockService
    {
        private IExpansionManager _expansionManager;
        private IUtilitiesService _utilitiesService;
        private IModDataService modDataService;
        private SDVMessageBox? box = null;
        private FishAreaData? selectedArea = null;
        private string? selectedAreaId = null;
        private GameLocation? selectedLocation = null;
        private Rectangle displayBox = new Rectangle(40, 40, 800, 600);
        private int currentListItemOffset = 0;
        private long scrollTicks = 0;
        private int scrollDir = 1;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IExpansionManager),
            typeof(IModDataService)
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
            _expansionManager = (IExpansionManager)args[1];
            modDataService = (IModDataService)args[2];

            _utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), HandleDayStarted);
            _utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), HandleSaveLoaded);
            _utilitiesService.GameEventsService.AddSubscription("ButtonPressedEventArgs", HandleButtonPressed);
        }
        internal void HandleSaveLoaded(EventArgs e)
        {
            SetFishAreaStocks();
        }
        private void CloseInfoBox()
        {
            box?.CloseBox();
            box = null;

            selectedArea = null;
            selectedAreaId = null;
            selectedLocation = null;
        }
        /// <summary>
        /// If enabled, display the details of the currently
        /// in focus fishing area
        /// </summary>
        /// <param name="e"></param>
        private void HandleButtonPressed(EventArgs e)
        {
            if (modDataService.Config.ShowFishAreaDetails)
            {
                ButtonPressedEventArgs buttonArgs = (ButtonPressedEventArgs)e;
                switch (buttonArgs.Button)
                {
                    case SButton.Escape:
                        if (box != null)
                        {
                            CloseInfoBox();
                        }
                        break;
                    case SButton.MouseLeft:
                        if (box != null && box.BoundingBox.Contains(buttonArgs.Cursor.ScreenPixels.X, buttonArgs.Cursor.ScreenPixels.Y))
                        {
                            CloseInfoBox();
                        }
                        break;
                    case SButton.MouseRight:
                        if (box != null)
                        {
                            if (box.BoundingBox.Contains(buttonArgs.Cursor.ScreenPixels.X, buttonArgs.Cursor.ScreenPixels.Y))
                            {
                                CloseInfoBox();
                                return;
                            }
                            CloseInfoBox();
                        }
                        if (Game1.currentLocation is FarmExpansionLocation exp)
                        {
                            foreach (var fisharea in exp.GetData().FishAreas)
                            {
                                if (fisharea.Value.Position.Value.Contains(buttonArgs.Cursor.Tile.X, buttonArgs.Cursor.Tile.Y))
                                {
                                    selectedArea = fisharea.Value;
                                    selectedAreaId = fisharea.Key;
                                    selectedLocation = exp;
                                    currentListItemOffset = 0;
                                    logger.Log($"Should show info about: {fisharea.Key}", LogLevel.Debug);
                                    box = new SDVMessageBox(_utilitiesService, RenderAreaDetails, displayBox.X, displayBox.Y, displayBox.Width, displayBox.Height);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
        }
        private void RenderAreaDetails(SpriteBatch b, int x, int y, int width, int height)
        {
            if (selectedArea != null)
            {
                int rowHeight = 35;
                int displayRow = 0;
                int secondColumn = 0;

                foreach (Season season in Enum.GetValues(typeof(Season)))
                {
                    if (Game1.dialogueFont.MeasureString(season.ToString()).X > secondColumn)
                        secondColumn = (int)Game1.dialogueFont.MeasureString(season.ToString()).X;
                }
                b.DrawString(Game1.dialogueFont, selectedArea?.DisplayName??selectedAreaId, new Vector2(x + 10, y + 10), Color.Black);
                displayRow++;

                var fishes = selectedLocation.GetData().Fish.Where(p => p.FishAreaId == selectedAreaId).OrderBy(p=>p?.Season.ToString()??"");// && p.Season==selectedLocation.GetSeason());

                int lineCount = fishes.Count();
                int fishCount = 0;
                foreach (var fish in fishes)
                {
                    if (fishCount >= currentListItemOffset)
                    {
                        ParsedItemData fishObject = ItemRegistry.GetData(fish.ItemId);
                        Rectangle srcRect = fishObject.GetSourceRect();
                        Texture2D texture = Game1.content.Load<Texture2D>(fishObject.TextureName);

                        b.Draw(texture, new Rectangle(x + secondColumn + 20, y + 20 + 10 + displayRow * rowHeight, 40, 30), srcRect, Color.White);
                        b.DrawString(Game1.dialogueFont, fish?.Season.ToString()??"", new Vector2(x + 10, y + 20 + displayRow * rowHeight), Color.Black);

                        string details = $"{TokenParser.ParseText(fishObject.DisplayName)} ({(fish.Chance * 100):N0}%)";

                        if (!string.IsNullOrEmpty(fish.Condition))
                        {
                            details += $" [{fish.Condition}]";
                        }
                        b.DrawString(Game1.dialogueFont, details, new Vector2(x + secondColumn + 65, y + 20 + displayRow * rowHeight), Color.Black);

                        displayRow++;
                        if (20 + (displayRow + 1) * rowHeight > displayBox.Height)
                        {
                            //b.DrawString(Game1.dialogueFont, "...", new Vector2(x + 15, y + 20 + displayRow * rowHeight), Color.Black);
                            break;
                        }
                    }
                    fishCount++;
                }
                if (lineCount > 15)
                {
                    if (scrollTicks == 0)
                        scrollTicks = Game1.ticks;
                    else
                    {
                        if (Game1.ticks - scrollTicks > 50)
                        {
                            currentListItemOffset += scrollDir;
                            scrollTicks = Game1.ticks;
                            if (currentListItemOffset <= 0)
                            {
                                scrollDir = 1;
                                currentListItemOffset = 0;
                            }
                            else if (currentListItemOffset > lineCount-17)
                            {
                                scrollDir = -1;
                                currentListItemOffset--;
                            }
                        }
                    }
                }
            }
        }
        private void HandleDayStarted(EventArgs e)
        {
            if (!Game1.IsMasterGame && Game1.dayOfMonth % 7 == 0)
            {
                //
                //  reset fish stocks at the beginning of the week
                //  for the current season
                //
                logger.Log("FishStockService resetting fish stocks", LogLevel.Debug, 1);

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
#if DEBUG
                logger.Log($"Setting seasonal stock for {area.DisplayName} ({area.Id}), season {season}, newFish: {newFish.Count}, prices: {prices.Count}", LogLevel.Debug);
#endif
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

            //  get all fish currently defined in Expansions
            List<string> stockedFish = _expansionManager.expansionManager.ExpDetails.SelectMany(p => p.Value.FishAreas.SelectMany(p => p.Value.StockData.Select(p => p.FishId))).Distinct().ToList();
            //  get max fish price
            int maxPrice = allFish.Select(p => p.Value).Max();

            //  remove the already stocked fish from the big list to make the candidate list
            List<string> newFish = allFish.Keys.Except(stockedFish).ToList();
#if DEBUG
            logger.Log($"FillFishAreas: allFish={allFish.Count}", LogLevel.Debug);
            logger.Log($"FillFishAreas: stockedFish={stockedFish.Count}", LogLevel.Debug);
            logger.Log($"FillFishAreas: newFish={newFish.Count()}", LogLevel.Debug);
#endif
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
                if (modDataService.farmExpansions[exp.Key].Active)
                    _expansionManager.expansionManager.AddLocationDataDefinitionToCache(modDataService.farmExpansions[exp.Key]);
            }
            _utilitiesService.InvalidateCache("Data/Locations", "Setting fish area stocks");
        }

    }
}
