using StardewModdingAPI.Events;
using System.Linq;
using StardewValley.GameData.Crops;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class CropsDataProvider : IGameDataProvider
    {
        public override string Name => "Data/Crops";
        private ICustomCropService customCropService;
        private ICustomObjectService customObjectService;
        private IUtilitiesService utilitiesService;
        public CropsDataProvider(ICustomCropService customCropService,ICustomObjectService customObjectService, IUtilitiesService utilitiesService)
        {
            this.customCropService = customCropService;
            this.customObjectService = customObjectService;
            this.utilitiesService = utilitiesService;
        }
        public override void CheckForActivations()
        {
            //
            //  get a list of potential activations
            //
            var potential = customCropService.Crops.Values.Where(p => !p.Added && !string.IsNullOrEmpty(p.Conditions)).ToList();
            bool haveNew = false;
            //
            //  check candidates to see if they can be activated
            //
            foreach (var crop in potential)
            {
                if (Game1.hasLoadedGame && GameStateQuery.CheckConditions(crop.Conditions))
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
                foreach (string cropKey in customCropService.Crops.Keys)
                {
                    if (string.IsNullOrEmpty(customCropService.Crops[cropKey].Conditions))
                    {
                        asset.AsDictionary<string, CropData>().Data.Add(cropKey, customCropService.Crops[cropKey].CropData);
                        customCropService.Crops[cropKey].Added = true;
                    }
                    else
                    {
                        //
                        //  check object conditions
                        //
                        if (GameStateQuery.CheckConditions(customCropService.Crops[cropKey].Conditions))
                        {
                            asset.AsDictionary<string, CropData>().Data.Add(cropKey, customCropService.Crops[cropKey].CropData);
                            customObjectService.objects[cropKey].Added = true;
                        }
                    }
                }
            });
        }

        public override void OnGameLaunched()
        {
            utilitiesService.InvalidateCache(Name);
        }
    }
}
