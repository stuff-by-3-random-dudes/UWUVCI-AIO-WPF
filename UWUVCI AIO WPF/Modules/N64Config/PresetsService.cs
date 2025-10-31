using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UWUVCI_AIO_WPF.Modules.N64Config
{
    public static class PresetsService
    {
        private static string PresetsRoot => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets", "N64");

        public static void SavePreset(string name, IniDocument doc)
        {
            Directory.CreateDirectory(PresetsRoot);
            var path = Path.Combine(PresetsRoot, Sanitize(name) + ".json");
            var dto = ToDto(doc);
            File.WriteAllText(path, JsonConvert.SerializeObject(dto, Formatting.Indented));
        }

        public static IEnumerable<string> ListPresetNames()
        {
            if (!Directory.Exists(PresetsRoot)) yield break;
            foreach (var f in Directory.GetFiles(PresetsRoot, "*.json"))
                yield return Path.GetFileNameWithoutExtension(f);
        }

        public static IniDocument LoadPreset(string name)
        {
            var path = Path.Combine(PresetsRoot, Sanitize(name) + ".json");
            var json = File.ReadAllText(path);
            var dto = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            return FromDto(dto ?? new Dictionary<string, Dictionary<string, string>>());
        }

        public static void DeletePreset(string name)
        {
            var path = Path.Combine(PresetsRoot, Sanitize(name) + ".json");
            if (File.Exists(path)) File.Delete(path);
        }

        private static string Sanitize(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Trim();
        }

        private static Dictionary<string, Dictionary<string, string>> ToDto(IniDocument doc)
        {
            var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var sec in doc.Sections)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in sec.Properties)
                    dict[p.Key] = p.Value;
                map[sec.Name] = dict;
            }
            return map;
        }

        private static IniDocument FromDto(Dictionary<string, Dictionary<string, string>> dto)
        {
            var doc = new IniDocument();
            foreach (var kv in dto)
            {
                var sec = doc.GetOrAddSection(kv.Key);
                foreach (var p in kv.Value)
                    sec.Set(p.Key, p.Value);
            }
            return doc;
        }
    }
}
