﻿using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IStardewRealty:IService
    {
        public override Type ServiceType => typeof(IStardewRealty);

    }
}
