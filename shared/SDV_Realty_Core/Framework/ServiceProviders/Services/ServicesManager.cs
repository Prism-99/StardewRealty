using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;


namespace SDV_Realty_Core.Framework.ServiceProviders.Services
{
    internal class ServicesManager : IServicesManager
    {
        private string currentFetchType = null;
        private string argFetch = null;
        private Stack<string> fetchingTopLevel = new Stack<string>();
        private Stack<string> fetchingArgs = new Stack<string>();
        private Dictionary<string, List<Action<object[]>>> eventRequesters = new();
        //private Dictionary<string, Func<string, object[]>> eventHandlers = new Dictionary<string, Func<string, object[]>>();
        private Dictionary<string, long> serviceLoadTimes = new Dictionary<string, long>();
        public ServicesManager(ILoggerService logger)
        {
            this.logger =  logger;
        }
        internal override void AddService(Type serviceType, IService manager)
        {
            services[serviceType] = manager;
        }
        internal override void AddService(IService manager)
        {
            services[manager.ServiceType] = manager;
        }

        internal override void DumpLoadTimes()
        {
            logger.Log($"Service Load Times", LogLevel.Debug);
            logger.Log($"------------------", LogLevel.Debug);
            foreach (string key in serviceLoadTimes.Keys.OrderBy(p => p))
            {
                logger.Log($"{key}: {serviceLoadTimes[key]}ms", LogLevel.Debug);
            }
        }
        public override void DumpConfiguration()
        {
            logger.Log($"-------------------------", LogLevel.Info);
            logger.Log($"Application Configuration", LogLevel.Info);
            logger.Log($"-------------------------", LogLevel.Info);
            logger.Log($"Defined Service Triggers:", LogLevel.Info);
            logger.Log("",LogLevel.Info);
            Dictionary<string, List<string>> requesters = new();
            Dictionary<string, List<string>> providers = new();
            foreach (KeyValuePair<Type, IService> service in services)
            {
                if (service.Value?.CustomServiceEventTriggers != null)
                {
                    foreach (string trigger in service.Value.CustomServiceEventTriggers)
                    {
                        if (requesters.ContainsKey(trigger))
                        {
                            requesters[trigger].Add(service.Key.Name);
                        }
                        else
                        {
                            requesters[trigger] = new List<string> { service.Key.Name };
                        }
                    }
                }
                if (service.Value?.CustomServiceEventSubscripitions != null)
                {
                    foreach (string trigger in service.Value.CustomServiceEventSubscripitions)
                    {
                        if (providers.ContainsKey(trigger))
                        {
                            providers[trigger].Add(service.Key.Name);
                        }
                        else
                        {
                            providers[trigger] = new List<string> { service.Key.Name };
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, List<string>> requestor in requesters)
            {
                logger.Log($"{requestor.Key}", LogLevel.Info);
                logger.Log(new string('-', requestor.Key.Length), LogLevel.Info);
                logger.Log($"Handlers:", LogLevel.Info);
                if (providers.ContainsKey(requestor.Key))
                {
                    foreach (string provider in providers[requestor.Key])
                    {
                        logger.Log($"  =>{provider}", LogLevel.Trace);
                    }
                    providers.Remove(requestor.Key);
                }
                else
                {
                    logger.Log("None", LogLevel.Info);
                }
                logger.Log($"Triggers:", LogLevel.Info);
                foreach (string sink in requesters[requestor.Key])
                {
                    logger.Log($"  <={sink}", LogLevel.Trace);
                }
                logger.Log("", LogLevel.Info);
            }
            logger.Log($"Non-Called Handlers", LogLevel.Info);
            logger.Log("-------------------", LogLevel.Info);
            if (providers.Any())
            {
                foreach (KeyValuePair<string, List<string>> provider in providers)
                {
                    logger.Log($"{provider.Key} ({providers.Values.Count()})", LogLevel.Info);
                }
            }
            else
            {
                logger.Log("None", LogLevel.Info);
            }
            //
            //  dump service details
            //
            logger.Log($"", LogLevel.Info);
            logger.Log($"Service Details", LogLevel.Info);
            logger.Log($"---------------", LogLevel.Info);
            foreach (KeyValuePair<Type, IService> serivce in services.OrderBy(p => p.Key.Name))
            {
                logger.Log($"{serivce.Key.Name}:", LogLevel.Info);
                List<IService> clients = services.Values.Where(p => p.InitArgs != null && p.InitArgs.Contains(serivce.Key)).ToList();
                clients.AddRange(services.Values.Where(p => p.Dependants != null && p.Dependants.Contains(serivce.Key)));
                if (clients.Any())
                {
                    foreach (IService client in clients.OrderBy(p => p.ServiceType.Name))
                    {
                        logger.Log($" -{client.ServiceType.Name}", LogLevel.Info);
                    }
                }
                else
                {
                    logger.Log($"   None", LogLevel.Info);
                }
                logger.Log("",LogLevel.Info);
            }
        }
        private int GetPadding()
        {
            return (fetchingTopLevel.Count - 1 + Math.Max(0, fetchingArgs.Count - 2)) * 5;
        }
        internal override T GetService<T>(Type serviceType)
        {
            if (fetchingTopLevel.Contains(serviceType.Name))
            {
                logger.Log($"{new string(' ', GetPadding())}Circular reference: Arg: {serviceType.Name}, Service: {fetchingTopLevel.Peek()}", LogLevel.Error);
                return default(T);
            }
            if (fetchingTopLevel.Count > 0)
            {
                logger.Log($"{new string(' ', GetPadding())}Getting service {serviceType.Name} for {fetchingTopLevel.Peek()}", LogLevel.Debug);
            }
            else
            {
                logger.Log($"Getting Service {serviceType.Name}", LogLevel.Debug);
            }
            if (cachedServices.TryGetValue(serviceType, out var cachedService))
            {
                return (T)Convert.ChangeType(cachedService, typeof(T));
            }


            if (fetchingArgs.Count == 0)
            {
                fetchingTopLevel.Push(serviceType.Name);
            }

            if (services.TryGetValue(serviceType, out var service))
            {
                Stopwatch sw = Stopwatch.StartNew();

                object[] args = null;
                if (service?.InitArgs != null)
                {
                    fetchingTopLevel.Push(serviceType.Name);
                    args = new object[service.InitArgs.Length];
                    int index = 0;
                    foreach (Type serviceArgs in service.InitArgs)
                    {
                        MethodInfo methodInfo = GetType().GetMethod("GetService", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Type) }, null);
                        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(serviceArgs);
                        fetchingArgs.Push(serviceArgs.Name);
                        IService argService = (IService)genericMethodInfo.Invoke(this, new object[] { serviceArgs });
                        if (argService == null)
                        {
                            logger.Log($"{new string(' ', GetPadding())}Cannot get argument type {serviceArgs.Name} for  Service {serviceType.Name}", LogLevel.Error);
                            return default;
                        }
                        else
                        {
                            //logger.Log($"{new string(' ', GetPadding())}Loaded service {argService.GetType().Name} in {sw.ElapsedMilliseconds:N0}ms", LogLevel.Debug);
                            //AddServiceHooks(argService);
                            args[index++] = argService;
                        }

                        fetchingArgs.Pop();
                    }
                    fetchingTopLevel.Pop();
                }
                if (service?.Dependants != null)
                {
                    //
                    //  initialize depandant services
                    //
                    foreach (Type dependant in service.Dependants)
                    {
                        MethodInfo methodInfo = this.GetType().GetMethod("GetService", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Type) }, null);
                        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(dependant);
                        fetchingArgs.Push(dependant.Name);
                        IService argService = (IService)genericMethodInfo.Invoke(this, new object[] { dependant });
                        //logger.Log($"{new string(' ', GetPadding())}Loaded service {serviceType.Name} in {sw.ElapsedMilliseconds:N0}ms", LogLevel.Debug);
                        fetchingArgs.Pop();
                        if (argService == null)
                        {
                            logger.Log($"{new string(' ', GetPadding())}Cannot get dependant type {dependant.Name} for Service {serviceType.Name}", LogLevel.Error);
                            return default;
                        }
                        //else
                        //{
                        //    AddServiceHooks(argService);
                        //}
                    }
                }
                service.OnRegisterEvent += Service_OnRegisterEvent;
                service.OnTriggerEvent += Service_OnTriggerEvent;
                service.Initialize(logger, args);
                sw.Stop();
                serviceLoadTimes[service.ServiceType.Name] = sw.ElapsedMilliseconds;
                cachedServices.Add(serviceType, service);
                if (fetchingArgs.Count == 0 && fetchingTopLevel.Count > 0)
                {
                    //logger.Log($"Loaded service {fetchingTopLevel.Peek()} in {sw.ElapsedMilliseconds:N0}ms", LogLevel.Debug);
                }
                else if (fetchingArgs.Count == 0)
                {
                    //logger.Log($"{new string(' ', GetPadding())}Loaded service {serviceType.Name} in {sw.ElapsedMilliseconds:N0}ms", LogLevel.Debug);
                }
                if (fetchingArgs.Count == 0) fetchingTopLevel.Pop();
                //fetchingTopLevel.Pop();

                return (T)Convert.ChangeType(service, typeof(T));
            }

            logger.Log($"Could not find Service provide for Type {serviceType.Name}", LogLevel.Debug);

            return default;
        }
      
        private void Service_OnTriggerEvent(string eventName, object[] eventParameters)
        {
            if (eventRequesters.ContainsKey(eventName))
            {
                foreach (Action<object[]> request in eventRequesters[eventName])
                {
                    request(eventParameters);
                }
            }
        }

        private void Service_OnRegisterEvent(string eventName, Action<object[]> callback)
        {
            if (eventRequesters.ContainsKey(eventName))
            {
                eventRequesters[eventName].Add(callback);
            }
            else
            {
                eventRequesters.Add(eventName, new List<Action<object[]>>
                {
                    callback
                });
            }
        }

        internal override bool IsServiceRegistered(Type serviceType)
        {
            return services.ContainsKey(serviceType);
        }

        internal override void RemoveService(Type serviceType)
        {
            services.Remove(serviceType);
            cachedServices.Remove(serviceType);
        }

        internal override void VerifyServices()
        {
            logger.Log($"Verifing Services configuration", LogLevel.Debug);
            foreach (IService service in services.Values)
            {
                if (service?.InitArgs != null)
                {
                    foreach (Type arg in service.InitArgs)
                    {
                        if (services.ContainsKey(arg))
                        {
                            if (services[arg]?.InitArgs != null)
                            {
                                //
                                // check for circular references
                                //
                                IEnumerable<Type> circReference = services[arg].InitArgs.Where(p => p == service.ServiceType);
                                if (circReference.Any())
                                {
                                    // circular reference
                                    logger.Log($"    {service.ServiceType.Name} has a circular reference to {arg.Name}", LogLevel.Error);
                                }
                            }
                        }
                        else
                        {
                            logger.Log($"    {service.ServiceType.Name} is missing arg type of {arg.Name}", LogLevel.Error);
                        }
                    }
                }
                if (service?.Dependants != null)
                {
                    foreach (Type dependant in service.Dependants)
                    {
                        if (services.ContainsKey(dependant))
                        {
                            if (services[dependant]?.InitArgs != null)
                            {
                                //
                                // check for circular references
                                //
                                IEnumerable<Type> circReference = services[dependant].InitArgs.Where(p => p == service.ServiceType);
                                if (circReference.Any())
                                {
                                    // circular reference
                                    logger.Log($"    {service.ServiceType.Name} has a dependant circular reference to {dependant.Name}", LogLevel.Error);
                                }
                            }
                        }
                        else
                        {
                            logger.Log($"    {service.ServiceType.Name} is missing dependant type of {dependant.Name}", LogLevel.Error);
                        }
                    }
                }
            }
            logger.Log($"Finished verifing Services configuration", LogLevel.Debug);
        }
    }
}
