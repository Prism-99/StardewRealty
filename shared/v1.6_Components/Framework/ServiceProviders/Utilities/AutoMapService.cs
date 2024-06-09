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
                    logger.Log($"added mapToken '{method.Name}'", LogLevel.Debug);
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
                EntrancePatches = new Dictionary<string, EntrancePatch> { }
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
    }
}
