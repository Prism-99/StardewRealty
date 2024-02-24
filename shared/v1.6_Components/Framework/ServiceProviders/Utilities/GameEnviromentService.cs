using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StardewModdingAPI.Events;
using StardewModdingAPI.Enums;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using Prism99_Core.MultiplayerUtils;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class GameEnviromentService : IGameEnvironmentService
    {
        private IModHelperService helper;
        private IConfigService config;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IConfigService),
            typeof(IModHelperService)
        };

        internal override string GamePath => environment.GamePath;

        internal override string ModPath => helper.modHelper.DirectoryPath;

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
            config= (IConfigService)args[1];
            helper = (IModHelperService)args[2];

            P99Core_MultiPlayer.Initialize(helper.modHelper, (SDVLogger)logger.CustomLogger);
            environment = new SDVEnvironment();
            environment.Initialize(helper, logger);
 
            //helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            //helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            eventsService.AddSubscription(new GameLaunchedEventArgs(), GameLaunched, 1000);
            eventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), LoadStageChanged,1000);
        }
        /// <summary>
        /// Get the details of the currently selected FarmType
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        public override FarmDetails GetFarmDetails(int farmId)
        {
            var olist = GameFarms?.Where(p => p.FarmType == farmId);

            if (olist != null && olist.Any())
            {
                return olist.First();
            }

            return null;
        }
        internal override bool AnyBlackListFarms()
        {
            if (BlackListedFarmMods != null)
            {
                foreach (string farmMod in BlackListedFarmMods)
                {
                    if (helper.ModRegistry.IsLoaded(farmMod))
                    {
                        logger.Log($"Blacklisted farm {farmMod} installed.  Side exits disabled", LogLevel.Trace);
                        return true;
                    }
                }
            }

            return false;
        }
        private void LoadStageChanged(EventArgs ep)
        {
            LoadStageChangedEventArgs e = (LoadStageChangedEventArgs)ep;
            if (e.NewStage == LoadStage.SaveAddedLocations || e.NewStage == LoadStage.CreatedInitialLocations)
            {
                //CaveEntrances.Clear();
                SetFarmProfile();
            }
        }
        public void SetFarmProfile()
        {
            logger.Log($"    Active farm type {Game1.whichFarm}", LogLevel.Debug);
            //
            //  check for patching exemptions
            //
            if (AnyBlackListFarms())
            {
                //
                //  blacklisted FarmMod do not patch the side exits
                //
                ActiveFarmProfile = new GridManager.FarmProfile
                {
                    PatchBackwoodExits = true,
                    PatchFarmExits = false,
                    UseFarmExit1 = false,
                    UseFarmExit2 = false,
                };
            }
            else
            {
                ActiveFarmProfile = new GridManager.FarmProfile
                {
                    PatchBackwoodExits = true,
                    PatchFarmExits = Game1.whichFarm == 0,
                    UseFarmExit1 = Game1.whichFarm == 0 && (config.config?.useNorthWestEntrance ?? true),
                    UseFarmExit2 = Game1.whichFarm == 0 && (config.config?.useSouthWestEntrance ?? true)
                };
            }
            ActiveFarmProfile.SDEInstalled = helper.ModRegistry.IsLoaded("FlashShifter.SVECode");

        }


        /// <summary>
        /// Loads required seed data files
        /// </summary>
        /// <param name="e"></param>
        public void GameLaunched(EventArgs e)
        {
            
            string sFileDef = Path.Combine(helper?.DirectoryPath ?? "", "data", "farms.json");
            try
            {
                string sContent = File.ReadAllText(sFileDef);
                GameFarms = JsonConvert.DeserializeObject<List<FarmDetails>>(sContent);
            }
            catch (Exception ex)
            {
                logger?.Log("error: " + ex.ToString(), LogLevel.Error);
                GameFarms = new List<FarmDetails> { };
            }

            //
            //  load other locations
            //
            sFileDef = Path.Combine(helper?.DirectoryPath ?? "", "data", "others.json");
            try
            {
                string sContent = File.ReadAllText(sFileDef);
                OtherLocations = JsonConvert.DeserializeObject<Dictionary<string, FarmDetails>>(sContent);
            }
            catch (Exception ex)
            {
                logger?.Log("error: " + ex.ToString(), LogLevel.Error);
                OtherLocations = new Dictionary<string, FarmDetails> { };
            }
            //
            //  load farm blacklist
            //BlackListedFarmMods
            sFileDef = Path.Combine(helper?.DirectoryPath ?? "", "data", "farm_mod_blacklist.json");
            try
            {
                string sContent = File.ReadAllText(sFileDef);
                BlackListedFarmMods =JsonConvert.DeserializeObject<List<string>>(sContent);
            }
            catch (Exception ex)
            {
                logger?.Log("error: " + ex.ToString(), LogLevel.Error);
                BlackListedFarmMods = new List<string> { };
            }
        }
    }
}
