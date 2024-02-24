using System;
using SDV_Realty_Core.ContentPackFramework.Objects;

namespace SDV_Realty_Core.ContentPackFramework.Helpers
{
    class ConditionParser
    {
        public static Requirements ParseConditions(string sConditions)
        {
            Requirements oReturn = new Requirements();

            string[] arKey = sConditions.Split('/');
            try
            {
                for (int iParam = 0; iParam < arKey.Length; iParam++)
                {
                    string[] arSubs = arKey[iParam].Split(' ');
                    switch (arSubs[0])
                    {
                        //case "A":
                        //    oReturn.A.Add(arSubs[1]);
                        //    break;
                        //case "d":
                        //    //oReturn.d = new string[arSubs.Length - 1];
                        //    // Array.Copy(arSubs, 1, oReturn.d, 0, arSubs.Length - 1);
                        //    for (int iItem = 1; iItem < arSubs.Length; iItem++)
                        //    {
                        //        oReturn.d.Add(arSubs[iItem]);
                        //    }
                        //    break;
                        //case "F":
                        //    oReturn.F = true;
                        //    break;
                        case "f":
                            oReturn.f.Add(arSubs[1], ParseInt(arSubs[2]));
                            break;
                        //case "r":
                        //    oReturn.r = ParseDouble(arSubs[1]);
                        //    break;
                        case "O":
                            oReturn.O = arSubs[1];
                            break;
                        //case "U":
                        //    oReturn.U = ParseInt(arSubs[1]);
                        //    break;
                        //case "V":
                        //    oReturn.v = arSubs[1];
                        //    break;
                        //case "w":
                        //    oReturn.w = arSubs[1];
                        //    break;
                        case "y":
                            oReturn.y = ParseInt(arSubs[1]);
                            break;
                        //case "z":
                        //    oReturn.z.Add(arSubs[1]);
                        //    break;
                        case "k":
                            oReturn.k.Add(Convert.ToInt32(arSubs[1]));
                            break;
                        case "Hn":
                            oReturn.Hn.Add(arSubs[1]);
                            break;
                        case "Hl":
                            oReturn.Hl.Add(arSubs[1]);
                            break;
                        //case "p":
                        //    oReturn.p.Add(arSubs[1]);
                        //    break;
                        case "o":
                            oReturn.o.Add(arSubs[1]);
                            break;
                        case "e":
                            oReturn.e.Add(Convert.ToInt32(arSubs[1]));
                            break;
                        case "q":
                            for (int iItem = 1; iItem < arSubs.Length; iItem++)
                            {
#if v16
                                oReturn.q.Add(arSubs[iItem]);
#else
                                oReturn.q.Add(Convert.ToInt32( arSubs[iItem]));                            
#endif
                            }

                            break;
                        //case "s":
                        //    for (int iItem = 1; iItem < arSubs.Length; iItem += 2)
                        //    {
                        //        oReturn.s.Add(arSubs[iItem], ParseInt(arSubs[iItem + 1]));
                        //    }
                        //    break;
                        //case "u":
                        //    for (int iItem = 1; iItem < arSubs.Length; iItem++)
                        //    {
                        //        oReturn.u.Add(ParseInt(arSubs[iItem]));
                        //    }
                        //    break;
                        case "*l":
                            for (int iItem = 1; iItem < arSubs.Length; iItem++)
                            {
                                oReturn._l.Add(arSubs[iItem]);
                            }
                            break;
                        case "*n":
                            for (int iItem = 1; iItem < arSubs.Length; iItem++)
                            {
                                oReturn._n.Add(arSubs[iItem]);
                            }
                            break;
                        //case "t":
                        //    oReturn.t_min = ParseInt(arSubs[1]);
                        //    oReturn.t_max = arSubs.Count() > 2 ? ParseInt(arSubs[2]) : 0;
                        //    break;
                        //case "x":
                        //    oReturn.x.Add(arSubs[1]);
                        //    break;
                        case "j":
                            oReturn.j = ParseInt(arSubs[1]);
                            break;
                        case "D":
                            oReturn.D = arSubs[1];
                            break;
                        case "b":
                            oReturn.b = ParseInt(arSubs[1]);
                            break;
                        case "n":
                            oReturn.n.Add(arSubs[1]);
                            break;
                        //case "H":
                        //    oReturn.H = true;
                        //    break;
                        //case "h":
                        //    oReturn.h = arSubs[1];
                        //    break;
                        case "m":
                            oReturn.m = ParseInt(arSubs[1]);
                            break;
                        //case "g":
                        //    oReturn.g = arSubs[1];
                        //    break;
                        case "C":
                            oReturn.C = true;
                            break;
                            //case "i":
                            //    oReturn.i = ParseInt(arSubs[1]);
                            //    break;

                    }
                    if (!oReturn.ParametersUsed.Contains(arSubs[0])) oReturn.ParametersUsed.Add(arSubs[0]);
                }
            }
            catch (Exception ex)
            {
                //StardewLogger.LogMessage("Key: '" + key + "', Error: " + ex.ToString(), "StardewEvent", true);
            }

            return oReturn;
        }

        private static int ParseInt(string sTestString)
        {
            if (int.TryParse(sTestString, out int iOut))
            {
                return iOut;
            }

            return 0;
        }
        private static double ParseDouble(string sTestString)
        {
            if (double.TryParse(sTestString, out double dOut))
            {
                return dOut;
            }

            return 0;
        }
    }
}
