using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewModdingAPI.Events;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/CraftingRecipes to support custom objects
    /// Uses CustomBigCraftableManager for source data
    /// </summary>
    internal class CraftingRecipesDataProvider : IGameDataProvider
    {
        ICustomEntitiesServices customEntitiesServices;
        IUtilitiesService utilitiesService;
        IModDataService modDataService;
        public CraftingRecipesDataProvider(IModDataService modDataService,ICustomEntitiesServices customEntitiesServices, IUtilitiesService utilitiesService)
        {
            this.customEntitiesServices = customEntitiesServices;
            this.utilitiesService = utilitiesService;
            this.modDataService = modDataService;
        }
        public override string Name => "Data/CraftingRecipes";

        public override void CheckForActivations()
        {
            //
            //  get a list of potential activations
            //
            List<KeyValuePair<string, CustomBigCraftableData>> potential = customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables.Where(p => !p.Value.Added && !string.IsNullOrEmpty(p.Value.Conditions)).ToList();
            bool haveNew = false;
            //
            //  check candidates to see if they can be activated
            //
            foreach (KeyValuePair<string, CustomBigCraftableData> recipe in potential)
            {
                if (Game1.hasLoadedGame && GameStateQuery.CheckConditions(recipe.Value.Conditions))
                {
                    haveNew = true;
                    break;
                }
            }
            //
            //  have an activation, invalidate cache to get item added
            //
            if (haveNew)
                utilitiesService.InvalidateCache(Name);
        }
        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                var recipes = asset.AsDictionary<string, string>().Data;
                //
                //  add crafting recipe for Mushroom Box
                //
                if (modDataService.Config.AddMushroomBoxRecipe)
                    recipes.Add("Mushroom Box", "388 5/Home/128/true/default/");

                foreach (CustomBigCraftableData bigCraftable in customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables.Values)
                {
                    if (string.IsNullOrEmpty(bigCraftable.Conditions))
                    {
                        recipes.Add(bigCraftable.BigCraftableData.Name, bigCraftable.CraftingRecipe());
                    }
                    else
                    {
                        //check if conditions are met
                        if (Game1.hasLoadedGame && GameStateQuery.CheckConditions(bigCraftable.Conditions))
                        {
                            recipes.Add(bigCraftable.BigCraftableData.Name, bigCraftable.CraftingRecipe());
                            bigCraftable.Added = true;
                        }
                    }
                }
            });
        }

        public override void OnGameLaunched()
        {

        }
    }
}
