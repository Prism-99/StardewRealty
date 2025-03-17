using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GUI
{
    internal abstract class IMiniMapService : IService
    {
        public override Type ServiceType => typeof(IMiniMapService);

        public class MiniMapEntry
        {
            public string DisplayName { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public MapWarp? Warp { get; set; } = null;
            public int GridId { get; set; }
            public string TexturePath { get; set; } = string.Empty;
            public Texture2D? Texture { get; set; } = null;
            public void FireWarp()
            {
                if (Warp != null)
                    DelayedAction.warpAfterDelay(Warp.ToMap, new Point(Warp.ToX, Warp.ToY), 100);
            }
        }
    }
}
