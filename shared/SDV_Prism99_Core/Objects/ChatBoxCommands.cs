using StardewValley.Menus;
using System;
using System.Collections.Generic;
using Prism99_Core.PatchingFramework;
using Prism99_Core.Utilities;

namespace Prism99_Core.Objects
{
    internal static class ChatBoxCommands
    {
        private static GamePatches patcher = new GamePatches();
        public static bool Initialized;
        private static readonly Dictionary<string, ChatBoxCommandHandlerDelegate> ChatHandlers = new Dictionary<string, ChatBoxCommandHandlerDelegate>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize(IModHelper helper, SDVLogger olog)
        {

            patcher.Initialize(helper.ModRegistry.ModID, olog);

            patcher.AddPatch(true, typeof(ChatBox), "runCommand",
                new Type[] { typeof(string) }, typeof(ChatBoxCommands), nameof(ChatBoxCommands.runCommand),
                "", "Util");

            patcher.ApplyPatches("");

            Initialized = true;
        }
        public static void AddCommand(string commandName, ChatBoxCommandHandlerDelegate fnc)
        {
            ChatHandlers.Add(commandName, fnc);
        }
        public static bool runCommand(string command, ChatBox __instance)
        {
            string[] arCommand = command.Split(' ');
            if (ChatHandlers.TryGetValue(arCommand[0], out var handler))
            {
                handler(command);
                return false;
            }
            return true;
        }
    }
}
