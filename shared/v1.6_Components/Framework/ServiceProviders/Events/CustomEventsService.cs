using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceProviders.Events
{
    internal class CustomEventsService : ICustomEventsService
    {
        private Dictionary<string, List<Action<object[]>>> customeventSubscriptions;
        public override Type[] InitArgs => new Type[]
        {

        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            customeventSubscriptions = new Dictionary<string, List<Action<object[]>>>();
        }
        internal override void TriggerCustomEvent(string eventType, object[] args)
        {
            if (customeventSubscriptions.ContainsKey(eventType))
            {
                foreach (var action in customeventSubscriptions[eventType])
                { action.Invoke(args); }
            }
            else
            {
                logger.Log($"TriggerCustomEvent {eventType} has no handler(s).", LogLevel.Error);
            }
        }
        internal override void AddCustomSubscription(string subscriptionName, Action<object[]> action)
        {
            if (customeventSubscriptions.ContainsKey(subscriptionName))
            {
                customeventSubscriptions[subscriptionName].Add(action);
            }
            else
            {
                customeventSubscriptions.Add(subscriptionName, new List<Action<object[]>> { action });
            }
        }
        internal override void DumpSubscriptions()
        {
            logger.Log("", LogLevel.Debug);
            logger.Log($"Custom Events", LogLevel.Debug);
            logger.Log($"------------------------", LogLevel.Debug);
            foreach (var prox in customeventSubscriptions)
            {
                logger.Log($"{prox.Key}", LogLevel.Debug);
                foreach (var evet in prox.Value)
                    logger.Log($" => {evet.Method.DeclaringType.Name}.{evet.Method.Name}", LogLevel.Debug);

            }
        }
    }
}