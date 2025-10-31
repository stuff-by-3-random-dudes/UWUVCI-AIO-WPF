using System.Collections.Generic;

namespace UWUVCI_AIO_WPF.Modules.N64Config
{
    public static class N64Tooltips
    {
        public static readonly Dictionary<string, string> Tips = new Dictionary<string, string>
        {
            // RomOption
            ["RetraceByVsync"] = "Prevents tearing by syncing to vblank.",
            ["UseTimer"] = "Use timed emulation loop; can affect frame pacing.",
            ["TrueBoot"] = "Use alternative boot path (game dependent).",
            ["Rumble"] = "Enable Rumble Pak.",
            ["MemPak"] = "Enable Controller Pak (memory pak).",
            ["BackupType"] = "Save type: 0 Auto, 1 SRAM, 2 Flash, 3 EEPROM.",
            ["BackupSize"] = "Save size (bytes or K values like 4K/16K).",
            ["RamSize"] = "RDRAM size in hex (e.g. 0x400000 for Expansion Pak).",
            ["PlayerNum"] = "Number of players (usually commented).",
            ["TicksPerFrame"] = "47280000 / target_FPS; controls frame timing.",
            ["RomType"] = "ROM type flag; used by specific titles only.",

            // Render
            ["CanvasWidth"] = "Output width (pixels).",
            ["CanvasHeight"] = "Output height (pixels).",
            ["UseColorDither"] = "Enable color dither.",
            ["ForceFilterPoint"] = "Force point sampling.",
            ["ForceRectFilterPoint"] = "Force point filtering on rectangles.",
            ["FrameClearCacheInit"] = "Clear caches at first frame.",
            ["NeedPreParse"] = "Run pre-parse step (game specific).",
            ["NeedTileSizeCheck"] = "Enable additional tile size checks.",

            // Input
            ["StickLimit"] = "Analog stick clamp percent (0-100).",
            ["StickModify"] = "Analog stick modification mode.",
            ["STICK_CLAMP"] = "Clamp classic controller stick.",
            ["VPAD_STICK_CLAMP"] = "Clamp GamePad stick.",

            // RSPG / Others
            ["GTaskDelay"] = "Delay before RSP task start (ms).",
            ["RDPDelay"] = "RDP start delay (ms).",
            ["RDPInt"] = "Enable RDP interrupts.",
            ["WaitOnlyFirst"] = "Wait only for first GTask.",

            // Cmp
            ["BlockSize"] = "Compression block size (hex).",
            ["FrameBlockLimit"] = "Limit of blocks per frame (hex).",
        };
    }
}

