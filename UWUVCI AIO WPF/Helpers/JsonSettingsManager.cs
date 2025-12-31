using System;
using System.IO;
using Newtonsoft.Json;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class JsonSettingsManager
    {
        private static string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UWUVCI-V3");
        public static string SettingsFile = Path.Combine(AppDataPath, "settings.json");

        public static JsonAppSettings Settings { get; private set; } = new JsonAppSettings();

        // Load settings with exception handling for invalid or missing files
        public static void LoadSettings()
        {
            try
            {
                // if file exists load it
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    Settings = JsonConvert.DeserializeObject<JsonAppSettings>(json) ?? new JsonAppSettings();
                }
                else
                {
                    Settings = new JsonAppSettings();
                }

                // ---- Determine simple platform defaults ----
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                string defaultTools;
                string defaultTemp;

                if (ToolRunner.HostIsMac())
                {
                    defaultTools = Path.Combine(home, "Library", "ApplicationSupport", "UWUVCI-V3", "Tools");
                    defaultTemp = Path.Combine(home, "Library", "ApplicationSupport", "UWUVCI-V3", "temp");
                }
                else if (ToolRunner.HostIsLinux())
                {
                    defaultTools = Path.Combine(home, ".uwuvci-v3", "Tools");
                    defaultTemp = Path.Combine(home, ".uwuvci-v3", "temp");
                }
                else // Windows
                {
                    defaultTools = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
                    defaultTemp = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
                }

                if (string.IsNullOrWhiteSpace(Settings.ToolsPath))
                    Settings.ToolsPath = defaultTools;

                if (string.IsNullOrWhiteSpace(Settings.TempPath))
                    Settings.TempPath = defaultTemp;

                // Save fixes
                SaveSettings();

                // ---- Delegate the real heavy-lifting to ToolRunner ----
                ToolRunner.InitializePaths(Settings.ToolsPath, Settings.TempPath);
            }
            catch
            {
                Settings = new JsonAppSettings();
                SaveSettings();
            }
        }

        // Save settings with retry mechanism in case of temporary file access issues
        public static void SaveSettings()
        {
            string caller = null;
#if DEBUG
            try
            {
                var st = new System.Diagnostics.StackTrace();
                caller = st.GetFrame(1)?.GetMethod()?.DeclaringType?.FullName + "." + st.GetFrame(1)?.GetMethod()?.Name;
            }
            catch { }
#endif

            // Ensure settings directory exists before any file checks
            try
            {
                if (!Directory.Exists(AppDataPath))
                    Directory.CreateDirectory(AppDataPath);
            }
            catch
            {
                Console.WriteLine("Settings directory could not be created. Unable to save settings.");
                return;
            }

            // Check if the settings file is writable
            if (!IsFileWritable(SettingsFile))
            {
                Console.WriteLine("Settings file is not writable. Unable to save settings.");
                return; // Exit early if the file is not writable
            }

            int retryCount = 0;
            const int maxRetry = 3;
            const int delayBetweenRetries = 1000; // 1 second

            while (retryCount < maxRetry)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);

                    // Skip writing if nothing changed to avoid churn
                    if (File.Exists(SettingsFile))
                    {
                        var existing = File.ReadAllText(SettingsFile);
                        if (string.Equals(existing, json, StringComparison.Ordinal))
                        {
#if DEBUG
                            if (!string.IsNullOrWhiteSpace(caller))
                                Console.WriteLine($"Settings unchanged; skip save (caller {caller}).");
#endif
                            break;
                        }
                    }

                    File.WriteAllText(SettingsFile, json);
                    Console.WriteLine("Settings saved successfully.");
#if DEBUG
                    if (!string.IsNullOrWhiteSpace(caller))
                        Console.WriteLine($"Settings saved by {caller}.");
#endif
                    break; // Success, exit loop
                }
                catch (IOException ex)
                {
                    retryCount++;
                    Console.WriteLine($"Error saving settings (attempt {retryCount}/{maxRetry}): {ex.Message}");

                    if (retryCount < maxRetry)
                        System.Threading.Thread.Sleep(delayBetweenRetries); // Wait before retrying
                    else
                        Console.WriteLine("Failed to save settings after multiple attempts.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error saving settings: {ex.Message}");
                    break; // If it's not an IO exception, don't retry
                }
            }
        }


        // Utility to check if the file is writable
        public static bool IsFileWritable(string path)
        {
            try
            {
                using FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
