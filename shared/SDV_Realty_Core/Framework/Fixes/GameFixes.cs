using StardewModdingAPI.Events;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.Fixes
{
    //
    //  obsolete, moved to GameFixesServices
    //
    internal class GameFixes
{
        public static void Initialize(IModHelper helper)
        {
            // chest fixes
            helper.Events.World.BuildingListChanged += FixBuildingAdding;
        }

        private static void FixBuildingAdding(object sender, BuildingListChangedEventArgs e)
        {
            if (e.Added != null)
            {
                foreach (var buildingAdded in e.Added)
                {
                    if (buildingAdded.indoors.Value != null)
                    {
                        //
                        //  check for chests added as objects
                        //
                        var chests = buildingAdded.indoors.Value.Objects.Values.Where(p => p.ItemId == "130" && p is not Chest).ToList();

                        foreach (var chest in chests)
                        {
                            buildingAdded.indoors.Value.objects.Remove(chest.TileLocation);
                            buildingAdded.indoors.Value.objects.Add(chest.TileLocation, new Chest(true, chest.TileLocation));
                        }
                        //
                        //  check for casks added as objects
                        //
                        var casks = buildingAdded.indoors.Value.Objects.Values.Where(p => p.ItemId == "(BC)163" && p is not Cask).ToList();

                        foreach (var cask in casks)
                        {
                            buildingAdded.indoors.Value.objects.Remove(cask.TileLocation);
                            buildingAdded.indoors.Value.objects.Add(cask.TileLocation, new Cask(cask.TileLocation));
                        }

                    }
                }
            }
        }
    }
}
