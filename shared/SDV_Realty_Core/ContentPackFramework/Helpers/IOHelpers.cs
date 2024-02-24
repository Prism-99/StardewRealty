using System;
using System.Linq;
using System.IO;



namespace SDV_Realty_Core.ContentPackFramework.Helpers
{
    class IOHelpers
    {
        public static readonly char PreferredPathSeparator = Path.DirectorySeparatorChar;
        public static readonly char[] PossiblePathSeparators = new[] { '/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.Distinct().ToArray();
        public const string WindowsUncRoot = @"\\";
         private static IModHelper  helper;
        public static GameFrameworks GameFramework;
        public enum GameFrameworks
        {
            /// <summary>The XNA Framework on Windows.</summary>
            Xna,

            /// <summary>The MonoGame framework, usually on non-Windows platforms.</summary>
            MonoGame
        }
        public static void Initialize(IModHelper oHelper)
        {
            helper = oHelper;
            OperatingSystem os_info = System.Environment.OSVersion;
            GameFramework = os_info.Platform.ToString().Contains("Win") ? GameFrameworks.Xna : GameFrameworks.MonoGame;
            //GameRoot = helper.DirectoryPath;
            //string sModDir = "Mods";
            //if (Environment.GetCommandLineArgs().Length > 2)
            //{
            //    if (Environment.GetCommandLineArgs()[1] == "--mods-path")
            //    {
            //        sModDir = Environment.GetCommandLineArgs()[2];
            //    }
            //}
            //int iModPos = SDVEnvironment.GamePath.IndexOf(sModDir, 0, StringComparison.OrdinalIgnoreCase);
            //if (iModPos > 0)
            //{
            //    GameRoot = SDVEnvironment.GamePath.Substring(0, iModPos );
            //}
        }
        public static string[] GetSegments(string path, int? limit = null)
        {
            return limit.HasValue
                ? path.Split(PossiblePathSeparators, limit.Value, StringSplitOptions.RemoveEmptyEntries)
                : path.Split(PossiblePathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string GetRelativePath(string sourceDir, string targetPath)
        {
            // convert to URIs
            Uri from = new Uri(sourceDir.TrimEnd(PossiblePathSeparators) + "/");
            Uri to = new Uri(targetPath.TrimEnd(PossiblePathSeparators) + "/");
            if (from.Scheme != to.Scheme)
                throw new InvalidOperationException($"Can't get path for '{targetPath}' relative to '{sourceDir}'.");

            // get relative path
            string rawUrl = Uri.UnescapeDataString(from.MakeRelativeUri(to).ToString());
            if (rawUrl.StartsWith("file://"))
                rawUrl = WindowsUncRoot + rawUrl.Substring("file://".Length);
            string relative = NormalizePath(rawUrl);

            // normalize
            if (relative == "")
                relative = ".";
            else
            {
                // trim trailing slash from URL
                if (relative.EndsWith(PreferredPathSeparator.ToString()))
                    relative = relative.Substring(0, relative.Length - 1);

                // fix root
                if (relative.StartsWith("file:") && !targetPath.Contains("file:"))
                    relative = relative.Substring("file:".Length);
            }

            return relative;
        }
        public static string NormalizePath(string path)
        {
            path = path?.Trim();
            if (string.IsNullOrEmpty(path))
                return path;

            // get basic path format (e.g. /some/asset\\path/ => some\asset\path)
            string[] segments = GetSegments(path);
            string newPath = string.Join(PreferredPathSeparator.ToString(), segments);

            // keep root prefix
            bool hasRoot = false;
            if (path.StartsWith(WindowsUncRoot))
            {
                newPath = WindowsUncRoot + newPath;
                hasRoot = true;
            }
            else if (PossiblePathSeparators.Contains(path[0]))
            {
                newPath = PreferredPathSeparator + newPath;
                hasRoot = true;
            }

            // keep trailing separator
            if ((!hasRoot || segments.Any()) && PossiblePathSeparators.Contains(path[path.Length - 1]))
                newPath += PreferredPathSeparator;

            return newPath;
        }
    }
}
