using SDV_Realty_Core.Framework.Buildings;
using StardewModdingAPI.Events;
using StardewValley.GameData.Buildings;
using System.Collections.Generic;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edit Data/Buildings to support custom buildings
    /// </summary>
    /// 
    internal class BuildingsDataProvider : IGameDataProvider
    {
        private Dictionary<string, ICustomBuilding> CustomBuildings;
        private IConfigService configService;
        public BuildingsDataProvider(Dictionary<string, ICustomBuilding> customBuildings, IConfigService configService)
        {
            CustomBuildings = customBuildings;
            this.configService = configService;
        }

        public override string Name => "Data/Buildings";

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (string buildingKey in CustomBuildings.Keys)
                {
                    //
                    //  trap errors so 1 bad building does not kill them all
                    //
                    try
                    {
                        BuildingData buildingData = CustomBuildings[buildingKey].BuildingDefinition();
                        if (configService.config.SkipBuildingConditions)
                        {
                            //
                            //  remove any build conditions
                            //
                            buildingData.BuildCondition = null;
                        }
                        asset.AsDictionary<string, BuildingData>().Data.Add(buildingKey, buildingData);
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Error adding custom building {buildingKey}", StardewModdingAPI.LogLevel.Error);
                        logger.LogError("Buildings.HandleEdit", ex);
                    }
                }
            });
        }

        public override void OnGameLaunched()
        {

        }
    }
}
