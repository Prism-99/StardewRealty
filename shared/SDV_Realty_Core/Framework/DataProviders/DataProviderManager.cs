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
        //private Dictionary<string, string> stringFromMaps;
        //public Maps mapDataProvider;
        private ChairTilesDataProviders chairTiles;
        private NPCGiftTastesDataProvider NPCTastes;
        private ILoggerService logger;
        private FEConfig config;
        private IModHelper helper;
        private SDRContentManager contentManager;
 

        public void On_GameLaunched(Object sender, GameLaunchedEventArgs e)
        {
            foreach (var provider in dataProviders)
            {
                provider.OnGameLaunched();
            }
        }
      
        public void AddChairTile(string tileRef, string seatDetails)
        {
            chairTiles.AddSeatTile(tileRef, seatDetails);
        }
        //public void AddMap(string mapPath, Map sourceMap)
        //{
        //    mapDataProvider.AddMap(mapPath, sourceMap);
        //}

        /// <summary>
        /// Calls data providers to check for activations
        /// </summary>
        internal void CheckForActivations(EventArgs e)
        {
            foreach (var provider in dataProviders)
            {
                provider.CheckForActivations();
            }
        }
        internal bool HandleDataEdit(AssetRequestedEventArgs e)
        {
            //
            //  main entry point for game call
            //
            var handler = GetGameDataProvider(e.NameWithoutLocale.Name);

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
            var handler = dataProviders.Where(p => p.Handles(dataType));
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