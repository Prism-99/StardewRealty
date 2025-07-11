using SDV_Realty_Core.Framework.Objects;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IInputService:IService
{
        internal Dictionary<KeybindList, Action<KeybindList>> actionHandlers = new();
        public FEInputHandler inputHandler;
        public override Type ServiceType => typeof(IInputService);
        public abstract void AddKeyBind(KeybindList key, Action<KeybindList> bind);
    }
}
