using SDV_Realty_Core.Framework.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Integrations
{
    internal abstract class ILocationTunerIntegrationService : IService
    {
        public override Type ServiceType => typeof(ILocationTunerIntegrationService);
        public abstract bool NoCrows(string locationName, out bool value);
        public abstract bool NoLightning(string locationName, out bool value);

    }
}
