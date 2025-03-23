using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Objects;
using StardewRealty.SDV_Realty_Interface;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using xTile;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    internal abstract class IModDataService : IService
    {
        public readonly SDict farmExpansions = new();
        public readonly Dictionary<string, ExpansionDetails> expDetails = new();
        public readonly Dictionary<string, ExpansionPack> validContents = new();
        public readonly Dictionary<string, string> stringFromMaps = new();
        public readonly Dictionary<string, Map> ExpansionMaps = new();
        public readonly Dictionary<string, Map> BuildingMaps = new();
        public readonly Dictionary<string, GameLocation> SubLocations = new();
        public readonly Dictionary<string, CustomizationDetails> CustomDefinitions = new();
        public readonly Dictionary<int, string> MapGrid = new();
        public readonly int MaximumExpansions = 10;
        public override Type ServiceType => typeof(IModDataService);
    }
}
