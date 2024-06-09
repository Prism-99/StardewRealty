using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Events
{
    internal abstract class ICustomEventsService : IService
    {
        public enum ModEvents
        {
            ConfigChanged,
            ExpansionActivated
        }
        public override Type ServiceType => typeof(ICustomEventsService);
        internal abstract void AddCustomSubscription(string subscriptionName, Action<object[]> action);
        internal abstract void TriggerCustomEvent(string eventType, object[] args);
        internal abstract void AddModEventSubscription(ModEvents modEvent, Action<object[]> action);
        internal abstract void TriggerModEvent(ModEvents modEvent, object[] args);    
        internal abstract void DumpSubscriptions();

    }
}
