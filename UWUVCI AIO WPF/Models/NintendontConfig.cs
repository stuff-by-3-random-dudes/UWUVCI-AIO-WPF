using System;
using System.Text;

namespace UWUVCI_AIO_WPF.Modules.Nintendont
{
    [Flags]
    public enum NinConfigFlags : uint
    {
        NIN_CFG_CHEATS = 1,
        NIN_CFG_DEBUGGER = 1u << 1,   // Wii only
        NIN_CFG_DEBUGWAIT = 1u << 2,   // Wii only
        NIN_CFG_MEMCARDEMU = 1u << 3,
        NIN_CFG_CHEAT_PATH = 1u << 4,
        NIN_CFG_FORCE_WIDE = 1u << 5,
        NIN_CFG_FORCE_PROG = 1u << 6,
        NIN_CFG_AUTO_BOOT = 1u << 7,
        NIN_CFG_HID = 1u << 8,   // alias/remnant; REMLIMIT shares bit in some sources
        NIN_CFG_REMLIMIT = 1u << 8,
        NIN_CFG_OSREPORT = 1u << 9,
        NIN_CFG_USB = 1u << 10,  // old WiiU wide bit
        NIN_CFG_LED = 1u << 11,  // Wii only
        NIN_CFG_LOG = 1u << 12,
        NIN_CFG_MC_MULTI = 1u << 13,
        NIN_CFG_NATIVE_SI = 1u << 14,  // Wii only
        NIN_CFG_WIIU_WIDE = 1u << 15,  // WiiU flag
        NIN_CFG_ARCADE_MODE = 1u << 16,
        NIN_CFG_CC_RUMBLE = 1u << 17,
        NIN_CFG_SKIP_IPL = 1u << 18,
        NIN_CFG_BBA_EMU = 1u << 19,
    }

    [Flags]
    public enum NinVideoMode : uint
    {
        // High mask selection
        NIN_VID_AUTO = 0u << 16,
        NIN_VID_FORCE = 1u << 16,
        NIN_VID_NONE = 2u << 16,
        NIN_VID_FORCE_DF = 4u << 16,
        NIN_VID_MASK = (0u << 16) | (1u << 16) | (2u << 16) | (4u << 16),

        // Type bits
        NIN_VID_FORCE_PAL50 = 1u << 0,
        NIN_VID_FORCE_PAL60 = 1u << 1,
        NIN_VID_FORCE_NTSC = 1u << 2,
        NIN_VID_FORCE_MPAL = 1u << 3,
        NIN_VID_FORCE_MASK = (1u << 0) | (1u << 1) | (1u << 2) | (1u << 3),

        // Extras
        NIN_VID_PROG = 1u << 4,
        NIN_VID_PATCH_PAL50 = 1u << 5,
    }

    public enum NintendontVideoForceMode
    {
        Auto = 0,
        Force = 1,
        ForceDeflicker = 2,
        None = 3
    }

    public enum NintendontVideoTypeMode
    {
        Auto = 0,
        NTSC = 1,
        MPAL = 2,
        PAL50 = 3,
        PAL60 = 4
    }

    public sealed class NintendontConfig
    {
        // Raw header/state
        public uint Magic { get; set; } = 0x01070CF6;
        public uint Version { get; set; } = 10;
        public uint GameId { get; set; } = 0;
        public byte[] GamePathRaw { get; set; } // keep as-is to preserve full compatibility
        public byte[] CheatPathRaw { get; set; }

        // UI/state fields (mapped to bitfields)
        // Gameplay
        public bool Cheats { get; set; }
        public bool AutoBoot { get; set; }
        public bool SkipIpl { get; set; }
        public bool ArcadeMode { get; set; }
        public bool BbaEmulation { get; set; }
        public bool CheatPath { get; set; }

        // Memory
        public bool MemcardEmulation { get; set; } = true;
        public int MemcardBlocksIndex { get; set; } = 0; // 0..5
        public bool MemcardMulti { get; set; }

        // Controller
        public bool CcRumble { get; set; }
        public bool NativeSi { get; set; } // Wii only, kept for parity

        // Video
        public NintendontVideoForceMode VideoForceMode { get; set; } = NintendontVideoForceMode.Auto;
        public NintendontVideoTypeMode VideoTypeMode { get; set; } = NintendontVideoTypeMode.Auto;
        public bool AutoVideoWidth { get; set; } = true;
        public int VideoWidth { get; set; } = 640; // 640..720; 0 when Auto
        public bool ForceWidescreen { get; set; }
        public bool ForceProgressive { get; set; }
        public bool PatchPAL50 { get; set; }

        // System
        public int LanguageIndex { get; set; } = 0; // 0 auto, else 1..6
        public int WiiUGamepadSlot { get; set; } = 0; // 0..3
        public int MaxPads { get; set; } = 4; // 1..4

        // Debug/Perf
        public bool Debugger { get; set; }
        public bool DebuggerWait { get; set; }
        public bool OsReport { get; set; }
        public bool EnableLog { get; set; }
        public bool DriveLed { get; set; }
        public bool UnlockReadSpeed { get; set; }

        // Extra numeric fields carried through the binary (not exposed much in UI)
        public sbyte VideoScale { get; set; } = 0;
        public sbyte VideoOffset { get; set; } = 0;
        public byte NetworkProfile { get; set; } = 0;

        public uint ToConfigBitfield()
        {
            ToBitfields(out uint configBits, out _);
            return configBits;
        }

        public uint ToVideoBitfield()
        {
            ToBitfields(out _, out uint videoBits);
            return videoBits;
        }

        public string GamePath
        {
            get => GamePathRaw != null ? Encoding.ASCII.GetString(GamePathRaw).TrimEnd('\0') : string.Empty;
            set => GamePathRaw = Encoding.ASCII.GetBytes((value ?? "").PadRight(256, '\0'));
        }


        public static NintendontConfig CreateDefault() => new NintendontConfig
        {
            Cheats = false,
            AutoBoot = false,
            SkipIpl = false,
            ArcadeMode = false,
            BbaEmulation = false,
            CheatPath = false,
            MemcardEmulation = true,
            MemcardBlocksIndex = 0,
            MemcardMulti = false,
            CcRumble = true,
            NativeSi = false,
            VideoForceMode = NintendontVideoForceMode.Auto,
            VideoTypeMode = NintendontVideoTypeMode.Auto,
            AutoVideoWidth = true,
            VideoWidth = 640,
            ForceWidescreen = false,
            ForceProgressive = false,
            PatchPAL50 = false,
            LanguageIndex = 0,
            WiiUGamepadSlot = 0,
            MaxPads = 4,
            Debugger = false,
            DebuggerWait = false,
            OsReport = false,
            EnableLog = false,
            DriveLed = false,
            UnlockReadSpeed = false,
            GamePathRaw = new byte[256],
            CheatPathRaw = new byte[256]
        };

        // Map from bitfields -> model
        public void FromBitfields(uint configBits, uint videoBits)
        {
            var cfg = (NinConfigFlags)configBits;
            var vid = (NinVideoMode)videoBits;

            Cheats = cfg.HasFlag(NinConfigFlags.NIN_CFG_CHEATS);
            AutoBoot = cfg.HasFlag(NinConfigFlags.NIN_CFG_AUTO_BOOT);
            SkipIpl = cfg.HasFlag(NinConfigFlags.NIN_CFG_SKIP_IPL);
            ArcadeMode = cfg.HasFlag(NinConfigFlags.NIN_CFG_ARCADE_MODE);
            BbaEmulation = cfg.HasFlag(NinConfigFlags.NIN_CFG_BBA_EMU);
            CheatPath = cfg.HasFlag(NinConfigFlags.NIN_CFG_CHEAT_PATH);

            MemcardEmulation = cfg.HasFlag(NinConfigFlags.NIN_CFG_MEMCARDEMU);
            MemcardMulti = cfg.HasFlag(NinConfigFlags.NIN_CFG_MC_MULTI);

            CcRumble = cfg.HasFlag(NinConfigFlags.NIN_CFG_CC_RUMBLE);
            NativeSi = cfg.HasFlag(NinConfigFlags.NIN_CFG_NATIVE_SI);

            ForceWidescreen = cfg.HasFlag(NinConfigFlags.NIN_CFG_FORCE_WIDE) || cfg.HasFlag(NinConfigFlags.NIN_CFG_WIIU_WIDE);
            ForceProgressive = cfg.HasFlag(NinConfigFlags.NIN_CFG_FORCE_PROG);
            OsReport = cfg.HasFlag(NinConfigFlags.NIN_CFG_OSREPORT);
            EnableLog = cfg.HasFlag(NinConfigFlags.NIN_CFG_LOG);
            DriveLed = cfg.HasFlag(NinConfigFlags.NIN_CFG_LED);
            UnlockReadSpeed = cfg.HasFlag(NinConfigFlags.NIN_CFG_REMLIMIT);
            Debugger = cfg.HasFlag(NinConfigFlags.NIN_CFG_DEBUGGER);
            DebuggerWait = cfg.HasFlag(NinConfigFlags.NIN_CFG_DEBUGWAIT);

            // Video force mode (mask)
            var modeMask = vid & NinVideoMode.NIN_VID_MASK;
            if (modeMask.HasFlag(NinVideoMode.NIN_VID_FORCE_DF)) VideoForceMode = NintendontVideoForceMode.ForceDeflicker;
            else if (modeMask.HasFlag(NinVideoMode.NIN_VID_NONE)) VideoForceMode = NintendontVideoForceMode.None;
            else if (modeMask.HasFlag(NinVideoMode.NIN_VID_FORCE)) VideoForceMode = NintendontVideoForceMode.Force;
            else VideoForceMode = NintendontVideoForceMode.Auto;

            // Video type
            if (vid.HasFlag(NinVideoMode.NIN_VID_FORCE_NTSC)) VideoTypeMode = NintendontVideoTypeMode.NTSC;
            else if (vid.HasFlag(NinVideoMode.NIN_VID_FORCE_MPAL)) VideoTypeMode = NintendontVideoTypeMode.MPAL;
            else if (vid.HasFlag(NinVideoMode.NIN_VID_FORCE_PAL50)) VideoTypeMode = NintendontVideoTypeMode.PAL50;
            else if (vid.HasFlag(NinVideoMode.NIN_VID_FORCE_PAL60)) VideoTypeMode = NintendontVideoTypeMode.PAL60;
            else VideoTypeMode = NintendontVideoTypeMode.Auto;

            if (vid.HasFlag(NinVideoMode.NIN_VID_PROG)) ForceProgressive = true;
            PatchPAL50 = vid.HasFlag(NinVideoMode.NIN_VID_PATCH_PAL50);

            // NOTE: Do NOT set AutoVideoWidth here — it’s determined by VideoScale (== 0).
        }


        // Map model -> bitfields
        public void ToBitfields(out uint configBits, out uint videoBits)
        {
            NinConfigFlags cfg = 0;
            NinVideoMode vid = 0;

            if (Cheats) cfg |= NinConfigFlags.NIN_CFG_CHEATS;
            if (AutoBoot) cfg |= NinConfigFlags.NIN_CFG_AUTO_BOOT;
            if (SkipIpl) cfg |= NinConfigFlags.NIN_CFG_SKIP_IPL;
            if (ArcadeMode) cfg |= NinConfigFlags.NIN_CFG_ARCADE_MODE;
            if (BbaEmulation) cfg |= NinConfigFlags.NIN_CFG_BBA_EMU;
            if (CheatPath) cfg |= NinConfigFlags.NIN_CFG_CHEAT_PATH;

            if (MemcardEmulation) cfg |= NinConfigFlags.NIN_CFG_MEMCARDEMU;
            if (MemcardMulti) cfg |= NinConfigFlags.NIN_CFG_MC_MULTI;

            if (CcRumble) cfg |= NinConfigFlags.NIN_CFG_CC_RUMBLE;
            if (NativeSi) cfg |= NinConfigFlags.NIN_CFG_NATIVE_SI;

            if (ForceWidescreen)
            {
                cfg |= NinConfigFlags.NIN_CFG_FORCE_WIDE;
                cfg |= NinConfigFlags.NIN_CFG_WIIU_WIDE; // set WiiU-wide too for parity
            }

            if (ForceProgressive) cfg |= NinConfigFlags.NIN_CFG_FORCE_PROG;
            if (OsReport) cfg |= NinConfigFlags.NIN_CFG_OSREPORT;
            if (EnableLog) cfg |= NinConfigFlags.NIN_CFG_LOG;
            if (DriveLed) cfg |= NinConfigFlags.NIN_CFG_LED;
            if (UnlockReadSpeed) cfg |= NinConfigFlags.NIN_CFG_REMLIMIT;
            if (Debugger) cfg |= NinConfigFlags.NIN_CFG_DEBUGGER;
            if (DebuggerWait) cfg |= NinConfigFlags.NIN_CFG_DEBUGWAIT;

            // Video mode
            switch (VideoForceMode)
            {
                case NintendontVideoForceMode.Auto: break; // NIN_VID_AUTO
                case NintendontVideoForceMode.Force: vid |= NinVideoMode.NIN_VID_FORCE; break;
                case NintendontVideoForceMode.ForceDeflicker: vid |= NinVideoMode.NIN_VID_FORCE_DF; break;
                case NintendontVideoForceMode.None: vid |= NinVideoMode.NIN_VID_NONE; break;
            }

            switch (VideoTypeMode)
            {
                case NintendontVideoTypeMode.NTSC: vid |= NinVideoMode.NIN_VID_FORCE_NTSC; break;
                case NintendontVideoTypeMode.MPAL: vid |= NinVideoMode.NIN_VID_FORCE_MPAL; break;
                case NintendontVideoTypeMode.PAL50: vid |= NinVideoMode.NIN_VID_FORCE_PAL50; break;
                case NintendontVideoTypeMode.PAL60: vid |= NinVideoMode.NIN_VID_FORCE_PAL60; break;
                case NintendontVideoTypeMode.Auto: default: break;
            }

            if (ForceProgressive) vid |= NinVideoMode.NIN_VID_PROG;
            if (PatchPAL50) vid |= NinVideoMode.NIN_VID_PATCH_PAL50;

            configBits = (uint)cfg;
            videoBits = (uint)vid;
        }
    }
}
