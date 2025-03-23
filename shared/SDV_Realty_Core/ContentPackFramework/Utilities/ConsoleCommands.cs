using System;
using StardewValley.Buildings;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using Prism99_Core.Extensions;
using System.Linq;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using StardewValley.GameData.Locations;
using StardewValley.TerrainFeatures;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;


namespace SDV_Realty_Core.ContentPackFramework.Utilities
{
    class ConsoleCommands
    {
        //
        //  Handle all console commands
        //
        private ILoggerService logger;
        private IFishStockService fishStockService;
        private ILandManager landManager;
        private IUtilitiesService utilitiesService;
        private IJunimoHarvesterService junimoHarvesterService;
        private IModDataService modDataService;
        private IContentManagerService contentManagerService;
        private IValleyStatsService valleyStatsService;
        private IMapRendererService mapRendererService;
        public ConsoleCommands(ILoggerService olog, IUtilitiesService utilitiesService, IFishStockService fishStockService, ILandManager landManager, IJunimoHarvesterService junimoHarvesterService, IModDataService modDataService, IContentManagerService contentManagerService, IValleyStatsService valleyStatsService, IMapRendererService mapRendererService)
        {
            logger = olog;
            this.utilitiesService = utilitiesService;
            this.fishStockService = fishStockService;
            this.landManager = landManager;
            this.junimoHarvesterService = junimoHarvesterService;
            this.modDataService = modDataService;
            this.contentManagerService = contentManagerService;
            this.valleyStatsService = valleyStatsService;
            this.mapRendererService = mapRendererService;
        }

        public void AddCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add("sdr", "SDR Commands.", HandleSDRCommand);
            helper.ConsoleCommands.Add("sdr_packs", "List the status of installed Farm Expansion Content Packs.", fe_packs);
            helper.ConsoleCommands.Add("sdr_forsale", "List the of Expansion packs that are For Sale.", fe_forsale);
            helper.ConsoleCommands.Add("sdr_mail", "List of messages the player has received.", fe_mail);
            helper.ConsoleCommands.Add("sdr_details", "Displays the details of the specified Content Pack.", fe_details);
            helper.ConsoleCommands.Add("sdr_hooks", "Displays what Harmony Patches and Game hooks are in place.", fe_hooks);
            helper.ConsoleCommands.Add("sdr_locs", "Display players current list of locations", fe_locs);
            helper.ConsoleCommands.Add("sdr_seeds", "Displays the seed catalogue used by the Junimos", fe_seeds);
            helper.ConsoleCommands.Add("sdr_stats", "Displays stats detailing work done by the Junimos", fe_stats);
            helper.ConsoleCommands.Add("sdr_loc_details", "Displays the definition of a location", fe_loc_details);
            helper.ConsoleCommands.Add("sdr_dump_locations", "Displays location lists.", fe_dump_locations);
            helper.ConsoleCommands.Add("sdr_reset_fishstocks", "Resets auto filled expansion fish stocks.", fe_reset_fishstocks);
            helper.ConsoleCommands.Add("sdr_serving", "Dump a list of external assets served by SDR.", sdr_serving);
            helper.ConsoleCommands.Add("sdr_brightness", "Set the global building brightness.", sdr_brightness);
            helper.ConsoleCommands.Add("sdr_grass", "Dump location grass.", sdr_grass);
            helper.ConsoleCommands.Add("sdr_gsq", "Test GSQs.", sdr_gsq);
            helper.ConsoleCommands.Add("sdr_togglecheats", "Toggle cheat mode.", sdr_togglecheat);
            helper.ConsoleCommands.Add("sdr_render_location", "Render location's map to a png file %1 location, %2 save filename. Images are stored in the User\\Pictures directory.", sdr_render_location);
            helper.ConsoleCommands.Add("sdr_config", "Dumps the current configuration settings to the console.", sdr_config);
            //helper.ConsoleCommands.Add("fe_perf", "Displays perf data.", fe_perf);

        }
        private void HandleSDRCommand(string name, string[] args)
        {
            if (args.Length == 0)
            {
                //
                //  dump command list
                //
                WriteLine("Command list:");
                WriteLine("purchase expansionName   : purchases the named expansion");

            }
            else
            {
                switch (args[0].ToLower())
                {
                    case "purchase":
                        Purchase(args.Skip(1).ToArray());
                        break;
                }
            }
        }
        private void Purchase(string[] args)
        {
            if (landManager.PurchaseLand(args[0], true, Game1.player.UniqueMultiplayerID))
                WriteLine($"Suceesfully purchased expansion '{args[0]}'");
            else
                WriteLine($"Could not purchase expansion '{args[0]}'");
        }
        
        /// <summary>
        /// Dumps the configuration to the console
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        private void sdr_config(string name, string[] args)
        {
            foreach (PropertyInfo prop in modDataService.Config.GetType().GetProperties().OrderBy(p => p.Name))
            {
                WriteLine($"{prop.Name}: {prop.GetValue(modDataService.Config, null)}");
            }
        }
        private void sdr_render_location(string name, string[] args)
        {
            //
            //  0 - location name
            //  1 - save filename
            //  2 - with details (bool)
            //
            if (args.Length < 2)
            {
                WriteLine($"Missing required argument");
                WriteLine($"sdr_render_location locationname savename");
                return;
            }
            bool withDetails = false;
            if (args.Length > 2)
            {
                bool.TryParse(args[2], out withDetails);
            }
            GameLocation gl = Game1.getLocationFromName(args[0]);
            if (gl == null)
            {
                WriteLine($"Unknown location '{args[0]}'");
                return;
            }
            try
            {
                string picfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Path.GetFileNameWithoutExtension(args[1]));
                mapRendererService.RenderMap(gl).Save($"{picfile}.png");
                WriteLine($"Map saved to {picfile}.png");
            }
            catch (Exception ex)
            {
                logger.LogError($"Render Location Map {args[0]}", ex);
            }
        }
        private void sdr_togglecheat(string name, string[] args)
        {
            Program.enableCheats = !Program.enableCheats;
            WriteLine($"Cheats enabled {Program.enableCheats}");
        }
        private void sdr_gsq(string name, string[] args)
        {
            try
            {
                string query = string.Join(" ", args);
                if (GameStateQuery.IsImmutablyTrue(query))
                {
                    WriteLine($"GSQ '{query}' will always return true");
                }
                else if (GameStateQuery.IsImmutablyFalse(query))
                {
                    WriteLine($"GSQ '{query}' will always return false");
                }
                else
                {
                    bool result = GameStateQuery.CheckConditions(query);
                    WriteLine($"GSQ '{query}' returns {result}");
                }
            }
            catch (Exception ex)
            {
                WriteLine($"GSQ error: {ex}");
            }
        }
        private void sdr_grass(string name, string[] args)
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> feature in Game1.player.currentLocation.terrainFeatures.Pairs)
            {
                if (feature.Value is Grass gr)
                {
                    WriteLine($"{feature.Key}: {gr.numberOfWeeds.Value} ({gr.Location})");
                }
            }
        }
        private void sdr_brightness(string name, string[] args)
        {
            if (args.Length == 0)
            {
                WriteLine($"  Current Light Level is {modDataService.Config.LightLevel * 100:N0}");
            }
            else
            {
                if (int.TryParse(args[0], out var brightness))
                {
                    modDataService.Config.LightLevel = Math.Clamp(brightness / 100f, 0, 1);
                    utilitiesService.ConfigService.SaveConfig();
                    WriteLine($"  Light Level is set to {modDataService.Config.LightLevel * 100:N0}");
                }
            }
        }
        private void sdr_serving(string name, string[] args)
        {
            WriteLine("----- Assets Served by SDR -----");
            foreach (string key in contentManagerService.ExternalReferences.Keys.OrderBy(p => p))
            {
                WriteLine($"{key}: {contentManagerService.ExternalReferences[key].GetType().Name}");
            }
            foreach (KeyValuePair<string, xTile.Map> map in contentManagerService.contentManager.Maps)
            {
                WriteLine($"{map.Key}: {map.Value.GetType().Name}");
            }
        }
        private void fe_reset_fishstocks(string name, string[] args)
        {
            fishStockService.ResetFishSeasonStock();
            fishStockService.SetFishAreaStocks();
        }
        private void fe_perf(string name, string[] args)
        {
            foreach (string loc in GamePerfMonData.PerfRecords.Keys.OrderBy(p => p))
            {
                WriteLine($"Location: {loc}");
                /*
                             logger.Log($"DayUpdate for {Name} took {dayUpdate.ElapsedMilliseconds}ms ", LogLevel.Debug);
            logger.Log($" Debris: {tm_debris}", LogLevel.Debug);
            logger.Log($" UpdateMap: {tm_updateMap - tm_debris}", LogLevel.Debug);
            logger.Log($" Sprites: {tm_sprites - tm_updateMap}", LogLevel.Debug);
            logger.Log($" Terrain Step 1: {tm_terrain1 - tm_sprites}", LogLevel.Debug);
            logger.Log($" Terrain Step 2: {tm_terrain2 - tm_terrain1}", LogLevel.Debug);
            logger.Log($" LargeTerrain: {tm_large - tm_terrain2}", LogLevel.Debug);
            logger.Log($" Objects Step 1: {tm_objects1 - tm_large}", LogLevel.Debug);
            logger.Log($" Objects Step 2: {tm_objects2 - tm_objects1}", LogLevel.Debug);
*/
                string[] headers = new string[] {"Total","Debris","Map","Sprites","Terr1"
                    , "Terr2","LTerr","Objs1","Objs2" };

                WriteLine(" " + string.Join('|', headers.Select(p => p.PadLeft(7, ' '))));
                foreach (GamePerfMonData.GameDataRecord data in GamePerfMonData.PerfRecords[loc].OrderBy(p => p.GameDay))
                {
                    WriteLine($"{GamePerfMonData.FormatData(data)}");
                }
            }
        }
        private void fe_dump_locations(string name, string[] args)
        {
            int index = 0;
            bool notFiltered = args.Count() > 0;

            WriteLine($".locations ({Game1.locations.Count()})");
            foreach (GameLocation loc in Game1.locations)
            {
                if (notFiltered || modDataService.farmExpansions.ContainsKey(loc.Name) || loc.Name == WarproomManager.StardewMeadowsLoacationName)
                {
                    WriteLine($"[{index}] {loc.Name}");
                }
                index++;
            }
            index = 0;
            WriteLine($"._locationLookup ({Game1._locationLookup.Count()})");
            foreach (KeyValuePair<string, GameLocation> loc in Game1._locationLookup)
            {
                if (notFiltered || modDataService.farmExpansions.ContainsKey(loc.Key) || loc.Key.StartsWith("fe.") || loc.Key == WarproomManager.StardewMeadowsLoacationName)
                {
                    WriteLine($"[{index}] {loc.Key}");
                }
                index++;
            }

            index = 0;
            IEnumerable<KeyValuePair<string, GameLocation>> always = Game1._locationLookup.Where(p => p.Value.isAlwaysActive.Value);
            WriteLine($"always active ({always.Count()})");
            foreach (KeyValuePair<string, GameLocation> loc in always)
            {
                if (notFiltered || modDataService.farmExpansions.ContainsKey(loc.Key) || loc.Key.StartsWith("fe.") || loc.Key == WarproomManager.StardewMeadowsLoacationName)
                {
                    WriteLine($"[{index}] {loc.Key}");
                }
                index++;
            }
        }
        private void fe_loc_details(string name, string[] args)
        {
            string locationName = "";
            if (args.Length == 0)
            {
                locationName = Game1.player.currentLocation.Name;
                //WriteLine("No location name supplied.");
                //return;
            }
            else
            {
                locationName = args[0];
            }
            GameLocation gl = Game1.getLocationFromName(locationName);

            if (gl == null)
            {
                WriteLine($"Unknown location '{locationName}'");

            }
            else
            {
                WriteLine($"Location name: {gl?.DisplayName ?? "--"} ({locationName})");
                WriteLine($"Season override: {(gl.GetSeason() == Game1.season ? "None" : gl.GetSeason())}");
                WriteLine($"Fishing Areas ({gl.GetData()?.FishAreas.Count ?? -1})");
                if (gl.GetData().FishAreas != null && gl.GetData().FishAreas.Count > 0)
                {
                    WriteLine($"ID      |Display Name        |Area");
                    WriteLine($"-----------------------------------");
                    foreach (string fkey in gl.GetData().FishAreas.Keys.OrderBy(p => p))
                    {
                        FishAreaData fd = gl.GetData().FishAreas[fkey];

                        WriteLine($"{fkey.PadRight(8, ' ')}|{fd?.DisplayName?.PadRight(20, ' ') ?? "".PadRight(20, ' ')}|({fd.Position.Value.X},{fd.Position.Value.Y},{fd.Position.Value.Width},{fd.Position.Value.Height})");
                    }
                    WriteLine("");
                }
                if (gl.GetData().Fish != null && gl.GetData().Fish.Count > 0)
                {
                    WriteLine($"ID           |Rules|Area Id      |RandomItemId");
                    WriteLine($"----------------------------------------------");
                    foreach (SpawnFishData fd in gl.GetData().Fish.OrderBy(p => p.FishAreaId))
                    {
                        string id = fd.Id;
                        if (fd.RandomItemId != null && fd.Id == string.Join('|', fd.RandomItemId))
                        {
                            id = "Random";
                        }
                        WriteLine($"{(id ?? "??").PadRight(13, ' ')}|{(fd.IgnoreFishDataRequirements ? "  N  " : "  Y  ")}|{(fd.FishAreaId ?? "??").PadRight(10, ' ')}  |{(fd.RandomItemId == null ? "".PadRight(20, ' ') : string.Join(',', fd.RandomItemId).PadRight(20, ' '))}");
                    }
                    WriteLine("");
                }
                WriteLine($" Out   |  Destination       |  In  ");
                WriteLine($"-----------------------------------");
                foreach (Warp warp in gl.warps)
                {
                    string outWarp = $" {warp.X},{warp.Y}";
                    string inWarp = $" {warp.TargetX},{warp.TargetY}";
                    WriteLine($"{outWarp.PadRight(7)}|{warp.TargetName.PadRight(20)}|{inWarp}");
                }
            }
        }
        private void fe_packs(string name, string[] args)
        {
            WriteLine("Expansion Name | Display Name    |DataLoc|Active|Format|For Sale|  Cost    | Conditions ");
            WriteLine("---------------|-----------------|-------|------|------|--------|----------|------------");
            foreach (string sKey in modDataService.validContents.Keys)
            {
                ExpansionPack oPack = modDataService.validContents[sKey];
                bool bActive = false;
                if (modDataService.farmExpansions != null)
                {
                    if (modDataService.farmExpansions.ContainsKey(sKey)) bActive = (modDataService.farmExpansions[sKey]).Active;
                }
                string sForSale = "Unknown ";

                sForSale = modDataService.LandForSale.Contains(sKey) ? "   Yes  " : "   No   ";

                WriteLine(sKey.Truncate(15).PadRight(15, ' ') + "|" + (oPack.DisplayName ?? oPack.LocationName).Truncate(17).PadRight(17, ' ')
                    + "|" + oPack.AddedToDataLocation.ToString().PadRight(7, ' ')
                    + "|" + bActive.ToString().PadRight(6, ' ')
                    + "|" + (oPack.FileFormat?.ToString().PadLeft(6, ' ') ?? " 1.0.0")
                    + "|" + (bActive ? "  N/A   " : sForSale)
                    + "|" + oPack.Cost.ToString("N0").PadLeft(10, ' ')
                    + "|" + (oPack.Requirements ?? "")
                    );
            }
        }
        private void fe_stats(string name, string[] args)
        {
            //
            //  Dump stats above Junimo fees
            //
            valleyStatsService.DumpStats();
        }
        private void fe_seeds(string name, string[] args)
        {
            //
            //  Dump the Junimo seeds costs to
            //  the console
            //
            junimoHarvesterService.JunimoHarvester.DumpSeeds();
        }
        private void fe_locs(string name, string[] args)
        {
            //
            //  List all active locations
            //
            foreach (GameLocation location in Game1.locations)
            {
                WriteLine("Location: " + location.NameOrUniqueName);

                if (location.IsBuildableLocation())
                {
                    GameLocation bl = location;

                    foreach (Building b in bl.buildings)
                    {
                        if (b?.indoors?.Value != null)
                        {
                            WriteLine("Building: " + b.indoors.Value.NameOrUniqueName);
                        }
                    }
                }
            }
        }
        private void fe_details(string sName, string[] args)
        {
            //
            //  Dump the details of an expansion pack
            //  to the console
            //
            string expansionName;
            if (args.Length < 1)
            {
                expansionName = Game1.player.currentLocation.Name;
                //WriteLine("No Expansion Pack Name supplied.");
            }
            else
            {
                expansionName = args[0];
            }
            if (modDataService.validContents.ContainsKey(expansionName))
            {
                ExpansionPack oPack = modDataService.validContents[expansionName];

                WriteLine($"Pack Name: '{expansionName}'");
                WriteLine($"Display Name: '{oPack.DisplayName ?? oPack.LocationName}'");
                WriteLine($"Description: '{oPack.Description ?? ""}'");
                WriteLine($"Cost: {oPack.Cost}");
                WriteLine($"Map name: '{oPack.MapName ?? "????"}'");
                WriteLine($"Thumbnail name: '{oPack.ThumbnailName ?? ""}'");
                WriteLine($"Requirements: '{oPack.Requirements ?? ""}'");
                WriteLine($"Entrances");
                foreach (string sEntranceId in oPack.EntrancePatches.Keys)
                {
                    WriteLine("--------------------------------------------");
                    WriteLine($"         Entrance Id '{sEntranceId}'");
                    WriteLine("--------------------------------------------");
                    EntrancePatch oPatch = oPack.EntrancePatches[sEntranceId];
                    WriteLine($"Map to Patch: '" + (oPatch.PatchMapName ?? "") + "'");
                    WriteLine($"MapPatchPoint: ({oPatch.MapPatchPointX},{oPatch.MapPatchPointY})");
                    WriteLine($"PathBlock: ({oPatch.PathBlockX},{oPatch.PathBlockY})");
                    WriteLine($"WarpOrientation: {oPatch.WarpOrientation}");
                    WriteLine($"WarpIn: ({oPatch.WarpIn.X},{oPatch.WarpIn.Y}) x{oPatch.WarpIn.NumberofPoints}");
                    WriteLine($"WarpOut: ({oPatch.WarpOut.X},{oPatch.WarpOut.Y}) x{oPatch.WarpOut.NumberofPoints}");
                    WriteLine("");
                }
            }
            else
            {
                WriteLine($"Unknown Expansion Pack '{expansionName}'");
            }

        }
        private void fe_mail(string sName, string[] args)
        {
            //
            //  Dump the mail ids of the player
            //  to the console
            //
            WriteLine("ReceivedMail Ids:" + string.Join(", ", Game1.player.mailReceived));
            WriteLine("Mailbox Mail Ids:" + string.Join(", ", Game1.player.mailbox));
        }
        private void fe_hooks(string sName, string[] args)
        {
            //
            //  Dump all of the harmony patches
            //  used
            //
            WriteLine("Harmony patches:");
            string[] arDetails = utilitiesService.PatchingService.patches.GetResult(false).Split(Environment.NewLine.ToCharArray());
            foreach (string patch in arDetails)
            {
                WriteLine(patch);
            }
        }
        private void fe_forsale(string sName, string[] args)
        {
            //
            //  Dump the list of currently for
            //  sales lands to the console
            //
            if (modDataService.LandForSale.Count == 0)
            {
                WriteLine("No land for sale.");
            }
            else
            {
                foreach (string sLand in modDataService.LandForSale)
                {
                    WriteLine("For Sale: " + sLand);
                }
            }
        }

        private void WriteLine(string sText)
        {
            logger.Log(sText, LogLevel.Info);
        }
        private void WriteLine(string sText, LogLevel lLevel)
        {
            logger.Log(sText, lLevel);
        }
    }
}
