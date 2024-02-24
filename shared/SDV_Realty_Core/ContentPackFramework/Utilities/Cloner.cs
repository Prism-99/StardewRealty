using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Buildings;
using Netcode;
using StardewValley;
using System.Reflection;
using StardewModdingAPI;

namespace SDV_Realty_Core.ContentPackFramework.Utilities
{
    class Cloner
    {
        public static Building CloneBuilding(Building oSource)
        {
            Building oClone = oSource;// new Building();
            //oClone.indoors.Value = null;

            //oClone.buildingType.Value = oSource.buildingType.Value;
            //oClone.maxOccupants.Value = oSource.maxOccupants.Value;
            //oClone.indoors.Value = new AnimalHouse();
            //if (oSource.indoors.Value is AnimalHouse oHouse)
            //{
            //    oClone.indoors.Value = new AnimalHouse(oHouse.mapPath.Value, oHouse.name.Value);
            //    AnimalHouse oSourceHouse = ((AnimalHouse)(oSource.indoors.Value));
            //    foreach (long obj in oSourceHouse.animals.Keys)
            //        ((AnimalHouse)oClone.indoors.Value).animals.Add(obj, oSourceHouse.animals[obj]);
            //}

            // oClone.indoors.Value = oIndoors.Value;

            return oClone;
        }

        public static void CompareInside(AnimalHouse oHouseA, AnimalHouse oHouseB, IMonitor monitor)
        {
            Type type = typeof(AnimalHouse);
            FieldInfo[] properties = type.GetFields();
            foreach (FieldInfo property in properties)
            {
                if (property.GetValue(oHouseA) != property.GetValue(oHouseB))
                {
                    monitor.Log($"Property{property.Name}, valueA: {property.GetValue(oHouseA)}, valueB: {property.GetValue(oHouseB)}", LogLevel.Trace);
                }
            }
        }
        public static void DumpHouse(AnimalHouse oHouse, IMonitor monitor)
        {
            Type type = typeof(AnimalHouse);
            FieldInfo[] properties = type.GetFields();
            foreach (FieldInfo property in properties)
            {
                monitor.Log($"[{property.Name}] = {property.GetValue(oHouse)}", LogLevel.Trace);
            }

        }
    }
}
