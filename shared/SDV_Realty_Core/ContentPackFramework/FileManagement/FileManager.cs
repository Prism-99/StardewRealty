using System;
using System.Collections.Generic;
using HarmonyLib;
using Prism99_Core.PatchingFramework;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.Menus;


namespace SDV_Realty_Core.ContentPackFramework.FileManagement
{
    internal class FileManager
    {
        private static ILoggerService logger;
        private static ISaveManagerService _saveManagerService;
        public FileManager(ILoggerService olog, IPatchingService Patches, ISaveManagerService saveManagerService)
        {
            _saveManagerService= saveManagerService;
            logger = olog;
            Patches.patches.AddPatch(true, typeof(LoadGameMenu), "deleteFile",
                new Type[] { typeof(int) }, typeof(FileManager), nameof(deleteFile),
               "Catch game deletes to delete SDR data.", "File management");

        }

        internal static bool deleteFile(LoadGameMenu __instance, int which)
        {
            logger.Log($"LoadGameMenu.deleteFile called which {which}.", LogLevel.Debug);
            List<LoadGameMenu.MenuSlot> slots = (List<LoadGameMenu.MenuSlot>)Traverse.Create(__instance).Field("menuSlots").GetValue();
            logger.Log($"  slots={(slots == null ? -1 : slots.Count)}", LogLevel.Debug);
            LoadGameMenu.SaveFileSlot slot = slots[which] as LoadGameMenu.SaveFileSlot;
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
