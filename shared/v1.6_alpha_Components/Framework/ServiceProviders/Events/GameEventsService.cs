using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.Events
{
    internal class GameEventsService : IGameEventsService
    {
        public override Type ServiceType => typeof(IGameEventsService);

        public override Type[] InitArgs => new Type[] { typeof(IModHelperService) };
        private IModHelper helper;
        private Dictionary<EventTypes, List<Action>> eventSubscriptions;
        private Dictionary<string, Func<object[], object>> proxyServers = new();
        private Dictionary<string, List<Tuple<int, Action<EventArgs>>>> actionSubscriptions;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            helper = ((IModHelperService)args[0]).modHelper;
            eventSubscriptions = new Dictionary<EventTypes, List<Action>>();
            actionSubscriptions = new Dictionary<string, List<Tuple<int, Action<EventArgs>>>>();
        }
        internal override void AddSubscription(string eventType, Action<EventArgs> fnc, int priority = 0)
        {
            logger.Log($"Adding GameEvent subscription for {eventType}", LogLevel.Debug);
            if (actionSubscriptions.ContainsKey(eventType))
            {
                actionSubscriptions[eventType].Add(Tuple.Create(priority, fnc));
            }
            else
            {
                actionSubscriptions.Add(eventType, new List<Tuple<int, Action<EventArgs>>> { Tuple.Create(priority, fnc) });
                AddGameEventHook(eventType);
            }
        }
        internal override void AddSubscription(EventArgs eventType, Action<EventArgs> fnc, int priority = 0)
        {
            logger.Log($"Adding GameEvent subscription for {eventType.GetType().Name}", LogLevel.Debug);
            if (actionSubscriptions.ContainsKey(eventType.GetType().Name))
            {
                actionSubscriptions[eventType.GetType().Name].Add(Tuple.Create(priority, fnc));
            }
            else
            {
                actionSubscriptions.Add(eventType.GetType().Name, new List<Tuple<int, Action<EventArgs>>> { Tuple.Create(priority, fnc) });
                AddGameEventHook(eventType);
            }
        }
        private void AddGameEventHook(string eventType)
        {
            switch (eventType)
            {
                case "AssetRequestedEventArgs":
                    helper.Events.Content.AssetRequested += GameEvent;
                    break;
                case "ButtonPressedEventArgs":
                    helper.Events.Input.ButtonPressed += GameEvent;
                    break;
                case "ObjectListChangedEventArgs":
                    helper.Events.World.ObjectListChanged += GameEvent;
                    break;
                case "TimeChangedEventArgs":
                    helper.Events.GameLoop.TimeChanged += GameEvent;
                    break;
                case "WarpedEventArgs":
                    helper.Events.Player.Warped += GameEvent;
                    break;
                case "NpcListChangedEventArgs":
                    helper.Events.World.NpcListChanged += GameEvent;
                    break;
                case "LocationListChangedEventArgs":
                    helper.Events.World.LocationListChanged += GameEvent;
                    break;
                case "GameLaunchedEventArgs":
                    helper.Events.GameLoop.GameLaunched += GameEvent;
                    break;
                case "SavedEventArgs":
                    helper.Events.GameLoop.Saved += GameEvent;
                    break;
                case "DayEndingEventArgs":
                    helper.Events.GameLoop.DayEnding += GameEvent;
                    break;
                case "SavingEventArgs":
                    helper.Events.GameLoop.Saving += GameEvent;
                    break;
                case "MenuChangedEventArgs":
                    helper.Events.Display.MenuChanged += GameEvent;
                    break;
                case "DayStartedEventArgs":
                    helper.Events.GameLoop.DayStarted += GameEvent;
                    break;
                case "SaveLoadedEventArgs":
                    helper.Events.GameLoop.SaveLoaded += GameEvent;
                    break;
                case "LoadStageChangedEventArgs":
                    helper.Events.Specialized.LoadStageChanged += GameEvent;
                    break;
                case "SaveCreatedEventArgs":
                    helper.Events.GameLoop.SaveCreated += GameEvent;
                    break;
                case "SaveCreatingEventArgs":
                    helper.Events.GameLoop.SaveCreating += GameEvent;
                    break;
                case "ReturnedToTitleEventArgs":
                    helper.Events.GameLoop.ReturnedToTitle += GameEvent;
                    break;
                case "BuildingListChangedEventArgs":
                    helper.Events.World.BuildingListChanged += GameEvent;
                    break;
                default:
                    logger.Log($"New game event type {eventType}", LogLevel.Error);
                    break;
            }
        }

        private void AddGameEventHook(EventArgs eventType)
        {
            switch (eventType)
            {
                case UpdateTickedEventArgs:
                    helper.Events.GameLoop.UpdateTicked += GameEvent;
                    break;
                case RenderedHudEventArgs:
                    helper.Events.Display.RenderedHud += GameEvent;
                    break;
                case OneSecondUpdateTickedEventArgs:
                    helper.Events.GameLoop.OneSecondUpdateTicked += GameEvent;
                    break;
                case UpdateTickingEventArgs:
                    helper.Events.GameLoop.UpdateTicking += GameEvent;
                    break;
                case NpcListChangedEventArgs:
                    helper.Events.World.NpcListChanged += GameEvent;
                    break;
                case LocationListChangedEventArgs:
                    helper.Events.World.LocationListChanged += GameEvent;
                    break;
                case GameLaunchedEventArgs:
                    helper.Events.GameLoop.GameLaunched += GameEvent;
                    break;
                case SavedEventArgs:
                    helper.Events.GameLoop.Saved += GameEvent;
                    break;
                case DayEndingEventArgs:
                    helper.Events.GameLoop.DayEnding += GameEvent;
                    break;
                case SavingEventArgs:
                    helper.Events.GameLoop.Saving += GameEvent;
                    break;
                case MenuChangedEventArgs:
                    helper.Events.Display.MenuChanged += GameEvent;
                    break;
                case DayStartedEventArgs:
                    helper.Events.GameLoop.DayStarted += GameEvent;
                    break;
                case SaveLoadedEventArgs:
                    helper.Events.GameLoop.SaveLoaded += GameEvent;
                    break;
                case LoadStageChangedEventArgs:
                    helper.Events.Specialized.LoadStageChanged += GameEvent;
                    break;
                case SaveCreatedEventArgs:
                    helper.Events.GameLoop.SaveCreated += GameEvent;
                    break;
                case SaveCreatingEventArgs:
                    helper.Events.GameLoop.SaveCreating += GameEvent;
                    break;
                case ReturnedToTitleEventArgs:
                    helper.Events.GameLoop.ReturnedToTitle += GameEvent;
                    break;
                default:
                    logger.Log($"New game event type {eventType.GetType().Name}", LogLevel.Debug);
                    break;
            }
        }
        private void GameEvent(object sender, EventArgs e)
        {
            if (actionSubscriptions.TryGetValue(e.GetType().Name, out List<Tuple<int, Action<EventArgs>>> actions))
            {
                foreach (Tuple<int, Action<EventArgs>> action in actions.OrderByDescending(p => p.Item1))
                {
                    action.Item2(e);
                }
            }
        }

        internal override void AddSubscription(EventTypes eventType, Action fnc)
        {
            if (eventSubscriptions.TryGetValue(eventType, out List<Action> actions))
            {
                actions.Add(fnc);
            }
            else
            {
                eventSubscriptions.Add(eventType, new List<Action> { fnc });
            }
        }
        internal override void TriggerEvent(EventTypes eventType)
        {
            if (eventSubscriptions.TryGetValue(eventType, out List<Action> actions))
            {
                foreach (Action action in actions)
                {
                    action();
                }
            }
        }

        internal override void DumpSubscriptions()
        {
            logger.Log($"Game Event Subscriptions", LogLevel.Info);
            logger.Log($"------------------------", LogLevel.Info);

            foreach (string key in actionSubscriptions.Keys.OrderBy(p => p))
            {
                logger.Log($"{key}", LogLevel.Debug);
                foreach (Tuple<int, Action<EventArgs>> action in actionSubscriptions[key].OrderByDescending(p => p.Item1))
                {
                    logger.Log($"   =>{action.Item2.Target.GetType().Name}.{action.Item2.Method.Name} [{action.Item1}]", LogLevel.Info);
                }
            }
            logger.Log("", LogLevel.Info);
            logger.Log($"Mod Event Subscriptions", LogLevel.Info);
            logger.Log($"------------------------", LogLevel.Info);
            foreach (EventTypes key in eventSubscriptions.Keys.OrderBy(p => p))
            {
                logger.Log($"{key}", LogLevel.Info);
                foreach (Action action in eventSubscriptions[key])
                {
                    logger.Log($"   =>{action.Target.GetType().Name}.{action.Method.Name}", LogLevel.Info);
                }
            }
            logger.Log("", LogLevel.Info);
            logger.Log($"Proxy Servers", LogLevel.Info);
            logger.Log($"------------------------", LogLevel.Info);
            foreach (KeyValuePair<string, Func<object[], object>> prox in proxyServers)
            {
                logger.Log($"{prox.Key} => {prox.Value.Method.DeclaringType.Name}.{prox.Value.Method.Name}", LogLevel.Info);
            }
        }

        internal override object GetProxyValue(string eventType, object[] args)
        {
            if (proxyServers.ContainsKey(eventType))
            {
                return proxyServers[eventType](args);
            }
            return null;
        }

        internal override void AddProxyServer(string eventType, Func<object[], object> action)
        {
            if (proxyServers.ContainsKey(eventType))
            {
                logger.Log($"Duplicate proxy servers for type {eventType}", LogLevel.Error);
            }
            else
            {
                proxyServers.Add(eventType, action);
            }
        }
    }
}
