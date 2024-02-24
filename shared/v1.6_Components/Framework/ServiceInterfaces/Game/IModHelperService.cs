using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Game
{
    internal abstract class IModHelperService : IService
    {
        public IModHelper modHelper;
        public override Type ServiceType => typeof(IModHelperService);
        public abstract string DirectoryPath { get; }
        public abstract ITranslationHelper Translation { get; }
        public abstract IModRegistry ModRegistry { get; }
        public abstract IMultiplayerHelper Multiplayer { get; }
        public abstract void WriteConfig<TConfig>(TConfig config) where TConfig : class, new();
        public abstract TConfig ReadConfig<TConfig>() where TConfig : class, new();

    }
}
