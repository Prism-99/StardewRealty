using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal partial class DataProviderManager
    {
        //
        //  common code
        //
        private List<IGameDataProvider> dataProviders;
        private ContentPackLoader contentPacks;
        private Dictionary<string, object> externalReferences;
        private NPCGiftTastesDataProvider NPCTastes;
        private ILoggerService logger;
        //private FEConfig config;
        //private IModHelper helper;
        private SDRContentManager contentManager;


        public void On_GameLaunched(EventArgs e)
        {
            foreach (var provider in dataProviders)
            {
                provider.OnGameLaunched();
            }
        }
        internal void ConfigurationChanged(object[] args)
        {
            foreach (IGameDataProvider provider in dataProviders)
            {
                provider.ConfigChanged();
            }
        }
        /// <summary>
        /// Calls data providers to check for activations
        /// </summary>
        internal void CheckForActivations(EventArgs e)
        {
            foreach (IGameDataProvider provider in dataProviders)
            {
                provider.CheckForActivations();
            }
        }
        internal bool HandleDataEdit(AssetRequestedEventArgs e)
        {
            //
            //  main entry point for game call
            //
            IGameDataProvider handler = GetGameDataProvider(e.NameWithoutLocale.Name);

            if (handler == null)
                return false;

            handler.HandleEdit(e);

            return true;
        }
        private IGameDataProvider GetGameDataProvider(string dataType)
        {
            //
            //  find dataProvider for specified dataType
            //
            //  return null if none found
            //
            IEnumerable<IGameDataProvider> handler = dataProviders.Where(p => p.Handles(dataType));
            if (handler.Any())
                return handler.First();

            return null;

            //var found = dataProviders.Where(p => dataType == p.Name);
            //if (found.Any())
            //    return found.First();

            ////return null;
            //found = dataProviders.Where(p => dataType.StartsWith(p.Name));
            //if (found.Any())
            //    return found.First();

            //return null;
        }

    }
}