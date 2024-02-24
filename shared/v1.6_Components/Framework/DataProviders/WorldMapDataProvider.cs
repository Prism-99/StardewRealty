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
using SDV_Realty_Core.Framework.ServiceInterfaces;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/WorldMap to add Stardew Meadows map
    /// </summary>
    internal class WorldMapDataProvider : IGameDataProvider
    {
        private ContentPackLoader ContentPacks;
        private IExpansionManager _expansionManager;
        private ILandManager _landManager;
        private IGridManager _gridManager;
        private IExitsService _exitsService;
        public WorldMapDataProvider(ContentPackLoader cPacks, IExpansionManager expansionManager, ILandManager landManager, IGridManager gridManager, IExitsService exitsService)
        {
            ContentPacks = cPacks;
            _expansionManager = expansionManager;
            _landManager = landManager;
            _gridManager = gridManager;
            _exitsService = exitsService;
        }

        public override string Name => "Data/WorldMap";

        public override void CheckForActivations()
        {
            
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                int iForSale = 0;
                int iTotalForSale = _landManager.LandForSale.Count;
                int iComingSoon = 0;
                int iComingSoonTotal = _expansionManager.expansionManager.farmExpansions.Values.Where(p => !p.Active && !_landManager.LandForSale.Contains(p.Name)).Count();

                WorldMapRegionData myRegion = new WorldMapRegionData();
                myRegion.BaseTexture.Add(new WorldMapTextureData
                {
                    Id = "stardewrealty.worldmap",
                    Texture = SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Images{FEConstants.AssetDelimiter}WorldMap.png"),
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

                        string leftNeighbour =_exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.West) ?? "";
                        string rightNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.East) ?? "";
                        string upNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.North) ?? "";
                        string downNeighbour = _exitsService.GetNeighbourExpansionTTId(gridId, EntranceDirection.South) ?? "";
                        if (!string.IsNullOrEmpty(seasonOverride))
                        {
                            displayName += $"\n[{seasonOverride}]";
                        }
                        myRegion.MapAreas.Add(new WorldMapAreaData
                        {
                            Id = $"{modId}.{locationKey}",
                            PixelArea = _gridManager.GetExpansionWorldMapLocation(contentPack.LocationName),
                            ScrollText = "Stardew Valley Meadows",
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
                            ScrollText = "Stardew Valley Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"SDR.FS.{iForSale}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                            Texture =$"SDR{FEConstants.AssetDelimiter}Images{FEConstants.AssetDelimiter}WorldMap_ForSale.png"
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
                            ScrollText = "Stardew Valley Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"SDR.CS.{gridId}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                            Texture =$"SDR{FEConstants.AssetDelimiter}Images{FEConstants.AssetDelimiter}WorldMap_ComingSoon.png"
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
                            ScrollText = "Stardew Valley Meadows",
                            Textures = new List<WorldMapTextureData>()
                                    {
                                        new WorldMapTextureData
                                        {
                                            Id = $"SDR.FG.{gridId}.texture",
                                            MapPixelArea = _gridManager.GetExpansionWorldMapLocation(gridId),
                                            Texture =$"SDR{FEConstants.AssetDelimiter}Images{FEConstants.AssetDelimiter}WorldMap_ForFuture.png"
                                        }
                                    },
                            Tooltips = new List<WorldMapTooltipData>
                                    {
                                        new WorldMapTooltipData
                                        {
                                             Id=$"SDR.FG.{gridId}.tt",
                                              Text="Future Growth.  Add content packs.",
                                              PixelArea=_gridManager.GetExpansionWorldMapLocation(gridId)
                                        }
                                    }

                        });
                    }
                }

                //var expansionList = FEFramework.GetActiveFarmExpansions().Select(p => p.Name).ToList();
                //logger?.Log($"Loading {expansionList.Count} expansions for mapping.", LogLevel.Debug);

                //foreach (string key in expansionList)
                //{
                // }
                asset.AsDictionary<string, WorldMapRegionData>().Data.Add($"stardewrealty", myRegion);
                //try
                //{
                //    Texture2D? customTexture = null;
                //    if (oPac.Owner.HasFile(@"assets/WorldMap.png"))
                //    {
                //        try
                //        {
                //            customTexture = oPac.Owner.ModContent.Load<Texture2D>(@"assets/WorldMap.png");
                //        }
                //        catch { }
                //    }
                //    if (customTexture == null)
                //    {
                //        logger?.Log($"Could not load WorldMap.png for mod '{oPac.LocationName}', using default.", LogLevel.Error);
                //        customTexture = FEFramework.helper.ModContent.Load<Texture2D>(@"data/assets/WorldMap.png");
                //    }
                //    Rectangle rMapLoc = FEFramework.GetExpansionWorldMapLocation(oPac.LocationName);
                //    if (!rMapLoc.IsEmpty)
                //    {
                //        logger?.Log($"rMapLoc for {oPac.LocationName} is {(rMapLoc.IsEmpty ? "Empty" : rMapLoc)}", LogLevel.Debug);

                //        asset.AsImage().PatchImage(customTexture, new Rectangle(0, 0, customTexture.Width, customTexture.Height), new Rectangle(rMapLoc.X, rMapLoc.Y, customTexture.Width, customTexture.Height));

                //    }
                //}

                //catch (Exception ex)
                //{
                //    logger?.Log($"Could not load WorldMap.png from the mod folder, world map will not be patched.\n{ex}", LogLevel.Error);
                //}



                //var mdata=   asset.AsDictionary<string, WorldMapRegionData>().Data;
                //logger?.Log($"{mdata}", LogLevel.Debug);
            });
        }

       

        public override void OnGameLaunched()
        {
        }
    }
}
