using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.ContentPackFramework.ContentPacks
{
    internal class OwnerManifest : IManifest
    {
        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string Author => throw new NotImplementedException();

        public ISemanticVersion Version => throw new NotImplementedException();

        public ISemanticVersion MinimumApiVersion => throw new NotImplementedException();

        public ISemanticVersion MinimumGameVersion => throw new NotImplementedException();

        public string UniqueID{ get; set; }

        public string EntryDll => throw new NotImplementedException();

        public IManifestContentPackFor ContentPackFor => throw new NotImplementedException();

        public IManifestDependency[] Dependencies => throw new NotImplementedException();

        public string[] UpdateKeys => throw new NotImplementedException();

        public IDictionary<string, object> ExtraFields => throw new NotImplementedException();
    }
}
