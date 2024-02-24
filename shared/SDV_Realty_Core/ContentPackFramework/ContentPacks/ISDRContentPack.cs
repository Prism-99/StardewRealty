using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks
{
    /// <summary>
    /// Base abstract class for CustomEntities
    ///     -Building
    ///     -SDObject
    ///     -MachineData
    ///     -BigCraftable
    ///     -AudioChange
    ///     -Crop
    ///     -Exapnsion
    ///     -LocationContext
    ///     -Movies
    ///     -SDRMachine (experimental)
    /// </summary>
    public abstract class ISDRContentPack
    {
        public enum ProductionModificationTypes
        {
            Speed = 0,
            Quality = 1,
            Quantity = 2,
            Output = 3
        }
        public enum ModificationTargets
        {
            Machine = 0,
            Crop = 1,
            Building = 2
        }
        public class ProductionModifier
        {
            public ModificationTargets ModificationTarget = ModificationTargets.Machine;
            public ProductionModificationTypes ModificationType;
            public float ModificationQuantity;
            public Vector2 OuputChestPosition;
            public string OutputItemId;
            public int OutputQuantity;
            public string OutputCondition;
        }
        internal ILoggerService logger;
        public Dictionary<string, List<ProductionModifier>> ProductionModifiers { get; set; }
        public SDVModVersion FileFormat { get; set; }
        public string DLLToLoad { get; set; }
        public string DLLNameSpace { get; set; }
        public string ModPath { get; set; }
        internal IContentPack Owner { get; set; }
        public string Conditions { get; set; }
        public bool Added { get; set; }
        public ITranslationHelper translations { get; set; }
        public abstract ISDRContentPack ReadContentPack(string fileName);
        public abstract string PackFileName { get; }
        public List<string> ContextTags { get; set; }
        internal void SetLogger(ILoggerService olog)
        {
            logger = olog;
        }
        private Type DLLType;
        internal bool dllLoaded = false;

        /// <summary>
        /// Load custom entity DLL.
        /// DLL is expected to have the following method:
        ///     Entry(IMonitor, IModHelper)
        /// </summary>
        /// <param name="dllName">Relative path to the DLL to load</param>
        /// <param name="contentPackName">Name of content pack loading the DLL</param>
        /// <param name="monitor">IMonitor instance to pass to the DLL</param>
        /// <param name="modHelper">IModHelper instance to pass to the DLL</param>
        internal void LoadDll(string dllName,string contentPackName, IMonitor monitor,IModHelper modHelper)
        {
            var DLL = Assembly.LoadFile(Path.Combine(ModPath, dllName));
            DLLType = DLL.GetType(DLLNameSpace);
            dllLoaded = true;
            InvokeDLLMethod(contentPackName,"Entry", new object[] {  monitor,modHelper});
            logger.Log($"Loaded DLL {dllName} for mod {Owner.Manifest.Name}.{contentPackName}", LogLevel.Debug);
        }

        /// <summary>
        /// Invoke a method in the entities DLL
        /// </summary>
        /// <param name="contentPackName">Pack calling method</param>
        /// <param name="methodName">Method to be invoked</param>
        /// <param name="args">Arguments to be passed to invocation</param>
        internal void InvokeDLLMethod(string contentPackName, string methodName, object[] args)
        {
            if (dllLoaded)
            {
                var instance = Activator.CreateInstance(DLLType);
                logger.Log($"Invoking DLL method {methodName} for mod {Owner.Manifest.Name}.{contentPackName}", LogLevel.Debug);
                DLLType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, instance, args);
            }
        }
    }
}

