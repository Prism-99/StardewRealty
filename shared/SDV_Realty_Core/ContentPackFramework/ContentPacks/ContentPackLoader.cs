using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewRealty.SDV_Realty_Interface;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks
{
    /// <summary>
    /// Common loader class
    /// </summary>
    internal partial class ContentPackLoader
    {
        public ILoggerService logger { get; }
        public List<ExpansionPack> Contents { get; }

        
        public Dictionary<string, Map> ExpansionMaps;
        public Dictionary<string, ExpansionPack> ValidContents { get; }
        private IModHelper helper;
        public ContentPackLoader() { }
        public ContentPackLoader(ILoggerService olog, IModHelper helper)
        {
            logger = olog;
            Contents = new List<ExpansionPack>();
            ValidContents = new Dictionary<string, ExpansionPack> { };
            this.helper = helper;
        }
        private void PrintContentPackListSummary()
        {
            logger.Log($"Loaded {ValidContents.Count} content packs:", LogLevel.Info);
            ValidContents.Select(c => c.Value.Owner.Manifest)
                .ToList()
                .ForEach(
                    (m) => logger.Log($"   {m.Name} {m.Version} by {m.Author} ({m.UniqueID})", LogLevel.Info));
        }
         private void Prepare(ISDRContentPack contentPack)
        {
            switch (contentPack)
            {
                case ExpansionPack content:

                    if (!string.IsNullOrEmpty(content.ThumbnailName))
                    {
                        try
                        {
                            FileStream filestream = new FileStream(Path.Combine(content.ModPath, "assets", content.ThumbnailName), FileMode.Open);
                            content.Thumbnail = Texture2D.FromStream(Game1.graphics.GraphicsDevice, filestream);
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(content.ForSaleImageName))
                    {
                        try
                        {
                            FileStream filestream = new FileStream(Path.Combine(content.ModPath, "assets", content.ForSaleImageName), FileMode.Open);
                            content.ForSaleImage = Texture2D.FromStream(Game1.graphics.GraphicsDevice, filestream);
                        }
                        catch { }
                    }
                    if (content.FishAreas == null || content.FishAreas.Count == 0)
                    {
                        content.FishAreas = new Dictionary<string, FishAreaDetails>
                        {
                            {"default",new FishAreaDetails{  DisplayName="Default",Id="default"} }
                        };
                    }
                    break;
            }
        }
        

        private ExpansionPack LoadExpansionContentPack(IContentPack contentPack)
        {
            ExpansionPack content = null;

            try
            {
                content = contentPack.ReadJsonFile<ExpansionPack>("expansion.json");

                if (content.FileFormat == null || content.FileFormat.MajorVersion < 1 || content.FileFormat.MinorVersion < 2)
                {
                    logger.Log($"{contentPack.Manifest.Name} is not at least version 1.2 and will not be loaded.", LogLevel.Error);
                    return null;
                }

                content.Owner = contentPack;
                content.ModPath = contentPack.DirectoryPath;
                Prepare(content);

                logger.Log("CP: " + content.LocationName + ", " + content.Description, LogLevel.Debug);

            }
            catch (Exception ex)
            {
                logger.Log($"Content pack {contentPack.Manifest.Name} failed loading 'expansion.json' and will not be available in game.", LogLevel.Error);
                logger.Log($"Error Details: {ex.Message}");
                content = null;
            }

            return content;
        }
    }
}
