using System.Collections.Generic;
using StardewRealty.SDV_Realty_Interface;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Buildings;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using static StardewValley.Menus.CharacterCustomization;

namespace SDV_Realty_Core.Framework.Objects
{
    /// <summary>An API provided by Farm Expansion for other mods to use.</summary>
    public partial class ModApi : IStardewRealtyAPI
    {
        //
        //  version 1.6
        //
        /*********
        ** Properties
        *********/
        /// <summary>The Farm Expansion core framework.</summary>
        private IModDataService _modDataService;
        private ICustomEntitiesServices _customEntitiesServices;
        private ICustomEventsService _eventsService;
        /********* 
        ** Public methods
        *********/
        internal ModApi(ILoggerService olog,  SDRContentManager cMan, ICustomEntitiesServices customEntitiesServices, IModDataService modDataService, ICustomEventsService eventsService)
        {
            //Framework = framework;
            //content = ocontent;
            logger = olog;
            contentManager = cMan;
            _customEntitiesServices = customEntitiesServices;
            _customBuildingManager = customEntitiesServices.customBuildingService.customBuildingManager;
            _modDataService= modDataService;
            _eventsService = eventsService;
        }
    
        public bool IsSDRMachine(string machineName)
        {
            if (machineName.StartsWith("(BC)"))
            {
                return _customEntitiesServices.customMachineDataService.Machines.ContainsKey(machineName);
            }
            else
            {
                return _customEntitiesServices.customMachineDataService.Machines.ContainsKey($"(BC){machineName}");
            }
        }
        public List<SpawnFishData> GetFishData(string expansionName, string fishAreaId)
        {
            if (_modDataService.validContents.ContainsKey(expansionName))
            {
                if (_modDataService.validContents[expansionName].FishData.ContainsKey(fishAreaId))
                {
                    return _modDataService.validContents[expansionName].FishData[fishAreaId];
                }

            }

            return null;
        }

        /// <summary>Add a blueprint to all future carpenter menus for the farm area.</summary>
        /// <param name="blueprint">The blueprint to add.</param>
        public void AddFarmBluePrint(BuildingData blueprint)
        {

        }

        /// <summary>Add a blueprint to all future carpenter menus for the expansion area.</summary>
        /// <param name="blueprint">The blueprint to add.</param>
        public void AddExpansionBluePrint(BuildingData blueprint)
        {

        }
        public void SetExpansionSeasonOverride(string sExpName, string seasonOverride)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                AddCustomizationEntry(sExpName);
            }

            _modDataService.CustomDefinitions[sExpName].SeasonOverride = seasonOverride;
            _modDataService.farmExpansions[sExpName].seasonUpdate();
            _modDataService.farmExpansions[sExpName].updateSeasonalTileSheets();
            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", new object[] { });
        }
        //public List<SpawnFishData> GetFishData(string expansionName, string fishAreaId)
        //{

        //    if (FEFramework.ContentPacks.ValidContents.ContainsKey(expansionName))
        //    {
        //        if (FEFramework.ContentPacks.ValidContents[expansionName].FishData.ContainsKey(fishAreaId))
        //        {
        //            return FEFramework.ContentPacks.ValidContents[expansionName].FishData[fishAreaId];
        //        }
        //    }

        //    return null;
        //}
        //public List<string> GetFishAreaDetails(string expansionName)
        //{

        //    if (FEFramework.ContentPacks.ValidContents.ContainsKey(expansionName))
        //    {
        //        return FEFramework.ContentPacks.ValidContents[expansionName].FishAreas.Keys.ToList();
        //    }

        //    return null;
        //}



        public bool AddBuilding(string locationName, BuildingData structureForPlacement, Vector2 tileLocation, Farmer who, bool complete)
        {

            return false;
        }



        //public List<string> GetCustomBuildingList()
        //{
        //    var list = CustomBuildings.buildings.Select(p => p.Value.Name).ToList();
        //     return list;
        //}

    }
}

