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
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    Settings = JsonConvert.DeserializeObject<JsonAppSettings>(json) ?? new JsonAppSettings();
                    Console.WriteLine("Settings loaded successfully.");
                }
                else
                {
                    Console.WriteLine("Settings file not found. Creating default settings.");
                    SaveSettings(); // Create a new file with default values
                }
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors, e.g., malformed or corrupted file
                Console.WriteLine($"Error parsing settings file: {ex.Message}. Reverting to default settings.");
                Settings = new JsonAppSettings(); // Reset to default settings
                SaveSettings(); // Save default settings to create a valid file
            }
            catch (Exception ex)
            {
                // Catch other exceptions (e.g., file I/O issues)
                Console.WriteLine($"Error loading settings: {ex.Message}. Reverting to default settings.");
                Settings = new JsonAppSettings(); // Reset to default
                SaveSettings();
            }
        }

        // Save settings with retry mechanism in case of temporary file access issues
        public static void SaveSettings()
        {
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
                    if (!Directory.Exists(AppDataPath))
                        Directory.CreateDirectory(AppDataPath);

                    var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                    File.WriteAllText(SettingsFile, json);
                    Console.WriteLine("Settings saved successfully.");
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
