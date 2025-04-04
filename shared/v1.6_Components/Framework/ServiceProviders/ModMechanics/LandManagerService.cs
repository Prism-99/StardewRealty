﻿using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomMenuFramework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.Objects;
using StardewModdingAPI;




namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class LandManagerService : ILandManager
    {
        //
        //  sales tracking
        //
        private bool NewLandMessageSentToday;

        private IContentPackService contentPackService;
        private ContentPackLoader contentPackLoader;
        private IUtilitiesService utilitiesService;
        //private IMultiplayerService multiplayerService;
        private IModDataService modDataService;
        private IGridManager gridManager;
        private IPlayerComms playerComms;
        private IContentManagerService contentManagerService;
        private static IForSaleMenuService forSaleMenuService;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentPackService),typeof(IUtilitiesService),
            typeof(IContentManagerService),typeof(IModDataService),
            typeof(IPlayerComms),typeof(IGridManager),
            typeof(IForSaleMenuService),typeof( IChatBoxCommandsService)

        };
        public override List<string> CustomServiceEventTriggers => new List<string>
        {
            "ActivateExpansion","SendFarmHandPurchase"
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
            contentPackService = (IContentPackService)args[0];
            utilitiesService = (IUtilitiesService)args[1];
            contentManagerService = (IContentManagerService)args[2];
            modDataService = (IModDataService)args[3];
            playerComms = (IPlayerComms)args[4];
            gridManager = (IGridManager)args[5];
            forSaleMenuService = (IForSaleMenuService)args[6];
            IChatBoxCommandsService chatBoxCommandsService = (IChatBoxCommandsService)args[7];

            chatBoxCommandsService.AddCustomCommand("buymenu", HandleChatBox);
            contentPackLoader = contentPackService.contentPackLoader;
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), DayStarted);
            utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), LoadStageChanged, 10);
            utilitiesService.CustomEventsService.AddModEventSubscription(ICustomEventsService.ModEvents.LandPurchased, HandleLandPurchasedEvent);
            GameLocation.RegisterTileAction(ModKeyStrings.Action_RealEstateOffice, HandleTouchAction);
        }
        private void HandleChatBox(string[] args)
        {
            PopBuyingMenu();
        }
        private void HandleLandPurchasedEvent(object[] args)
        {
            if (args.Length == 3)
            {
                if (args[1] is bool popup && args[2] is Int64 buyer)
                {
                    PurchaseLand(args[0].ToString(), popup, buyer);
                }
            }
        }
        private bool HandleTouchAction(GameLocation location, string[] args, Farmer who, Point pos)
        {
            if (!modDataService.farmExpansions.Where(p => p.Value.Active).Any())
            {
                PopBuyingMenu();
            }
            else
            {
                List<KeyValuePair<string, string>> choices = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("buying",I18n.LandManagerBuying()),
                    new KeyValuePair<string, string>("selling",I18n.LandManagerSelling())
                };
                GenericPickListMenu pickMode = new GenericPickListMenu();
                pickMode.ShowPagedResponses(I18n.LandManagerBuyingSelling(), choices, delegate (string value)
                {
                    if (value == "buying")
                    {
                        PopBuyingMenu();
                    }
                    else if (value == "selling")
                    {
                        PopSellingMenu();
                    }

                }, auto_select_single_choice: true);
            }

            return true;
        }
        private void HandlePopChoice(string value)
        {
            if (value == "buying")
            {
                PopBuyingMenu();
            }
            else if (value == "selling")
            {
                PopSellingMenu();
            }
        }
        internal override void PopBuyingMenu()
        {
            if (modDataService.farmExpansions.Where(p => p.Value.Active).Count() >= modDataService.MaximumExpansions)
            {
                //
                // expansion grid is full
                //
                utilitiesService.PopMessage(I18n.LandManagerMaxExp(), IUtilitiesService.PopupType.Dialogue);
            }
            else if (modDataService.LandForSale.Count == 0)
            {
                //
                //  no land for sale
                //
                utilitiesService.PopMessage(I18n.LandManagerNone());
            }
            else
            {
                //Game1.activeClickableMenu = forSaleMenuService.GetMenu();
                //Game1.activeClickableMenu.exitFunction = ForSaleClosed;

                ForSaleListingsMenu menu = new ForSaleListingsMenu(0, 0, Game1.uiViewport.Width - 20, Game1.uiViewport.Height - 20, LandBought, true, !Game1.options.gamepadControls, false);
                menu.SetGlobalPriceMode(modDataService.Config.globalPriceMode, modDataService.Config.globalPrice);

                List<ExpansionPack> listings = new List<ExpansionPack>();
                foreach (string forsale in modDataService.LandForSale)
                {
                    listings.Add(modDataService.validContents[forsale]);
                }
                menu.SetListings(listings);
                menu.exitFunction = ForSaleClosed;
                Game1.activeClickableMenu = menu;
            }
        }
        internal override void LandSold(string expansionName)
        {
            if (modDataService.farmExpansions.TryGetValue(expansionName, out var selling))
            {
                selling.Active = false;
                if (selling.GridId > -1)
                {
                    gridManager.RemoveLocationFromGrid(selling.GridId);
                    selling.GridId = -1;
                }
                int sellingPrice;
                if (modDataService.expDetails[expansionName].PurchasePrice.HasValue)
                {
                    sellingPrice = modDataService.expDetails[expansionName].PurchasePrice.Value;
                }
                else
                {
                    sellingPrice = GetLandPrice(expansionName);
                }
                Game1.player.Money += sellingPrice;

                modDataService.validContents[selling.Name].Added = false;
                modDataService.validContents[selling.Name].Purchased = false;
                modDataService.validContents[selling.Name].AddedToFarm = false;
                modDataService.expDetails[selling.Name].Active = false;
                modDataService.expDetails[selling.Name].ForSale = false;
                modDataService.expDetails[selling.Name].PurchaseDate = -1;
                modDataService.expDetails[selling.Name].PurchasedBy = -1;
                modDataService.addedLocationTracker.Remove(selling.Name);

                modDataService.CustomMineCartNetworks.Remove(selling.Name);

                utilitiesService.MapUtilities.ResetGameLocation(selling);
                //if (!modDataService.validContents[selling.Name].isAdditionalFarm)
                //    contentPackService.LoadExpansionMap(selling.Name);
                //selling.reloadMap();

                if (!string.IsNullOrEmpty(modDataService.validContents[selling.Name].MailId))
                {
                    // remove sold letter
                    Game1.player.mailReceived.Remove(modDataService.validContents[selling.Name].MailId);
                    Game1.player.mailbox.Remove(modDataService.validContents[selling.Name].MailId);
                }
                AddLandForSale(selling.Name);
                Game1.locations.Remove(Game1.locations.Where(p => p.Name == expansionName).First());
                Game1._locationLookup.Remove(expansionName);

                utilitiesService.CustomEventsService.TriggerModEvent(ICustomEventsService.ModEvents.LandSold, new object[] { selling.Name });
                utilitiesService.PopMessage(I18n.LandManagerLandSold(selling.DisplayName, sellingPrice));
            }
        }
        internal override void PopSellingMenu()
        {
            GenericPickListMenu pickProperty = new GenericPickListMenu();
            List<KeyValuePair<string, string>> locations = modDataService.farmExpansions.Where(p => p.Value.Active).Select(p => new KeyValuePair<string, string>(p.Key, $"{p.Value.DisplayName} ({modDataService.expDetails[p.Key]?.PurchasePrice ?? GetLandPrice(p.Key):N0}g)")).ToList();

            pickProperty.ShowPagedResponses(I18n.LandManagerSelling(), locations, delegate (string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    //
                    //   remove land from active expansions
                    //
                    LandSold(value);
                }
            }, auto_select_single_choice: false);

        }
        private void ForSaleClosed()
        {
            Game1.dialogueUp = false;
            Game1.player.CanMove = true;
        }
        private void LoadStageChanged(EventArgs e)
        {
            LoadStageChangedEventArgs evArgs = (LoadStageChangedEventArgs)e;
            if (evArgs.NewStage == LoadStage.SaveAddedLocations)
            {
                //
                //  called by existing save load
                //
                CheckForExpansionActivation("LoadStageChanged");

            }

        }
        /// <summary>
        ///  Joins the entrance/exit warps of two maps
        ///  removes the path blocks for the entrance from the 2 maps
        /// </summary>
        /// <param name="oExitPatch"></param>
        /// <param name="sLocationNameA"></param>
        /// <param name="oEntrancePath"></param>
        /// <param name="sLocationNameB"></param>
        //private void AddExpansionIntersection(EntrancePatch oExitPatch, string sLocationNameA, EntrancePatch oEntrancePath, string sLocationNameB)
        //{
        //    //
        //    //  get Gameloactions
        //    //
        //    GameLocation glA = Game1.getLocationFromName(sLocationNameA);
        //    if (glA is null)
        //    {
        //        logger?.Log($"AddExpansionIntersection could not location: {sLocationNameA}", LogLevel.Debug);
        //        return;
        //    }
        //    GameLocation glB = Game1.getLocationFromName(sLocationNameB);
        //    if (glB is null)
        //    {
        //        logger?.Log($"AddExpansionIntersection could not location: {sLocationNameB}", LogLevel.Debug);
        //        return;
        //    }
        //    //
        //    //  remove path blocks from each expansion
        //    //
        //    utilitiesService.MapUtilities.RemovePathBlock(new Vector2(oExitPatch.PathBlockX, oExitPatch.PathBlockY), glA, oExitPatch.WarpOrientation, oExitPatch.WarpOut.NumberofPoints);
        //    utilitiesService.MapUtilities.RemovePathBlock(new Vector2(oEntrancePath.PathBlockX, oEntrancePath.PathBlockY), glB, oEntrancePath.WarpOrientation, oEntrancePath.WarpOut.NumberofPoints);
        //    //
        //    //  add the warps between the expansions
        //    //
        //    AddExpansionWarps(oExitPatch, glA, oEntrancePath, glB);
        //}
        /// <summary>
        /// add warps between 2 expansions
        /// Compensates for mis-matches in the size of each destination
        /// </summary>
        /// <param name="oExitPatch"></param>
        /// <param name="glA"></param>
        /// <param name="oEntrancePath"></param>
        /// <param name="glB"></param>
        //public void AddExpansionWarps(EntrancePatch oExitPatch, GameLocation glA, EntrancePatch oEntrancePath, GameLocation glB)
        //{
        //    int numWarps = Math.Max(oExitPatch.WarpOut.NumberofPoints, oEntrancePath.WarpIn.NumberofPoints);

        //    logger.Log($"       AddExpansionWarps. {glA.NameOrUniqueName} to {glB.NameOrUniqueName}", LogLevel.Debug);
        //    logger.Log($"       numwarps: {numWarps}", LogLevel.Debug);
        //    logger.Log($"       exit points: {oExitPatch.WarpOut.NumberofPoints} {oExitPatch.WarpOrientation}", LogLevel.Debug);
        //    logger.Log($"   entrance points: {oEntrancePath.WarpIn.NumberofPoints} {oEntrancePath.WarpOrientation}", LogLevel.Debug);

        //    for (int iWarp = 0; iWarp < numWarps; iWarp++)
        //    {
        //        try
        //        {
        //            //
        //            //  check for patch point underflows
        //            //
        //            int iExitCell = iWarp < oExitPatch.WarpOut.NumberofPoints ? iWarp : oExitPatch.WarpOut.NumberofPoints - 1;
        //            int iEntCell = iWarp < oEntrancePath.WarpIn.NumberofPoints - 1 ? iWarp : oEntrancePath.WarpIn.NumberofPoints - 1;

        //            if (oExitPatch.WarpOrientation == WarpOrientations.Horizontal)
        //            {
        //                //
        //                //  remove existing warp to support swapping expansions
        //                //
        //                var existingWarp = glA.warps.Where(p => p.X == oExitPatch.WarpOut.X + iExitCell && p.Y == oExitPatch.WarpOut.Y);
        //                if (existingWarp.Any())
        //                {
        //                    glA.warps.Remove(glA.warps.Where(p => p.X == oExitPatch.WarpOut.X + iExitCell && p.Y == oExitPatch.WarpOut.Y).First());
        //                }
        //                glA.warps.Add(new Warp(oExitPatch.WarpOut.X + iExitCell, oExitPatch.WarpOut.Y, glB.Name, oEntrancePath.WarpIn.X + iEntCell, oEntrancePath.WarpIn.Y, false));
        //                //
        //                //  remove existing warp to support swapping expansions
        //                //
        //                existingWarp = glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X + iExitCell && p.Y == oEntrancePath.WarpOut.Y);
        //                if (existingWarp.Any())
        //                {
        //                    glB.warps.Remove(glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X + iExitCell && p.Y == oEntrancePath.WarpOut.Y).First());
        //                }
        //                glB.warps.Add(new Warp(oEntrancePath.WarpOut.X + iEntCell, oEntrancePath.WarpOut.Y, glA.Name, oExitPatch.WarpIn.X + iExitCell, oExitPatch.WarpIn.Y, false));
        //            }
        //            else
        //            {
        //                var existingWarp = glA.warps.Where(p => p.X == oExitPatch.WarpOut.X && p.Y == oExitPatch.WarpOut.Y + iExitCell);
        //                if (existingWarp.Any())
        //                {
        //                    glA.warps.Remove(glA.warps.Where(p => p.X == oExitPatch.WarpOut.X && p.Y == oExitPatch.WarpOut.Y + iExitCell).First());
        //                }
        //                glA.warps.Add(new Warp(oExitPatch.WarpOut.X, oExitPatch.WarpOut.Y + iExitCell, glB.Name, oEntrancePath.WarpIn.X, oEntrancePath.WarpIn.Y + iEntCell, false));
        //                existingWarp = glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X && p.Y == oEntrancePath.WarpOut.Y + iEntCell);
        //                if (existingWarp.Any())
        //                {
        //                    glB.warps.Remove(glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X && p.Y == oEntrancePath.WarpOut.Y + iEntCell).First());
        //                }
        //                glB.warps.Add(new Warp(oEntrancePath.WarpOut.X, oEntrancePath.WarpOut.Y + iEntCell, glA.Name, oExitPatch.WarpIn.X, oExitPatch.WarpIn.Y + iExitCell, false));
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError($"AddExpansionWarps", ex);
        //        }
        //    }
        //}

        /// <summary>
        /// Patches an Expansion into its location
        /// </summary>
        /// <param name="iGridId"></param>
        //public void PatchInMap(int iGridId)
        //{
        //    string sExpansionName = gridManager.MapGrid[iGridId];
        //    logger?.Log($"     PatchInMap {iGridId}:{sExpansionName}", LogLevel.Debug);

        //    try
        //    {

        //        GameLocation oExp = modDataService.farmExpansions[sExpansionName];
        //        bool bAutoAdd = modDataService.farmExpansions[sExpansionName].AutoAdd;
        //        ExpansionPack oPackToActivate = contentPackLoader.ValidContents[sExpansionName];
        //        //AddSign(ContentPacks.ValidContents[sExpansionName]);
        //        if (iGridId > -1)
        //        {
        //            //
        //            //  based upon the grid location, remove path blocks and add warps
        //            //  to join new Expansion to its neighbours
        //            //
        //            FarmDetails oFarm = null;
        //            ExpansionPack oNewPack = contentPackLoader.ValidContents[sExpansionName];
        //            EntrancePatch oExpPatch;
        //            int iGridRow;
        //            int iGridCol;
        //            //ExpansionPack oRightPack;
        //            //EntrancePatch oRightExpPatch;
        //            int iPatchIndex;

        //            switch (iGridId)
        //            {
        //                case 0:
        //                    //
        //                    //  initial grid spot
        //                    //  linked off of the Backwoods
        //                    //
        //                    oFarm = utilitiesService.GameEnvironment.GetFarmDetails(999999);

        //                    if (oFarm != null)
        //                    {
        //                        GameLocation glBackwoods = Game1.getLocationFromName(oFarm.MapName);
        //                        EntrancePatch oBackWoodsPatch = oFarm.PathPatches["0"];
        //                        //RemovePathBlock(new Vector2(oFarm.PathPatches["0"].PathBlockX, oFarm.PathPatches["0"].PathBlockY), glBackwoods, oBackWoodsPatch.WarpOrientation, oBackWoodsPatch.WarpOut.NumberofPoints);
        //                        iGridRow = iGridId / oFarm.PathPatches.Count;
        //                        iGridCol = iGridId % oFarm.PathPatches.Count;
        //                        //
        //                        //    add basic left side warp ins/outs
        //                        //
        //                        oExpPatch = oNewPack.EntrancePatches[IMapUtilities.eastSidePatch];
        //                        AddExpansionIntersection(oBackWoodsPatch, oFarm.MapName, oExpPatch, sExpansionName);

        //                        if (oExpPatch.Sign?.UseSign ?? false)
        //                        {
        //                            gridManager.AddSignPost(oExp, oExpPatch, "EastMessage", utilitiesService.MapUtilities.FormatDirectionText(IMapUtilities.eastSidePatch, glBackwoods.Name));
        //                        }
        //                        if (gridManager.MapGrid.ContainsKey(1))
        //                        {
        //                            JoinMaps(iGridId, iGridId + 1, IMapUtilities.westSidePatch);
        //                        }
        //                    }
        //                    break;
        //                case 1:
        //                case 4:
        //                case 7:
        //                    //
        //                    //  top row 
        //                    //
        //                    //
        //                    //  get expansion to right details
        //                    //
        //                    int iRightGridId = iGridId == 1 ? 0 : iGridId - 3;

        //                    JoinMaps(iGridId, iRightGridId, IMapUtilities.eastSidePatch);

        //                    //
        //                    //  check for other neighbours from swapping
        //                    //
        //                    //  check for neighbour to the south
        //                    //
        //                    if (gridManager.MapGrid.ContainsKey(iGridId + 1))
        //                    {
        //                        JoinMaps(iGridId, iGridId + 1, IMapUtilities.southSidePatch);
        //                    }
        //                    //
        //                    //  check for neighbour to the west
        //                    //
        //                    if (gridManager.MapGrid.ContainsKey(iGridId + 3))
        //                    {
        //                        JoinMaps(iGridId, iGridId + 3, IMapUtilities.westSidePatch);
        //                    }
        //                    break;
        //                case 2:
        //                case 5:
        //                case 8:
        //                    //
        //                    //  middle row
        //                    //
        //                    //  remove patch block to the right
        //                    //
        //                    iPatchIndex = iGridId == 2 ? -1 : iGridId - 3;
        //                    string sRightKey = iPatchIndex == -1 ? "Farm" : gridManager.MapGrid[iPatchIndex];
        //                    string sMessageKey = iPatchIndex == -1 ? "WestMessage.1" : "WestMessage";

        //                    GameLocation glMidRight = Game1.getLocationFromName(sRightKey);
        //                    EntrancePatch oMidRightPatch = null;
        //                    ExpansionPack oMidRightPack = null;

        //                    if (iPatchIndex == -1)
        //                    {
        //                        oFarm = utilitiesService.GameEnvironment.GetFarmDetails(Game1.whichFarm);
        //                        oMidRightPatch = oFarm?.PathPatches["0"];
        //                    }
        //                    else
        //                    {
        //                        oMidRightPack = contentPackLoader.ValidContents[gridManager.MapGrid[iPatchIndex]];
        //                        oMidRightPatch = oMidRightPack.EntrancePatches[IMapUtilities.westSidePatch];
        //                    }

        //                    if (oMidRightPatch != null && (iPatchIndex == -1 && utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit1) || iPatchIndex > -1)
        //                    {
        //                        //RemovePathBlock(new Vector2(oMidRightPatch?.PathBlockX ?? 0, oMidRightPatch?.PathBlockY ?? 0), glMidRight, oMidRightPatch?.WarpOrientation ?? 0, oMidRightPatch?.WarpOut.NumberofPoints ?? 0);

        //                        //
        //                        //    add basic right/left side warp ins/outs
        //                        //
        //                        oExpPatch = oNewPack.EntrancePatches[IMapUtilities.eastSidePatch];
        //                        if (oMidRightPatch != null)
        //                            AddExpansionIntersection(oMidRightPatch, sRightKey, oExpPatch, sExpansionName);

        //                        if (oExpPatch.Sign?.UseSign ?? false)
        //                        {
        //                            gridManager.AddSignPost(oExp, oExpPatch, "EastMessage", utilitiesService.MapUtilities.FormatDirectionText(IMapUtilities.eastSidePatch, iPatchIndex == -1 ? "Farm" : oMidRightPack.DisplayName));
        //                        }
        //                        if (oMidRightPatch.Sign != null && (oMidRightPatch.Sign?.UseSign ?? false))
        //                        {
        //                            gridManager.AddSignPost(Game1.getLocationFromName(sRightKey), oMidRightPatch, sMessageKey, utilitiesService.MapUtilities.FormatDirectionText(IMapUtilities.westSidePatch, oPackToActivate.DisplayName));
        //                        }

        //                    }
        //                    //
        //                    //  add northside exit
        //                    //
        //                    JoinMaps(iGridId, iGridId - 1, IMapUtilities.northSidePatch);
        //                    //
        //                    //  check for southside exit
        //                    //
        //                    if (gridManager.MapGrid.ContainsKey(iGridId + 1))
        //                    {
        //                        JoinMaps(iGridId, iGridId + 1, IMapUtilities.southSidePatch);
        //                    }
        //                    //
        //                    //  check for westside exit
        //                    //
        //                    if (gridManager.MapGrid.ContainsKey(iGridId + 3))
        //                    {
        //                        JoinMaps(iGridId, iGridId + 3, IMapUtilities.westSidePatch);
        //                    }
        //                    break;
        //                case 3:
        //                case 6:
        //                case 9:
        //                    //
        //                    // bottom row
        //                    //
        //                    //
        //                    //  remove patch block to the right
        //                    //
        //                    iPatchIndex = iGridId == 3 ? -1 : iGridId - 3;
        //                    string sBottomRightKey = iPatchIndex == -1 ? "Farm" : gridManager.MapGrid[iPatchIndex];

        //                    GameLocation glBottomRight = Game1.getLocationFromName(sBottomRightKey);
        //                    EntrancePatch oBottomRightPatch;
        //                    ExpansionPack oBottomRightPack = null;

        //                    if (iPatchIndex == -1)
        //                    {
        //                        oFarm = utilitiesService.GameEnvironment.GetFarmDetails(Game1.whichFarm);
        //                        oBottomRightPatch = oFarm?.PathPatches["1"];
        //                    }
        //                    else
        //                    {
        //                        oBottomRightPack = contentPackLoader.ValidContents[gridManager.MapGrid[iPatchIndex]];
        //                        oBottomRightPatch = oBottomRightPack.EntrancePatches[IMapUtilities.westSidePatch];
        //                    }

        //                    if (oBottomRightPatch != null && (iPatchIndex == -1 && utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit2) || iPatchIndex > -1)
        //                    {
        //                        utilitiesService.MapUtilities.RemovePathBlock(new Vector2(oBottomRightPatch.PathBlockX, oBottomRightPatch.PathBlockY), glBottomRight, oBottomRightPatch.WarpOrientation, oBottomRightPatch.WarpOut.NumberofPoints);

        //                        //
        //                        //    add basic right/left side warp ins/outs
        //                        //
        //                        oExpPatch = oNewPack.EntrancePatches[IMapUtilities.eastSidePatch];
        //                        AddExpansionIntersection(oBottomRightPatch, sBottomRightKey, oExpPatch, sExpansionName);
        //                        if (oExpPatch.Sign?.UseSign ?? false)
        //                        {
        //                            gridManager.AddSignPost(oExp, oExpPatch, "EastMessage", utilitiesService.MapUtilities.FormatDirectionText(IMapUtilities.eastSidePatch, (iPatchIndex == -1 ? "Farm" : oBottomRightPack?.DisplayName ?? "")));
        //                        }
        //                        if (oBottomRightPatch.Sign?.UseSign ?? false)
        //                        {
        //                            gridManager.AddSignPost(Game1.getLocationFromName(sBottomRightKey), oBottomRightPatch, "WestMessage", utilitiesService.MapUtilities.FormatDirectionText(IMapUtilities.westSidePatch, oPackToActivate.DisplayName));
        //                        }
        //                    }
        //                    //
        //                    //  add northside patch
        //                    //
        //                    JoinMaps(iGridId, iGridId - 1, IMapUtilities.northSidePatch);
        //                    //
        //                    //  check for westside exit
        //                    //
        //                    if (gridManager.MapGrid.ContainsKey(iGridId + 3))
        //                    {
        //                        JoinMaps(iGridId, iGridId + 3, IMapUtilities.westSidePatch);
        //                    }
        //                    break;
        //            }

        //        }
        //        else if (!bAutoAdd)
        //        {
        //            //
        //            //  expansion has explicit location details
        //            //
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger?.Log($"Error patching in map {iGridId}:{sExpansionName}", LogLevel.Error);
        //        logger?.LogError($"PatchInMap", ex);
        //    }
        //}
        //private void JoinMaps(int sourceGrid, int targetGrid, string sourceSide)
        //{
        //    //
        //    //  Join intersecting expansions
        //    //
        //    //  - remove path blocks
        //    //  - add warps
        //    //  - add signs
        //    //
        //    string targetSide = utilitiesService.MapUtilities.GetNeighbourSide(sourceSide);
        //    string sourceMessageName = utilitiesService.MapUtilities.GetSignPostMessagePrefix(sourceSide);
        //    string targetMessageName = utilitiesService.MapUtilities.GetSignPostMessagePrefix(targetSide);

        //    ExpansionPack targetPack = contentPackLoader.ValidContents[gridManager.MapGrid[targetGrid]];
        //    EntrancePatch targetEntPatch = targetPack.EntrancePatches[targetSide];

        //    ExpansionPack sourcePack = contentPackLoader.ValidContents[gridManager.MapGrid[sourceGrid]];
        //    EntrancePatch sourceEntPatch = sourcePack.EntrancePatches[sourceSide];

        //    AddExpansionIntersection(sourceEntPatch, sourcePack.LocationName, targetEntPatch, targetPack.LocationName);

        //    if (sourceEntPatch.Sign?.UseSign ?? false)
        //    {
        //        gridManager.AddSignPost(modDataService.farmExpansions[sourcePack.LocationName], sourceEntPatch, sourceMessageName, utilitiesService.MapUtilities.FormatDirectionText(sourceSide, targetPack.DisplayName));
        //    }
        //    if (targetEntPatch.Sign?.UseSign ?? false)
        //    {
        //        gridManager.AddSignPost(modDataService.farmExpansions[targetPack.LocationName], targetEntPatch, targetMessageName, utilitiesService.MapUtilities.FormatDirectionText(targetSide, sourcePack.DisplayName));
        //    }

        //}
        /// <summary>
        /// Add a Sign Post to the map with the name of the expansion the exit leads to
        /// </summary>
        /// <param name="oExp"></param>
        /// <param name="oExpPatch"></param>
        /// <param name="messageKeySuffix"></param>
        /// <param name="signMessage"></param>
        //internal void AddSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix, string signMessage)
        //{
        //    gridManager.AddSignPost(oExp, oExpPatch, messageKeySuffix, signMessage);
        //    //
        //    //  the sign post can either be on the building layer to block,
        //    //  or added to the Front of an existing map with a building layer tile
        //    //
        //    //try
        //    //{
        //    //    string layerName = oExpPatch.Sign.UseFront ? "Front" : "Buildings";
        //    //    int iTileSheetId = utilitiesService.MapUtilities.GetTileSheetId(oExp.map, "*_outdoorsTileSheet");
        //    //    utilitiesService.MapUtilities.SetTile(oExp.map, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, iTileSheetId, 435, layerName);
        //    //    //
        //    //    //  add name of expansion the the MapStrings
        //    //    //
        //    //    string messageKey = oExp.Name + "." + messageKeySuffix;

        //    //    contentManagerService.UpsertStringFromMap(messageKey, signMessage);
        //    //    //
        //    //    //  add tile action to view the message
        //    //    //
        //    //    utilitiesService.MapUtilities.SetTileProperty(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, "Buildings", "Action", "Message \"" + messageKey + "\"");
        //    //    utilitiesService.MapUtilities.SetTile(oExp.map, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y - 1, iTileSheetId, 410, "Front");
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    logger.Log($"Error AddingSignPost. {ex}", LogLevel.Error);
        //    //}
        //}


        /// <summary>
        /// Checks to see if any Expansions should be activated
        /// </summary>
        /// <param name="sCaller"></param>
        internal void CheckForExpansionActivation(string sCaller)
        {
#if DEBUG_LOG
            logger.Log($"     LandManager CheckForExpansionActivation for {sCaller}", LogLevel.Debug);
            logger.Log("Check for expansion activation: " + sCaller, LogLevel.Trace);
            logger.Log($"Player has {Game1.player.mailReceived.Count} messages", LogLevel.Info);
            logger.Log("Messages: " + string.Join(", ", Game1.player.mailReceived), LogLevel.Info);
#endif
            foreach (string sKey in modDataService.farmExpansions.Keys)
            {
                FarmExpansionLocation oExp = modDataService.farmExpansions[sKey];
                if (!oExp.Active)
                {
                    bool active = false;
                    //
                    //  check if land can be activated
                    //
                    if (!string.IsNullOrEmpty(modDataService.validContents[sKey].MailId))
                    {
                        //
                        //  check if player received mail
                        //

                        logger.Log("      Checking for mailId: " + modDataService.validContents[sKey].MailId, LogLevel.Debug);

                        if (Game1.IsMultiplayer)
                        {
                            if (Game1.MasterPlayer.mailReceived.Contains(modDataService.validContents[sKey].MailId) || Game1.MasterPlayer.mailbox.Contains(modDataService.validContents[sKey].MailId))
                            {
                                utilitiesService.CustomEventsService.TriggerCustomEvent("ActivateExpansion", new object[] { sKey });
                                active = true;
                            }
                        }
                        else
                        {
                            if (Game1.player.mailReceived.Contains(modDataService.validContents[sKey].MailId) || Game1.player.mailbox.Contains(modDataService.validContents[sKey].MailId))
                            {
                                //
                                //  mail received, activate expansion
                                //
                                utilitiesService.CustomEventsService.TriggerCustomEvent("ActivateExpansion", new object[] { sKey });
                                active = true;
                            }
                        }
                    }
                    if (!active)
                    {
                        CheckIfCanBePutForSale(sKey);
                    }
                }
            }

        }
        private void CheckMail()
        {
            //
            //  check mail to be read
            //
            List<string> mailread = new List<string> { };

            foreach (string key in modDataService.ModState.MailToBeRead.Keys)
            {
                if (Game1.player.mailReceived.Contains(key))
                {
                    //
                    //  player read offer letter, put land
                    //  for sale
                    //
                    AddLandForSale(modDataService.ModState.MailToBeRead[key]);
                    modDataService.farmExpansions[modDataService.ModState.MailToBeRead[key]].OfferLetterRead = true;
                    //
                    //  remove letter, keep player mailbox clean
                    //
                    Game1.player.mailReceived.Remove(key);
                    mailread.Add(key);
                }
            }
            //
            //  clean up mail read
            //
            if (mailread.Count > 0)
            {
                PopupHUDNewLandMessage();
                foreach (string readmmail in mailread) modDataService.ModState.MailToBeRead.Remove(readmmail);
            }
        }
        private void DayStarted(EventArgs e)
        {
            if (utilitiesService.IsMasterGame())
            {
                NewLandMessageSentToday = false;
                CheckMail();
                CheckForExpansionActivation("DayStarted");
            }
        }
        internal override bool PurchaseLand(string expansionName, bool withPopup, long purchasedBy)
        {
            if (modDataService.MapGrid.Count >= modDataService.MaximumExpansions)
            {
                // grid is full
                return false;
            }
            if (modDataService.validContents.TryGetValue(expansionName, out ExpansionPack oContent))
            {
                try
                {
                    if (withPopup && Game1.IsMultiplayer)
                    {
                        ChatSnippet oSnip = new ChatSnippet("Land bought", LocalizedContentManager.LanguageCode.en);
                        utilitiesService.ModHelperService.modHelper.Multiplayer.SendMessage(new ChatMessage() { message = new List<ChatSnippet> { { oSnip } }, language = LocalizedContentManager.LanguageCode.en }, "Info");
                    }
                    if (Game1.IsMasterGame)
                    {
                        if (!string.IsNullOrEmpty(oContent?.MailId))
                        {
                            logger.Log("Sent purchase letter: " + oContent.MailId, LogLevel.Debug);
                            playerComms.SendPlayerMail(oContent.MailId, false, true);
                        }
                        int sellingPrice = GetLandPrice(expansionName);

                        LandBought(oContent.LocationName ?? "", withPopup, purchasedBy, sellingPrice);
                    }
                    else
                    {
                        TriggerEvent("SendFarmHandPurchase", new object[] { expansionName });
                    }
                }
                catch (Exception ex)
                {
                    logger.Log("Pick error: " + ex.ToString(), LogLevel.Error);
                    return false;
                }

                if (withPopup)
                    Game1.drawObjectDialogue(I18n.DeedTomorrow());

                return true;
            }

            logger.Log($"Attempt to purchase unknown expansion '{expansionName}'", LogLevel.Error);
            return false;
        }
        public int GetLandPrice(string expansionName)
        {
            switch (modDataService.Config.globalPriceMode)
            {
                case "Flat Price":
                    return modDataService.Config.globalPrice;
                case "By Tile":
                    return (int)(modDataService.validContents[expansionName].MapSize.X * modDataService.validContents[expansionName].MapSize.Y * modDataService.Config.globalPrice);
                default:
                    return modDataService.validContents[expansionName]?.Cost ?? 0;
            }
            //if (modDataService.Config.useGlobalPrice)
            //    return modDataService.Config.globalPrice;
            //else
            //{
            //    return modDataService.validContents[expansionName]?.Cost ?? 0;
            //}
        }
        internal void LandBought(string expansionName)
        {
            if (!string.IsNullOrEmpty(expansionName))
            {
                utilitiesService.CustomEventsService.TriggerModEvent(ICustomEventsService.ModEvents.LandPurchased, new object[]
                {
                    expansionName,true,Game1.player.UniqueMultiplayerID
                });
            }
        }
        internal override void LandBought(string sExpansionName, bool withPopup, long purchasedBy, int price)
        {
            if (!modDataService.validContents[sExpansionName].AllowMultipleInstances && modDataService.LandForSale.Contains(sExpansionName))
            {
                modDataService.LandForSale.Remove(sExpansionName);
            }
            //utilitiesService.CustomEventsService.TriggerCustomEvent("ActivateExpansion", new object[] { sExpansionName, modDataService.farmExpansions[sExpansionName].GridId });
            utilitiesService.CustomEventsService.TriggerCustomEvent("ActivateExpansion", new object[] { sExpansionName });

            modDataService.expDetails[sExpansionName].Active = true;
            modDataService.expDetails[sExpansionName].ForSale = modDataService.validContents[sExpansionName].AllowMultipleInstances;
            if (purchasedBy != 0)
                modDataService.expDetails[sExpansionName].PurchasedBy = purchasedBy;

            modDataService.farmExpansions[sExpansionName].reloadMap();
            modDataService.farmExpansions[sExpansionName].seasonUpdate(false);

            modDataService.expDetails[sExpansionName].PurchaseDate = Game1.Date.TotalDays;
            modDataService.expDetails[sExpansionName].PurchasePrice = price;
            Game1.player.Money -= price;
        }
        internal void AddLandForSale(string expansionName)
        {
            if (!modDataService.LandForSale.Contains(expansionName))
            {
                modDataService.LandForSale.Add(expansionName);
                modDataService.expDetails[expansionName].ForSale = true;
                //if (Game1.IsMultiplayer && Game1.IsMasterGame)
                //{
                //    SDRMultiplayer.SendNewLandForSale(-1, expansionName);
                //}
                PopupHUDNewLandMessage();
            }
        }
        /// <summary>
        /// Pops a HUD message about land being for sale.  Only pops once a day
        /// </summary>
        public void PopupHUDNewLandMessage()
        {
            if (!NewLandMessageSentToday)
            {
                NewLandMessageSentToday = true;
                utilitiesService.PopMessage(I18n.CheckMsgBd());
            }
        }
        private void SendPlayerMail(string mailId, string expansionName, bool addToReadList, bool sendToday = false)
        {
            playerComms.SendPlayerMail(mailId, sendToday, true, addToReadList);

            if (addToReadList && !modDataService.ModState.MailToBeRead.ContainsKey(mailId))
            {
                modDataService.ModState.MailToBeRead.Add(mailId, expansionName);
                modDataService.SaveModState();
            }
        }
        private string MigrateRequirements(string requirement)
        {
            if (string.IsNullOrEmpty(requirement))
                return requirement;

            string tranlsation = requirement;
            string[] arParts = requirement.Split(' ');

            if (requirement.StartsWith("y"))
            {
                // convert year requirement

                return $"YEAR {arParts[1]}";
            }
            if (requirement.StartsWith("j"))
            {
                // convert days played requirement

                return $"DAYS_PLAYED {arParts[1]}";
            }

            return tranlsation;
        }
        internal void CheckAllExpansionsForCanBePutForSale()
        {
            //
            //  add lands that can be sold
            //
            var activeExpansions = modDataService.farmExpansions.Where(p => !p.Value.Active).Select(r => r.Key).ToList();

            foreach (string expansionName in activeExpansions)
            {
                CheckIfCanBePutForSale(expansionName);
            }
        }
        internal override void CheckIfCanBePutForSale(string sExpansionName)
        {
            //
            //  check if expansion can be put up for sale
            //
            if (!modDataService.LandForSale.Contains(sExpansionName) && (!modDataService.validContents[sExpansionName].Added || (modDataService.farmExpansions.ContainsKey(sExpansionName) && !modDataService.farmExpansions[sExpansionName].Active)))
            {
                ExpansionPack oPack = modDataService.validContents[sExpansionName];

                string condition;
                int price;
                //
                //  get activation condition
                //
                if (modDataService.Config.useGlobalCondition)
                    condition = modDataService.Config.globalCondition;
                else
                    condition = oPack.Requirements;

                if (modDataService.Config.useGlobalPrice)
                    price = modDataService.Config.globalPrice;
                else
                    price = oPack.Cost;

                if (modDataService.Config.SkipRequirements || string.IsNullOrEmpty(condition))
                {
                    //
                    //  no requirements, can be put for sale
                    //
                    if (price > 0 && (string.IsNullOrEmpty(oPack.VendorMailId) || modDataService.farmExpansions[sExpansionName].OfferLetterRead))
                    {
                        //
                        //  no offer mail, add for sale directly
                        //
                        logger.Log($"   {(string.IsNullOrEmpty(oPack.VendorMailId) ? "    No email requirements, putting for sale." : "    Received email, putting for sale.")}: {sExpansionName}", LogLevel.Debug);
                        AddLandForSale(sExpansionName);
                    }
                    else if (price > 0 && !string.IsNullOrEmpty(oPack.VendorMailId))
                    {
                        //
                        //  expansion has offer mail, send player
                        //  the offer letter
                        //  the land will be put for sale after
                        //  the letter has been read
                        //
                        logger.Log($"  Requirements met, sending letter for {sExpansionName}", LogLevel.Debug);
                        SendPlayerMail(oPack.VendorMailId, sExpansionName, false, true);
                    }
                    else
                    {
                        //
                        //  no cost, just activate
                        //
                        logger.Log($"   No requirements, no cost ({oPack.Cost}), activating {sExpansionName}", LogLevel.Debug);
                        if (!PurchaseLand(sExpansionName, false, -1))
                        {
                            // grid is full, so put the land for sale
                            AddLandForSale(sExpansionName);
                        }
                    }
                }
                else
                {
                    // check requirements
                    //
                    //  ver 1.6 moved to using GSQs
                    //
                    string migrated_condition = MigrateRequirements(condition);
                    bool bPassedRequirements = GameStateQuery.CheckConditions(migrated_condition);

                    if (bPassedRequirements && price == 0)
                    {
                        //
                        //  land is free, notify player of new land
                        //  and activate it
                        //
                        if (!string.IsNullOrEmpty(oPack.MailId) && !modDataService.farmExpansions[sExpansionName].OfferLetterRead)
                        {

                            logger.Log("Sent free land letter: " + oPack.MailId, LogLevel.Debug);

                            SendPlayerMail(oPack.MailId, sExpansionName, false, true);
                        }
                        utilitiesService.CustomEventsService.TriggerCustomEvent("ActivateExpansion", new object[] { oPack.LocationName });
                    }
                    else if (bPassedRequirements)
                    {
                        //
                        //  requirements passed, check what next
                        //
                        if (string.IsNullOrEmpty(oPack.VendorMailId) || modDataService.farmExpansions[sExpansionName].OfferLetterRead)
                        {
                            //
                            //  no email to send, just put it for sale
                            //
                            AddLandForSale(oPack.LocationName);
                        }
                        else
                        {
                            //
                            //  send  player email about sale,
                            //  expansion will not be put on
                            //  sale until the player reads the
                            //  email
                            //
                            SendPlayerMail(oPack.VendorMailId, sExpansionName, true);
                        }
                    }
                }
            }
        }

    }
}
