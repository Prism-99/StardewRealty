using SDV_Realty_Core.Framework.Objects;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IChatBoxCommandsService:IService
{
        public SDRChatboxCommands chatBoxCommands;
        public override Type ServiceType => typeof(IChatBoxCommandsService);
    }
}
