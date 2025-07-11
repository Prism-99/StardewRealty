using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Integrations
{
    internal abstract class ICentralStationIntegrationService:IService
{
        public override Type ServiceType => typeof(ICentralStationIntegrationService);

    }
}
