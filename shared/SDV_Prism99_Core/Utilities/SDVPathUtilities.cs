using System;
using System.IO;
using System.Linq;


namespace Prism99_Core.Utilities
{
    internal static class SDVPathUtilities
    {
        public static readonly char PreferredPathSeparator = Path.DirectorySeparatorChar;
        public static readonly char[] PossiblePathSeparators = new[] { '/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.Distinct().ToArray();
        public const string WindowsUncRoot = @"\\";

 
        public static string NormalizeAssetNameForPlatform(string key) { return key.Replace('\\', '/'); }// based on MonoGame's ContentManager.Load<T> logic

        public static string AssertAndNormalizeAssetName(string assetName)
        {
             if (string.IsNullOrWhiteSpace(assetName))
                throw new Exception("The asset key or local path is empty.");
            if (assetName.Intersect(Path.GetInvalidPathChars()).Any())
                throw new Exception("The asset key or local path contains invalid characters.");

            return NormalizeKey(assetName);
        }
        public static string NormalizeKey(string key)
        {
            key = NormalizePath(key);
            return key.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase)
                ? key.Substring(0, key.Length - 4)
                : NormalizeAssetNameForPlatform(key);
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
    }
}

