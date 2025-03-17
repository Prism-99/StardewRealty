using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders
{
    class WeedFarm : IFarmServiceProvider
    {
        public WeedFarm(IModDataService modDataService, IUtilitiesService utilitiesService)
        {
            this.modDataService = modDataService;
            this.utilitiesService = utilitiesService;
            serviceCharge = 0.1f;
        }
        public override string Key => "weed_farm";

        public override string DisplayValue => I18n.WF_Label();

        public override string TooltTip => I18n.WF_Label_TT();

        internal override bool PerformAction(GameLocation loctaion, string[] args, Farmer who, Point pos)
        {
            Picklocation(I18n.WF_Header(), RemoveWeeds, GetQuote);

            return true;
        }
        private string GetQuote(string expansion, int tiles)
        {
            return I18n.WF_Quote((int)(WeedCount(expansion) * serviceCharge));
        }
        private void RemoveWeeds(string expansionName)
        {
            GameLocation? selectedFarm = null;
            if (expansionName == "Farm")
                selectedFarm = Game1.getFarm();
            else
               if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation? expansion))
                selectedFarm = expansion;

            if (selectedFarm != null)
            {
                int weedCount = WeedCount(selectedFarm);
                int totalCost = (int)(weedCount * serviceCharge);
                if (totalCost > Game1.player.Money)
                {
                    utilitiesService.PopMessage(I18n.NoMoney(), IUtilitiesService.PopupType.Dialogue);
                }
                else
                {
                    List<Vector2> weedsToClear = selectedFarm.objects.Values.Where(p => p.HasContextTag("item_weeds")).Select(p => p.tileLocation.Value).ToList();

                    foreach (Vector2 vec in weedsToClear)
                    {
                        selectedFarm.objects.Remove(vec);
                    }

                    utilitiesService.PopMessage(I18n.WF_Total(weedsToClear.Count(), totalCost), IUtilitiesService.PopupType.Dialogue);
                    Game1.player.Money -= totalCost;
                }
            }
            else
            {
                loggerService.Log($"Unknown location to weed ({expansionName})", LogLevel.Error);
            }
        }

        private int WeedCount(string expansionName)
        {
            if (expansionName == "Farm")
                return WeedCount(Game1.getFarm());
            else if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation expansion))
                return WeedCount(expansion);

            return -1;
        }
        private int WeedCount(GameLocation expansion)
        {
            return expansion.objects.Values.Where(p => p.HasContextTag("item_weeds")).Count();
        }
    }
}
