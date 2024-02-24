using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Integrations
{
    public interface IExtendedMineCartAPI
    {
        void AddDestination(string displayName, string locationName, int X, int Y);
    }
}
