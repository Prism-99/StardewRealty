using System;
using xTile;
using System.IO;
using xTile.Tiles;
using xTile.Format;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.Helpers;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_xTile
{
    /*
     *  
     *  Class for loading tbin files
     * 
     *  code is liberally borrowing upon code inside of
     *  SMAPI by Pathoschild, with some names and locations
     *  changed to protect the innocent
     * 
     *  The code has been watered down from generic to single 
     *  purpose
     */
    public  class MapLoader
    {
        public  string FullRootDirectory;
        private  readonly string[] LocalTilesheetExtensions = { ".png", ".xnb" };
        private  IModHelperService helper;
        private  ILoggerService logger;
        internal  Map LoadMap(string sGameRootPath, string sMapRelativePath, string sLocationName, bool bLoadTileSheets)
        {
            return LoadMap(sGameRootPath, sMapRelativePath, sLocationName, bLoadTileSheets, true);
        }
        internal  Map LoadMap(string sGameRootPath, string sMapRelativePath, string sLocationName, bool bLoadTileSheets, bool removeExt, bool fixTileSheets = true)
        {
            FullRootDirectory = sGameRootPath;
            FormatManager formatManager = FormatManager.Instance;
            sMapRelativePath = SDVPathUtilities.NormalizePath(sMapRelativePath.Replace(sGameRootPath, ""));
            // Monitor.Log($"Map path: {sMapPath}", LogLevel.Info);
            string assetName = Path.GetFileName(sMapRelativePath);
            sMapRelativePath = sMapRelativePath.TrimStart('/').TrimStart('\\');
            logger.Log($"LoadMap sMapRelativePath: {sMapRelativePath}", LogLevel.Debug);
            logger.Log($"LoadMap sGameRootPath: {sGameRootPath}", LogLevel.Debug);
            logger.Log($"Load map path: {Path.Combine(sGameRootPath, sMapRelativePath)}", LogLevel.Debug);
            Map mNewMap = formatManager.LoadMap(Path.Combine(sGameRootPath, sMapRelativePath));
            mNewMap.Id = sLocationName;
            mNewMap.assetPath = Path.Combine(sGameRootPath, sMapRelativePath);
            if (fixTileSheets)
                FixTilesheetPaths(mNewMap, logger, relativeMapPath: sMapRelativePath, fixEagerPathPrefixes: false, sLocationName, removeExt);
            //Map mNewMap = oPack.Owner.LoadAsset<Map>(Path.Combine( "assets", oPack.MapName));
            if (bLoadTileSheets)
            {

                logger.Log($"Loading Tilesheets for expansion: '{sLocationName}'", LogLevel.Debug);
                mNewMap.LoadTileSheets(Game1.mapDisplayDevice);
            }
            return mNewMap;
        }

        internal  void Initialize(IModHelperService oHelper, ILoggerService olog)
        {
            helper = oHelper;
            logger = olog;
            //if (Constants.GameFramework == GameFramework.Xna)
            //{

            //Reflector Reflection = new Reflector();
            //IReflectedMethod method = reflection.GetMethod(typeof(TitleContainer), "GetCleanPath");
            //else if (EarlyConstants.IsWindows64BitHack)
            //    this.NormalizeAssetNameForPlatform = PathUtilities.NormalizePath;
            //else
            //}
        }
        internal  void FixTilesheetPaths(Map map, ILoggerService monitor, string relativeMapPath, bool fixEagerPathPrefixes, string sModName, bool removeExt)
        {
            // get map info
            relativeMapPath = SDVPathUtilities.AssertAndNormalizeAssetName(relativeMapPath); // Mono's Path.GetDirectoryName doesn't handle Windows dir separators
            string relativeMapFolder = Path.GetDirectoryName(relativeMapPath) ?? ""; // folder path containing the map, relative to the mod folder

            // fix tilesheets
#if LOG_TRACE
            monitor.Log($"Fixing tilesheet paths for map '{relativeMapPath}' from mod '{sModName}'...");
#endif
            foreach (TileSheet tilesheet in map.TileSheets)
            {

                // get image source
                tilesheet.ImageSource = SDVPathUtilities.NormalizePath(tilesheet.ImageSource);
                string imageSource = tilesheet.ImageSource;

                // reverse incorrect eager tilesheet path prefixing
                if (fixEagerPathPrefixes && relativeMapFolder.Length > 0 && imageSource.StartsWith(relativeMapFolder))
                    imageSource = imageSource.Substring(relativeMapFolder.Length + 1);

                // validate tilesheet path
                string errorPrefix = $"{sModName} loaded map '{relativeMapPath}' with invalid tilesheet path '{imageSource}'.";
                if (Path.IsPathRooted(imageSource) || imageSource.Contains(".."))
                    throw new Exception($"{errorPrefix} Tilesheet paths must be a relative path without directory climbing (../).");

                // load best match
                try
                {
                    if (!TryGetTilesheetAssetName(relativeMapFolder, imageSource, out string assetName, out string error))
                        throw new Exception($"{errorPrefix} {error}");

                    if (!removeExt)
                    {
                        string[] arExt = tilesheet.ImageSource.Split('.');
                        if (arExt.Length > 1)
                        {
                            assetName = assetName + "." + arExt[arExt.Length - 1];
                        }
                    }
 
                    if (assetName.Contains(relativeMapFolder))
                    {
                        assetName = Path.Combine("Maps", "femaps", sModName, assetName.Replace(relativeMapFolder, "").Replace("\\", ""));
                        if (assetName.EndsWith(".png") && removeExt) assetName = assetName.Substring(0, assetName.Length - 4);
                    }
                    if (assetName.StartsWith("Maps\\femaps"))
                        assetName = assetName.Replace("Maps\\femaps", "SDR/assets");

                    tilesheet.ImageSource = assetName.Replace("\\", "/");

                    //if (assetName != tilesheet.ImageSource)
                    //    monitor.Log($"   Mapped tilesheet '{tilesheet.ImageSource}' to '{assetName}'.  Remove ext {removeExt}");


                 }
                catch (Exception ex)
                {
                    monitor.Log($"{errorPrefix} The tilesheet couldn't be loaded." + ex.Message, LogLevel.Error);
                }
            }
        }
        //public static string NormalizePath(string sPath)
        //{
        //    string sClean = sPath.Replace("/", "\\");
        //    sClean = sPath.Replace("\\\\", "\\");

        //    return sClean;
        //}
        public  bool TryGetTilesheetAssetName(string modRelativeMapFolder, string relativePath, out string assetName, out string error)
        {
            assetName = null;
            error = null;

            // nothing to do
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                assetName = relativePath;
                return true;
            }

            // special case: local filenames starting with a dot should be ignored
            // For example, this lets mod authors have a '.spring_town.png' file in their map folder so it can be
            // opened in Tiled, while still mapping it to the vanilla 'Maps/spring_town' asset at runtime.
            {
                string filename = Path.GetFileName(relativePath);
                if (filename.StartsWith("."))
                    relativePath = Path.Combine(Path.GetDirectoryName(relativePath) ?? "", filename.TrimStart('.'));
            }

            // get relative to map file
            {
                string localKey = Path.Combine(modRelativeMapFolder, relativePath);
                if (GetModFile(localKey).Exists)
                {
                    assetName = GetInternalAssetKey(localKey, assetName);
                    return true;
                }
            }
            // get from game assets
            string contentKey = GetContentKeyForTilesheetImageSource(relativePath);
            try
            {
                //GameContentManager.Load<Texture2D>(contentKey, "en", useCache: true); // no need to bypass cache here, since we're not storing the asset
                assetName = contentKey;
                return true;
            }
            catch
            {
                // ignore file-not-found errors
                // TODO: while it's useful to suppress an asset-not-found error here to avoid
                // confusion, this is a pretty naive approach. Even if the file doesn't exist,
                // the file may have been loaded through an IAssetLoader which failed. So even
                // if the content file doesn't exist, that doesn't mean the error here is a
                // content-not-found error. Unfortunately XNA doesn't provide a good way to
                // detect the error type.
                if (GetContentFolderFileExists(contentKey))
                    throw;
            }

            error = "The tilesheet couldn't be found relative to either map file or the game's content folder.";
            return false;
        }
        private  bool GetContentFolderFileExists(string key)
        {
            // get file path
            string path = Path.Combine(FullRootDirectory, "Content", key);
            if (!path.EndsWith(".xnb"))
                path += ".xnb";

            // get file
            return new FileInfo(path).Exists;
        }
        private  string GetContentKeyForTilesheetImageSource(string relativePath)
        {
            string key = relativePath;
            string topFolder = IOHelpers.GetSegments(key, limit: 2)[0];

            // convert image source relative to map file into asset key
            //if (!topFolder.Equals("Maps", StringComparison.OrdinalIgnoreCase))
            //    key = Path.Combine("Maps", key);

            // remove file extension from unpacked file
            if (key.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                key = key.Substring(0, key.Length - 4);

            return key;
        }
        public  string GetInternalAssetKey(string key, string sModName)
        {
            FileInfo file = GetModFile(key);
            string relativePath = SDVPathUtilities.GetRelativePath(FullRootDirectory, file.FullName);
            return Path.Combine(sModName ?? "", relativePath);
        }
        private  FileInfo GetModFile(string path)
        {
            // try exact match
            FileInfo file = new FileInfo(Path.Combine(FullRootDirectory, path));

            // try with default extension
            if (!file.Exists && file.Extension == string.Empty)
            {
                foreach (string extension in LocalTilesheetExtensions)
                {
                    FileInfo result = new FileInfo(file.FullName + extension);
                    if (result.Exists)
                    {
                        file = result;
                        break;
                    }
                }
            }

            return file;
        }
        //public static string AssertAndNormalizeAssetName(string assetName)
        //{
        //    // NOTE: the game checks for ContentLoadException to handle invalid keys, so avoid
        //    // throwing other types like ArgumentException here.
        //    if (string.IsNullOrWhiteSpace(assetName))
        //        throw new Exception("The asset key or local path is empty.");
        //    if (assetName.Intersect(Path.GetInvalidPathChars()).Any())
        //        throw new Exception("The asset key or local path contains invalid characters.");

        //    return NormalizeKey(assetName);
        //}
        //public static string NormalizeKey(string key)
        //{
        //    key = NormalizePathSeparators(key);
        //    return key.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase)
        //        ? key.Substring(0, key.Length - 4)
        //        : NormalizeAssetNameForPlatform(key);
        //}
        //public static string NormalizePathSeparators(string path)
        //{
        //    return IOHelpers.NormalizePath(path);
        //}

        //private static Func<string, string> NormalizeAssetNameForPlatform;


    }
}
