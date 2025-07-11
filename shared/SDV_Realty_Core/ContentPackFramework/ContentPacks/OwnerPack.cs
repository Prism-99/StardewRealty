using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.ContentPackFramework.ContentPacks
{
    internal class OwnerPack : IContentPack
    {
        private OwnerManifest ownerManifest;

        public OwnerPack(OwnerManifest manifest)
        {
            ownerManifest=manifest;
        }
        public string DirectoryPath => throw new NotImplementedException();

        public IManifest Manifest => ownerManifest;

        public ITranslationHelper Translation => throw new NotImplementedException();

        public IModContentHelper ModContent => throw new NotImplementedException();

        public bool HasFile(string path)
        {
            throw new NotImplementedException();
        }

        public TModel ReadJsonFile<TModel>(string path) where TModel : class
        {
            throw new NotImplementedException();
        }

        public void WriteJsonFile<TModel>(string path, TModel data) where TModel : class
        {
            throw new NotImplementedException();
        }
    }
}
