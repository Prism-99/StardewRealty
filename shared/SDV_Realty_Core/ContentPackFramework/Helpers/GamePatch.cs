using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using HarmonyLib;

namespace ContentPackFramework.Helpers
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
    }
}
