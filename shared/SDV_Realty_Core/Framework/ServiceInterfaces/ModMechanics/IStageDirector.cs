using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    /// <summary>
    /// Provide game stage hooks
    /// </summary>
    internal abstract class IStageDirector : IService
    {
        public override Type ServiceType => typeof(IStageDirector);



    }
}
