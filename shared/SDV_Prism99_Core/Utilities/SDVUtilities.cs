using System;
using System.Collections.Generic;

namespace Prism99_Core.Utilities
{
    internal class SDVUtilities
    {
        public static string GetCleanSeason(string season)
        {
            return season.ToLower() switch
            {
                "summer" => "Summer",
                "spring" => "Spring",
                "fall" => "Fall",
                "winter" => "Winter",
                _ => null
            };
        }

        public static Season GetGameSeason(string season)
        {
            return season.ToLower() switch{
                "spring"=> Season.Spring,
                "summer"=>Season.Summer,
                "fall"=>Season.Fall,
                "winter"=>Season.Winter,
                    _ => Season.Spring
            };
        }

        public static bool TryParseEnum<TEnum>(string value, out TEnum parsed) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out parsed))
            {
                return typeof(TEnum).IsEnumDefined(parsed);
            }
            return false;
        }
        public static string GetCurrencyText(int currency)
        {
            return currency switch
            {
                0 => " gold",
                1 => " star tokens",
                2 => " Qi coins",
                3 => " Qi gems",
                _ => " Unknown"
            };
        }
        public static string GetText(string content)
        {
            if (string.IsNullOrEmpty(content)) return null;

            string[] arLines = content.Split("\\n");
            List<string> textLines = new List<string> { };

            foreach (string line in arLines)
            {
                if (line.StartsWith("["))
                {
                    string[] arParts = line.Substring(1, line.Length - 2).Split(' ');
                    switch (arParts[0])
                    {
                        case "LocalizedText":
                            textLines.Add(Game1.content.LoadString(arParts[1]));
                            break;
                        default:
                            textLines.Add(line);
                            break;
                    }
                }
                else
                {
                    textLines.Add(line);
                }
            }

            return string.Join('\n', textLines);
        }

        internal static Vector2 GetVectorFromString(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                //
                //  check for comman delimited
                //
                string[] arParts = str.Split(',');
                if (arParts.Length == 2)
                {
                    return new Vector2(Convert.ToInt32(arParts[0]), Convert.ToInt32(arParts[0]));
                }
                //
                //  checkc for space delimited
                //
                arParts = str.Split(' ');
                if (arParts.Length == 2)
                {
                    return new Vector2(Convert.ToInt32(arParts[0]), Convert.ToInt32(arParts[0]));
                }
            }

            return Vector2.Zero;
        }
    }
}
