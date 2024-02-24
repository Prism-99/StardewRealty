using System.Reflection;
using HarmonyLib;

namespace Prism99_Core.PatchingFramework
{
    class GamePatch
    {
        public bool IsPrefix { get; set; }
        public MethodBase Original { get; set; }
        public HarmonyMethod Target { get; set; }
        public string Description { get; set; }
        public bool Applied { get; set; }
        public bool Failed { get; set; }
        public string FailureDetails { get; set; }
        public int Priority { get; set; } = -1;
    }
}
