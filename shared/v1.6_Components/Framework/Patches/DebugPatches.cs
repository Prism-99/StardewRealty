using StardewValley.GameData.Locations;
using System;
using StardewValley.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;


namespace SDV_Realty_Core.Framework.Patches
{
    internal class DebugPatches
    {
        private static ILoggerService logger;
        private static IGameEventsService _eventsService;
        public static void Initialize(ILoggerService ologger, IGameEventsService eventsService)
        {
            logger = ologger;
            _eventsService = eventsService;
        }
        
        public static bool CreateGameLocation(string id, CreateLocationData createData, Game1 __instance, ref GameLocation __result)
        {
            if (createData == null || id != "MovieTheater")
            {
                return true;
            }
            try
            {
                MovieTheater movieTheater = new MovieTheater();
                string theater = movieTheater.GetType().AssemblyQualifiedName;
                Type tType = Type.GetType(theater);
                Type tType2 = Type.GetType(createData.Type);
                Type tType3 = Type.GetType("StardewValley.Locations.Mine, Stardew Valley");
                //GameLocation location = ((createData.Type == null) ? new GameLocation(createData.MapPath, id) : ((GameLocation)Activator.CreateInstance(Type.GetType(createData.Type) ?? throw new Exception("Invalid type for location " + id + ": " + createData.Type), createData.MapPath, id)));
                __result = ((createData.Type == null) ? new GameLocation(createData.MapPath, id) : ((GameLocation)Activator.CreateInstance(Type.GetType(theater), createData.MapPath, id)));
                __result.isAlwaysActive.Value = createData.AlwaysActive;
                return false;// location;
            }
            catch (Exception e)
            {
                string err = e.ToString();
            }

            return true;
        }
    }
}
