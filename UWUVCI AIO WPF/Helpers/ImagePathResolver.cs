using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UWUVCI_AIO_WPF.Helpers
{
    /// <summary>
    /// Resolves image paths for contribution. PNG-only. Pure helpers to allow unit testing.
    /// </summary>
    public static class ImagePathResolver
    {
        /// <summary>
        /// Given a textbox path, resolve to an existing PNG path.
        /// - If the exact path exists and is .png, returns it.
        /// - Else, tries the same path with .png extension.
        /// - Else, returns null.
        /// </summary>
        public static string ResolveExistingPng(string original)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(original)) return null;
                if (File.Exists(original) && string.Equals(Path.GetExtension(original), ".png", StringComparison.OrdinalIgnoreCase))
                    return original;
                var pngCandidate = Path.ChangeExtension(original, ".png");
                if (!string.Equals(pngCandidate, original, StringComparison.OrdinalIgnoreCase) && File.Exists(pngCandidate))
                    return pngCandidate;
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// If a path is missing, search peer directories for a canonical key file (e.g., iconTex.png).
        /// </summary>
        public static string ResolveFromPeers(string missingKey, IEnumerable<string> peerDirectories)
        {
            if (string.IsNullOrWhiteSpace(missingKey) || peerDirectories == null) return null;
            try
            {
                foreach (var dir in peerDirectories)
                {
                    if (string.IsNullOrWhiteSpace(dir)) continue;
                    var candidate = Path.Combine(dir, missingKey + ".png");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Normalize an array of image paths using canonical keys.
        /// Step 1: Resolve each original to an existing PNG via ResolveExistingPng.
        /// Step 2: For any still-missing entry with a canonical key, attempt to find key.png next to any resolved peer.
        /// </summary>
        public static string[] NormalizeAll(string[] originalPaths, string[] canonicalKeys)
        {
            if (originalPaths == null) return Array.Empty<string>();
            var normalized = new string[originalPaths.Length];
            for (int i = 0; i < originalPaths.Length; i++)
                normalized[i] = ResolveExistingPng(originalPaths[i]);

            // collect peer directories from resolved entries
            var peerDirs = normalized.Where(p => !string.IsNullOrWhiteSpace(p))
                                     .Select(p => SafeGetDir(p))
                                     .Where(d => !string.IsNullOrWhiteSpace(d))
                                     .Distinct()
                                     .ToArray();

            for (int i = 0; i < normalized.Length; i++)
            {
                if (normalized[i] != null) continue;
                var key = (canonicalKeys != null && i < canonicalKeys.Length) ? canonicalKeys[i] : null;
                if (string.IsNullOrWhiteSpace(key)) continue;
                var fromPeer = ResolveFromPeers(key, peerDirs);
                if (!string.IsNullOrWhiteSpace(fromPeer)) normalized[i] = fromPeer;
            }
            return normalized;
        }

        private static string SafeGetDir(string path)
        {
            try { return Path.GetDirectoryName(path); } catch { return null; }
        }
    }
}
