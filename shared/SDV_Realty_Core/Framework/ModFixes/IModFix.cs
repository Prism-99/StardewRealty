using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ModFixes
{
    internal abstract class IModFix
    {
        public abstract bool ShouldApply(IModHelper helper);
        public abstract void ApplyFixes(IModHelper helper, IMonitor monitor, ILoggerService logger);   
    }
}
