using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using StardewModdingAPI;
using StardewModHelpers;
using StardewValley.GameData.WorldMaps;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SDV_Realty_Core.Framework.Menus
{
    internal class SDRWorldMapMenu : IClickableMenu
    {
        private Texture2D backgroundTexture;
        private ILandManager landManager;
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        private ILoggerService logger;
        private Texture2D bordertexture_dark;
        private Texture2D bordertexture_orange;
        private Texture2D bordertexture_darkorange;
        private Texture2D mapFrame;

        public SDRWorldMapMenu(int x, int y, Texture2D backgroundTexture, ILandManager landmanager, IModDataService modDataService, IUtilitiesService utilitiesService) :
            base(x, y, backgroundTexture.Width, backgroundTexture.Height, true)
        {
             landManager = landmanager;
            this.modDataService = modDataService;
            this.utilitiesService = utilitiesService;
            logger = utilitiesService.logger;
            this.backgroundTexture = backgroundTexture;

            var colors = new Color[] { new Color(0x5bf,0x2bf,0x2af) };
            bordertexture_dark = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            bordertexture_dark.SetData<Color>(colors);

            colors = new Color[] { new Color(0xdcf, 0x7bf, 5f) };
            bordertexture_orange = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            bordertexture_orange.SetData<Color>(colors);

            colors = new Color[] { new Color(0xb1f, 0x4ef, 5f) };
            bordertexture_darkorange = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            bordertexture_darkorange.SetData<Color>(colors);

            mapFrame = utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(Path.Combine("data", "assets", "images", "land_frame.png"));
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
            b.Draw(backgroundTexture, new Rectangle((int)Position.X, (int)Position.Y, width, height), Color.White);
            DrawMapGrid(b,width,height);

            base.draw(b);
            drawMouse(b);
        }
        private Texture2D AddFrame(Texture2D image,SpriteBatch spriteBatch)
        {
            int borderElementWidth = 5;
            StardewBitmap frame = new StardewBitmap(image);

            frame.DrawRectangle(new Color(0x5bf, 0x2bf, 0x2af), 0, 0, image.Width, image.Height, 1);
            //spriteBatch.Draw(bordertexture_dark, new Rectangle(0, 0, image.Width, borderElementWidth),Color.AliceBlue);
            //spriteBatch.Draw(bordertexture_dark, new Rectangle(image.Width-1, 0, borderElementWidth, image.Height), Color.AliceBlue);
            //spriteBatch.Draw(bordertexture_dark, new Rectangle(0, image.Height-1, image.Width, borderElementWidth), Color.AliceBlue);
            //spriteBatch.Draw(bordertexture_dark, new Rectangle(0, 0, borderElementWidth, image.Height), Color.AliceBlue);
        
            return frame.Texture();
        }
        private void DrawMapGrid(SpriteBatch spriteBatch,int containerWidth,int containerHeight)
        {
            int iForSale = 0;
            int iTotalForSale = landManager.LandForSale.Count;
            int iComingSoon = 0;
            int iComingSoonTotal = modDataService.farmExpansions.Values.Where(p => !p.Active && !landManager.LandForSale.Contains(p.Name)).Count();
            Rectangle diplayArea = new Rectangle((int)Position.X+25, (int)Position.Y+5, width-35, height-75);
            Dictionary<Rectangle, string> tooltips = new();
            for (int gridId = 0; gridId < IGridManager.MaxFarms; gridId++)
            {
                if (modDataService.MapGrid.ContainsKey(gridId))
                {
                    string locationKey = modDataService.MapGrid[gridId];
                    if (modDataService.validContents.TryGetValue(locationKey, out ExpansionPack contentPack))
                    {
                        string modId = contentPack.Owner.Manifest.UniqueID;
                        string seasonOverride = modDataService.farmExpansions[locationKey].SeasonOverride;
                        string displayName = contentPack.DisplayName;
                        //
                        //  add any SeasonOverride to the Expansion name
                        //
                        if (!string.IsNullOrEmpty(seasonOverride))
                        {
                            displayName += $"\n[{seasonOverride}]";
                        }
                        Rectangle areaRectangle =utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);                        
                        Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{Path.GetFileName( contentPack.WorldMapTexture)}"));
                        //areaTexture=AddFrame(areaTexture,spriteBatch);
                        spriteBatch.Draw(areaTexture,areaRectangle,Color.White);
                        spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                        tooltips.Add(areaRectangle,displayName);
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
                    Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale.png"));
                    spriteBatch.Draw(areaTexture, areaRectangle, Color.White);
                    spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                    tooltips.Add(areaRectangle, I18n.CheckMsgBd());
                    iForSale++;
                }
                else if (iComingSoon < iComingSoonTotal)
                {
                    //  add coming soon image
                    //
                    Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                    Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon.png"));
                    spriteBatch.Draw(areaTexture, areaRectangle, Color.White);
                    spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                    iComingSoon++;
                }
                else
                {
                    //  future growth
                    Rectangle areaRectangle = utilitiesService.GetGridLocationCoordinates(gridId, diplayArea);
                    Texture2D areaTexture = utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}SDR{FEConstants.AssetDelimiter}WorldMap_ForFuture.png"));
                    spriteBatch.Draw(areaTexture, areaRectangle, Color.White);
                    spriteBatch.Draw(mapFrame, areaRectangle, Color.White);
                }
            }

            Point mouseCoords = Game1.getMousePosition(ui_scale: true);
            List<KeyValuePair<Rectangle, string>> tooltip = tooltips.Where(p => p.Key.Contains(mouseCoords.X, mouseCoords.Y)).ToList();
            if (tooltip.Any())
            {
                drawHoverText(spriteBatch, tooltip.First().Value, Game1.smallFont);
            }

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
