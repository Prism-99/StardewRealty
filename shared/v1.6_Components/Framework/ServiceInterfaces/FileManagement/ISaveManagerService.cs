using SDV_Realty_Core.Framework.ModFixes;
using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
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

        //utilitiesService.ModHelperService.modHelper.Events.GameLoop.Saved
        //utilitiesService.ModHelperService.modHelper.Events.GameLoop.Saving

        //utilitiesService.ModHelperService.modHelper.Events.GameLoop.SaveCreating
        internal readonly List<Tuple<int, Action>> PreSaveHooks = new ();
        //utilitiesService.ModHelperService.modHelper.Events.GameLoop.SaveLoaded
        internal readonly List<Tuple<int, Action>> LoadedHooks = new ();
        //utilitiesService.ModHelperService.modHelper.Events.GameLoop.SaveCreated
        internal readonly List<Tuple<int, Action>> PostSaveHooks = new ();

        internal readonly List<Tuple<int, Action>> PreLoadHooks = new ();

        public SaveManagerV2 saveManager;
       
        public void AddPreLoadHook(Action action,int priority=999)
        {
            PreLoadHooks.Add(Tuple.Create(priority, action));
        }
        public void AddLoadedHook(Action action, int priority = 999)
        {
            LoadedHooks.Add(Tuple.Create(priority, action));
        }
        public void AddPreSaveHook(Action action, int priority = 999)
        {
            PreSaveHooks.Add(Tuple.Create(priority, action));
        }
        public void AddPostSaveHook(Action action, int priority = 999)
        {
            PostSaveHooks.Add(Tuple.Create(priority, action));
        }

        public abstract void PreSave();
        public abstract void PostSave();
        
        internal abstract void LoadSaveFile(string loadContext, string saveFilename = null, bool loadOnly=false);
        internal void AddPreSaveFix(IModFix modFix)
        {
            preSaveFixes.Add(modFix);
        }
     
    }
}
