using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.ServiceProviders.Events
{
    /// <summary>
    /// Handles game event triggers
    /// </summary>
    internal class GameEventsService : IGameEventsService
    {
        //private IModDataService modDataService;
        public override Type ServiceType => typeof(IGameEventsService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService)
        };

        private IModHelper helper;
        private Dictionary<EventTypes, List<Tuple<int, Action>>> eventSubscriptions;
        private Dictionary<string, Func<object[], object>> proxyServers = new();
        private Dictionary<string, List<Tuple<int, Action<EventArgs>>>> actionSubscriptions;
        private bool modsLoadedTriggered = false;
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
            eventSubscriptions = new();
            actionSubscriptions = new Dictionary<string, List<Tuple<int, Action<EventArgs>>>>();

            helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;

        }

        private void Display_RenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (!modsLoadedTriggered && Game1.activeClickableMenu.GetType()?.Name == "TitleMenu")
            {
                modsLoadedTriggered = true;
                TriggerEvent(EventTypes.AllModsLoaded);
            }
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
                case "AssetReadyEventArgs":
                    helper.Events.Content.AssetReady += GameEvent;
                    break;
                case "WindowResizedEventArgs":
                    helper.Events.Display.WindowResized += GameEvent;
                    break;
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
        private void GameEvent(object? sender, EventArgs e)
        {
            switch (e.GetType().Name)
            {
                case "UpdateTickedEventArgs":
                case "UpdateTickingEventArgs":
                case "OneSecondUpdateTickedEventArgs":
                case "RenderedHudEventArgs":
                    break;
                case "AssetReadyEventArgs":
                    AssetReadyEventArgs ev = (AssetReadyEventArgs)e;
                    if (assetRequests.ContainsKey(ev.NameWithoutLocale.Name))
                    {
                        var val = assetRequests[ev.NameWithoutLocale.Name];
                        var incVal = Tuple.Create(val.Item1, val.Item2 + 1);
                        assetRequests[ev.NameWithoutLocale.Name] = incVal;
                    }
                    else
                    {
                        assetRequests.Add(ev.NameWithoutLocale.Name, Tuple.Create(0, 1));
                    }
                    //logger.Log($"GameEvent {e.GetType().Name} fired. Asset='{ev.NameWithoutLocale}'", LogLevel.Debug);
                    break;
                case "AssetRequestedEventArgs":
                    AssetRequestedEventArgs ev1 = (AssetRequestedEventArgs)e;
                    if (assetRequests.ContainsKey(ev1.NameWithoutLocale.Name))
                    {
                        var val = assetRequests[ev1.NameWithoutLocale.Name];
                        var incVal = Tuple.Create(val.Item1 + 1, val.Item2);
                        assetRequests[ev1.NameWithoutLocale.Name] = incVal;
                    }
                    else
                    {
                        assetRequests.Add(ev1.NameWithoutLocale.Name, Tuple.Create(1, 0));
                    }
                    //logger.Log($"GameEvent {e.GetType().Name} fired. Asset='{ev1.NameWithoutLocale}'", LogLevel.Debug);
                    break;
                default:
                    logger.Log($"GameEvent {e.GetType().Name} fired", LogLevel.Debug);
                    break;
            }
            if (actionSubscriptions.TryGetValue(e.GetType().Name, out List<Tuple<int, Action<EventArgs>>> actions))
            {
                foreach (Tuple<int, Action<EventArgs>> action in actions.OrderByDescending(p => p.Item1))
                {
                    action.Item2(e);
                }
            }
        }
        //internal override void AddSubscription(EventTypes eventType, Action fnc)
        //{
        //    throw new NotImplementedException();
        //}
        internal override void AddSubscription(EventTypes eventType, Action fnc, int priority = 0)
        {
            if (eventSubscriptions.TryGetValue(eventType, out List<Tuple<int, Action>> actions))
            {
                actions.Add(Tuple.Create(priority, fnc));
            }
            else
            {
                eventSubscriptions.Add(eventType, new List<Tuple<int, Action>> { Tuple.Create(priority, fnc) });
            }
        }
        internal override void TriggerEvent(EventTypes eventType)
        {
            if (eventSubscriptions.TryGetValue(eventType, out List<Tuple<int, Action>> actions))
            {
                foreach (Action action in actions.OrderBy(p => p.Item1).Select(p => p.Item2))
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
                foreach (var action in eventSubscriptions[key])
                {
                    logger.Log($"   =>{action.Item2.Target.GetType().Name}.{action.Item2.Method.Name}, priority: {action.Item1}", LogLevel.Info);
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
