using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Configuration
{
    internal abstract class IConfigService:IService
    {
        //public FEConfig config;
        public abstract void SaveConfig();
        public abstract void LoadConfig();
    }
}
