using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using StardewModdingAPI;
using Prism99_Core;
using Prism99_Core.Utilities;
namespace ContentPackFramework.Helpers
{

    internal static class GamePatches
    {
        public static Dictionary<string, List<GamePatch>> Patches;
        private static Harmony harmony;
        private static SDVLogger logger;
        private static bool showComments = true;
  
        public static void Initialize(string UniqueID, SDVLogger olog)
        {
            logger = olog;
            Patches = new Dictionary<string, List<GamePatch>> { };
            harmony = new Harmony(UniqueID);
        }
        public static string GetResult(bool useHTML)
        {
            string sNewline = useHTML ? "<br>" : Environment.NewLine;

            StringBuilder sbDetails = new StringBuilder("Harmony Patches:" + sNewline);

            foreach (string group in Patches.Keys)
            {
                foreach (GamePatch oPatch in Patches[group])
                {
                    if (!string.IsNullOrEmpty(group))
                    {
                        sbDetails.Append(("Group: " + group + sNewline));
                    }
                    sbDetails.Append("Details:" + sNewline + GetPatchDetails(oPatch) + sNewline);
                    sbDetails.Append("Status: " + (oPatch.Failed ? "Failed" + sNewline + "Failure details: " + oPatch.FailureDetails + sNewline : "Applied" + sNewline));
                    sbDetails.Append(sNewline);
                }
            }


            return sbDetails.ToString();
        }
        public static string GetPatchDetails(GamePatch oPatch)
        {
            string details = $"original: {oPatch.Original.ReflectedType.Name}.{oPatch.Original.Name}, redirectedTo: {oPatch.Target.method.DeclaringType.Name}.{oPatch.Target.method.Name}";
            return $"{(oPatch.IsPrefix ? "Prefix" : "Postfix")} patch: { details}";
        }
        public static void AddPatch(GamePatch oPatch, string sPatchGroup)
        {
            oPatch.Applied = false;
            if (!Patches.ContainsKey(sPatchGroup))
            {
                Patches.Add(sPatchGroup, new List<GamePatch> { });
            }
            Patches[sPatchGroup].Add(oPatch);
        }

        public static void ApplyPatches(string sPatchGroup)
        {
            foreach (string key in Patches.Keys)
            {
                if (string.IsNullOrEmpty(sPatchGroup) || key == sPatchGroup)
                {
                    foreach (GamePatch oPatch in Patches[key])
                    {
                        if (!oPatch.Applied)
                        {
                            string details = "";
                            try
                            {
                                details = GetPatchDetails(oPatch);

                                string sFullDetails = $"{(oPatch.IsPrefix ? "Prefix" : "Postfix")} patch: { details}";
                                try
                                {
                                    oPatch.Applied = true;

                                    if (oPatch.Target.method.IsStatic)
                                    {
                                        if (oPatch.IsPrefix)
                                        {
                                            if (oPatch.Target.method.ReturnType == typeof(bool))
                                            {
                                                harmony.Patch(
                                                original: oPatch.Original,
                                                prefix: oPatch.Target
                                                );
                                            }
                                            else
                                            {
                                                oPatch.Failed = true;
                                                oPatch.FailureDetails = "Patch skipped.  The prefix method does return a bool and the patch would fail";

                                                logger.Log($"Patch skipped.  The prefix method does return a bool and the patch would fail", LogLevel.Error);
                                                logger.Log($"Patch details: {sFullDetails}", LogLevel.Error);
                                            }
                                        }
                                        else
                                        {
                                            harmony.Patch(
                                            original: oPatch.Original,
                                            postfix: oPatch.Target
                                            );
                                        }
                                        logger.Log($"Applied Harmony {sFullDetails}", LogLevel.Debug);
                                        if (showComments)
                                        {
                                            logger.Log($"Pacth purpose: {oPatch.Description}", LogLevel.Debug);
                                        }
                                    }
                                    else
                                    {
                                        oPatch.Failed = true;
                                        oPatch.FailureDetails = "Patch skipped.  Patch skipped. The redirected method is not static and the patch would fail";

                                        logger.Log($"Patch skipped. The redirected method is not static and the patch would fail", LogLevel.Error);
                                        logger.Log($"Patch details: {sFullDetails}", LogLevel.Error);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    logger.Log($"Patch failed. Patch details: {sFullDetails}", LogLevel.Error);
                                    oPatch.Failed = true;
                                    logger.LogError("GamePatches.ApplyPatch", ex);
                                }

                            }
                            catch (Exception ex)
                            {
                                logger.Log($"Error applying patch {sPatchGroup}:{oPatch.Description}", LogLevel.Error);
                                logger.LogError($"GamePatches.ApplyPatch", ex);
                            }
                        }

                    }
                }
            }
        }
    }

}