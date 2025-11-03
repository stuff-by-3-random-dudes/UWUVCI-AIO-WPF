using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace UWUVCI_AIO_WPF.Services
{
    public static class BaseExtractor
    {
        // Returns the fully extracted BASE directory path (ending with ...\BASE)
        // Caches by MD5 of the zip into %LocalAppData%\UWUVCI_AIO_WPF\Cache\<name>_<md5>\BASE
        public static string GetOrExtractBase(string toolsPath, string baseZipName = "BASE.zip")
        {
            if (string.IsNullOrWhiteSpace(toolsPath)) throw new ArgumentException("toolsPath is required", nameof(toolsPath));
            var zipPath = Path.Combine(toolsPath, baseZipName);
            if (!File.Exists(zipPath)) throw new FileNotFoundException($"BASE archive not found: {zipPath}", zipPath);

            string cacheRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI_AIO_WPF", "Cache");
            Directory.CreateDirectory(cacheRoot);

            string hash = ComputeFileMd5(zipPath);
            string keyName = Path.GetFileNameWithoutExtension(baseZipName) + "_" + hash;
            string targetDir = Path.Combine(cacheRoot, keyName);
            string marker = Path.Combine(targetDir, ".ok");
            string baseDir = Path.Combine(targetDir, "BASE");

            if (Directory.Exists(baseDir) && File.Exists(marker))
            {
                return baseDir;
            }

            // (Re)extract
            if (Directory.Exists(targetDir))
            {
                try { Directory.Delete(targetDir, true); } catch { /* best-effort */ }
            }
            Directory.CreateDirectory(targetDir);
            ZipFile.ExtractToDirectory(zipPath, targetDir);

            // Minimal validation
            if (!Directory.Exists(baseDir))
                throw new DirectoryNotFoundException($"Expected 'BASE' folder not found after extracting {baseZipName}.");

            File.WriteAllText(marker, DateTime.UtcNow.ToString("o"));
            return baseDir;
        }

        private static string ComputeFileMd5(string path)
        {
            using (var md5 = MD5.Create())
            using (var fs = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(fs);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        public static bool ClearCache()
        {
            try
            {
                string cacheRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI_AIO_WPF", "Cache");
                if (Directory.Exists(cacheRoot)) Directory.Delete(cacheRoot, true);
                return true;
            }
            catch { return false; }
        }
    }
}
