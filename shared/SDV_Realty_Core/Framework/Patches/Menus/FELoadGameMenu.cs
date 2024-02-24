using HarmonyLib;
using Prism99_Core.Utilities;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;
using static StardewValley.Menus.LoadGameMenu;
using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces;

namespace SDV_Realty_Core.Framework.Patches.Menus
{
    internal  class FELoadGameMenu
    {
        private static SDVLogger logger;
        private static ISaveManagerService saveManagerService;

        public FELoadGameMenu(SDVLogger olog, ISaveManagerService saveManager)
        {
            logger = olog;
            saveManagerService = saveManager;
        }
        internal static bool deleteFile(LoadGameMenu __instance, int which)
        {
            logger.Log($"LoadGameMenu.deleteFile called which {which}.", LogLevel.Debug);
            List<MenuSlot> slots = (List<MenuSlot>)Traverse.Create(__instance).Field("menuSlots").GetValue();
            logger.Log($"  slots={(slots == null ? -1 : slots.Count)}", LogLevel.Debug);
            SaveFileSlot slot = slots[which] as SaveFileSlot;
            if (slot != null)
            {
                string filenameNoTmpString = slot.Farmer.slotName;
                logger.Log($"File slot name: {filenameNoTmpString}", LogLevel.Debug);
                saveManagerService.saveManager.DeleteSave(filenameNoTmpString);
            }

            return true;
        }
    }
}
