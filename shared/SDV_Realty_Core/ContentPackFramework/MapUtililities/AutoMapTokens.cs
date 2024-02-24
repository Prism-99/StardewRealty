using System;
using System.Collections.Generic;
using System.Linq;
using ContentPackFramework.MapUtililities;
using StardewRealty.SDV_Realty_Interface;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewValley.GameData.Locations;



namespace ContentPackFramework.Helpers
{
    internal partial class AutoMapTokens
    {
        //
        //  common
        //
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

            var endOfArea = tokens.Where(p => p.TokenName == "fish_area_end" && p.TokenValue == areaName).ToList();

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
#if v16
                        List<SpawnFishData> fishTypes = new List<SpawnFishData>();
#endif                    
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
#if v16
                                            fishTypes.Add(new SpawnFishData
                                            {
                                                FishAreaId = areaName,
                                                ItemId = arData[i],
                                                IgnoreFishDataRequirements = true
                                            });
#endif
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
#if v16
                        if (fishTypes.Count > 0)
                        {
                            if (cPac.FishData == null)
                                cPac.FishData = new Dictionary<string, List<SpawnFishData>> { };

                            cPac.FishData.Add(areaName, fishTypes);
                        }
#endif
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
