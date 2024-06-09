using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Objects;
using StardewValley.Menus;
using xTile.Dimensions;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;


namespace SDV_Realty_Core.Framework.Buildings.CheeseFactory
{
    public class FromagerieLocation : GameLocation
    {
        private static bool[] VatRunning = new bool[3];
        private bool ChestVerified = false;
        public FromagerieLocation() : base()
        {
            IsOutdoors = false;

        }
        public FromagerieLocation(string mapPath, string name) : base(mapPath, name)
        {
            InitVats();
        }


        private Dictionary<string, Tuple<string, int>> recipes = new Dictionary<string, Tuple<string, int>>
        {
            {"184", Tuple.Create( "424",0) },
            {"436", Tuple.Create("426",0) },
            {"438", Tuple.Create("426",2) },
            {"186", Tuple.Create("424",2) }
           // {"438","3013" },
            //{"186","3012" }
        };
        public struct CheeseVat
        {
            public int Quantity;
            public int MinutesRemaining;
            public bool Running;
            public string InputId;
            public string OutputId;
            public int OutputOldId;
            public int Quality;
        }
        public Dictionary<int, SDObject> GetBuildingProduction()
        {
            return new Dictionary<int, SDObject>
            {
                {0,cheeseVatList[0].Running? new SDObject(Vector2.Zero, cheeseVatList[0].OutputOldId.ToString()){ Quality=(int)cheeseVatList[0].Quality,Stack=cheeseVatList[0].Quantity,MinutesUntilReady=cheeseVatList[0].MinutesRemaining }:null },
                {1,cheeseVatList[1].Running? new SDObject(Vector2.Zero, cheeseVatList[1].OutputOldId.ToString()){Quality=(int)cheeseVatList[1].Quality,Stack=cheeseVatList[1].Quantity,MinutesUntilReady=cheeseVatList[1].MinutesRemaining  }:null },
                {2,cheeseVatList[2].Running? new SDObject(Vector2.Zero, cheeseVatList[2].OutputOldId.ToString()){Quality=(int)cheeseVatList[2].Quality,Stack=cheeseVatList[2].Quantity ,MinutesUntilReady=cheeseVatList[2].MinutesRemaining }:null }
            };
        }
        public CheeseVat[] cheeseVatList = new CheeseVat[] { new CheeseVat(), new CheeseVat(), new CheeseVat() };

        private void AddVat(Vector2 pos)
        {
            objects.Add(pos, new Chest(true, pos));
        }
        private void InitVats()
        {
            if (!objects.ContainsKey(new Vector2(-100, -100)))
                AddVat(new Vector2(-100, -100));
            if (!objects.ContainsKey(new Vector2(-200, -100)))
                AddVat(new Vector2(-200, -100));
            if (!objects.ContainsKey(new Vector2(-100, -200)))
                AddVat(new Vector2(-100, -200));
            VatRunning[0] = false;
            VatRunning[1] = false;
            VatRunning[2] = false;
        }
        //
        //   overrides
        //

        public override void updateWarps()
        {
            base.updateWarps();
            if (modData.ContainsKey(IModDataKeysService.FELocationName))
            {
                //FEFramework.FixWarps(this, modData[IModDataKeysService.FELocationName]);
            }
        }
        public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
        {
            base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);

            AlwaysUpdate(0);
            AlwaysUpdate(1);
            AlwaysUpdate(2);

        }
        public override void performTenMinuteUpdate(int timeOfDay)
        {
            base.performTenMinuteUpdate(timeOfDay);

            TenMinuteUpdateMachine(0, timeOfDay);
            TenMinuteUpdateMachine(1, timeOfDay);
            TenMinuteUpdateMachine(2, timeOfDay);
        }
        private void UpdateVatProduction(int machineId, int timeOfDay, string callSource)
        {
            //
            //  Check for idle vats and see if they have
            //  the required input
            //

            //
            //  High traffic event, only enable logging
            //  when really needed
            //
            //FEFramework.logger.Log($"UpdateVatProduction [{timeOfDay}][{callSource}.{machineId}]", StardewModdingAPI.LogLevel.Debug);

            Chest chest = machineId switch
            {
                0 => (Chest)objects[new Vector2(-100, -100)],
                1 => (Chest)objects[new Vector2(-200, -100)],
                2 => (Chest)objects[new Vector2(-100, -200)],
                _ => null
            };
            if (chest != null)
            {
                if (!cheeseVatList[machineId].Running)
                {
                    //
                    //  vat is not running, look for ingredients
                    //
                    foreach (string key in recipes.Keys)
                    {
                        List<Item> input = chest.Items.Where(p => p != null && p.ParentSheetIndex.ToString() == key && p.Stack >= 50).ToList();
                        Item inputItem = null; ;
                        int quality = 0;
                        bool hasInput = false;

                        for (quality = 0; quality < 5; quality++)
                        {
                            if (quality == 3) continue;
                            List<Item> qualifiedInput = input.Where(p => p is SDObject && ((SDObject)p).Quality == quality && p.Stack >= 50).ToList();
                            if (qualifiedInput.Any())
                            {
                                hasInput = true;
                                inputItem = qualifiedInput.First();
                                break;
                            }
                        }

                        if (hasInput)
                        {
                            cheeseVatList[machineId].Running = true;
                            cheeseVatList[machineId].OutputId = recipes[key].Item1;
                            cheeseVatList[machineId].OutputOldId = Convert.ToInt32(recipes[key].Item1.Replace("(O)", ""));
                            cheeseVatList[machineId].InputId = key;
                            cheeseVatList[machineId].MinutesRemaining = 60;
                            cheeseVatList[machineId].Quantity = 50;
                            cheeseVatList[machineId].Quality = recipes[key].Item2;
                            if (inputItem.Stack == 50)
                            {
                                chest.Items.Remove(inputItem);
                            }
                            else
                            {
                                inputItem.Stack = inputItem.Stack - 50;
                            }
                            break;
                        }
                    }
                }
                else
                {

                }
            }
        }
        private void AlwaysUpdate(int machineId)
        {
            //
            //  verify check contents for JSON shuffle
            //
            if (!ChestVerified)
            {
                for (int i = 0; i < 3; i++)
                {
                    Chest tChest = GetChest(i);
                }
                ChestVerified = true;
            }
            UpdateVatProduction(machineId, Game1.timeOfDay, "AlwaysUpdate");
        }
        public override void DayUpdate(int dayOfMonth)
        {
            base.DayUpdate(dayOfMonth);

        }
        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            UpdateVatStatus(0);
            UpdateVatStatus(1);
            UpdateVatStatus(2);
        }
        private void UpdateVatStatus(int machineId)
        {
            Chest chest = machineId switch
            {
                0 => (Chest)objects[new Vector2(-100, -100)],
                1 => (Chest)objects[new Vector2(-200, -100)],
                2 => (Chest)objects[new Vector2(-100, -200)],
                _ => null
            };

            if (VatRunning[machineId] != cheeseVatList[machineId].Running)
            {
                SetMachineState(machineId, cheeseVatList[machineId].Running);
            }
        }
        private Chest? GetChest(int machineId)
        {
            return machineId switch
            {
                0 => (Chest)objects[new Vector2(-100, -100)],
                1 => (Chest)objects[new Vector2(-200, -100)],
                2 => (Chest)objects[new Vector2(-100, -200)],
                _ => null
            };

        }
        private void TenMinuteUpdateMachine(int machineId, int timeOfDay)
        {
            Chest? chest = GetChest(machineId);

            if (chest != null)
            {
                if (cheeseVatList[machineId].Running)
                {
                    cheeseVatList[machineId].MinutesRemaining -= 10;

                    if (cheeseVatList[machineId].MinutesRemaining <= 0)
                    {
                        //
                        //  get output at end of process, in case mods change
                        //
                        SDObject outputItem = new SDObject(recipes[cheeseVatList[machineId].InputId].Item1, cheeseVatList[machineId].Quantity);
                        outputItem.Quality = recipes[cheeseVatList[machineId].InputId].Item2;

                        chest.addItem(outputItem);
                        cheeseVatList[machineId].InputId = null;
                        if (outputItem.ParentSheetIndex == 426)
                        {
                            Game1.stats.GoatCheeseMade += (uint)outputItem.Stack;
                        }
                        else
                        {
                            Game1.stats.CheeseMade += (uint)outputItem.Stack;
                        }

                        cheeseVatList[machineId].Running = false;
                    }

                }
            }
        }

        private void PopVatStatus(int vatIndex)
        {
            if (cheeseVatList[vatIndex].Running)
            {
                string prodName = Game1.objectData[cheeseVatList[vatIndex].OutputId].Name;
                Game1.activeClickableMenu = new DialogueBox($"Producing {cheeseVatList[vatIndex].Quantity}x{prodName} in {cheeseVatList[vatIndex].MinutesRemaining} minutes");
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox("Idle.  No ingredients.");
            }

        }
        public override bool performAction(string action, Farmer who, Location tileLocation)
        {
            switch (action)
            {
                case "OpenCheeseChestA":
                    OpenChest(0, who);
                    return false;
                case "OpenCheeseVatA":
                    PopVatStatus(0);
                    return false;
                case "OpenCheeseChestB":
                    OpenChest(1, who);
                    return false;
                case "OpenCheeseVatB":
                    PopVatStatus(1);
                    return false;
                case "OpenCheeseChestC":
                    OpenChest(2, who);
                    return false;
                case "OpenCheeseVatC":
                    PopVatStatus(2);

                    return false;
                default:
                    return base.performAction(action, who, tileLocation);
            }
        }
        public void OpenChest(int chestIndex, Farmer who)
        {
            Chest tmpChest = chestIndex switch
            {
                0 => (Chest)objects[new Vector2(-100, -100)],
                1 => (Chest)objects[new Vector2(-200, -100)],
                2 => (Chest)objects[new Vector2(-100, -200)],
                _ => null
            };
            if (tmpChest != null)
            {
                Game1.activeClickableMenu = new ItemGrabMenu(tmpChest.GetItemsForPlayer(who.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, tmpChest.grabItemFromInventory, null, tmpChest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, tmpChest.fridge.Value ? null : tmpChest, -1, tmpChest);
            }
        }

        public void SetMachineState(int machineId, bool isOn)
        {
            int iStart = machineId switch
            {
                0 => 5,
                1 => 11,
                2 => 17,
                _ => 0
            };

            VatRunning[machineId] = isOn;

            try
            {
                if (isOn)
                {
                    int tileIndex = 18;
                    //FEFramework.setMapTileIndex(map, iStart, 5, tileIndex, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 1, 5, tileIndex + 1, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 2, 5, tileIndex + 2, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 3, 5, tileIndex + 3, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 4, 5, tileIndex + 4, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 5, 5, tileIndex + 5, "Buildings", 5);

                    //tileIndex = 31;
                    //FEFramework.setMapTileIndex(map, iStart + 1, 6, tileIndex, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 2, 6, tileIndex + 1, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 3, 6, tileIndex + 2, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 4, 6, tileIndex + 3, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 5, 6, tileIndex + 4, "Buildings", 5);

                    //tileIndex = 44;
                    //FEFramework.setMapTileIndex(map, iStart + 2, 7, tileIndex, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 3, 7, tileIndex + 1, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 4, 7, tileIndex + 2, "Buildings", 5);
                }
                else
                {
                    //int tileIndex = 12;
                    //FEFramework.setMapTileIndex(map, iStart, 5, tileIndex, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 1, 5, tileIndex + 1, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 2, 5, tileIndex + 2, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 3, 5, tileIndex + 3, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 4, 5, tileIndex + 4, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 5, 5, tileIndex + 5, "Buildings", 5);

                    //tileIndex = 25;
                    //FEFramework.setMapTileIndex(map, iStart + 1, 6, tileIndex, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 2, 6, tileIndex + 1, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 3, 6, tileIndex + 2, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 4, 6, tileIndex + 3, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 5, 6, tileIndex + 4, "Buildings", 5);

                    //tileIndex = 38;
                    //FEFramework.setMapTileIndex(map, iStart + 2, 7, tileIndex, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 3, 7, tileIndex + 1, "Buildings", 5);
                    //FEFramework.setMapTileIndex(map, iStart + 4, 7, tileIndex + 2, "Buildings", 5);
                }
            }
            catch { }
        }
    }
}
