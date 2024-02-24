using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.ContentPackFramework.Utilities;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IConsoleCommandsService:IService
    {
        public override Type ServiceType => typeof(IConsoleCommandsService);
        public ConsoleCommands consoleCommands;
    }
}
