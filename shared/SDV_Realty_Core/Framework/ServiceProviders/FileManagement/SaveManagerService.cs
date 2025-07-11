using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using StardewModdingAPI.Events;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ModFixes;
using StardewModdingAPI.Enums;
using System.Linq;
using SDV_Realty_Core.Framework.Locations;

namespace SDV_Realty_Core.Framework.ServiceProviders.FileManagement
{
    internal class SaveManagerService : ISaveManagerService
    {

        public override Type ServiceType => typeof(ISaveManagerService);
        private IUtilitiesService utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
           typeof(IGameEventsService),
           typeof(IUtilitiesService),typeof(IModDataService),
            typeof(IGridManager),typeof(IExpansionManager),
            typeof(IContentPackService),typeof(IExitsService),
            
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
            IGameEventsService eventsService = (IGameEventsService)args[0];
            utilitiesService = (IUtilitiesService)args[1];
            IModDataService modDataService = (IModDataService)args[2];
            IGridManager gridManager = (IGridManager)args[3];
            IExpansionManager expansionManager = (IExpansionManager)args[4];
            IContentPackService contentPackService = (IContentPackService)args[5];
            IExitsService exitsService = (IExitsService)args[6];

            saveManager = new SaveManagerV2();

            saveManager.Initialize(logger, utilitiesService,  gridManager, expansionManager, exitsService, modDataService);

            eventsService.AddSubscription(new SaveCreatedEventArgs(), HandleSaveCreated);
            eventsService.AddSubscription(new SaveCreatingEventArgs(), HandleSaveCreating);
            eventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), HandleLoadStage , 20);
            eventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), saveManager.Specialized_LoadStageChanged_Step2, 5);
            eventsService.AddSubscription(new ReturnedToTitleEventArgs(), saveManager.ExitedToTitle);
            eventsService.AddSubscription(new DayStartedEventArgs(), saveManager.DoDayStartedTasks);
            eventsService.AddSubscription(new SavingEventArgs(), HandleSaving);
            eventsService.AddSubscription(new SavedEventArgs(),HandleSaveCreated);
#if DEBUG
            eventsService.AddSubscription(new GameLaunchedEventArgs(), HandleGameLaunched);
#endif
            //
            //  add required load/save hooks
            //
            AddPreLoadHook(expansionManager.HandlePreLoad);
            AddPreLoadHook(saveManager.HandlePreload);

            AddPreSaveHook(saveManager.PreSave);

            //AddPostSaveHook(saveManager.PostSave);
            AddPostSaveHook(expansionManager.HandleGameSaved);
            //
            //  add pre-save fixes
            //
            // AddPreSaveFix(new SmallerFishPonds());
        }
        public void HandleSaveCreated(EventArgs e)
        {
            logger.Log($"SaveManagerService => HandleSaveCreated",LogLevel.Debug);
            PostSave();
            //saveManager.SaveCreated(e);
        }
        /// <summary>
        /// Receive LoadStageChanged smapi event
        /// to trigger PreloadHooks or LoadedHooks
        /// </summary>
        /// <param name="e"></param>
        private void HandleLoadStage(EventArgs e)
        {
            LoadStageChangedEventArgs stage = (LoadStageChangedEventArgs)e;

#if DEBUG
            bool haveMeadows = Game1.getLocationFromName(WarproomManager.StardewMeadowsLoacationName) != null;
            logger.Log($"Load Stage={stage.NewStage}, Meadows={haveMeadows}",LogLevel.Trace);
#endif

            if (stage.NewStage == LoadStage.CreatedInitialLocations || stage.NewStage == LoadStage.SaveAddedLocations)
            {
                foreach(var act in PreLoadHooks.OrderBy(p=>p.Item1))
                {
                    logger.Log($"Firing PreLoadHook {act.Item2.Method.DeclaringType.Name}.{act.Item2.Method.Name}", LogLevel.Debug);
                    act.Item2();
                }
                //saveManager.LoadSaveFile(stage.NewStage.ToString());
                //foreach (Action act in PostSaveHooks)
                //{
                //    logger.Log($"Firing PostSaveHook {act.Method.DeclaringType.Name}.{act.Method.Name}",LogLevel.Debug);
                //    act();
                //}
            }
            else if(stage.NewStage== LoadStage.Loaded)
            {
                //
                //  trigger LoadedHooks actions
                //
                foreach(Action act in LoadedHooks.OrderBy(p=>p.Item1).Select(p=>p.Item2))
                {
                    logger.Log($"Firing LoadedHook {act.Method.DeclaringType.Name}.{act.Method.Name}", LogLevel.Debug);
                    act();
                }
            }
        }
        /// <summary>
        /// Fired after locations are created, before the save is loaded
        /// </summary>
        //public void PreLoad()
        //{
        //    foreach (Action act in PreSaveHooks.OrderBy(p=>p.Item1).Select(p=>p.Item2))
        //    {
        //        logger.Log($"Firing PreLoadHook {act.Method.DeclaringType.Name}.{act.Method.Name}", LogLevel.Debug);
        //        act();
        //    }
        //}


        ///
        /// <summary>
        /// Fire PreSave events
        /// </summary>
        public override void PreSave()
        {
#if DEBUG
            logger.Log($"SaveManagerService => PreSave", LogLevel.Debug);
#endif
            //
            //  apply any pre-save data fixes
            //
            foreach (IModFix preSave in preSaveFixes)
            {
                if (preSave.ShouldApply(utilitiesService.ModHelperService.modHelper))
                {
                    preSave.ApplyFixes(utilitiesService.ModHelperService.modHelper, logger.Monitor, logger);
                }
            }
            foreach (Action act in PreSaveHooks.OrderBy(p=>p.Item1).Select(p=>p.Item2))
            {
#if DEBUG
                logger.Log($"Firing PreSaveHook {act.Method.DeclaringType?.Name??""}.{act.Method.Name}", LogLevel.Debug,1);
#endif
                act();
            }
        }
        /// <summary>
        /// Trigger PostSave actions
        /// </summary>
        public override void PostSave()
        {
#if DEBUG
            logger.Log("SaveManagerService => PostSave", LogLevel.Debug);
#endif
            //saveManager.PostSave();
            foreach (Action act in PostSaveHooks.OrderBy(p=>p.Item1).Select(p=>p.Item2))
            {
                logger.Log($"Firing PostSave action: {act.Method.DeclaringType?.Name??""}.{act.Method.Name}", LogLevel.Debug,1);
                act();
            }
        }
        private void HandleGameLaunched(EventArgs e)
        {
            //
            //  dump hook list
            //
#if DEBUG
            logger.Log($"==================", LogLevel.Debug);
            logger.Log($"|  Save Manager  |", LogLevel.Debug);
            logger.Log($"|  Event Hooks   |", LogLevel.Debug);
            logger.Log($"==================", LogLevel.Debug);
            logger.Log($"PreLoad Hooks:",LogLevel.Debug);
            foreach(var act in PreLoadHooks.OrderBy(p=>p.Item1).Select(p=>p.Item2))
            {
                logger.Log($"{act.Method.DeclaringType.Name}.{act.Method.Name}", LogLevel.Debug,1);
            }
            logger.Log($"Loaded Hooks:", LogLevel.Debug);
            foreach (var act in LoadedHooks.OrderBy(p => p.Item1).Select(p => p.Item2))
            {
                logger.Log($"{act.Method.DeclaringType?.Name??""}.{act.Method.Name}", LogLevel.Debug,1);
            }
            logger.Log($"PreSave Hooks:", LogLevel.Debug);
            foreach (var act in PreSaveHooks.OrderBy(p => p.Item1).Select(p => p.Item2))
            {
                logger.Log($"{act.Method.DeclaringType?.Name??""}.{act.Method.Name}", LogLevel.Debug,1);
            }
            logger.Log($"PostSave Hooks:", LogLevel.Debug);
            foreach (var act in PostSaveHooks.OrderBy(p => p.Item1).Select(p => p.Item2))
            {
                logger.Log($"{act.Method.DeclaringType?.Name??""}.{act.Method.Name}", LogLevel.Debug,1);
            }
#endif
        }
        /// <summary>
        /// Receive SaveCreating smapi event
        /// to trigger PreSaveHooks
        /// </summary>
        /// <param name="e"></param>
        /// 
        private void HandleSaving(EventArgs e)
        {
#if DEBUG
            logger.Log($"SaveManagerService => HandlingSaving", LogLevel.Debug);
#endif
            PreSave();
        }
        private void HandleSaveCreating(EventArgs e)
        {
#if DEBUG
            logger.Log($"SaveManagerService => HandleSaveCreating", LogLevel.Debug);
#endif
            PreSave();
            //saveManager.GameLoop_SaveCreating(e);
        }
        public override bool UnitTest()
        {
            string testFilename = @"Y:\Mods-min\StardewRealty-v16\pslocationdata\mp_368284331.xml";

            LoadSaveFile("unit test", testFilename, true);

            return base.UnitTest();
        }

        internal override void LoadSaveFile(string loadContext, string saveFilename = null, bool loadOnly = false)
        {
            saveManager.LoadSaveFile(loadContext, saveFilename, loadOnly);
        }
    }
}
