using System.IO;
using Newtonsoft.Json;
using UWUVCI_AIO_WPF.Models;
using System;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class JsonSettingsManager
    {
        private static string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UWUVCI");
        private static string SettingsFile = Path.Combine(AppDataPath, "settings.json");

        public static JsonAppSettings Settings { get; private set; } = new JsonAppSettings();

        public static void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                Settings = JsonConvert.DeserializeObject<JsonAppSettings>(json);
            }
            else
            {
                // Create a new file with default values
                SaveSettings(); 
            }
        }

        public static void SaveSettings()
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }
    }

}
