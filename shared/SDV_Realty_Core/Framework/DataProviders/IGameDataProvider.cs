using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Inteface for classes to edit game data
    /// </summary>
    internal abstract class IGameDataProvider
    {
        protected ILoggerService logger;

        public void SetLogger(ILoggerService olog)
        {
             logger = olog;
        }
        /// <summary>
        /// Name of the Data Type the class edits
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Method called by manager to request data editing
        /// </summary>
        /// <param name="e"></param>
        public abstract void HandleEdit(AssetRequestedEventArgs e);
        public virtual bool Handles(string assetName)
        {
            return assetName == Name;
        }
        public abstract void OnGameLaunched();
        public abstract void CheckForActivations();
        internal bool UseLore {  get; set; }
    }
}
