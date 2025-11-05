using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF
{
    // Local reader for VCB files to avoid external dependency.
    // VCB files are GZip-compressed BinaryFormatter payloads of List<GameBases>.
    public static class VCBTool
    {
        public static List<GameBases> ReadBasesFromVCB(string vcbPath)
        {
            // Try requested path first
            var result = TryRead(vcbPath);
            if (result != null && result.Count > 0)
                return result;

            // Fallback: try local primebases/bases under UWUVCI-VCB-master
            try
            {
                string suffix = ExtractConsoleSuffix(vcbPath);
                if (!string.IsNullOrEmpty(suffix))
                {
                    foreach (var candidate in EnumerateFallbacks(suffix))
                    {
                        result = TryRead(candidate);
                        if (result != null && result.Count > 0)
                        {
                            Logger.Log($"VCB fallback used: {candidate}");
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"VCB fallback resolution failed: {ex.Message}");
            }

            return new List<GameBases>();
        }

        private static List<GameBases> TryRead(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return null;

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    var obj = formatter.Deserialize(gz);
                    if (obj is List<GameBases> list)
                        return list;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to read VCB '{path}': {ex.Message}");
            }
            return null;
        }

        private static string ExtractConsoleSuffix(string path)
        {
            try
            {
                string file = Path.GetFileName(path) ?? string.Empty;
                int idx = file.LastIndexOf(".vcb", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0 && idx + 4 <= file.Length)
                    return file.Substring(idx + 4); // part after .vcb
            }
            catch { }
            return string.Empty;
        }

        private static IEnumerable<string> EnumerateFallbacks(string suffix)
        {
            // Look in local checked-in VCB repository copies
            string[] roots = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "UWUVCI-VCB-master"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "SourceFiles")
            };

            foreach (var root in roots)
            {
                if (string.IsNullOrWhiteSpace(root)) continue;
                yield return Path.Combine(root, $"primebases.vcb{suffix}");
                yield return Path.Combine(root, $"bases.vcb{suffix}");
            }
        }
    }
}

