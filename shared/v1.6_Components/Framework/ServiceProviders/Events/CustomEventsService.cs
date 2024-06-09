using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using System.Collections.Generic;



namespace SDV_Realty_Core.Framework.ServiceProviders.Events
{
    internal class CustomEventsService : ICustomEventsService
    {
        private Dictionary<string, List<Action<object[]>>> customeventSubscriptions;
        private Dictionary<ModEvents, List<Action<object[]>>> modEventSubscriptions;
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
            customeventSubscriptions = new();
            modEventSubscriptions = new();

         }
        internal override void AddModEventSubscription(ModEvents modEvent, Action<object[]> action)
        {
            if (modEventSubscriptions.ContainsKey(modEvent))
            {
                modEventSubscriptions[modEvent].Add(action);
            }
            else
            {
                modEventSubscriptions.Add(modEvent, new List<Action<object[]>> { action });
            }
        }
        internal override void TriggerModEvent(ModEvents modEvent, object[] args)
        {
            logger.Log($"Triggering Mod Event {modEvent} for {logger.GetCaller()}", LogLevel.Debug);
            if (modEventSubscriptions.ContainsKey(modEvent))
            {
                foreach (var action in modEventSubscriptions[modEvent])
                { action.Invoke(args); }
            }
        }
        internal override void TriggerCustomEvent(string eventType, object[] args)
        {
            logger.Log($"Triggering Custom Event {eventType} for {logger.GetCaller()}", LogLevel.Debug);
            if (customeventSubscriptions.ContainsKey(eventType))
            {
                foreach (var action in customeventSubscriptions[eventType])
                { action.Invoke(args); }
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
            logger.Log("", LogLevel.Info);
            logger.Log("Mod Events",LogLevel.Info);
            logger.Log($"------------------------", LogLevel.Info);
            foreach(var modEvents in modEventSubscriptions)
            {
                logger.Log($"{modEvents.Key}", LogLevel.Info);
                foreach (Action<object[]> evet in modEvents.Value)
                    logger.Log($" => {evet.Method.DeclaringType.Name}.{evet.Method.Name}", LogLevel.Info);
            }
            logger.Log("", LogLevel.Info);
            logger.Log($"Custom Events", LogLevel.Info);
            logger.Log($"------------------------", LogLevel.Info);
            foreach (KeyValuePair<string, List<Action<object[]>>> prox in customeventSubscriptions)
            {
                logger.Log($"{prox.Key}", LogLevel.Info);
                foreach (Action<object[]> evet in prox.Value)
                    logger.Log($" => {evet.Method.DeclaringType.Name}.{evet.Method.Name}", LogLevel.Info);
            }
        }      
    }
}