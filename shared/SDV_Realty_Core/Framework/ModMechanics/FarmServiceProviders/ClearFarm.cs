using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders
{
    internal class ClearFarm : IFarmServiceProvider
    {
        public override string Key => "clear_farm";
        public override string DisplayValue => I18n.CF_Label();
        public override string TooltTip => I18n.CF_Label_TT();

        public ClearFarm(IModDataService modDataService, IUtilitiesService utilitiesService) : base(modDataService, utilitiesService)
        {
            serviceCharge = 1f;
        }

        internal override bool PerformAction(GameLocation location, string[] args, Farmer who, Point pos)
        {
            Picklocation(I18n.CF_Header(), ClearExpansionLand, GetQuote);

            return true;
        }
        private string GetQuote(string expansion, int tiles)
        {
            return I18n.CF_Quote((int)(ClearCount(expansion) * serviceCharge));
        }
        private int ClearCount(string expansion)
        {
            if (expansion == "Farm")
                return ClearCount(Game1.getFarm());
            else
                return ClearCount(modDataService.farmExpansions[expansion]);
        }
        private int ClearCount(GameLocation expansion)
        {
            return expansion.objects.Values.Where(p => p.Category == -999).Count() + expansion.largeTerrainFeatures.Count() + expansion.terrainFeatures.Count() + expansion.resourceClumps.Count();
        }
        private void ClearExpansionLand(string expansionName)
        {
            GameLocation? selectedFarm = null;
            if (expansionName == "Farm")
                selectedFarm = Game1.getFarm();
            else
               if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation? expansion))
                selectedFarm = expansion;

            if (selectedFarm != null)
            {
                int clearedCount = ClearCount(selectedFarm);
                int totalCost = (int)(clearedCount * serviceCharge);
                if (totalCost > Game1.player.Money)
                {
                    utilitiesService.PopMessage(I18n.NoMoney(), IUtilitiesService.PopupType.Dialogue);
                }
                else
                {
                    List<Vector2> objectsToClear = selectedFarm.objects.Values.Where(p => p.Category == -999).Select(p => p.tileLocation.Value).ToList();

                    foreach (Vector2 vec in objectsToClear)
                    {
                        selectedFarm.objects.Remove(vec);
                    }
                    selectedFarm.largeTerrainFeatures.Clear();
                    selectedFarm.terrainFeatures.Clear();
                    selectedFarm.resourceClumps.Clear();

                    utilitiesService.PopMessage(I18n.CF_Total(selectedFarm.DisplayName, totalCost), IUtilitiesService.PopupType.Dialogue);
                    Game1.player.Money -= totalCost;
                }
            }
            else
            {
                loggerService.Log($"Unknown location to clear ({expansionName})", LogLevel.Error);
            }
        }
    }
}
