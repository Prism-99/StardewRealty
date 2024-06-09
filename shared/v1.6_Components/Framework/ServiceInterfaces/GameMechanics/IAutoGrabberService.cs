using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics
{
    /// <summary>
    /// Provides Deluxe Auto-Grabber functionality to expansions
    /// </summary>
    internal abstract class IAutoGrabberService : IService
    {
        public override Type ServiceType => typeof(IAutoGrabberService);
    }
}
