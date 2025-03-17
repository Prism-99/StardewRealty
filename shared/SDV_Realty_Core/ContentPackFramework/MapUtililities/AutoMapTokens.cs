using System;
using System.Collections.Generic;
using System.Linq;
using ContentPackFramework.MapUtililities;
using StardewRealty.SDV_Realty_Interface;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Minecarts;


namespace ContentPackFramework.Helpers
{
    internal class AutoMapTokens
    {
        //
        //  internal token utilities
        //
        private static string GetSubTokenValue(string tokenName, Dictionary<string, string> subTokens)
        {
            //
            //  get the value of a Tile property
            //
            return subTokens.Where(p => p.Key == tokenName).Select(p => p.Value).FirstOrDefault();
        }
        private static List<MapToken> GetTokenListByName(string tokenName, List<MapToken> tokens, string tokenValue = null)
        {
            //
            //  get the Token with the specified name (and TokenValue == tokenValue)
            //
            if (string.IsNullOrEmpty(tokenValue))
            {
                return tokens.Where(p => p.TokenName == tokenName).ToList();
            }

            return tokens.Where(p => p.TokenName == tokenName && p.TokenValue == tokenValue).ToList();
        }
        private static void DeleteToken(Point pos, string tokenName, List<MapToken> tokens)
        {
            //
            //  removes a used token from the list
            //
            IEnumerable<MapToken> token = tokens.Where(p => p.Position == pos && p.TokenName == tokenName);
            if (token.Count() == 1)
            {
                tokens.Remove(token.First());
            }
        }

        //
        //  map tokens
        //
        public static Tuple<bool, string> busin(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            cPac.BusIn = token.Position;

            return Tuple.Create(false, "busin");
        }
        public static Tuple<bool, string> boatdockin(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            cPac.BoatDockIn = token.Position;

            return Tuple.Create(false, "boatdockin");
        }
        public static Tuple<bool,string> trainstationin(MapToken token, ExpansionPack cPac, List<MapToken> tokens) 
        {
            cPac.TrainStationIn = token.Position;

            return Tuple.Create(false, "trainstationin");
        }
        public static Tuple<bool, string> minecart_internal(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            if (cPac.InternalMineCarts == null) 
                cPac.InternalMineCarts = new MinecartNetworkData();
            if (cPac.InternalMineCarts.Destinations == null)
                cPac.InternalMineCarts.Destinations = new();

            string[] args = token.TokenValue.Split(',');
            cPac.InternalMineCarts.Destinations.Add(new MinecartDestinationData()
            {
                DisplayName = args[0],
                //TargetLocation = cPac.LocationName,
                TargetTile = token.Position,
                Id = args[0].Replace(" ","")
            }
            );
            return Tuple.Create(false, "minecart_internal");
        }
        private static Tuple<bool, string> setup_minecart(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            MapToken landingToken = null;
            List<MapToken> actionTokens = new List<MapToken> { };

            if (token.TokenName == "minecart_action")
            {
                actionTokens.Add(token);
                landingToken = GetTokenListByName("minecart_landing", tokens, token.TokenValue).FirstOrDefault();
                DeleteToken(landingToken.Position, "minecart_landing", tokens);
            }
            else
            {
                landingToken = token;
            }
            List<MapToken> actionTokenList = GetTokenListByName("minecart_action", tokens, landingToken?.TokenValue);
            actionTokens.AddRange(actionTokenList);

            foreach (MapToken actionToken in actionTokens)
            {
                DeleteToken(actionToken.Position, actionToken.TokenName, tokens);
            }
            if (actionTokens.Count == 0)
            {
                return Tuple.Create(false, "No minecart_action token");
            }
            else if (landingToken == null)
            {
                return Tuple.Create(false, "No minecart_landing token");
            }
            else
            {
                cPac.MineCarts.Add(new ExpansionPack.MineCartSpot
                {
                    Exit = new EntranceWarp { NumberofPoints = 1, X = (int)landingToken.Position.X, Y = (int)landingToken.Position.Y },
                    MineCartActionPoints = actionTokens.Select(p => p.Position).ToList(),
                    MineCartDisplayName = GetSubTokenValue("displayname", landingToken.TokenProperties),
                    Condition = GetSubTokenValue("condition", landingToken.TokenProperties),
                    MineCartDirection = GetSubTokenValue("direction", landingToken.TokenProperties) ?? "down",
                    MineCartEnabled = true
                });
            }


            return Tuple.Create(false, "minecart_landing");
        }
        public static Tuple<bool, string> minecart_action(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            return setup_minecart(token, cPac, tokens);
        }
        public static Tuple<bool, string> minecart_landing(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            return setup_minecart(token, cPac, tokens);
        }
        public static Tuple<bool, string> treasure_area_end(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            //
            //  sould not be called. treasure_area_start should alway be first in map
            //
            return Tuple.Create(false, "treasure_area_start should not be called.");
        }
        public static Tuple<bool, string> treasure_area_start(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            //
            //  add a Treasure Area
            //
            bool bError = false;
            string result = "";
            string areaName = token.TokenValue.Trim();

            List<MapToken> endOfArea = tokens.Where(p => p.TokenName == "treasure_area_end" && p.TokenValue == areaName).ToList();

            switch (endOfArea.Count)
            {
                case 0:
                    result = $"No closure for 'treasure_area_start' [{areaName}]";
                    bError = true;
                    break;
                case 1:
                    // good entry
                    result = "treasure_area_start";
                    //  get map positions
                    Point endPos = endOfArea.First().Position;
                    // remove end token from token list
                    tokens.Remove(endOfArea.First());

                    if (endPos != Point.Zero)
                    {
                        Rectangle treasureArea = new Rectangle(token.Position.X, token.Position.Y, endPos.X - token.Position.X, endPos.Y - token.Position.Y);
                        int maxItems = 5;
                        ExpansionDetails.TreasureArea treasureAreaDetails = new ExpansionDetails.TreasureArea
                        {
                            Items = new List<ExpansionDetails.TreasureAreaItem> { },
                            Area = treasureArea,
                            MaxItems = maxItems
                        };

                        foreach (string tprop in token.TokenProperties.Keys)
                        {
                            switch (tprop.ToLower())
                            {
                                case "maxitems":
                                    if (int.TryParse(token.TokenProperties[tprop], out maxItems))
                                        treasureAreaDetails.MaxItems = maxItems;
                                    break;
                                case string a when a.StartsWith("items"):
                                    string[] arDetails = token.TokenProperties[tprop].Split(' ');

                                    switch (arDetails.Length)
                                    {
                                        case 2:
                                            treasureAreaDetails.Items.Add(new ExpansionDetails.TreasureAreaItem
                                            {
                                                ItemId = arDetails[1],
                                                Quantity = int.Parse(arDetails[0])
                                            });
                                            break;
                                        case >= 3:
                                            if (arDetails[1] == "Arch" || arDetails[1] == "Object")
                                            {
                                                treasureAreaDetails.Items.Add(new ExpansionDetails.TreasureAreaItem
                                                {
                                                    Quantity = int.Parse(arDetails[0]),
                                                    ItemId = arDetails[1] + " " + arDetails[2],
                                                    Condition = string.Join(" ", arDetails.Skip(3))
                                                });

                                            }
                                            else
                                            {
                                                treasureAreaDetails.Items.Add(new ExpansionDetails.TreasureAreaItem
                                                {
                                                    Quantity = int.Parse(arDetails[0]),
                                                    ItemId = arDetails[1],
                                                    Condition = string.Join(" ", arDetails.Skip(2))
                                                });
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                        if (treasureAreaDetails.Items.Any())
                        {
                            cPac.TreasureSpots.Add(areaName, treasureAreaDetails);
                        }
                        else
                        {
                            // no item details
                            result = $"No Item property for TreasureArea [{areaName}]";
                            bError = true;
                        }
                    }
                    else
                    {
                        result = $"treasure_area_start {areaName}: map points wrong";
                        bError = true;
                    }
                    break;
                default:
                    result = $"more than 1 treasure_area_end for '{areaName}'";
                    bError = true;
                    break;

            }

            return Tuple.Create(bError, result);
        }
        public static Tuple<bool, string> suspension_bridge(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            cPac.suspensionBridges.Add(token.Position);

            return Tuple.Create(false, "suspension_bridge");
        }
        public static Tuple<bool, string> fish_area_end(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            //
            //  sould not be called. fish_area_start should alway be first in map
            //
            return Tuple.Create(false, "fish_area_end should not be called.");
        }
        public static Tuple<bool, string> fish_area_start(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            //
            //  add a FishArea
            //
            bool bError = false;
            string result = "";
            string areaName = token.TokenValue.Trim();

            List<MapToken> endOfArea = tokens.Where(p => p.TokenName == "fish_area_end" && p.TokenValue == areaName).ToList();

            switch (endOfArea.Count)
            {
                case 0:
                    result = $"No closure for 'fish_area_start' [{areaName}]";
                    bError = true;
                    break;
                case 1:
                    // good entry
                    result = "fish_area_start";
                    //  get map positions
                    Point endPos = endOfArea.First().Position;
                    // remove end token from token list
                    tokens.Remove(endOfArea.First());

                    if (endPos != Point.Zero)
                    {
                        FishAreaDetails fdata = new FishAreaDetails
                        {
                            Position = new Rectangle(token.Position.X, token.Position.Y, endPos.X - token.Position.X, endPos.Y - token.Position.Y)
                        };
                        fdata.StockData = new List<FishStockData> { };

                        List<SpawnFishData> fishTypes = new List<SpawnFishData>();

                        foreach (string tprop in token.TokenProperties.Keys)
                        {
                            switch (tprop.ToLower())
                            {
                                case "autofill":
                                    //  auto fill the fish data from game inventory of fish
                                    if (bool.TryParse(token.TokenProperties[tprop], out bool bAtuo))
                                        fdata.AutoFill = bAtuo;
                                    break;
                                case "maxfishtypes":
                                    //  defines the max number of fish autofill can add
                                    if (int.TryParse(token.TokenProperties[tprop], out int maxfish))
                                        fdata.MaxFishTypes = maxfish;
                                    break;
                                case "displayname":
                                    //  sets the display name of the fish area
                                    fdata.DisplayName = token.TokenProperties[tprop];
                                    break;
                                case "spring":
                                case "summer":
                                case "fall":
                                case "winter":
                                    // seasonal fish stock values
                                    string[] arData = token.TokenProperties[tprop].Split(' ');
                                    string cleanSeason = SDVUtilities.GetCleanSeason(tprop);
                                    for (int i = 0; i < arData.Length - 1; i += 2)
                                    {
                                        if (i + 1 < arData.Length)
                                        {

                                            fishTypes.Add(new SpawnFishData
                                            {
                                                FishAreaId = areaName,
                                                ItemId = arData[i],
                                                IgnoreFishDataRequirements = true
                                            });

                                            if (float.TryParse(arData[i + 1], out float chance))
                                            {
                                                fdata.StockData.Add(new FishStockData
                                                {
                                                    Chance = chance,
                                                    FishId = arData[i],
                                                    Season = cleanSeason
                                                });
                                            }

                                        }
                                    }
                                    break;
                            }
                        }

                        if (cPac.FishAreas == null)
                            cPac.FishAreas = new Dictionary<string, FishAreaDetails> { };

                        cPac.FishAreas.Add(areaName, fdata);

                        if (fishTypes.Count > 0)
                        {
                            if (cPac.FishData == null)
                                cPac.FishData = new Dictionary<string, List<SpawnFishData>> { };

                            cPac.FishData.Add(areaName, fishTypes);
                        }
                    }
                    else
                    {
                        result = $"fish_area_start {areaName}: map points wrong";
                        bError = true;
                    }
                    break;
                default:
                    result = $"more than 1 fish_area_end for '{areaName}'";
                    bError = true;
                    break;

            }

            return Tuple.Create(bError, result);
        }
        public static Tuple<bool, string> bush(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            if (Int32.TryParse(token.TokenValue, out int iSize))
            {
                cPac.Bushes.Add(Tuple.Create(token.Position, iSize));
                return Tuple.Create(false, "bush");
            }

            return Tuple.Create(true, "no bush size");
        }

        public static Tuple<bool, string> shippingbin(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            cPac.ShippingBinLocation = new Vector2(token.Position.X, token.Position.Y);
            return Tuple.Create(false, "shippingbin");
        }

        public static Tuple<bool, string> northsign(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;

            EntrancePatch north = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("0"))
            {
                north = cPac.EntrancePatches["0"];
                exists = true;
            }

            north.Sign = new SignDetails
            {
                Position = new EntranceWarp { NumberofPoints = 1, X = token.Position.X, Y = token.Position.Y },
                UseFront = token.TokenProperties.ContainsKey("usefront"),
                UseSign = true
            };

            if (exists)
                cPac.EntrancePatches.Remove("0");

            cPac.EntrancePatches.Add("0", north);

            return Tuple.Create(error, "northsign");
        }
        public static Tuple<bool, string> eastsign(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;

            EntrancePatch east = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("1"))
            {
                east = cPac.EntrancePatches["1"];
                exists = true;
            }

            east.Sign = new SignDetails
            {
                Position = new EntranceWarp { NumberofPoints = 1, X = token.Position.X, Y = token.Position.Y },
                UseFront = token.TokenProperties.ContainsKey("usefront"),
                UseSign = true
            };

            if (exists)
                cPac.EntrancePatches.Remove("1");

            cPac.EntrancePatches.Add("1", east);

            return Tuple.Create(error, "eastsign");
        }
        public static Tuple<bool, string> southsign(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;

            EntrancePatch south = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("2"))
            {
                south = cPac.EntrancePatches["2"];
                exists = true;
            }

            south.Sign = new SignDetails
            {
                Position = new EntranceWarp { NumberofPoints = 1, X = token.Position.X, Y = token.Position.Y },
                UseFront = token.TokenProperties.ContainsKey("usefront"),
                UseSign = true
            };

            if (exists)
                cPac.EntrancePatches.Remove("2");

            cPac.EntrancePatches.Add("2", south);

            return Tuple.Create(error, "southsign");
        }
        public static Tuple<bool, string> westsign(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;

            EntrancePatch west = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("3"))
            {
                west = cPac.EntrancePatches["3"];
                exists = true;
            }

            west.Sign = new SignDetails
            {
                Position = new EntranceWarp { NumberofPoints = 1, X = token.Position.X, Y = token.Position.Y },
                UseFront = token.TokenProperties.ContainsKey("usefront"),
                UseSign = true
            };

            if (exists)
                cPac.EntrancePatches.Remove("3");

            cPac.EntrancePatches.Add("3", west);

            return Tuple.Create(error, "westsign");
        }
        public static Tuple<bool, string> north(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;
            string tokename = "north";

            var northTokens = tokens.Where(p => p.TokenName == tokename).ToList();
            //
            //  remove other north tokens from the token list
            //
            foreach (var vtoken in northTokens)
            {
                tokens.Remove(vtoken);
            }

            var vList = northTokens.Select(p => p.Position).ToList();

            vList.Add(token.Position);

            EntrancePatch north = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("0"))
            {
                north = cPac.EntrancePatches["0"];
                exists = true;
            }

            north.PatchSide = EntranceDirection.North;
            north.WarpIn = new EntranceWarp { NumberofPoints = vList.Count, X = vList.Min(p => p.X), Y = vList[0].Y };
            north.WarpOut = new EntranceWarp { NumberofPoints = vList.Count, X = vList.Min(p => p.X), Y = vList[0].Y - 1 };
            north.WarpOrientation = WarpOrientations.Horizontal;
            north.PathBlockX = vList.Min(p => p.X);
            north.PathBlockY = vList[0].Y;

            if (exists)
                cPac.EntrancePatches.Remove("0");

            cPac.EntrancePatches.Add("0", north);

            return Tuple.Create(error, tokename);
        }
        public static Tuple<bool, string> east(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;
            string tokename = "east";

            var eastTokens = tokens.Where(p => p.TokenName == tokename).ToList();
            //
            //  remove other south tokens from the token list
            //
            foreach (var vtoken in eastTokens)
            {
                tokens.Remove(vtoken);
            }

            var vList = eastTokens.Select(p => p.Position).ToList();

            vList.Add(token.Position);

            EntrancePatch east = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("1"))
            {
                east = cPac.EntrancePatches["1"];
                exists = true;
            }

            east.PatchSide = EntranceDirection.East;
            east.WarpIn = new EntranceWarp { NumberofPoints = vList.Count, X = vList[0].X, Y = vList.Min(p => p.Y) };
            east.WarpOut = new EntranceWarp { NumberofPoints = vList.Count, X = vList[0].X + 1, Y = vList.Min(p => p.Y) };
            east.WarpOrientation = WarpOrientations.Vertical;
            east.PathBlockX = vList[0].X;
            east.PathBlockY = vList.Min(p => p.Y);

            if (exists)
                cPac.EntrancePatches.Remove("1");

            cPac.EntrancePatches.Add("1", east);

            return Tuple.Create(error, tokename);
        }
        public static Tuple<bool, string> south(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;
            string tokename = "south";

            var southTokens = tokens.Where(p => p.TokenName == tokename).ToList();
            //
            //  remove other south tokens from the token list
            //
            foreach (var vtoken in southTokens)
            {
                tokens.Remove(vtoken);
            }

            var vList = southTokens.Select(p => p.Position).ToList();

            vList.Add(token.Position);

            EntrancePatch south = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("2"))
            {
                south = cPac.EntrancePatches["2"];
                exists = true;
            }

            south.PatchSide = EntranceDirection.South;
            south.WarpIn = new EntranceWarp { NumberofPoints = vList.Count, X = vList.Min(p => p.X), Y = vList[0].Y };
            south.WarpOut = new EntranceWarp { NumberofPoints = vList.Count, X = vList.Min(p => p.X), Y = vList[0].Y + 1 };
            south.WarpOrientation = WarpOrientations.Horizontal;
            south.PathBlockX = vList.Min(p => p.X);
            south.PathBlockY = vList[0].Y;

            if (exists)
                cPac.EntrancePatches.Remove("2");

            cPac.EntrancePatches.Add("2", south);

            return Tuple.Create(error, tokename);
        }
        public static Tuple<bool, string> west(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;
            string tokename = "west";

            var westTokens = tokens.Where(p => p.TokenName == tokename).ToList();
            //
            //  remove other south tokens from the token list
            //
            foreach (var vtoken in westTokens)
            {
                tokens.Remove(vtoken);
            }

            var vList = westTokens.Select(p => p.Position).ToList();

            vList.Add(token.Position);

            EntrancePatch west = new EntrancePatch { };
            bool exists = false;

            if (cPac.EntrancePatches.ContainsKey("3"))
            {
                west = cPac.EntrancePatches["3"];
                exists = true;
            }

            west.PatchSide = EntranceDirection.West;
            west.WarpIn = new EntranceWarp { NumberofPoints = vList.Count(), X = vList[0].X, Y = vList.Min(p => p.Y) };
            west.WarpOut = new EntranceWarp { NumberofPoints = vList.Count(), X = vList[0].X - 1, Y = vList.Min(p => p.Y) };
            west.WarpOrientation = WarpOrientations.Vertical;
            west.PathBlockX = vList[0].X;
            west.PathBlockY = vList.Min(p => p.Y);

            if (exists)
                cPac.EntrancePatches.Remove("3");

            cPac.EntrancePatches.Add("3", west);

            return Tuple.Create(error, tokename);
        }
        public static Tuple<bool, string> cavein(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;

            cPac.CaveEntrance.WarpOut = new EntranceWarp { NumberofPoints = 1, X = token.Position.X, Y = token.Position.Y };

            return Tuple.Create(error, "cavein");
        }
        public static Tuple<bool, string> caveout(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            bool error = false;

            cPac.CaveEntrance.WarpIn = new EntranceWarp { NumberofPoints = 1, X = token.Position.X, Y = token.Position.Y };

            return Tuple.Create(error, "caveout");
        }

    }
}
