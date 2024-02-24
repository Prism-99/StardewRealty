using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewModdingAPI;
using StardewValley.Network;
//using SDObject = StardewValley.Object;
using StardewValley.Menus;
using Netcode;

namespace SDV_Realty_Core.Framework.Patches.Integrations
{
    internal class FEBetterJunimoHuts
    {
        public static bool GetAllHuts(ref List<JunimoHut> __result)
        {
            //List<JunimoHut> huts = new List<JunimoHut>();

            foreach (GameLocation gl in Game1.locations)
            {
#if v16
                if(gl.IsBuildableLocation())
                { 
                    GameLocation bl=gl;
#else
                if (gl is BuildableGameLocation bl)
                {
#endif
                    foreach (Building blding in bl.buildings)
                    {
                        if (blding is JunimoHut)
                        {
                            __result.Add((JunimoHut)blding);
                        }
                    }
                }
            }

            return false;
        }
        public static bool GetHutFromId(Guid id, ref JunimoHut __result)
        {

            foreach (GameLocation gl in Game1.locations)
            {
#if v16
                if (gl.IsBuildableLocation()) 
                {
                    GameLocation bl = gl;
#else
                if (gl is BuildableGameLocation bl)
                {
#endif
                    if (bl.buildings.ContainsGuid(id))
                    {
                        __result = (JunimoHut)bl.buildings[id];
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool SpawnJunimoAtHut(JunimoHut hut)
        {
            Vector2 pos = new Vector2((float)hut.tileX.Value + 1f, (float)hut.tileY.Value + 1f) * 64f + new Vector2(0f, 32f);
            Type t = Type.GetType("BetterJunimos.Utils.Util, BetterJunimos");
            //Type t = Type.GetType("BetterJunimos.Utils.Util");
            var bjh = Activator.CreateInstance(t);
            //SpawnJunimoAtPosition(Game1.getFarm(), pos, hut, hut.getUnusedJunimoNumber());


            return false;
        }
//        public static bool SpawnJunimoAtPosition(Vector2 pos, JunimoHut hut, int junimoNumber)
//        {
//             if (hut != null)
//            {
//                Farm farm = Game1.getFarm();
//                bool isPrismatic = false;
//                Color? gemColor = getGemColor(ref isPrismatic, hut);
//#if v16
//                JunimoHarvester val = new JunimoHarvester(pos, hut, junimoNumber, gemColor);
//#else
//                JunimoHarvester val = new JunimoHarvester(pos, hut, junimoNumber, gemColor);
//#endif
//                ((NetFieldBase<bool, NetBool>)val.isPrismatic).Value = isPrismatic;
//                ((GameLocation)farm).characters.Add((NPC)(object)val);
//                hut.myJunimos.Add(val);
//                if (Game1.isRaining)
//                {
//                    //IReflectedField<float> field = Reflection.GetField<float>(val, "alpha");
//                    //field.SetValue(Config.FunChanges.RainyJunimoSpiritFactor);
//                }
//                if (Utility.isOnScreen(Utility.Vector2ToPoint(pos), 64, (GameLocation)(object)farm))
//                {
//#if v16
//                    ((GameLocation)farm).playSound("junimoMeep1");
//#else
//                    ((GameLocation)farm).playSound("junimoMeep1", (NetAudio.SoundContext)0);
//#endif
//                }
//            }

//            return false;
//        }
        public static Color? getGemColor(ref bool isPrismatic, JunimoHut hut)
        {
            List<Color> list = new List<Color>();

#if v16
            Chest value = hut.GetOutputChest(); 
            IEnumerator<Item> enumerator = (value.Items).GetEnumerator();
#else
            Chest value = ((NetFieldBase<Chest, NetRef<Chest>>)hut.output).Value;
            IEnumerator<Item> enumerator = ((NetList<Item, NetRef<Item>>)value.items).GetEnumerator();
#endif
            try
            {
                while (enumerator.MoveNext())
                {
                    Item current = enumerator.Current;
                    if (current != null && (current.Category == -12 || current.Category == -2))
                    {
                        Color? dyeColor = TailoringMenu.GetDyeColor(current);
                        if (current.Name == "Prismatic Shard")
                        {
                            isPrismatic = true;
                        }
                        if (dyeColor.HasValue)
                        {
                            list.Add(dyeColor.Value);
                        }
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            if (list.Count > 0)
            {
                return list[Game1.random.Next(list.Count)];
            }
            return null;
        }
        public static bool GetHutIdFromHut(JunimoHut hut, ref Guid __result)
        {
            foreach (GameLocation gl in Game1.locations)
            {
#if v16
                if(gl.IsBuildableLocation())
                {
                    GameLocation bl = gl;
#else
                if (gl is BuildableGameLocation bl)
                {
#endif
                    if (bl.buildings.Contains(hut))
                    {
                        __result = bl.buildings.GuidOf(hut);
                        return false;
                    }
                }
            }
            return true;

            //return ((BuildableGameLocation)Game1.getFarm()).buildings.GuidOf((Building)(object)hut);
        }
    }
}
