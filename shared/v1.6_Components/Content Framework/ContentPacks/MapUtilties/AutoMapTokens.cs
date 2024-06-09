using System;
using System.Collections.Generic;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using System.Linq;
using ContentPackFramework.MapUtililities;

namespace ContentPackFramework.Helpers
{
    internal partial class AutoMapTokens
    {
        //
        //  version 1.6
        //
        private static string GetSubTokenValue(string tokenName, Dictionary<string, string> subTokens)
        {
            //
            //  get the value of a Tile property
            //
            return subTokens.Where(p => p.Key == tokenName).Select(p => p.Value).FirstOrDefault();
        }
        private static List<MapToken> GetTokenListByName(string tokenName, List<MapToken> tokens, string tokenValue=null)
        {
            //
            //  get the Token with the specified name (and TokenValue == tokenValue)
            //
            if (string.IsNullOrEmpty(tokenValue))
            {
                return tokens.Where(p => p.TokenName == tokenName).ToList();
            }

            return tokens.Where(p => p.TokenName == tokenName && p.TokenValue==tokenValue).ToList();
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
        private static Tuple<bool, string> setup_minecart(MapToken token, ExpansionPack cPac, List<MapToken> tokens)
        {
            MapToken landingToken = null;
            List<MapToken> actionTokens = new List<MapToken> { };

            if (token.TokenName == "minecart_action")
            {
                actionTokens.Add(token);
                landingToken = GetTokenListByName("minecart_landing", tokens,token.TokenValue).FirstOrDefault();
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
    }
}
