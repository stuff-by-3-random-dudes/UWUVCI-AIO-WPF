using System;
using System.Collections.Generic;

namespace UWUVCI_AIO_WPF.Modules.Nintendont
{
    public sealed class NintendontPreset
    {
        public string Name { get; private set; }
        public Func<NintendontConfig, NintendontConfig> ApplyTo { get; private set; }

        public NintendontPreset(string name, Func<NintendontConfig, NintendontConfig> applyTo)
        {
            Name = name;
            ApplyTo = applyTo;
        }
    }

    public static class NintendontPresets
    {
        public static readonly List<NintendontPreset> AllPresets = new List<NintendontPreset>
        {
            new NintendontPreset("Normal", cfg =>
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

            new NintendontPreset("Widescreen", cfg =>
            {
                var c = NintendontConfig.CreateDefault();
                c.MemcardEmulation = true;
                c.CcRumble = true;
                c.ForceWidescreen = true;
                c.ForceProgressive = true; // good default on modern displays
                c.AutoVideoWidth = true;
                c.VideoForceMode = NintendontVideoForceMode.Auto;
                c.VideoTypeMode  = NintendontVideoTypeMode.Auto;
                return c;
            }),

            new NintendontPreset("Debug", cfg =>
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

            new NintendontPreset("Arcade", cfg =>
            {
                var c = NintendontConfig.CreateDefault();
                c.ArcadeMode = true;
                c.AutoBoot = true;
                c.SkipIpl = true;
                c.ForceProgressive = true;
                c.ForceWidescreen = false; // many TRI titles are 4:3
                c.VideoForceMode = NintendontVideoForceMode.Auto;
                c.VideoTypeMode  = NintendontVideoTypeMode.Auto;
                c.AutoVideoWidth = true;
                return c;
            })
        };
    }
}
