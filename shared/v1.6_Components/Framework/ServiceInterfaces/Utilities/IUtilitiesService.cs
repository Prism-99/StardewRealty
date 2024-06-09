using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Diagnostics;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    /// <summary>
    /// Container for the main utilitie Services
    /// </summary>
    internal abstract class IUtilitiesService:IService
    {
        public override Type ServiceType => typeof(IUtilitiesService);
        public abstract IModHelperService ModHelperService { get; }
        public abstract IManifestService ManifestService { get; }
        public abstract IMonitorService MonitorService { get; }
        public abstract IConfigService ConfigService { get; }
        public abstract IPatchingService PatchingService { get; }
        public abstract IGameEventsService GameEventsService { get; }
        public abstract ICustomEventsService CustomEventsService { get; }
        public abstract IMapLoaderService MapLoaderService { get; }
        public abstract IMapUtilities MapUtilities { get; }
        public abstract IGameEnvironmentService GameEnvironment { get; }
        public bool HaveExpansions = false;
        public string GetCaller()
        {
            StackTrace stackTrace = new StackTrace();
            // Get calling method name
            return $"{stackTrace.GetFrame(2).GetMethod().DeclaringType.Name}.{stackTrace.GetFrame(2).GetMethod().Name}";
        }
       
        public  void InvalidateCache(string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName)) return;

            try
            {
                ModHelperService.modHelper.GameContent.InvalidateCache(cacheName);
            }
            catch (Exception ex)
            {
                logger.Log($"Error invalidating cache {cacheName}", LogLevel.Error);
                logger.LogError("InvalidateCache", ex);
            }
        }
        public bool IsMasterGame()
        {
            return (Game1.IsMultiplayer && Game1.IsMasterGame) || !Game1.IsMultiplayer;
        }
        internal Rectangle GetGridLocationCoordinates(int iGridId, Rectangle container)
        {
            int iRow = (iGridId - 1) / 3;
            int iCol = (iGridId - 1) % 3;

            int topMargin = 30;
            int leftMargin = 5;
            int boxPadding = 4;
            int imageBoxWidth = (container.Width - boxPadding*5) / 4;
            int imageBoxHeight = (container.Height - boxPadding*2 - topMargin) / 3;
            int imageWidth = imageBoxWidth - boxPadding * 2;
            int imageHeight = imageBoxHeight - boxPadding * 2;
            int left =container.Left- leftMargin*2 + (imageBoxWidth + boxPadding) * 3;
            int top = container.Top;
            switch (iGridId)
            {
                case 0: //by the Backwoods
                    return new Rectangle(left + boxPadding, top+ topMargin + boxPadding, imageWidth, imageHeight);
                //case 1:
                //    return new Rectangle(63, 64 + iMapIndex * 14, iImageWidth, iImageHeight);
                default:
                    return new Rectangle(left - iRow * imageBoxWidth - imageBoxWidth + boxPadding, top+ topMargin + iCol * imageBoxHeight + boxPadding, imageWidth, imageHeight);
            }

        }
    }
}
