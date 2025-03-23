using StardewModdingAPI.Events;
using StardewValley.Objects;
using System.Data;
using System.Linq;

namespace SDV_Realty_Core.Framework.Fixes
{
    internal static class Fixes
    {
        public static void Initialize(IModHelper helper)
        {
            // chest fixes
            helper.Events.World.BuildingListChanged += FixBuildingChests;
        }

        private static void FixBuildingChests(object sender, BuildingListChangedEventArgs e)
        {
            if (e.Added != null)
            {
                foreach (var buildingAdded in e.Added)
                {
                    if (buildingAdded.indoors.Value != null)
                    {
                        var chests = buildingAdded.indoors.Value.Objects.Values.Where(p => p.ItemId == "130" && p is not Chest).ToList();

                        foreach (var chest in chests)
                        {
                            buildingAdded.indoors.Value.objects.Remove(chest.TileLocation);
                            buildingAdded.indoors.Value.objects.Add(chest.TileLocation, new Chest(true, chest.TileLocation));
                        }
                    }
                }
            }
        }
    }
}
