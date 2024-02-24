using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IPlayerComms:IService
{
        public override Type ServiceType => typeof(IPlayerComms);
        internal abstract void SendPlayerMail(string mailId, string expansionName, bool addToReadList);
        internal abstract void SendPlayerMail(string mailId, string expansionName, bool addToReadList, bool sendToday);


    }
}
