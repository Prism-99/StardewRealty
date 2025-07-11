using StardewModdingAPI.Events;
using StardewValley.GameData.WorldMaps;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using Prism99_Core.Utilities;
using System.IO;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using StardewValley.Delegates;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using StardewModdingAPI;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/WorldMap to add Stardew Meadows map
    /// </summary>
    internal class WorldMapDataProvider : IGameDataProvider
    {
        private static IExpansionManager _expansionManager;
        private ILandManager _landManager;
        private IGridManager _gridManager;
        private IExitsService _exitsService;
        private IContentManagerService _contentManagerService;
        private IModHelperService _modHelperService;
        private IModDataService _modDataService;
        public WorldMapDataProvider(IModDataService modDataService, IUtilitiesService utilitiesService, IExpansionManager expansionManager, ILandManager landManager, IGridManager gridManager, IExitsService exitsService, IContentManagerService contentManagerService, IModHelperService modHelperService)
        {
            _expansionManager = expansionManager;
            _landManager = landManager;
            _gridManager = gridManager;
            _exitsService = exitsService;
            _contentManagerService = contentManagerService;
            _modHelperService = modHelperService;
            _modDataService = modDataService;
            //
            //  add map textures
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}no_world_map", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "no_world_map.png"));
            //
            // WorldMap background
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_bg_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_spring", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_bg_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_summer", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_bg_summer.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_fall", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_bg_fall.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_winter", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_bg_winter.png"));
            // land for sale
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForSale_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale_spring", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForSale_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale_summer", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForSale_summer.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale_fall", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForSale_fall.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale_winter", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForSale_winter.png"));
            // future land for sale (not enough expansion packs)
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForFuture_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture_spring", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForFuture_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture_summer", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForFuture_summer.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture_fall", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForFuture_fall.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture_winter", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ForFuture_winter.png"));
            // land coming for sale, when conditions are met
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ComingSoon_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon_spring", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ComingSoon_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon_summer", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ComingSoon_summer.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon_fall", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ComingSoon_fall.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon_winter", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "WorldMap", "WorldMap_ComingSoon_winter.png"));


            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_spring.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "meadows_valley_area_spring.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_summer.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "meadows_valley_area_summer.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_fall.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "meadows_valley_area_fall.png"));
            _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_winter.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "images", "meadows_valley_area_winter.png"));
            GameStateQuery.Register("prism99.advize.stardewrealty.ismeadows", HandleIsMeadowsQuery);
            // add GSQ to get location season based on location season
            //GameStateQuery.Register("prism99.advize.stardewrealty.LocationSeason", HandleGetLocationSeason);

            utilitiesService.ModHelperService.modHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object? sender, ButtonPressedEventArgs e)
        {

        }

        public override string Name => "Data/WorldMap";

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            Rectangle worldMapOverlay = new Rectangle(42, 50, 22, 18);
            e.Edit(asset =>
            {
                IDictionary<string, WorldMapRegionData> worldMapData = asset.AsDictionary<string, WorldMapRegionData>().Data;
                //
                //  Create a new WorldMapAreaData for the Meadows in
                //  the Valley WorldMapRegion
                //
                WorldMapAreaData meadowsData = new WorldMapAreaData
                {
                    Id = "prism99.advize.stardewrealty_StardewMeadows",
                    Condition = "!prism99.advize.stardewrealty.ismeadows",
                    ScrollText = I18n.WM_ScrollText(),
                    PixelArea = worldMapOverlay,
                    Textures = new List<WorldMapTextureData>
                    {
                        new WorldMapTextureData
                        {
                            Condition = "SEASON Spring",
                            Id = "spring",
                            Texture = $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_spring.png"
                        },
                        new WorldMapTextureData
                        {
                            Condition = "SEASON Summer",
                            Id = "summer",
                            Texture = $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_summer.png"
                        },
                        new WorldMapTextureData
                        {
                            Condition = "SEASON Fall",
                            Id = "fall",
                            Texture = $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_fall.png"
                        },
                        new WorldMapTextureData
                        {
                            Condition = "SEASON Winter",
                            Id = "winter",
                            Texture = $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area_winter.png"
                        }
                    },
                    //WorldPositions =new List<WorldMapAreaPositionData>
                    //{
                    //    new WorldMapAreaPositionData
                    //    {
                    //        //MapPixelArea=worldMapOverlay
                    //    }
                    //},
                    Tooltips = new List<WorldMapTooltipData>
                    {
                        new WorldMapTooltipData
                        {
                            Id = "Default",
                            Text =I18n.WM_ScrollText(),
                            DownNeighbor = "Forest/WizardTower",
                            UpNeighbor = "Desert/Default",
                            RightNeighbor = "Farm/Default",
                            //PixelArea=worldMapOverlay//new Rectangle(47,53,20,22)
                        }
                    }
                };
                //
                //  adjust world map game controller map
                //
                worldMapData["Valley"].MapAreas.Add(meadowsData);
                var forest = worldMapData["Valley"].MapAreas.Where(p => p.Id == "Forest");
                if (forest.Any())
                {
                    var wizardTower = forest.First().Tooltips.Where(p => p.Id == "WizardTower");
                    if (wizardTower.Any())
                    {
                        wizardTower.First().UpNeighbor = "prism99.advize.stardewrealty_StardewMeadows/Default";
                    }

                }
                var desert = worldMapData["Valley"].MapAreas.Where(p => p.Id == "Desert");
                if (desert.Any())
                {
                    desert.First().Tooltips[0].DownNeighbor = "prism99.advize.stardewrealty_StardewMeadows/Default";
                }
                var secretWoods = worldMapData["Valley"].MapAreas.Where(p => p.Id == "SecretWoods");
                if (secretWoods.Any())
                {
                    secretWoods.First().Tooltips[0].UpNeighbor = "prism99.advize.stardewrealty_StardewMeadows/Default";
                }


                int iForSale = 0;
                int iTotalForSale = _modDataService.LandForSale.Count;
                int iComingSoon = 0;
                int iComingSoonTotal = _expansionManager.expansionManager.farmExpansions.Values.Where(p => !p.Active && !_modDataService.LandForSale.Contains(p.Name)).Count();

                //
                //  add new WorldMapRegionData for Stardew Valley Meadows
                //
                //  Region is only active when the player is in the Meadows.
                //  This allows for viewing other farmers in the main map when not
                //  in the Meadows
                //
                //  Uses a custom GSQ query (prism99.advize.stardewrealty.ismeadows)
                //  to check the Condition
                //
                WorldMapRegionData myRegion = new WorldMapRegionData();
                myRegion.BaseTexture.Add(new WorldMapTextureData
                {
                    Id = "stardewrealty.worldmap",
                    Texture = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap"),
                    //Texture = SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}SDR{FEConstants.AssetDelimiter}WorldMap.png"),
                    MapPixelArea = new Rectangle(0, 0, 300, 180)
                });

                foreach (KeyValuePair<int, IMiniMapService.MiniMapEntry> entry in _modDataService.MiniMapGrid)
                {
                    string mapTexture = $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale";
                    if (!string.IsNullOrEmpty(entry.Value.TexturePath))
                        mapTexture = $"SDR{FEConstants.AssetDelimiter}WorldMap{FEConstants.AssetDelimiter}{entry.Value.Key}";

                    WorldMapAreaData miniData = new WorldMapAreaData
                    {
                        Id = $"{entry.Key}",
                        PixelArea = _gridManager.GetExpansionWorldMapLocation(entry.Key),
                        ScrollText = I18n.WM_ScrollText(),
                        Textures = new List<WorldMapTextureData>()
                        {
                            new WorldMapTextureData
                            {
                                Id = $"SDR.Mini{entry.Value.Key}.texture",
                                MapPixelArea = _gridManager.GetExpansionWorldMapLocation(entry.Key),
                                Texture = mapTexture
                            }
                        },
                        Tooltips = new List<WorldMapTooltipData>
                        {
                        }
                        //    WorldPositions = new List<WorldMapAreaPositionData> {
                        //new WorldMapAreaPositionData
                        //{
                        //    Id=$"SDR.FS.{iForSale}.mp",
                        //     LocationName= oPac.LocationName,
                        //      MapPixelArea=FEFramework.GetExpansionWorldMapLocation(oPac.LocationName)
                        //}
                        //}
                    };
                    WorldMapTooltipData toolTip = new WorldMapTooltipData
                    {
                        Id = "Default",
                        Text = entry.Value.DisplayName,
                        PixelArea = _gridManager.GetExpansionWorldMapLocation(entry.Key)

                    };
                    PopulateToolTip(entry, toolTip);
                    miniData.Tooltips.Add(toolTip);
                    myRegion.MapAreas.Add(miniData);
                }
                //
                //  add dumby area for Stardew Meadows
                //
                myRegion.MapAreas.Add(new WorldMapAreaData
                {
                    ScrollText = I18n.WM_ScrollText(),
                    WorldPositions = new List<WorldMapAreaPositionData>
                    {
                        new WorldMapAreaPositionData
                        {
                            LocationName = WarproomManager.StardewMeadowsLoacationName
                        }
                    }
                });
                //myRegion.MapAreas.Add(new WorldMapAreaData
                //{
                //    Id = $"SDR.Home",
                //    PixelArea = _gridManager.GetExpansionWorldMapLocation(-1),
                //    ScrollText = "Stardew Meadows",
                //    Textures = new List<WorldMapTextureData>()
                //                    {
                //                        new WorldMapTextureData
                //                        {
                //                            Id = $"SDR.Home.texture",
                //                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(-1),
                //                            Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale"
                //                        }
                //                    },
                //    Tooltips = new List<WorldMapTooltipData>
                //                    {
                //                        new WorldMapTooltipData
                //                        {
                //                            Id=$"SDR.Home.tt",
                //                            Text= "Home",
                //                            PixelArea=_gridManager.GetExpansionWorldMapLocation(-1)
                //                        }
                //                    }
                //    //    WorldPositions = new List<WorldMapAreaPositionData> {
                //    //new WorldMapAreaPositionData
                //    //{
                //    //    Id=$"SDR.FS.{iForSale}.mp",
                //    //     LocationName= oPac.LocationName,
                //    //      MapPixelArea=FEFramework.GetExpansionWorldMapLocation(oPac.LocationName)
                //    //}
                //    //}
                //});


                for (int gridId = 0; gridId < _modDataService.MaximumExpansions; gridId++)
                {

                    string leftNeighbour = _gridManager.GetNeighbourExpansionTTId(gridId, EntranceDirection.West) ?? "";
                    string rightNeighbour = _gridManager.GetNeighbourExpansionTTId(gridId, EntranceDirection.East) ?? "";
                    string upNeighbour = _gridManager.GetNeighbourExpansionTTId(gridId, EntranceDirection.North) ?? "";
                    string downNeighbour = _gridManager.GetNeighbourExpansionTTId(gridId, EntranceDirection.South) ?? "";
                    if (_modDataService.MapGrid.ContainsKey(gridId))
                    {
                        string locationKey = _modDataService.MapGrid[gridId];
                        if (_modDataService.validContents.TryGetValue(locationKey, out ExpansionPack contentPack))
                        {
                            string modId = contentPack.Owner?.Manifest.UniqueID ?? locationKey;
                            string seasonOverride = _expansionManager.expansionManager.farmExpansions[locationKey].GetSeasonOverride();

                            string displayName = contentPack.DisplayName;
                            //
                            //  add Expansion info to main world map, conidional on the
                            //  Player not being in the Meadows
                            //
                            //meadowsData.WorldPositions.Add(new WorldMapAreaPositionData
                            //{
                            //    Id = $"{modId}.{locationKey}.mp",
                            //    Condition = "!prism99.advize.stardewrealty.ismeadows",
                            //    LocationName = contentPack.LocationName,
                            //    MapPixelArea = _gridManager.GetExpansionMainMapLocation(gridId, 0, 53),
                            //    ScrollText = contentPack.DisplayName

                            //});
                            //worldMapData["Valley"].BaseTexture.Add(new WorldMapTextureData
                            //{
                            //    Id = contentPack.LocationName,
                            //    MapPixelArea = _gridManager.GetExpansionMainMapLocation(gridId, 0, 53),
                            //    SourceRect = new Rectangle(0, 0, 20, 10),
                            //    Texture = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[locationKey].WorldMapTexture}")
                            //});
                            //
                            //  add Expansion to Stardew Valley Meadows map
                            //
                            //
                            //  add any SeasonOverride to the Expansion name
                            //
                            if (!string.IsNullOrEmpty(seasonOverride))
                            {
                                displayName += $"\n[{seasonOverride}]";
                            }
                            //
                            //  Add area data for the Expansion
                            //
                            string mapTexture;

                            if (contentPack.isAdditionalFarm)
                            {
                                mapTexture = contentPack.WorldMapTexture;
                            }
                            else
                            {
                                mapTexture = contentPack.GetSeasonalWorldMapTexture(seasonOverride).Replace(".png", "", StringComparison.CurrentCultureIgnoreCase);
                            }
                            List<string>? locations = null;
                            string? locationName = contentPack.LocationName;
                            if (contentPack.SubLocations?.Count > 0)
                            {
                                locations = contentPack.SubLocations;
                                locations.Add(contentPack.LocationName);
                                locationName = null;
                            }
                            WorldMapAreaData expansionData = new WorldMapAreaData
                            {
                                Id = $"{gridId}",
                                Condition = "prism99.advize.stardewrealty.ismeadows",
                                PixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                                ScrollText = I18n.WM_ScrollText(),
                                Textures = new List<WorldMapTextureData>(),
                                Tooltips = new List<WorldMapTooltipData>
                                {
                                    new WorldMapTooltipData
                                    {
                                        Id = $"Default",
                                        Text = displayName,
                                        PixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                                        LeftNeighbor = leftNeighbour,
                                        RightNeighbor = rightNeighbour,
                                        UpNeighbor = upNeighbour,
                                        DownNeighbor = downNeighbour
                                    }
                                },
                                WorldPositions = new List<WorldMapAreaPositionData>
                                {
                                    new WorldMapAreaPositionData
                                    {
                                        Id = $"{locationKey}.mp",
                                        LocationName = locationName,
                                        LocationNames = locations,
                                        TileArea = Rectangle.Empty,
                                        MapPixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName)
                                    }
                                }
                            };
                            if (contentPack.SeasonalWorldMapTextures != null && contentPack.SeasonalWorldMapTextures.Count > 0)
                            {
                                foreach (var texture in contentPack.SeasonalWorldMapTextures)
                                {
                                    expansionData.Textures.Add(
                                    new WorldMapTextureData
                                    {
                                        Id = $"{texture.Key}",
                                        MapPixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                                        // game code suffixes texture path with '_season'
                                        //Texture = $"{FEConstants.AssetDelimiter}SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{Path.GetFileNameWithoutExtension( texture.Value.Replace($"_{texture.Key}","",StringComparison.CurrentCultureIgnoreCase))}" ,
                                        Texture = $"{FEConstants.AssetDelimiter}SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{Path.GetFileNameWithoutExtension(texture.Value)}",
                                        Condition = $"LOCATION_SEASON {locationKey} {texture.Key}"
                                    });
                                }
                            }
                            else
                            {
                                expansionData.Textures.Add(
                                   new WorldMapTextureData
                                   {
                                       Id = $"{modId}.area.{locationKey}.texture",
                                       MapPixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                                       Texture = mapTexture
                                       //Texture =SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}{locationKey}{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[locationKey].WorldMapTexture}")
                                   });
                            }
                            myRegion.MapAreas.Add(expansionData);
                        }
                        else
                        {
                            logger.LogDebug($"Missing grid expansion pack {locationKey}");
                        }
                    }
                    else if (iForSale < iTotalForSale)
                    {
                        //  add for sale square

                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"{gridId}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                            ScrollText = I18n.WM_ScrollText(),
                            Textures = new List<WorldMapTextureData>()
                            {
                                new WorldMapTextureData
                                {
                                    Id = $"SDR.FS.{iForSale}.texture",
                                    MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                    Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale"
                                }
                            },
                            Tooltips = new List<WorldMapTooltipData>
                            {
                                new WorldMapTooltipData
                                {
                                    Id=$"Default",
                                    Text= I18n.CheckMsgBd(),
                                    PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId),
                                    LeftNeighbor=leftNeighbour,
                                    RightNeighbor=rightNeighbour,
                                    UpNeighbor=upNeighbour,
                                    DownNeighbor=downNeighbour
                                }
                            }
                            //    WorldPositions = new List<WorldMapAreaPositionData> {
                            //new WorldMapAreaPositionData
                            //{
                            //    Id=$"SDR.FS.{iForSale}.mp",
                            //     LocationName= oPac.LocationName,
                            //      MapPixelArea=FEFramework.GetExpansionWorldMapLocation(oPac.LocationName)
                            //}
                            //}
                        });

                        iForSale++;
                    }
                    else if (iComingSoon < iComingSoonTotal)
                    {
                        //  add coming soon image
                        //
                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"{gridId}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                            ScrollText = I18n.WM_ScrollText(),
                            Textures = new List<WorldMapTextureData>()
                            {
                                new WorldMapTextureData
                                {
                                    Id = $"SDR.CS.{gridId}.texture",
                                    MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                    Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon"
                                }
                            },
                            Tooltips = new List<WorldMapTooltipData>
                            {
                                new WorldMapTooltipData
                                {
                                        Id=$"Default",
                                        Text=I18n.WM_Requirements(),
                                        PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId),
                                        LeftNeighbor=leftNeighbour,
                                        RightNeighbor=rightNeighbour,
                                        UpNeighbor=upNeighbour,
                                        DownNeighbor=downNeighbour

                                }
                            }
                        });

                        iComingSoon++;
                    }
                    else
                    {
                        //  future growth
                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"{gridId}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                            ScrollText = I18n.WM_ScrollText(),
                            Textures = new List<WorldMapTextureData>()
                            {
                                new WorldMapTextureData
                                {
                                    Id = $"SDR.FG.{gridId}.texture",
                                    MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                    Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture"
                                }
                            },
                            Tooltips = new List<WorldMapTooltipData>
                            {
                                new WorldMapTooltipData
                                {
                                        Id=$"Default",
                                        Text=I18n.WM_AddPacks(),
                                        PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId),
                                        LeftNeighbor=leftNeighbour,
                                        RightNeighbor=rightNeighbour,
                                        UpNeighbor=upNeighbour,
                                        DownNeighbor=downNeighbour
                                }
                            }

                        });
                    }
                }
                //
                //  add the Stardew Valley Meadows to the worldMaps
                //
                worldMapData.Add("prism99.advize.stardewrealty_StardewMeadows", myRegion);
                //
                //  add Stardew Valley Meadows to the Stardew Valley map
                //
                //worldMapData["Valley"].MapAreas.Add(meadowsData);
            });
        }

        public override void OnGameLaunched()
        {
        }

        private void PopulateToolTip(KeyValuePair<int, IMiniMapService.MiniMapEntry> key, WorldMapTooltipData toolTip)
        {
            if (key.Key >= 0)
            {
                toolTip.UpNeighbor = $"{key.Key - 1}/Default";
                toolTip.DownNeighbor = $"{key.Key + 1}/Default";
            }
            else
            {
                toolTip.UpNeighbor = $"{key.Key + 1}/Default";
                if (_modDataService.MiniMapGrid.Count > key.Key * -1)
                    toolTip.DownNeighbor = $"{key.Key - 1}/Default";
                if (key.Key > -3)
                    toolTip.LeftNeighbor = "2/Default";
                else
                    toolTip.LeftNeighbor = "3/Default";
            }
        }

        public static bool HandleIsMeadowsQuery(string[] query, GameStateQueryContext context)
        {
            GameLocation parent = context.Location;

            if (context.Location.GetParentLocation() != null)
                parent = context.Location.GetParentLocation();

            if (_expansionManager.expansionManager.farmExpansions.ContainsKey(parent.Name))
                return true;

            return parent.Name == WarproomManager.StardewMeadowsLoacationName;
        }
    }
}
