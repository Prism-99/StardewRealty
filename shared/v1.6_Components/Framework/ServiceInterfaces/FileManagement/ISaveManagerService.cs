using SDV_Realty_Core.Framework.ModFixes;
using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement
{
    /// <summary>
    /// Handles side saving and loading of Expansion data
    /// </summary>
    internal abstract class ISaveManagerService:IService
    {
        protected List<IModFix> preSaveFixes=new List<IModFix>();
        public SaveManagerV2 saveManager;
        internal abstract void LoadSaveFile(string loadContext, string saveFilename = null, bool loadOnly=false);
        internal void AddPreSaveFix(IModFix modFix)
        {
            preSaveFixes.Add(modFix);
        }
    }
}
