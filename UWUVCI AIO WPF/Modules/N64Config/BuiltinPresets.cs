using System.Collections.Generic;

namespace UWUVCI_AIO_WPF.Modules.N64Config
{
    public class BuiltinPreset
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, Dictionary<string, string>> Values { get; set; }
    }

    public static class BuiltinPresets
    {
        public static readonly List<BuiltinPreset> All = new List<BuiltinPreset>
        {
            new BuiltinPreset
            {
                Name = "Default (Safe)",
                Description = "Basic defaults with VSync enabled.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"RetraceByVsync","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "VSync + Default RAM",
                Description = "Enable VSync and set RAM size to default (4MB).",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"RetraceByVsync","1"},
                        {"RamSize","0x400000"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Expansion Pak (8MB)",
                Description = "Set RAM size to 8MB (Expansion Pak).",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"RamSize","0x800000"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "VSync + Timer",
                Description = "Enable VSync and UseTimer for smoother pacing.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"RetraceByVsync","1"},
                        {"UseTimer","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "MemPak Save (EEPROM 512)",
                Description = "Use Controller Pak, set EEPROM 512 bytes.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"MemPak","1"},
                        {"Rumble","0"},
                        {"BackupType","3"},
                        {"BackupSize","512"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "MemPak Save (EEPROM 2K)",
                Description = "Use Controller Pak, set EEPROM 2048 bytes.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"MemPak","1"},
                        {"Rumble","0"},
                        {"BackupType","3"},
                        {"BackupSize","2048"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Rumble Pak",
                Description = "Enable Rumble; disable MemPak.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"Rumble","1"},
                        {"MemPak","0"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Performance (RSP MultiCore)",
                Description = "Enable RSPMultiCore and UseTimer for smoother pacing.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"RSPMultiCore","1"},
                        {"UseTimer","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "RSP MultiCore + Wait",
                Description = "Enable RSPMultiCore with AMultiCoreWait and UseTimer.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"RSPMultiCore","1"},
                        {"RSPAMultiCoreWait","1"},
                        {"UseTimer","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Canvas 854x480 + Dither Off",
                Description = "Set canvas to 854x480 and disable color dither.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Render"] = new Dictionary<string, string>
                    {
                        {"CanvasWidth","854"},
                        {"CanvasHeight","480"},
                        {"UseColorDither","0"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "720p Output",
                Description = "Force 720p output (if supported)",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Render"] = new Dictionary<string, string>
                    {
                        {"bForce720P","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Point Filtering",
                Description = "Use nearest/point filtering for sharper pixels.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Render"] = new Dictionary<string, string>
                    {
                        {"ForceFilterPoint","1"},
                        {"ForceRectFilterPoint","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Disable Dither",
                Description = "Disable color dithering.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Render"] = new Dictionary<string, string>
                    {
                        {"UseColorDither","0"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Disable ZClip",
                Description = "Disable Z clipping (common fix in official INIs).",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Render"] = new Dictionary<string, string>
                    {
                        {"ZClip","0"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "Stick Clamp On",
                Description = "Clamp analog sticks (both native and VPAD).",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Input"] = new Dictionary<string, string>
                    {
                        {"STICK_CLAMP","1"},
                        {"VPAD_STICK_CLAMP","1"}
                    }
                }
            },
            new BuiltinPreset
            {
                Name = "TrueBoot On",
                Description = "Enable TrueBoot.",
                Values = new Dictionary<string, Dictionary<string, string>>
                {
                    ["RomOption"] = new Dictionary<string, string>
                    {
                        {"TrueBoot","1"}
                    }
                }
            },
        };

        public static IniDocument ToIni(BuiltinPreset preset)
        {
            var doc = new IniDocument();
            foreach (var sec in preset.Values)
            {
                var s = doc.GetOrAddSection(sec.Key);
                foreach (var kv in sec.Value)
                    s.Set(kv.Key, kv.Value);
            }
            return doc;
        }
    }
}
