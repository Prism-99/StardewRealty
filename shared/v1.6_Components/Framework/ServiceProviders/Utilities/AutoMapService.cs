using ContentPackFramework.Helpers;
using ContentPackFramework.MapUtililities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile.Layers;
using xTile.Tiles;
using xTile;
using StardewValley.GameData.Minecarts;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class AutoMapService : IAutoMapperService
    {
        private readonly Dictionary<string, MapTokenHandlerDelegate> MapTokenHandlers = new Dictionary<string, MapTokenHandlerDelegate>(StringComparer.OrdinalIgnoreCase);

        public override Type[] InitArgs => new Type[]
            {

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

            MethodInfo[] methods = typeof(AutoMapTokens).GetMethods(BindingFlags.Static | BindingFlags.Public);
            logger.Log($"mapToken Dictionary", LogLevel.Debug);
            logger.Log($"-------------------", LogLevel.Debug);
            foreach (MethodInfo method in methods)
            {
                try
                {
                    MapTokenHandlers[method.Name] = (MapTokenHandlerDelegate)Delegate.CreateDelegate(typeof(MapTokenHandlerDelegate), method);
#if DEBUG
                    logger.Log($"added mapToken '{method.Name}'", LogLevel.Debug);
#endif
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to initialize debug command " + method.Name + ".", ex);
                }
            }
        }

        internal override void AddMapToken(string token, MapTokenHandlerDelegate fnc)
        {
            //
            //  Add custom Token
            //
            if (MapTokenHandlers.ContainsKey(token))
            {
                logger.Log($"Map Token '{token}' already exists", LogLevel.Error);
            }
            else
            {
                MapTokenHandlers.Add(token, fnc);
            }
        }

        private Tuple<bool, bool, string> TryHandle(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            // result Tuple<handled,error,result>
            //
            if (string.IsNullOrWhiteSpace(token.TokenName))
            {
                return Tuple.Create(false, false, "Can't parse an empty Token.");
            }

            if (!MapTokenHandlers.TryGetValue(token.TokenName, out var handler))
            {
                return Tuple.Create(false, false, "Unknown token command '" + token + "'.");
            }
            try
            {
                Tuple<bool, string> hres = handler(token, cPac, tokens);
                return Tuple.Create(true, hres.Item1, hres.Item2);

            }
            catch (Exception ex)
            {
                logger.LogError("Error parsing mapping token '" + string.Join(" ", token) + "'.", ex);
                return Tuple.Create(true, true, $"Error parsing mapping token '{token}'. Err: {ex}");
            }
        }
        private List<Tuple<string, string>> GetTileTokens(Vector2 tilePos, List<Tuple<Vector2, string, string>> tokenList)
        {
            List<Tuple<Vector2, string, string>> tlist = tokenList.Where(p => p.Item1 == tilePos).ToList();
            List<Tuple<string, string>> cleanList = tokenList.Where(p => p.Item1 == tilePos).Select(p => Tuple.Create(p.Item2, p.Item3)).ToList();

            foreach (Tuple<Vector2, string, string> t in tlist)
            {
                tokenList.Remove(t);
            }
            return cleanList;
        }
        private bool IsValidToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) { return false; }
            return MapTokenHandlers.ContainsKey(token);
        }
        private Tuple<bool, bool, string> ParseToken(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {

            try
            {
                return TryHandle(token, cPac, tokens);
            }
            catch (Exception ex)
            {
                logger.LogError("AutoMapper.ExecuteCommand", ex);
                return Tuple.Create(true, true, "Error.  See Log for details.");
            }
        }
        internal override ExpansionPack ParseMap(Map sourceMap)
        {
            ExpansionPack result = new ExpansionPack
            {
                EntrancePatches = new Dictionary<string, EntrancePatch> { },
                InternalMineCarts = new MinecartNetworkData()
            };

            result.CaveEntrance = new EntranceDetails
            {
                WarpOut = new EntranceWarp { NumberofPoints = 1, X = -1, Y = -1 },
                WarpIn = new EntranceWarp { NumberofPoints = 1, X = -1, Y = -1 }
            };

            Layer backLayer = sourceMap.GetLayer("Back");
            List<MapToken> tokens = new List<MapToken> { };
            // position, token, value, subtokens

            if (backLayer != null)
            {
                //
                //  Scan for all valid tokens
                //
                for (int x = 0; x < backLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < backLayer.LayerHeight; y++)
                    {
                        if (backLayer.IsValidTileLocation(x, y))
                        {
                            Tile tile = backLayer.Tiles[x, y];
                            if (tile != null && tile.Properties != null)
                            {
                                foreach (string key in tile.Properties.Keys)
                                {
                                    if (IsValidToken(key.ToLower()))
                                    {
                                        Dictionary<string, string> subTokens = new Dictionary<string, string> { };
                                        //
                                        //  add all keys in the Tile
                                        //
                                        foreach (string subkey in tile.Properties.Keys)
                                        {
                                            if (!subkey.StartsWith("@") && subkey != key)
                                                subTokens.Add(subkey.ToLower(), tile.Properties[subkey].ToString());
                                        }
                                        tokens.Add(new MapToken { Position = new Point(x, y), TokenName = key.ToLower(), TokenValue = tile.Properties[key].ToString(), TokenProperties = subTokens });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                //
                //  Process all of the found tokens
                //
                while (tokens.Count > 0)
                {
                    MapToken token = tokens[0];
                    tokens.RemoveAt(0);

                    Tuple<bool, bool, string> parseRes = ParseToken(token, result, tokens);
                    if (parseRes.Item1 && !parseRes.Item2)
                    {
                        logger.Log($"map token: '{parseRes.Item3}'", LogLevel.Debug);
                    }
                    else if (parseRes.Item1 && parseRes.Item2)
                    {
                        logger.Log($"map token error: {parseRes.Item3}", LogLevel.Warn);
                    }
                }

                return result;
            }

            return null;
        }

        internal override bool MergeAutoMap(ExpansionPack autoParse, ExpansionPack destinationPack, Map mNewMap)
        {
            bool mapIsValid = true;

            destinationPack.ShippingBinLocation = autoParse.ShippingBinLocation;
            destinationPack.Bushes.AddRange(autoParse.Bushes);
            destinationPack.TreasureSpots = autoParse.TreasureSpots;

            if (autoParse.InternalMineCarts.Destinations?.Any() ?? false)
            {
                autoParse.InternalMineCarts.Destinations.ForEach(p => p.TargetLocation = destinationPack.LocationName);
                if (destinationPack.InternalMineCarts == null)
                {
                    destinationPack.InternalMineCarts = autoParse.InternalMineCarts;
                }
                else
                {
                    destinationPack.InternalMineCarts.Destinations = autoParse.InternalMineCarts.Destinations;
                }
                if (string.IsNullOrEmpty(destinationPack.InternalMineCarts.ChooseDestinationMessage))
                    destinationPack.InternalMineCarts.ChooseDestinationMessage = "Select your destination";
            }

            if (autoParse.CaveEntrance.WarpIn.X != -1 && autoParse.CaveEntrance.WarpIn.Y != -1)
            {
                logger.Log($"   auto-mapping CaveEntrance.WarpIn", LogLevel.Debug);
                if (destinationPack.CaveEntrance == null) { destinationPack.CaveEntrance = new EntranceDetails(); }
                destinationPack.CaveEntrance.WarpIn = autoParse.CaveEntrance.WarpIn;
            }
            else
            {
                logger.Log($"    missing CaveEntrance WarpIn tag", LogLevel.Warn);
            }
            if (autoParse.CaveEntrance.WarpOut.X != -1 && autoParse.CaveEntrance.WarpOut.Y != -1)
            {
                logger.Log($"   auto-mapping CaveEntrance.WarpOut", LogLevel.Debug);
                if (destinationPack.CaveEntrance == null) { destinationPack.CaveEntrance = new EntranceDetails(); }
                destinationPack.CaveEntrance.WarpOut = autoParse.CaveEntrance.WarpOut;
            }
            else
            {
                logger.Log($"    missing CaveEntrance WarpOut tag", LogLevel.Warn);
            }
            if (destinationPack.EntrancePatches == null)
            {
                destinationPack.EntrancePatches = new Dictionary<string, EntrancePatch> { };

            }
            if (autoParse.EntrancePatches.ContainsKey("0"))
            {
                logger.Log($"   auto-mapping North Entrance Patch", LogLevel.Debug);
                if (destinationPack.EntrancePatches.ContainsKey("0"))
                    destinationPack.EntrancePatches.Remove("0");

                destinationPack.EntrancePatches.Add("0", autoParse.EntrancePatches["0"]);
            }
            else
            {
                mapIsValid = false;
                logger.Log($"    missing North Entrance Patch", LogLevel.Warn);
            }
            if (autoParse.EntrancePatches.ContainsKey("1"))
            {
                logger.Log($"   auto-mapping East Entrance Patch", LogLevel.Debug);
                if (destinationPack.EntrancePatches.ContainsKey("1"))
                    destinationPack.EntrancePatches.Remove("1");

                destinationPack.EntrancePatches.Add("1", autoParse.EntrancePatches["1"]);
            }
            else
            {
                mapIsValid = false;
                logger.Log($"    missing East Entrance Patch", LogLevel.Warn);
            }
            if (autoParse.EntrancePatches.ContainsKey("2"))
            {
                logger.Log($"   auto-mapping South Entrance Patch", LogLevel.Debug);
                if (destinationPack.EntrancePatches.ContainsKey("2"))
                    destinationPack.EntrancePatches.Remove("2");

                destinationPack.EntrancePatches.Add("2", autoParse.EntrancePatches["2"]);
            }
            else
            {
                mapIsValid = false;
                logger.Log($"    missing South Entrance Patch", LogLevel.Warn);
            }
            if (autoParse.EntrancePatches.ContainsKey("3"))
            {
                logger.Log($"   auto-mapping West Entrance Patch", LogLevel.Debug);
                if (destinationPack.EntrancePatches.ContainsKey("3"))
                    destinationPack.EntrancePatches.Remove("3");

                destinationPack.EntrancePatches.Add("3", autoParse.EntrancePatches["3"]);
            }
            else
            {
                mapIsValid = false;
                logger.Log($"    missing West Entrance Patch", LogLevel.Warn);
            }
            if (autoParse.FishAreas != null)
            {
                foreach (var area in autoParse.FishAreas)
                {
                    if (destinationPack.FishAreas.ContainsKey(area.Key))
                    {
                        destinationPack.FishAreas.Remove(area.Key);
                    }

                    destinationPack.FishAreas.Add(area.Key, area.Value);
                }
            }

            destinationPack.FishData = autoParse.FishData;

            destinationPack.MineCarts = autoParse.MineCarts;

            if (autoParse.MineCarts.Any())
            {
                Layer backLayer = mNewMap.GetLayer("Buildings");

                if (backLayer == null)
                {
                    logger.Log($"    Could not find Buildings layer for minecart mapping.", LogLevel.Debug);
                }
                else
                {
                    foreach (ExpansionPack.MineCartSpot mineCartStation in autoParse.MineCarts)
                    {
                        foreach (Point aTile in mineCartStation.MineCartActionPoints)
                        {
                            backLayer.Tiles[aTile.X, aTile.Y].Properties.Add("Action", $"MinecartTransport Default {destinationPack.LocationName}.{mineCartStation.MineCartDisplayName}");
                        }
                        logger.Log($"    Added minecart Action square(s)", LogLevel.Debug);
                    }
                }
            }
            logger.Log($"    FishData: {(destinationPack.FishData == null ? 0 : destinationPack.FishData.Count())}", LogLevel.Debug);
            logger.Log($"   FishAreas: {(destinationPack.FishAreas == null ? 0 : destinationPack.FishAreas.Count())}", LogLevel.Debug);

            destinationPack.suspensionBridges = autoParse.suspensionBridges;
            logger.Log($"       Added {destinationPack.suspensionBridges.Count()} Suspension Bridges", LogLevel.Debug);

            return mapIsValid;
        }
    }
}
