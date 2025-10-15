using System;
using System.Collections.Generic;
using System.Linq;

namespace UWUVCI_AIO_WPF.Modules.Nintendont
{
    public sealed class NintendontPreset
    {
        public string Name { get; }
        public string Description { get; }
        public Func<NintendontConfig, NintendontConfig> ApplyTo { get; }

        public NintendontPreset(string name, string description, Func<NintendontConfig, NintendontConfig> applyTo)
        {
            Name = name;
            Description = description;
            ApplyTo = applyTo;
        }
    }

    public static class NintendontPresets
    {
        public static readonly List<NintendontPreset> AllPresets = new List<NintendontPreset>
        {
            new NintendontPreset("Recommended",
                "Balanced for modern TVs – widescreen, progressive, enables cheats, and fast loading.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.MemcardEmulation = true;
                    c.MemcardMulti = true;
                    c.Cheats = true;
                    c.CcRumble = true;
                    c.ForceWidescreen = true;
                    c.ForceProgressive = true;
                    c.VideoForceMode = NintendontVideoForceMode.Auto;
                    c.VideoTypeMode = NintendontVideoTypeMode.Auto;
                    c.AutoVideoWidth = true;
                    c.PatchPAL50 = true;
                    c.UnlockReadSpeed = true;
                    c.MaxPads = 4;
                    c.LanguageIndex = 0;
                    return c;
                }),

            new NintendontPreset("Normal",
                "Classic defaults – conservative and compatible for all setups.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.MemcardEmulation = true;
                    c.CcRumble = true;
                    c.VideoForceMode = NintendontVideoForceMode.Auto;
                    c.VideoTypeMode = NintendontVideoTypeMode.Auto;
                    c.AutoVideoWidth = true;
                    c.ForceWidescreen = false;
                    c.ForceProgressive = false;
                    c.PatchPAL50 = false;
                    c.LanguageIndex = 0;
                    c.MaxPads = 4;
                    return c;
                }),

            new NintendontPreset("Widescreen",
                "For 16:9 displays – widescreen and progressive enabled.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.MemcardEmulation = true;
                    c.CcRumble = true;
                    c.ForceWidescreen = true;
                    c.ForceProgressive = true;
                    c.AutoVideoWidth = true;
                    c.VideoForceMode = NintendontVideoForceMode.Auto;
                    c.VideoTypeMode = NintendontVideoTypeMode.Auto;
                    return c;
                }),

            new NintendontPreset("Compatibility",
                "Best for older TVs or tricky games – disables widescreen and forcing.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.MemcardEmulation = true;
                    c.CcRumble = true;
                    c.ForceWidescreen = false;
                    c.ForceProgressive = false;
                    c.VideoForceMode = NintendontVideoForceMode.None;
                    c.VideoTypeMode = NintendontVideoTypeMode.Auto;
                    c.AutoVideoWidth = true;
                    c.PatchPAL50 = false;
                    return c;
                }),

            new NintendontPreset("Debug",
                "Enables cheats, logs, and debugger for advanced troubleshooting.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.Cheats = true;
                    c.CheatPath = true;
                    c.OsReport = true;
                    c.EnableLog = true;
                    c.Debugger = true;
                    c.DebuggerWait = false;
                    c.ForceProgressive = true;
                    c.AutoVideoWidth = true;
                    return c;
                }),

            new NintendontPreset("Arcade",
                "For Triforce and arcade-style setups – boots straight to game.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.ArcadeMode = true;
                    c.AutoBoot = true;
                    c.SkipIpl = true;
                    c.ForceProgressive = true;
                    c.ForceWidescreen = false;
                    c.VideoForceMode = NintendontVideoForceMode.Auto;
                    c.VideoTypeMode = NintendontVideoTypeMode.Auto;
                    c.AutoVideoWidth = true;
                    return c;
                }),

            new NintendontPreset("Speedrun",
                "Optimized for fast booting – skip IPL and maximize responsiveness.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.AutoBoot = true;
                    c.SkipIpl = true;
                    c.MemcardEmulation = true;
                    c.MemcardMulti = false;
                    c.CcRumble = true;
                    c.ForceProgressive = true;
                    c.ForceWidescreen = true;
                    c.VideoForceMode = NintendontVideoForceMode.Auto;
                    c.UnlockReadSpeed = true;
                    return c;
                }),

            new NintendontPreset("Streaming",
                "Clean output for capture – widescreen, progressive, and no debug logs.",
                cfg =>
                {
                    var c = NintendontConfig.CreateDefault();
                    c.MemcardEmulation = true;
                    c.ForceWidescreen = true;
                    c.ForceProgressive = true;
                    c.PatchPAL50 = true;
                    c.VideoForceMode = NintendontVideoForceMode.Auto;
                    c.VideoTypeMode = NintendontVideoTypeMode.Auto;
                    c.OsReport = false;
                    c.EnableLog = false;
                    c.UnlockReadSpeed = true;
                    return c;
                })
        };

        public static NintendontPreset Default => AllPresets.First(p => p.Name == "Recommended");
    }
}
