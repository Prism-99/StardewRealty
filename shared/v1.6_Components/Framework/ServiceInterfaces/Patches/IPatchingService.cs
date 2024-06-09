using System;
using Prism99_Core.PatchingFramework;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Patches
{
    /// <summary>
    /// Provides Harmony patching services
    /// </summary>
    internal abstract class IPatchingService:IService
    {
        public GamePatches patches;
        public override Type ServiceType => typeof(IPatchingService);
        internal abstract void ApplyPatches();
        internal abstract void ApplyPatches(string patchGroup);
    }
}
