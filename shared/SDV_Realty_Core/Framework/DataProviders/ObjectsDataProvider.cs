using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/Objects to add SDR custom objects
    /// Uses CustomObjectManager for source data
    /// </summary>
    internal class ObjectsDataProvider : IGameDataProvider
    {
        private ICustomObjectService customObjectService;
        public override string Name => "Data/Objects";

        public ObjectsDataProvider(ICustomObjectService customObjectService)
        {
            this.customObjectService = customObjectService;
        }
        public override void CheckForActivations()
        {
            
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (string objectkey in customObjectService.objects.Keys)
                {
                    if (string.IsNullOrEmpty(customObjectService.objects[objectkey].Conditions))
                    {
                        asset.AsDictionary<string, ObjectData>().Data.Add(objectkey, customObjectService.objects[objectkey].ObjectData);
                        customObjectService.objects[objectkey].Added = true;
                    }
                    else
                    {
                        //
                        //  check object conditions
                        //
                        if (GameStateQuery.CheckConditions(customObjectService.objects[objectkey].Conditions))
                        {
                            asset.AsDictionary<string, ObjectData>().Data.Add(objectkey, customObjectService.objects[objectkey].ObjectData);
                            customObjectService.objects[objectkey].Added = true;
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
