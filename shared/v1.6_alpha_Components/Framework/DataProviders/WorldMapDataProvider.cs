using StardewModdingAPI.Events;
using StardewValley.GameData.WorldMaps;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using Prism99_Core.Utilities;
using System.IO;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using StardewValley.Delegates;
using SDV_Realty_Core.Framework.Locations;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/WorldMap to add Stardew Meadows map
    /// </summary>
    internal class WorldMapDataProvider : IGameDataProvider
    {
        private ContentPackLoader ContentPacks;
        private static IExpansionManager _expansionManager;
        private ILandManager _landManager;
        private IGridManager _gridManager;
        private IExitsService _exitsService;
        private IContentManagerService _contentManagerService;
        private IModHelperService _modHelperService;

        public WorldMapDataProvider(ContentPackLoader cPacks, IExpansionManager expansionManager, ILandManager landManager, IGridManager gridManager, IExitsService exitsService, IContentManagerService contentManagerService, IModHelperService modHelperService)
        {
            ContentPacks = cPacks;
            _expansionManager = expansionManager;
            _landManager = landManager;
            _gridManager = gridManager;
            _exitsService = exitsService;
            _contentManagerService = contentManagerService;
            _modHelperService = modHelperService;
            //
            //  add map textures
            //
            _contentManagerService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "worldmap.png"));
            _contentManagerService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "WorldMap_ForSale.png"));
            _contentManagerService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "WorldMap_ForFuture.png"));
            _contentManagerService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "WorldMap_ComingSoon.png"));
            _contentManagerService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area.png", Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "meadows_valley_area.png"));
            GameStateQuery.Register("prism99.advize.stardewrealty.ismeadows", HandleIsMeadowsQuery);

        }

        public override string Name => "Data/WorldMap";

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                IDictionary<string, WorldMapRegionData> worldMapData = asset.AsDictionary<string, WorldMapRegionData>().Data;
                //
                //  Create a new WorldMapAreaData for the Meadows in
                //  the Valley WorldMapRegion
                //
                WorldMapAreaData meadowsData = new WorldMapAreaData
                {
                    Id = "StardewMeadows",
                    Condition = "!prism99.advize.stardewrealty.ismeadows",
                    ScrollText = "Stardew Meadows",
                    PixelArea = new Rectangle(0, 53, 105, 22),
                    Textures = new List<WorldMapTextureData>
                    {
                        new WorldMapTextureData
                        {                                 
                            //MapPixelArea=new Rectangle(0,39,90,35),
                            Id="stardemeadows",
                            Texture=  $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}meadows_valley_area.png"
                        }
                    },

                    Tooltips = new List<WorldMapTooltipData>
                    {
                        new WorldMapTooltipData
                        {
                            Id="Default",
                            Text="Stardew Meadows"
                        }
                    }
                };

                int iForSale = 0;
                int iTotalForSale = _landManager.LandForSale.Count;
                int iComingSoon = 0;
                int iComingSoonTotal = _expansionManager.expansionManager.farmExpansions.Values.Where(p => !p.Active && !_landManager.LandForSale.Contains(p.Name)).Count();

                //
                //  add new WorldMapRegionData for Stardew Valley Meadows
                //
                //  Region is only active when the player is in the Meadows.
                //  This allows for viewing other farmers in the main map when not
                //  in the Meadows
                //
                //  The Expansions are only visible in the Meadows map when the 
                //  player is in the Meadows
                //
                //  Uses a custom GSQ query (prism99.advize.stardewrealty.ismeadows)
                //  to check the Condition
                //

                WorldMapRegionData myRegion = new WorldMapRegionData();
                myRegion.BaseTexture.Add(new WorldMapTextureData
                {
                    Id = "stardewrealty.worldmap",
                    Texture = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap.png"),
                    //Texture = SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}SDR{FEConstants.AssetDelimiter}WorldMap.png"),
                    MapPixelArea = new Rectangle(0, 0, 300, 180)
                });

                for (int gridId = 0; gridId < IGridManager.MaxFarms; gridId++)
                {
                    if (_gridManager.MapGrid.ContainsKey(gridId))
                    {
                        string locationKey = _gridManager.MapGrid[gridId];
                        ExpansionPack contentPack = ContentPacks.ValidContents[locationKey];
                        string modId = contentPack.Owner.Manifest.UniqueID;
                        string seasonOverride = _expansionManager.expansionManager.farmExpansions[locationKey].SeasonOverride;
                        string displayName = contentPack.DisplayName;
                        //
                        //  add Expansion info to main world map, conidional on the
                        //  Player not being in the Meadows
                        //
                        meadowsData.WorldPositions.Add(new WorldMapAreaPositionData
                        {
                            Id = $"{modId}.{locationKey}.mp",
                            Condition = "!prism99.advize.stardewrealty.ismeadows",
                            LocationName = contentPack.LocationName,
                            MapPixelArea = _gridManager.GetExpansionMainMapLocation(gridId, 0, 53),
                            ScrollText = contentPack.DisplayName

                        });
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

                        string leftNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.West) ?? "";
                        string rightNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.East) ?? "";
                        string upNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.North) ?? "";
                        string downNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.South) ?? "";
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
                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"{modId}.{locationKey}",
                            Condition = "prism99.advize.stardewrealty.ismeadows",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                            ScrollText = "Stardew Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"{modId}.area.{locationKey}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                                            Texture =SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{locationKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[locationKey].WorldMapTexture}")
                                            //Texture =SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}{locationKey}{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[locationKey].WorldMapTexture}")
                                        }
                                    },
                            Tooltips = new List<WorldMapTooltipData>
                                    {
                                        new WorldMapTooltipData
                                        {
                                             Id=$"{modId}.{locationKey}.tt",
                                              Text=displayName,
                                              PixelArea=_gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                                              LeftNeighbor=leftNeighbour,
                                              RightNeighbor=rightNeighbour,
                                              UpNeighbor=upNeighbour,
                                              DownNeighbor=downNeighbour
                                        }
                                    },
                            WorldPositions = new List<WorldMapAreaPositionData>
                                    {
                                        new WorldMapAreaPositionData
                                        {
                                            Id=$"{modId}.{locationKey}.mp",
                                            LocationName= contentPack.LocationName,
                                            MapPixelArea=_gridManager.GetExpansionWorldMapLocation(contentPack.LocationName)
                                        }
                                    }
                        });

                    }
                    else if (iForSale < iTotalForSale)
                    {
                        //  add for sale square

                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"SDR.FS.{iForSale}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                            ScrollText = "Stardew Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"SDR.FS.{iForSale}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                            Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale.png"
                                        }
                                    },
                            Tooltips = new List<WorldMapTooltipData>
                                    {
                                        new WorldMapTooltipData
                                        {
                                            Id=$"SDR.FS.{iForSale}.tt",
                                            Text="For Sale",
                                            PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId)
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
                            Id = $"SDR.CS.{gridId}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                            ScrollText = "Stardew Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"SDR.CS.{gridId}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                            Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon.png"
                                        }
                                    },
                            Tooltips = new List<WorldMapTooltipData>
                                    {
                                        new WorldMapTooltipData
                                        {
                                             Id=$"SDR.CS.{gridId}.tt",
                                              Text="Requirments have not been met.",
                                              PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId)
                                        }
                                    }

                        });

                        iComingSoon++;
                    }
                    else
                    {
                        //  future growth
                        //SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}SDR{FEConstants.AssetDelimiter}WorldMap_ForFuture.png")
                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"SDR.FG.{gridId}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                            ScrollText = "Stardew Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"SDR.FG.{gridId}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                            Texture =$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture.png"
                                        }
                                    },
                            Tooltips = new List<WorldMapTooltipData>
                                    {
                                        new WorldMapTooltipData
                                        {
                                             Id=$"SDR.FG.{gridId}.tt",
                                              Text="Future Growth. Add content packs.",
                                              PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId)
                                        }
                                    }

                        });
                    }
                }
                //
                //  add the Stardew Valley Meadows to the worldMaps
                //
                worldMapData.Add($"stardewrealty", myRegion);
                //
                //  add Stardew Valley Meadows to the Stardew Valley map
                //
                worldMapData["Valley"].MapAreas.Add(meadowsData);
            });
        }

        public override void OnGameLaunched()
        {
        }

        public static bool HandleIsMeadowsQuery(string[] query, GameStateQueryContext context)
        {
            if (_expansionManager.expansionManager.farmExpansions.ContainsKey(context.Location.Name))
                return true;

            if (context.Location.Name == WarproomManager.WarpRoomLoacationName)
                return true;

            return false;
        }

    }
}
