using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using  SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;
using Prism99_Core.Utilities;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IGameEnvironmentService : IService
    {
        public List<FarmDetails> GameFarms = null;
        public SDVEnvironment environment;
        public Dictionary<string, FarmDetails> OtherLocations = null;
        public List<string> BlackListedFarmMods = new List<string> { };
        public GridManager.FarmProfile ActiveFarmProfile;
        public bool ExitsLoaded = false;

        public override Type ServiceType => typeof(IGameEnvironmentService);
        public abstract FarmDetails GetFarmDetails(int farmId);
        internal abstract bool AnyBlackListFarms();
        internal abstract string GamePath { get; }
        internal abstract string ModPath { get; }
    }
}
