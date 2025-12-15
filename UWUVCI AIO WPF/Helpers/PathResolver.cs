using System;
using System.IO;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class PathResolver
    {
        /// <summary>
        /// Returns the correct absolute path to the Tools directory.
        /// Automatically creates the directory if missing.
        /// </summary>
        /*
        public static string GetToolsPath()
        {
            string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");

            try
            {
                // On macOS/Linux, redirect to writable user location
                if (ToolRunner.HostIsMac() || ToolRunner.HostIsLinux())
                {
                    string appSupport = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "UWUVCI-V3", "Tools");

                    Directory.CreateDirectory(appSupport);
                    return appSupport;
                }

                return defaultPath;
            }
            catch
            {
                // Fallback to local bin/Tools
                return defaultPath;
            }
        }
        */
        public static string GetToolsPath() => JsonSettingsManager.Settings.ToolsPath;
        /*
        public static string GetTempPath()
        {
            string baseDir;

            if (ToolRunner.HostIsMac())
            {
                // true macOS Application Support location
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                baseDir = Path.Combine(
                    home, "..", "Library", "Application Support", "UWUVCI-V3", "temp"
                );
                baseDir = Path.GetFullPath(baseDir);
            }
            else if (ToolRunner.HostIsLinux())
            {
                baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".uwuvci", "temp"
                );
            }
            else
            {
                baseDir = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
            }

            Directory.CreateDirectory(baseDir);
            return baseDir;
        }
        */
        public static string GetTempPath() => JsonSettingsManager.Settings.TempPath;
    }
}
