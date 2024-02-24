using StardewValley.Buildings;
using HarmonyLib;

namespace Prism99_Core.Extensions
{
    internal static class Building_Ext
    {
        public static void SetAnimalDoorState(this Building house, bool doorOpen)
        {
            if (house.animalDoorOpen.Value != doorOpen)
            {
                house.animalDoorOpen.Value = !house.animalDoorOpen.Value;
                Traverse.Create(house).Field("animalDoorMotion").Property("Value").SetValue(house.animalDoorOpen.Value ? (-3) : 2);
            }
        }
    }
}
