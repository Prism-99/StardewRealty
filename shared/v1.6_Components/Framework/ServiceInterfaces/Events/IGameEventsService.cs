using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Events
{
    internal abstract class IGameEventsService : IService
    {
        public Dictionary<string, Tuple<int, int>> assetRequests = new();
        public enum EventTypes
        {
            ContentLoaded,
            GUILoaded,
            AllModsLoaded,
            PreSave,
            AfterSave
        }
        internal abstract void TriggerEvent(EventTypes eventTypes);
        internal abstract void AddSubscription(EventTypes eventType, Action fnc, int priority = 0);
        internal abstract void AddSubscription(EventArgs eventType, Action<EventArgs> fnc, int priority = 0);
        internal abstract void AddSubscription(string eventType, Action<EventArgs> fnc, int priority = 0);
        internal abstract object GetProxyValue(string eventType, object[] args);
        internal abstract void AddProxyServer(string eventType, Func<object[], object> action);
        internal abstract void DumpSubscriptions();
    }
}
