using System;
using System.Collections.Generic;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;

namespace SDV_Realty_Core.Framework.Menus
{
    class FEForSaleMenu : IClickableMenu
    {
        private ILoggerService logger;

        public class LandForSaleDetails
        {
            public string ExpansionName;
            public string DisplayName;
            public string Description;
            public int Cost;
            public string Vendor;
            public Texture2D Thumbnail;
        }
        private Texture2D billboardTexture;
        public ClickableComponent purchaseLandLeftButton;
        public ClickableComponent purchaseLandRightButton;
        public string[] emojiIndices = new string[38]
        {
            "Abigail", "Penny", "Maru", "Leah", "Haley", "Emily", "Alex", "Shane", "Sebastian", "Sam",
            "Harvey", "Elliott", "Sandy", "Evelyn", "Marnie", "Caroline", "Robin", "Pierre", "Pam", "Jodi",
            "Lewis", "Linus", "Marlon", "Willy", "Wizard", "Morris", "Jas", "Vincent", "Krobus", "Dwarf",
            "Gus", "Gunther", "George", "Demetrius", "Clint", "Baby", "Baby", "Bear"
        };

        public LandForSaleDetails leftLand;
        public string leftLandName;
        public LandForSaleDetails rightLand;
        public string rightLandName;
        private ILandManager landManager;
        private IModDataService modDataService;
        public FEForSaleMenu(ILoggerService olog,ILandManager landManager, IModDataService modDataService)
        {
            this.landManager = landManager;
            this.modDataService = modDataService;
            logger=olog;          
        }
        public void InitializeGUI()
        {
            string sButtonText = "Purchase";
            billboardTexture = Game1.temporaryContent.Load<Texture2D>($"LooseSprites\\SpecialOrdersBoard");

            width = 1352;
            height = 792;
            Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            xPositionOnScreen = (int)center.X;
            yPositionOnScreen = (int)center.Y;
            purchaseLandLeftButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 4 - 128, yPositionOnScreen + height - 140, (int)Game1.dialogueFont.MeasureString(sButtonText).X + 42, (int)Game1.dialogueFont.MeasureString(sButtonText).Y + 24), "")
            {
                myID = 0,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                upNeighborID = -99998,
                downNeighborID = -99998
            };
            purchaseLandRightButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width * 3 / 4 - 128, yPositionOnScreen + height - 140, (int)Game1.dialogueFont.MeasureString(sButtonText).X + 42, (int)Game1.dialogueFont.MeasureString(sButtonText).Y + 24), "")
            {
                myID = 1,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                upNeighborID = -99998,
                downNeighborID = -99998
            };
            //
            //  select land to post
            //
            if (landManager.LandForSale.Count == 1)
            {
                leftLandName = landManager.LandForSale[0];
            }
            else if (landManager.LandForSale.Count == 2)
            {
                leftLandName = landManager.LandForSale[0];
                rightLandName = landManager.LandForSale[1];
            }
            else if (landManager.LandForSale.Count > 0)
            {
                Random rnd = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Hour * 420);
                int iFirst = rnd.Next(0, landManager.LandForSale.Count);
                int iSecond = iFirst;
                while (iSecond == iFirst)
                {
                    iSecond = rnd.Next(0, landManager.LandForSale.Count);
                }
                leftLandName = landManager.LandForSale[iFirst];
                rightLandName = landManager.LandForSale[iSecond];
            }


            if (!string.IsNullOrEmpty(leftLandName) && modDataService.validContents.ContainsKey(leftLandName))
            {
                ExpansionPack oContent = modDataService.validContents[leftLandName];
                leftLand = new LandForSaleDetails { Thumbnail = oContent.ForSaleImage, DisplayName = oContent.DisplayName, Description = oContent.GetDescription(), Cost = oContent.Cost, ExpansionName = leftLandName, Vendor = oContent.Vendor };
            }
            if (!string.IsNullOrEmpty(rightLandName) && modDataService.validContents.ContainsKey(rightLandName))
            {
                ExpansionPack oContent = modDataService.validContents[rightLandName];
                rightLand = new LandForSaleDetails { Thumbnail = oContent.ForSaleImage, DisplayName = oContent.DisplayName, Description = oContent.GetDescription(), Cost = oContent.Cost, ExpansionName = rightLandName, Vendor = oContent.Vendor };
            }
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            Game1.playSound("bigSelect");
            UpdateButtons();
            if (Game1.options.SnappyMenus)
            {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }
        private Texture2D SizeThumbnail(Texture2D sourceTexture)
        {
            Texture2D tImage = sourceTexture;


            return tImage;
        }
        public virtual void UpdateButtons()
        {
            if (leftLand == null || (leftLand != null && leftLand.Cost > Game1.player.Money))
            {
                purchaseLandLeftButton.visible = false;
            }
            if (rightLand == null || (rightLand != null && rightLand.Cost > Game1.player.Money))
            {
                purchaseLandRightButton.visible = false;
            }

        }
        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(0);
            snapCursorToCurrentSnappedComponent();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            Game1.activeClickableMenu = new FEForSaleMenu(logger, landManager,modDataService);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            Game1.playSound("bigDeSelect");
            exitThisMenu();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (purchaseLandLeftButton.visible && purchaseLandLeftButton.containsPoint(x, y))
            {
                Game1.playSound("newArtifact");
                if (leftLand != null)
                {
                    //Content oContent = FEFramework.ContentPacks.ValidContents[leftLandName];
#if DEBUG
                    logger.Log("Purchased land: " + leftLandName, LogLevel.Info);
#endif
                    landManager.PurchaseLand(leftLandName,true,Game1.player.UniqueMultiplayerID);
                    UpdateButtons();
                    //try
                    //{
                    //    if (Game1.IsMultiplayer)
                    //    {
                    //        ChatSnippet oSnip = new ChatSnippet("Land bought", LocalizedContentManager.LanguageCode.en);
                    //        FEFramework.helper.Multiplayer.SendMessage(new ChatMessage() { message = new List<ChatSnippet> { { oSnip } }, language = LocalizedContentManager.LanguageCode.en }, "Info");
                    //        //Game1.mull.globalChatInfoMessage("New Land Purchase", Game1.player.Name, leftLand.DisplayName);
                    //    }
                    //    if (!string.IsNullOrEmpty(oContent.MailId))
                    //    {
                    //        monitor.Log("Sent purchase letter: " + oContent.MailId, LogLevel.Info);
                    //        Game1.MasterPlayer.mailForTomorrow.Add(oContent.MailId);
                    //    }
                    //    Game1.player.Money -= oContent.Cost;
                    //}
                    //catch (Exception ex)
                    //{
                    //    monitor.Log("Pick error: " + ex.ToString(), LogLevel.Error);
                    //}
                    //UpdateButtons();
                    //Game1.drawObjectDialogue("You will receive the deed to your new land in tommorrow's mail");
                    //FEFramework.LandBought(oContent.LocationName);
                }
            }
            else if (purchaseLandRightButton.visible && purchaseLandRightButton.containsPoint(x, y))
            {
                Game1.playSound("newArtifact");
                if (rightLand != null)
                {
                    //Content oContent = FEFramework.ContentPacks.ValidContents[rightLandName];
#if DEBUG
                    logger.Log("Purchased land: " + leftLandName, LogLevel.Info);
#endif
                    try
                    {
                        landManager.PurchaseLand(rightLandName,true, Game1.player.UniqueMultiplayerID);
                        UpdateButtons();

                        //    if (Game1.IsMultiplayer)
                        //    {
                        //        ChatSnippet oSnip = new ChatSnippet("Land bought", LocalizedContentManager.LanguageCode.en);
                        //        FEFramework.helper.Multiplayer.SendMessage(new ChatMessage() { message = new List<ChatSnippet> { { oSnip } }, language = LocalizedContentManager.LanguageCode.en }, "Info");
                        //        //Game1.mull.globalChatInfoMessage("New Land Purchase", Game1.player.Name, leftLand.DisplayName);
                        //    }
                        //    if (!string.IsNullOrEmpty(oContent.MailId))
                        //    {
                        //        monitor.Log("Sent purchase letter: " + oContent.MailId, LogLevel.Info);
                        //        Game1.player.mailForTomorrow.Add(oContent.MailId);
                        //    }
                        //    Game1.player.Money -= oContent.Cost;
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Pick error: " + ex.ToString(), LogLevel.Error);
                    }

                    //UpdateButtons();
                    //Game1.drawObjectDialogue("You will receive the deed to your new land in tommorrow's mail");
                    //FEFramework.LandBought(oContent.LocationName);
                }
            }
        }



        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            float oldScale = purchaseLandLeftButton.scale;
            purchaseLandLeftButton.scale = (purchaseLandLeftButton.bounds.Contains(x, y) ? 1.5f : 1f);
            if (purchaseLandLeftButton.scale > oldScale)
            {
                Game1.playSound("Cowboy_gunshot");
            }
            oldScale = purchaseLandRightButton.scale;
            purchaseLandRightButton.scale = (purchaseLandRightButton.bounds.Contains(x, y) ? 1.5f : 1f);
            if (purchaseLandRightButton.scale > oldScale)
            {
                Game1.playSound("Cowboy_gunshot");
            }
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            b.Draw(billboardTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 198, 338, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            string sLandForSale =I18n.LandForSale();

            SpriteText.drawStringWithScrollCenteredAt(b, sLandForSale, xPositionOnScreen + width / 2, Math.Max(10, yPositionOnScreen - 70), SpriteText.getWidthOfString(sLandForSale) + 1);

            string sButtonText = I18n.Purchase();

            if (leftLand != null)
            {
                DrawLandDetails(b, leftLand, xPositionOnScreen + 64 + 32);
            }
            if (rightLand != null)
            {
                DrawLandDetails(b, rightLand, xPositionOnScreen + 704 + 32);
            }
            if (purchaseLandLeftButton.visible)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), purchaseLandLeftButton.bounds.X, purchaseLandLeftButton.bounds.Y, purchaseLandLeftButton.bounds.Width, purchaseLandLeftButton.bounds.Height, (purchaseLandLeftButton.scale > 1f) ? Color.LightPink : Color.White, 4f * purchaseLandLeftButton.scale);
                Utility.drawTextWithShadow(b, sButtonText, Game1.dialogueFont, new Vector2(purchaseLandLeftButton.bounds.X + 25, purchaseLandLeftButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
            }
            if (purchaseLandRightButton.visible)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), purchaseLandRightButton.bounds.X, purchaseLandRightButton.bounds.Y, purchaseLandRightButton.bounds.Width, purchaseLandRightButton.bounds.Height, (purchaseLandRightButton.scale > 1f) ? Color.LightPink : Color.White, 4f * purchaseLandRightButton.scale);
                Utility.drawTextWithShadow(b, sButtonText, Game1.dialogueFont, new Vector2(purchaseLandRightButton.bounds.X + 25, purchaseLandRightButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
            }
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;
            if (!Game1.options.SnappyMenus || purchaseLandLeftButton.visible || purchaseLandRightButton.visible)
            {
                drawMouse(b);
            }
        }

        public KeyValuePair<Texture2D, Rectangle>? GetPortraitForRequester(string requester_name)
        {
            if (string.IsNullOrEmpty(requester_name))
            {
                return null;
            }
            for (int i = 0; i < emojiIndices.Length; i++)
            {
                if (emojiIndices[i] == requester_name)
                {
                    return new KeyValuePair<Texture2D, Rectangle>(ChatBox.emojiTexture, new Rectangle(i % 14 * 9, 99 + i / 14 * 9, 9, 9));
                }
            }
            return null;
        }

        public void DrawLandDetails(SpriteBatch b, LandForSaleDetails oDetails, int x)
        {
            bool dehighlight = false;

            SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
            Color font_color = Color.White;
            float shadow_intensity = 0.5f;
            float graphic_alpha = 1f;
            dehighlight = oDetails.Cost > Game1.player.Money;

            if (dehighlight)
            {
                font_color = Color.LightGray * 0.25f;
                shadow_intensity = 0f;
                graphic_alpha = 0.25f;
            }

            int header_y = yPositionOnScreen + 128;
            string order_name = oDetails.DisplayName;
            KeyValuePair<Texture2D, Rectangle>? drawn_portrait = GetPortraitForRequester(oDetails.Vendor);
            if (drawn_portrait.HasValue)
            {
                b.Draw(drawn_portrait.Value.Key, new Rectangle(x , header_y,60, 60), drawn_portrait.Value.Value, Color.White * graphic_alpha);
                //Utility.drawWithShadow(b, drawn_portrait.Value.Key, new Vector2(x, header_y), drawn_portrait.Value.Value, Color.White * graphic_alpha, 0f, Vector2.Zero, 4f, flipped: false, -1f, -1, -1, shadow_intensity * 0.6f);
            }
            if (oDetails.Thumbnail != null)
            {
                b.Draw(oDetails.Thumbnail, new Rectangle(x + 60, 300, 300, 200), new Rectangle(0, 0, oDetails.Thumbnail.Width, oDetails.Thumbnail.Height), Color.White * graphic_alpha);
                //Utility.drawWithShadow(b, oDetails.Thumbnail, new Vector2((x + 80), 300), new Rectangle(0, 0, 100, 80), Color.White * graphic_alpha, 0f, Vector2.Zero, 4f, flipped: false, -1f, -1, -1, shadow_intensity * 0.6f);
            }
            Utility.drawTextWithShadow(b, order_name, font, new Vector2((float)(x + 256) - font.MeasureString(order_name).X / 2f, header_y), font_color, 1f, -1f, -1, -1, shadow_intensity);
            string raw_description = oDetails.Description;
            string description = Game1.parseText(raw_description, font, 512);
            float height = font.MeasureString(description).Y;
            float scale = 1f;
            float max_height = 400f;
            while (height > max_height && !(scale <= 0.25f))
            {
                scale -= 0.05f;
                description = Game1.parseText(raw_description, font, (int)(512f / scale));
                height = font.MeasureString(description).Y;
            }
            Utility.drawTextWithShadow(b, description, font, new Vector2(x, yPositionOnScreen + 192), font_color, scale, -1f, -1, -1, shadow_intensity);
            if (oDetails.Cost > 0)
            {
                int due_date_y_position = yPositionOnScreen + 576;
                Utility.drawTextWithShadow(b, Game1.parseText(I18n.Cost(oDetails.Cost), Game1.dialogueFont, width - 128), Game1.dialogueFont, new Vector2(x + 48, due_date_y_position), font_color, 1f, -1f, -1, -1, shadow_intensity);
            }
        }
    }
}
