using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal  abstract class IThreadSafeLoaderService:IService
{
        public override Type ServiceType => typeof(IThreadSafeLoaderService);
    }
}
