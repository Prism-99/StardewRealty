﻿using SDV_Realty_Core.Framework.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Integrations
{
    internal abstract class IGMCMIntergrationService:IService
{
        internal GMCM_Integration CGCMIntegration;
        public override Type ServiceType => typeof(IGMCMIntergrationService);
    }
}
