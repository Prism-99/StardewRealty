using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;
using System.Linq;
using xTile;
using StardewValley.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.Locations;
using System.Collections.Generic;
using System.Security;


namespace SDV_Realty_Core.Framework.Objects
{
    internal class SDRChatboxCommands
    {
        private IModHelperService helper;
        private ILoggerService logger;
        private IGridManager _gridManager;
        private IContentManagerService _contentManagerService;
        private Dictionary<string, Action<string[]>> customCommands = new();
        public SDRChatboxCommands(ILoggerService olog,  IModHelperService ohelper, IGridManager gridManager, IContentManagerService contentManagerService)
        {
            helper = ohelper;
            logger = olog;
            _gridManager = gridManager;
            _contentManagerService = contentManagerService;
#if v168
            ChatBoxCommands.Initialize(helper.modHelper, (SDVLogger)logger.CustomLogger);
            ChatBoxCommands.AddCommand("sdr", sdr);
#else
            ChatCommands.Register($"sdr", ChatBoxHandler, name => $"{name} [message]: sdr chat commands.");
#endif
        }
        public void AddCustomCommand(string command, Action<string[]> action)
        {
            customCommands[command] = action;
        }
        private void PopHUDMessage(string message, int duration = 6000)
        {
            Game1.addHUDMessage(new HUDMessage(message) { noIcon = true, timeLeft = duration });
        }
#if !v168
        internal void ChatBoxHandler(string[] command, ChatBox chatBox)
        {
            sdr(string.Join(" ", command));
        }
#endif
        internal void sdr(string commandLine)
        {
            //
            //  /sdr command
            //
            string[] arWords = commandLine.Substring(3).Trim().Split(' ');

            switch (arWords[0])
            {
                case "":
                    PopHUDMessage($"Stardew Realty version {helper.ModRegistry.Get(helper.ModRegistry.ModID).Manifest.Version}");
                    break;
                //case "train":
                //    string location = WarproomManager.StardewMeadowsLoacationName;
                //    if (arWords.Length > 1)
                //        location = arWords[1];

                //    int delay = 2000;
                //    if (arWords.Length > 2 && int.TryParse(arWords[2], out int dly))
                //        delay = dly;

                //    customTrainService.setTrainComing(location, delay, true);
                //    break;
                case "swap":
                    swap(arWords);
                    break;
                case "mapstrings":
                    mapstrings(arWords);
                    break;
                case "assets":
                    assets(arWords);
                    break;
                default:
                    if (customCommands.TryGetValue(arWords[0], out var command))
                    {
                        command(arWords);
                    }
                    break;
            }
        }
        public void assets(string[] arWords)
        {
            logger.Log($"SDR Assets Served:", LogLevel.Info);

            foreach (string key in _contentManagerService.ExternalReferences.Keys.OrderBy(p => p))
            {
                if (arWords.Length == 1 || key.Contains(arWords[1], StringComparison.OrdinalIgnoreCase))
                {
                    switch (_contentManagerService.ExternalReferences[key])
                    {
                        case Texture2D txt:
                            logger.Log($"{key}: Texture2D: {txt.Name}", LogLevel.Info);
                            break;
                        case Map mp:
                            logger.Log($"{key}: Map: {mp.Id}", LogLevel.Info);
                            break;
                        default:
                            logger.Log($"{key}: {_contentManagerService.ExternalReferences[key]}", LogLevel.Info);
                            break;
                    }
                }
            }
        }
        private void mapstrings(string[] arWords)
        {
            foreach (string key in _contentManagerService.stringFromMaps.Keys.OrderBy(p => p))
            {
                logger.Log($"{key}: {_contentManagerService.stringFromMaps[key]}", LogLevel.Info);
            }
        }
        private void swap(string[] arWords)
        {
            if (int.TryParse(arWords[1], out int gridA))
            {
                if (int.TryParse(arWords[2], out int gridB))
                {
                    PopHUDMessage($"{_gridManager.SwapGridLocations(gridA, gridB)}");
                }
                else
                {
                    PopHUDMessage($"GridB: {gridB} is not a valid format.");
                }
            }
            else
            {
                PopHUDMessage($"GridA: {gridA} is not a valid format.");
            }
        }
    }
}
