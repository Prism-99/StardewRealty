using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    internal abstract class IContentManagerService : IService
    {
        public override Type ServiceType => typeof(IContentManagerService);

        internal abstract Dictionary<string, string> stringFromMaps  { get; }
        internal abstract Dictionary<string, object> ExternalReferences { get; }

        internal SDRContentManager contentManager;

        public abstract void UpsertStringFromMap(string assetName, string assValue);
        public abstract void RemoveStringFromMap(string assetName);
        public abstract void AddExternalReference(string assetName, object assValue,bool removeExt=true);
        public abstract bool TryGetExternalReference(string assetName, out object? assValue);
        public abstract Texture2D? GetExternalTexture2D(string assetName);
    }
}
