using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomMenuFramework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using static SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics.ICustomTrainService;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class WarproomService : IWarproomService
    {
        private IModDataService modDataService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentManagerService),typeof(IUtilitiesService),
            typeof(IMultiplayerService),typeof(IExpansionManager),
            typeof(IModDataService),typeof(IInputService),
            typeof(ISaveManagerService),typeof(IAutoMapperService),
            typeof(ICustomTrainService)

        };
        public override List<string> CustomServiceEventSubscripitions => new List<string>
        {

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
            IContentManagerService contentManager = (IContentManagerService)args[0];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[1];
            IMultiplayerService multiplayerService = (IMultiplayerService)args[2];
            IExpansionManager expansionManager = (IExpansionManager)args[3];
            modDataService = (IModDataService)args[4];
            IInputService inputService = (IInputService)args[5];
            ISaveManagerService saveManagerService = (ISaveManagerService)args[6];
            IAutoMapperService autoMapperService = (IAutoMapperService)args[7];
            ICustomTrainService customTrainService = (ICustomTrainService)args[8];

            warproomManager = new WarproomManager(logger, WarpToTheMeadows, autoMapperService, inputService, modDataService, contentManager, utilitiesService, multiplayerService, expansionManager, saveManagerService);
            utilitiesService.CustomEventsService.AddCustomSubscription("AddCaveEntrance", HandleAddCaveEntrance);
            GameLocation.RegisterTouchAction(ModKeyStrings.TAction_BackWoods, HandleBackWoodExit);
            customTrainService.AddTrainLocation(new ICustomTrainService.TrainDetails
            {
                ApproachingMessage = "train.arriving",
                LocationName = WarproomManager.StardewMeadowsLoacationName,
                TrainTime = modDataService.Config.TrainArrivalTime,
                CrossingY = 2792f,
                DropZones = new List<DropRange>
                {
                    new DropRange{ MinPos=3* Game1.tileSize ,MaxPos = 32* Game1.tileSize},
                    new DropRange{MinPos=44* Game1.tileSize,MaxPos=86* Game1.tileSize}
                }
            });
        }
        private void WarpToTheMeadows(Vector2 pos, string currentLocation, int destination = 0)
        {
            warproomManager.lastPlayerPoint = pos;
            warproomManager.lastPlayerLocation = currentLocation;

            if (destination == 0)
                Game1.warpFarmer(WarproomManager.StardewMeadowsLoacationName, WarproomManager.StardewMeadowsSoutherEntrancePoint.X, WarproomManager.StardewMeadowsSoutherEntrancePoint.Y, 3);
            else
                Game1.warpFarmer(WarproomManager.StardewMeadowsLoacationName, WarproomManager.EasternEntrance.X, WarproomManager.EasternEntrance.Y, 3);
        }
        private void HandleBackWoodExit(GameLocation location, string[] args, Farmer who, Vector2 pos)
        {
            if (!modDataService.MapGrid.Any())
            {
                // warp to Meadows
                WarpToTheMeadows(pos, Game1.player.currentLocation.Name,1);
            }
            else
            {
                // pop pickmenu to select destination
                string destination = modDataService.MapGrid[0];
                string displayName = modDataService.farmExpansions[destination].DisplayName;

                List<KeyValuePair<string, string>> locations = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("meadows","Stardew Meadows"),
                    new KeyValuePair<string, string>("expansion",displayName)
                };
                GenericPickListMenu pickLocationResponse = new GenericPickListMenu();
                pickLocationResponse.ShowPagedResponses(I18n.GridManagerSelectDestination(), locations, delegate (string value)
                {
                    if (value == "meadows")
                    {
                        WarpToTheMeadows(pos, Game1.player.currentLocation.Name,1);
                    }
                    else if (value == "expansion")
                    {
                        Point warp = new Point(modDataService.validContents[destination].EntrancePatches["1"].WarpIn.X, modDataService.validContents[destination].EntrancePatches["1"].WarpIn.Y);
                        Game1.warpFarmer(destination, warp.X, warp.Y, false);
                    }

                }, auto_select_single_choice: true);
            }
        }
        private void HandleAddCaveEntrance(object[] args)
        {
            warproomManager.AddCaveEntrance(args[0].ToString(), (Tuple<string, EntranceDetails>)args[1]);
        }
    }
}
