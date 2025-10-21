using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace UWUVCI_AIO_WPF.Services
{
    public static class EnvironmentInfoService
    {
        /// <summary>
        /// Returns a short multi-line summary of the current environment.
        /// Safe under Wine/CrossOver. Never throws exceptions.
        /// </summary>
        public static string TryGetSummary()
        {
            try
            {
                var summary = string.Empty;

                summary += $"OS Version: {GetOSDisplayName()}{Environment.NewLine}";
                summary += $"Architecture: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}{Environment.NewLine}";
                summary += $".NET Framework: {Environment.Version}{Environment.NewLine}";
                summary += $"Machine Name: {Environment.MachineName}{Environment.NewLine}";
                summary += $"User Locale: {CultureInfo.CurrentCulture.DisplayName}{Environment.NewLine}";
                summary += $"Time Zone: {TimeZoneInfo.Local.DisplayName}{Environment.NewLine}";
                summary += $"Processor Count: {Environment.ProcessorCount}{Environment.NewLine}";
                summary += $"System Directory: {SafeGetSystemDirectory()}{Environment.NewLine}";
                summary += $"Current Directory: {Environment.CurrentDirectory}{Environment.NewLine}";
                summary += $"Is Wine Environment: {(IsWine() ? "Yes" : "No")}{Environment.NewLine}";
                summary += $"Memory (approx): {GetApproxMemory()} MB";

                return summary.Trim();
            }
            catch (Exception ex)
            {
                return $"(Failed to gather environment info: {ex.Message})";
            }
        }

        // --------------------------
        // Helpers
        // --------------------------

        private static string GetOSDisplayName()
        {
            try
            {
                string os = RuntimeInformation.OSDescription;
                if (string.IsNullOrEmpty(os))
                    os = Environment.OSVersion.ToString();

                // Clean up common Wine/CrossOver identifiers
                if (os.IndexOf("Wine", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Wine (Windows Compatibility Layer)";
                if (os.IndexOf("CrossOver", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "CrossOver (Wine variant)";
                if (os.IndexOf("Darwin", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "macOS (Darwin kernel)";
                if (os.IndexOf("Linux", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Linux";

                return os.Trim();
            }
            catch
            {
                return "Unknown OS";
            }
        }

        private static bool IsWine()
        {
            try
            {
                // On Wine, the registry path below usually exists.
                string wineKey = @"Software\Wine";
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(wineKey);
                if (key != null) 
                    return true;
            }
            catch
            {
                // No access or registry not available
            }

            try
            {
                string osDesc = RuntimeInformation.OSDescription ?? string.Empty;
                if (osDesc.IndexOf("Wine", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch { }

            return false;
        }

        private static string GetApproxMemory()
        {
            try
            {
                var ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
                ulong mem = ci.TotalPhysicalMemory / (1024 * 1024);
                return mem.ToString("N0");
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string SafeGetSystemDirectory()
        {
            try
            {
                return Environment.SystemDirectory;
            }
            catch
            {
                try
                {
                    // Fallback for Wine/Linux/macOS
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        return "/usr/bin";
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        return "/System/Library";
                }
                catch { }

                return "(Unavailable)";
            }
        }
    }
}
