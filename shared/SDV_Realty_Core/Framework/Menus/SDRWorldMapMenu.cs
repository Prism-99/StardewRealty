using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SDV_Realty_Core.Framework.Menus
{
    internal class SDRWorldMapMenu : IClickableMenu
    {
        private ILandManager landManager;
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        private ILoggerService logger;
        private Dictionary<string, Rectangle> mapHotSpots = new();
        private static int mapWidth = 800;
        private static int mapHeight = 600;
        private static Texture2D? forSaleTexure = null;
        private static Texture2D? comingSoonTexture = null;
        private static Texture2D? futureTexture = null;
        private const int baseControlId = 1000;
        public SDRWorldMapMenu(int x, int y, Texture2D backgroundTexture, ILandManager landmanager, IModDataService modDataService, IUtilitiesService utilitiesService) :
            base(x, y, mapWidth, mapHeight, false)
        {
            landManager = landmanager;
            this.modDataService = modDataService;
            this.utilitiesService = utilitiesService;
            logger = utilitiesService.logger;
            if (forSaleTexure == null)
                forSaleTexure = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale_{Game1.season.ToString().ToLower()}"));
            if (comingSoonTexture == null)
                comingSoonTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon_{Game1.season.ToString().ToLower()}"));
            if (futureTexture == null)
                futureTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture_{Game1.season.ToString().ToLower()}"));

            populateClickableComponentList();
            if (Game1.options.SnappyMenus)
            {
                snapToDefaultClickableComponent();
            }
        }
        public void Open(IClickableMenu gameMenu)
        {
            if (gameMenu is GameMenu gmenu)
            {
                gmenu.SetChildMenu(this);
            }
        }
        public override void draw(SpriteBatch b)
        {
            DrawOuterFrame(b);
            mapHotSpots.Clear();
            DrawMapGrid(b, width, height);

            base.draw(b);
            drawMouse(b);
        }
        public override void populateClickableComponentList()
        {
            base.populateClickableComponentList();
            AddClickableComponents();
        }
        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(baseControlId);
            snapCursorToCurrentSnappedComponent();
        }
        private void AddClickableComponents()
        {
            Rectangle diplayArea = new Rectangle((int)Position.X + 25, (int)Position.Y + 5, width - 35, height - 75);
            for (int gridId = 0; gridId < modDataService.MaximumExpansions; gridId++)
            {
                Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                ClickableComponent control = new ClickableComponent(areaRectangle, $"grid{gridId}")
                {
                    myID = baseControlId + gridId
                };
                SetNeighbours(control);
                allClickableComponents.Add(control);
            }
            foreach (var entry in modDataService.MiniMapGrid)
            {
                Rectangle miniRectangle = utilitiesService.GetGridLocationCoordinates(entry.Key, diplayArea);
                ClickableComponent control = new ClickableComponent(miniRectangle, $"minigrid{entry.Key}")
                {
                    myID = baseControlId + entry.Key
                };
                SetNeighbours(control);
                allClickableComponents.Add(control);
            }
        }
        private void SetNeighbours(ClickableComponent control)
        {
            switch (control.myID - baseControlId)
            {
                case 0:
                    control.leftNeighborID = baseControlId + 1;
                    control.downNeighborID = baseControlId - 1;
                    break;
                case 1:
                    control.leftNeighborID = control.myID + 3;
                    control.downNeighborID = control.myID + 1;
                    control.rightNeighborID = control.myID - 1;
                    break;
                case 2:
                case 3:
                    control.leftNeighborID = control.myID + 3;
                    control.upNeighborID = control.myID - 1;
                    control.rightNeighborID = baseControlId - 1;
                    if (control.myID - baseControlId != 3)
                        control.downNeighborID = control.myID + 1;
                    break;
                default:
                    if (control.myID < baseControlId)
                    {
                        control.upNeighborID = control.myID + 1;
                        control.downNeighborID = control.myID - 1;
                        control.leftNeighborID = baseControlId + 3;
                    }
                    else if ((control.myID - baseControlId - 1) % 3 == 0)
                    {
                        control.leftNeighborID = control.myID + 3;
                        control.rightNeighborID = control.myID - 3;
                        //control.upNeighborID = control.myID - 1;
                        control.downNeighborID = control.myID + 1;
                    }
                    else if ((control.myID - baseControlId) % 3 == 0)
                    {
                        control.leftNeighborID = control.myID + 3;
                        control.rightNeighborID = control.myID - 3;
                        control.upNeighborID = control.myID - 1;
                        //control.downNeighborID = control.myID + 1;
                    }
                    else
                    {
                        control.leftNeighborID = control.myID + 3;
                        control.rightNeighborID = control.myID - 3;
                        control.upNeighborID = control.myID - 1;
                        control.downNeighborID = control.myID + 1;
                    }
                    break;
            }
        }
        private void DrawOuterFrame(SpriteBatch b)
        {
            int paddingLeft = 5;
            int paddingRight = 10;
            int paddingTop = 5;
            int bgLeftOffset = 20;
            int bgRightOffset = 20;
            int bgTopOffset = 20;
            int bgBottomOffset = 20;

            Rectangle background = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 9);

            Rectangle topLeft = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 0);
            Rectangle topCentre = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 2);
            Rectangle topRight = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 3);

            Rectangle centreLeft = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 8);
            Rectangle centreRight = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 11);

            Rectangle bottomLeft = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 12);
            Rectangle bottomCentre = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 14);
            Rectangle bottomRight = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 15);

            Texture2D bgTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_{Game1.season.ToString().ToString().ToLower()}"));

            b.Draw(bgTexture, new Rectangle((int)Position.X - paddingLeft + bgLeftOffset, (int)Position.Y - paddingTop + bgTopOffset, mapWidth - bgLeftOffset - bgRightOffset + paddingLeft + paddingRight, mapHeight + paddingTop - bgTopOffset - bgBottomOffset), Color.White);
            //b.Draw(Game1.menuTexture, new Rectangle((int)Position.X - paddingLeft + bgLeftOffset, (int)Position.Y - paddingTop + bgTopOffset, mapWidth - bgLeftOffset - bgRightOffset + paddingLeft + paddingRight, mapHeight + paddingTop - bgTopOffset - bgBottomOffset), background, Color.White);

            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X - paddingLeft, (int)Position.Y - paddingTop, topLeft.Width, topLeft.Height), topLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X - paddingLeft + topLeft.Width, (int)Position.Y - paddingTop, mapWidth - topLeft.Width * 2 + paddingLeft + paddingRight, topCentre.Height), topCentre, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X + mapWidth - topRight.Width + paddingRight, (int)Position.Y - paddingTop, topRight.Width, topRight.Height), topRight, Color.White);

            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X - paddingLeft, (int)Position.Y + topLeft.Height - paddingTop, centreLeft.Width, mapHeight - centreLeft.Height * 2 + paddingTop), centreLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X + mapWidth - centreRight.Width + paddingRight, (int)Position.Y + topRight.Height - paddingTop, centreRight.Width, mapHeight - centreRight.Height * 2 + paddingTop), centreRight, Color.White);

            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X - paddingLeft, (int)Position.Y + mapHeight - bottomLeft.Height, bottomLeft.Width, bottomLeft.Height), bottomLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X - paddingLeft + bottomCentre.Width, (int)Position.Y + mapHeight - bottomCentre.Height, mapWidth - bottomCentre.Width * 2 + paddingLeft + paddingRight, bottomCentre.Height), bottomCentre, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle((int)Position.X + mapWidth - bottomRight.Width + paddingRight, (int)Position.Y + mapHeight - bottomRight.Height, bottomRight.Width, bottomRight.Height), bottomRight, Color.White);
        }
        private void DrawMapGridCell(SpriteBatch spriteBatch, int gridId, Rectangle displayArea, Texture2D cellTexture, bool withBorder = true)
        {
            Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, displayArea);
            int mapOffset = 5;
            Rectangle mapArea;
            if (gridId < 0)
            {
                mapArea = new Rectangle(areaRectangle.X + 9, areaRectangle.Y + 3, areaRectangle.Width - mapOffset * 3, areaRectangle.Height - 6);
            }
            else
            {
                mapArea = new Rectangle(areaRectangle.X + 9, areaRectangle.Y + 9, areaRectangle.Width - mapOffset * 3, areaRectangle.Height - 16);
            }
            //Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{Path.GetFileName(contentPack.WorldMapTexture)}"));
            //areaTexture=AddFrame(areaTexture,spriteBatch);
            if (withBorder)
            {
                Rectangle expansionFrame = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 56);
                spriteBatch.Draw(Game1.menuTexture, areaRectangle, expansionFrame, Color.White);
            }
            spriteBatch.Draw(cellTexture, mapArea, Color.White);
            //spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
        }
        private void DrawMapGrid(SpriteBatch spriteBatch, int containerWidth, int containerHeight)
        {
            int iForSale = 0;
            int iTotalForSale = modDataService.LandForSale.Count;
            int iComingSoon = 0;
            int iComingSoonTotal = modDataService.farmExpansions.Values.Where(p => !p.Active && !modDataService.LandForSale.Contains(p.Name)).Count();
            Rectangle diplayArea = new Rectangle((int)Position.X + 25, (int)Position.Y + 5, width - 35, height - 75);
            Dictionary<Rectangle, string> tooltips = new();

            //
            //  add minimap
            //
            foreach (var entry in modDataService.MiniMapGrid)
            {
                Rectangle miniRectangle = utilitiesService.GetGridLocationCoordinates(entry.Key, diplayArea);
                //if (entry.Value.Texture == null && !string.IsNullOrEmpty(entry.Value.TexturePath))
                //{
                //    try
                //    {
                //        entry.Value.Texture = utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(entry.Value.TexturePath);
                //    }
                //    catch { }
                //}
                DrawMapGridCell(spriteBatch, entry.Key, diplayArea, entry.Value.Texture == null ? forSaleTexure : entry.Value.Texture, false);

                tooltips.Add(miniRectangle, entry.Value.DisplayName);
                mapHotSpots.Add(entry.Value.Key, miniRectangle);

            }
            //
            //  add Home square
            //
            //Rectangle homeRectangle = utilitiesService.GetGridLocationCoordinates(-1, diplayArea);
            //string homeDisplayName = "Home";
            //DrawMapGridCell(spriteBatch, -1, diplayArea, forSaleTexure);

            //tooltips.Add(homeRectangle, homeDisplayName);
            //mapHotSpots.Add("Home", homeRectangle);
            //
            //  add Stardew Meadows
            //
            //Rectangle meadowsRectangle = utilitiesService.GetGridLocationCoordinates(-2, diplayArea);
            //string meadowsDisplayName = "Stardew Meadows";
            //DrawMapGridCell(spriteBatch, -2, diplayArea, futureTexture);

            //tooltips.Add(meadowsRectangle, meadowsDisplayName);
            //mapHotSpots.Add("Meadows", meadowsRectangle);

            //
            //  add expansion grid
            //
            for (int gridId = 0; gridId < modDataService.MaximumExpansions; gridId++)
            {
                if (modDataService.MapGrid.ContainsKey(gridId))
                {
                    string locationKey = modDataService.MapGrid[gridId];
                    if (modDataService.validContents.TryGetValue(locationKey, out ExpansionPack contentPack))
                    {
                        string modId = contentPack.Owner?.Manifest.UniqueID ?? locationKey;
                        string seasonOverride = modDataService.farmExpansions[locationKey].GetSeasonOverride();

                        string displayName = contentPack.DisplayName;
                        //
                        //  add any SeasonOverride to the Expansion name
                        //
                        if (!string.IsNullOrEmpty(seasonOverride))
                        {
                            displayName += $"\n[{seasonOverride}]";
                        }
                        Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                        //int mapOffset = 5;
                        //Rectangle mapArea = new Rectangle(areaRectangle.X+ 9,areaRectangle.Y+ 9,areaRectangle.Width- mapOffset*3,areaRectangle.Height- 16);
                        Texture2D areaTexture = null;
                        string mapPath = contentPack.GetSeasonalWorldMapTexture(seasonOverride).Replace(".png", "", System.StringComparison.CurrentCultureIgnoreCase);
                        //if (contentPack.SeasonalWorldMapTextures.TryGetValue(Game1.season.ToString().ToLower(), out string textureName))
                        //{
                        //    mapPath = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{utilitiesService.RemoveMapExtensions(textureName)}");
                        //}
                        //else
                        //{
                        //    if (string.IsNullOrEmpty(contentPack.WorldMapTexture))
                        //    {
                        //        mapPath = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}no_world_map.png");
                        //    }
                        //    else
                        //    {
                        //        mapPath = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{utilitiesService.RemoveMapExtensions(contentPack.WorldMapTexture)}");
                        //    }
                        //}
                        try
                        {
                            areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(mapPath);
                        }
                        catch (Exception ex)
                        {

                        }
                        ////areaTexture=AddFrame(areaTexture,spriteBatch);
                        //Rectangle expansionFrame= Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 56);
                        //spriteBatch.Draw(Game1.menuTexture,areaRectangle,expansionFrame,Color.White);
                        //spriteBatch.Draw(areaTexture, mapArea, Color.White);
                        //spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                        if (areaTexture != null)
                            DrawMapGridCell(spriteBatch, gridId, diplayArea, areaTexture);

                        tooltips.Add(areaRectangle, displayName);
                        mapHotSpots.Add(locationKey, areaRectangle);
                    }
                    else
                    {
                        logger.LogDebug($"Missing grid expansion pack {locationKey}");
                    }
                }
                else if (iForSale < iTotalForSale)
                {
                    //  add for sale square
                    Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                    //Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale_{Game1.season.ToString().ToLower()}"));
                    //spriteBatch.Draw(areaTexture, areaRectangle, Color.White);
                    //spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                    DrawMapGridCell(spriteBatch, gridId, diplayArea, forSaleTexure);
                    tooltips.Add(areaRectangle, I18n.CheckMsgBd());
                    iForSale++;
                }
                else if (iComingSoon < iComingSoonTotal)
                {
                    //  add coming soon image
                    //
                    Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                    //spriteBatch.Draw(areaTexture, areaRectangle, Color.White);
                    //spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                    DrawMapGridCell(spriteBatch, gridId, diplayArea, comingSoonTexture);
                    iComingSoon++;
                }
                else
                {
                    //  future growth
                    Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                    //Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}SDR{FEConstants.AssetDelimiter}WorldMap_ForFuture.png"));
                    //Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture_{Game1.season.ToString().ToLower()}"));
                    //spriteBatch.Draw(areaTexture, areaRectangle, Color.White);
                    //spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                    DrawMapGridCell(spriteBatch, gridId, diplayArea, futureTexture);
                }
            }

            Point mouseCoords = Game1.getMousePosition(ui_scale: true);
            List<KeyValuePair<Rectangle, string>> tooltip = tooltips.Where(p => p.Key.Contains(mouseCoords.X, mouseCoords.Y)).ToList();
            if (tooltip.Any())
            {
                drawHoverText(spriteBatch, tooltip.First().Value, Game1.smallFont);
            }

        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (modDataService.Config.UseMapWarps)
            {
                var spot = mapHotSpots.Where(p => p.Value.Contains(x, y));
                if (spot != null && spot.Any())
                {
                    if (modDataService.MiniMapGrid.Where(p => p.Value.Key == spot.First().Key).Any())
                    {
                        modDataService.MiniMapGrid.Where(p => p.Value.Key == spot.First().Key).First().Value.FireWarp();
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                    else if (modDataService.validContents.TryGetValue(spot.First().Key, out ExpansionPack contentPack))
                    {
                        DelayedAction.warpAfterDelay(spot.First().Key, new Point(contentPack.CaveEntrance.WarpIn.X, contentPack.CaveEntrance.WarpIn.Y), 100);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                    else
                        base.receiveLeftClick(x, y, playSound);
                }
                else
                    base.receiveLeftClick(x, y, playSound);
            }
            else
                base.receiveLeftClick(x, y, playSound);
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape || key == Keys.M)
            {
                Game1.activeClickableMenu.exitThisMenu();
            }
            else
                base.receiveKeyPress(key);
        }
    }
}
