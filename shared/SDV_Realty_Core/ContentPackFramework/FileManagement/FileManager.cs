using System;
using System.Collections.Generic;
using HarmonyLib;
using Prism99_Core.PatchingFramework;
using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using StardewValley.Menus;
using static StardewValley.Menus.LoadGameMenu;

namespace SDV_Realty_Core.ContentPackFramework.FileManagement
{
    internal class FileManager
    {
        private static SDVLogger logger;
        private static ISaveManagerService _saveManagerService;
        public FileManager(SDVLogger olog, GamePatches Patches, ISaveManagerService saveManagerService)
        {
            _saveManagerService= saveManagerService;
            logger = olog;
            Patches.AddPatch(true, typeof(LoadGameMenu), "deleteFile",
                new Type[] { typeof(int) }, typeof(FileManager), nameof(deleteFile),
               "Catch game deletes to delete SDR data.", "File management");

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
                _saveManagerService.saveManager.DeleteSave(filenameNoTmpString);
            }

            return true;
        }
    }
}
