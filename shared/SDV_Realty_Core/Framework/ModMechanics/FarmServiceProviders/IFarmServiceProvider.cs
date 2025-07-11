using CustomMenuFramework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders
{
    /// <summary>
    /// Abstract class for all farm service providers (FSP).
    /// FSPs provide bulk farm maintenance services
    /// </summary>
    internal abstract class IFarmServiceProvider
    {
        protected IModDataService modDataService;
        protected IUtilitiesService utilitiesService;
        protected ILoggerService loggerService;
        protected float serviceCharge = 1f;

        public IFarmServiceProvider(IModDataService modDataService, IUtilitiesService utilitiesService)
        {
            this.modDataService = modDataService;
            this.utilitiesService = utilitiesService;
        }
        public IFarmServiceProvider(IModDataService modDataService, IUtilitiesService utilitiesService, ILoggerService loggerService) : this(modDataService, utilitiesService)
        {
            this.loggerService = loggerService;
        }

        public abstract string Key { get; }
        public abstract string DisplayValue { get; }
        public abstract string TooltTip { get; }

        internal abstract bool PerformAction(GameLocation loctaion, string[] args, Farmer who, Point pos);

        protected void Picklocation(string header, Action<string> selected, Func<string, int, string> getQuote)
        {
            GenericPickListMenu pickExpansionMenu = new GenericPickListMenu();
            List<KeyValuePair<string, string>> locations = modDataService.farmExpansions.Where(p => p.Value.Active).Select(p => new KeyValuePair<string, string>(p.Key, p.Value.DisplayName + " " + getQuote(p.Key, (int)(p.Value.map.DisplayWidth / Game1.tileSize * p.Value.map.DisplayHeight / Game1.tileSize)))).ToList();
            locations.Add(new KeyValuePair<string, string>("Farm", $"Farm {getQuote("Farm", (int)(Game1.getFarm().map.DisplayWidth / Game1.tileSize * Game1.getFarm().map.DisplayHeight / Game1.tileSize))}"));

            pickExpansionMenu.ShowPagedResponses(header, locations.OrderBy(p => p.Value).ToList(), delegate (string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    selected(value);
                }
            }, auto_select_single_choice: false);
        }
        protected bool IsTillable(GameLocation location, Vector2 tile)
        {
            return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
        }

        protected bool IsOccupied(GameLocation location, Vector2 tile)
        {
            if (location.terrainFeatures.ContainsKey(tile))
                return true;

            if (location.objects.ContainsKey(tile) || location.largeTerrainFeatures.Any((LargeTerrainFeature p) => p.Tile == tile))
                return true;

            if (!location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
                return true;


            if (location.terrainFeatures.TryGetValue(tile, out var feature) && !(feature is HoeDirt { crop: null }))
                return true;

            if (location.buildings.Any((Building building) => building.occupiesTile(tile)))
                return true;

            return false;
        }
    }
}
