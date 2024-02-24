using System.Collections.Generic;
using Prism99_Core.Utilities;

namespace Prism99_Core.SMAPIEventSubscription
{
    internal static class EventSubscribers
    {
        public  static Dictionary<string,IEventSubcriber> _Subscribers = new Dictionary<string, IEventSubcriber> { };
        private static SDVLogger logger;

        public static void Initialize(SDVLogger olog)
        {
            logger = olog;
        }
        public static void AddSubscriber(IEventSubcriber oEvent)
        {
            if (!_Subscribers.ContainsKey(oEvent.GetType().Name))
            {
                oEvent.Hook();
                _Subscribers.Add(oEvent.GetType().Name,oEvent);

                logger.LogDebug("AddSubscriber", $"Added subscription '{oEvent.GetType()}', active subs={_Subscribers.Count}");
            }
            else
            {
                logger.LogTrace("AddSubscriber", $"Attempt to add same hook twice, '{oEvent.GetType()}', active subs={_Subscribers.Count}");
            }
        }

        public static void RemoveSubscriber(IEventSubcriber oEvent)
        {
            if (_Subscribers.ContainsKey(oEvent.GetType().Name))
            {
                oEvent.UnHook();
                _Subscribers.Remove(oEvent.GetType().Name);

                logger.LogDebug("RemoveSubscriber", $"Removed subscription '{oEvent.GetType()}', active subs={_Subscribers.Count}");
            }
        }
    }
}
