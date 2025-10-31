using System.Collections.Generic;

namespace UWUVCI_AIO_WPF.Modules.N64Config
{
    public enum N64FieldType { Bool01, IntDec, IntHex, String }

    public class N64Field
    {
        public string Key { get; set; }
        public N64FieldType Type { get; set; }
        public string? Display { get; set; }
        public string? Group { get; set; }
        public N64Field(string key, N64FieldType type, string? display = null, string? group = null)
        { Key = key; Type = type; Display = display; Group = group; }
    }

    public static class N64Schema
    {
        public static readonly Dictionary<string, List<N64Field>> Sections = new Dictionary<string, List<N64Field>>
        {
            ["RomOption"] = new List<N64Field>
            {
                // Common keys (some also wired in manual controls; duplicates are harmless but we’ll skip those in UI build)
                new N64Field("RetraceByVsync", N64FieldType.Bool01),
                new N64Field("UseTimer", N64FieldType.Bool01),
                new N64Field("TrueBoot", N64FieldType.Bool01),
                new N64Field("Rumble", N64FieldType.Bool01),
                new N64Field("MemPak", N64FieldType.Bool01),
                new N64Field("BackupType", N64FieldType.IntDec),
                new N64Field("BackupSize", N64FieldType.IntDec),
                new N64Field("RamSize", N64FieldType.IntHex),

                // Fullset
                new N64Field("AIIntPerFrame", N64FieldType.Bool01),
                new N64Field("AISetControl", N64FieldType.Bool01),
                new N64Field("AISetDAC", N64FieldType.Bool01),
                new N64Field("AISetRateBit", N64FieldType.Bool01),
                new N64Field("BootEnd", N64FieldType.Bool01),
                new N64Field("BootPCChange", N64FieldType.Bool01),
                new N64Field("CmpBlockAdvFlag", N64FieldType.Bool01),
                new N64Field("EEROMInitValue", N64FieldType.IntHex),
                new N64Field("g_nN64CpuCmpBlockAdvFlag", N64FieldType.Bool01),
                new N64Field("NoCntPak", N64FieldType.Bool01),
                new N64Field("PDFURL", N64FieldType.String),
                new N64Field("PlayerNum", N64FieldType.IntDec),
                new N64Field("RomType", N64FieldType.Bool01),
                new N64Field("RSPAMultiCoreWait", N64FieldType.Bool01),
                new N64Field("RSPMultiCore", N64FieldType.Bool01),
                new N64Field("RSPMultiCoreInt", N64FieldType.IntDec),
                new N64Field("RSPMultiCoreWait", N64FieldType.Bool01),
                new N64Field("ScreenCaptureNG", N64FieldType.Bool01),
                new N64Field("TicksPerFrame", N64FieldType.IntDec),
                new N64Field("TimeIntDelay", N64FieldType.Bool01),
                new N64Field("TLBMissEnable", N64FieldType.Bool01),
                new N64Field("TPak", N64FieldType.Bool01),
            },

            ["Render"] = new List<N64Field>
            {
                new N64Field("CanvasWidth", N64FieldType.IntDec, null, "Output"),
                new N64Field("CanvasHeight", N64FieldType.IntDec, null, "Output"),
                new N64Field("bForce720P", N64FieldType.Bool01, null, "Output"),
                new N64Field("UseColorDither", N64FieldType.Bool01, null, "Filtering"),
                new N64Field("ForceFilterPoint", N64FieldType.Bool01, null, "Filtering"),
                new N64Field("ForceRectFilterPoint", N64FieldType.Bool01, null, "Filtering"),
                new N64Field("CalculateLOD", N64FieldType.Bool01, null, "Geometry/LOD"),
                new N64Field("TexEdgeAlpha", N64FieldType.Bool01, null, "Geometry/LOD"),
                new N64Field("PolygonOffset", N64FieldType.Bool01, null, "Geometry/LOD"),
                new N64Field("CopyAlphaForceOne", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("CopyColorAfterTask", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("CopyColorBuffer", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("CopyDepthBuffer", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("CopyMiddleBuffer", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("ClearVertexBuf", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("FlushMemEachTask", N64FieldType.Bool01, null, "Buffers & Memory"),
                new N64Field("XClip", N64FieldType.Bool01, null, "Clipping"),
                new N64Field("YClip", N64FieldType.Bool01, null, "Clipping"),
                new N64Field("ZClip", N64FieldType.Bool01, null, "Clipping"),
                new N64Field("bCutClip", N64FieldType.Bool01, null, "Clipping"),
                new N64Field("ClipTop", N64FieldType.IntDec, null, "Clipping"),
                new N64Field("ClipRight", N64FieldType.IntDec, null, "Clipping"),
                new N64Field("ClipBottom", N64FieldType.IntDec, null, "Clipping"),
                new N64Field("ClipLeft", N64FieldType.IntDec, null, "Clipping"),
                new N64Field("CheckTlutValid", N64FieldType.Bool01, null, "Checks"),
                new N64Field("DepthCompare", N64FieldType.Bool01, null, "Checks"),
                new N64Field("DepthCompareLess", N64FieldType.Bool01, null, "Checks"),
                new N64Field("DepthCompareMore", N64FieldType.Bool01, null, "Checks"),
                new N64Field("DoubleFillCheck", N64FieldType.Bool01, null, "Checks"),
                new N64Field("FrameClearCacheInit", N64FieldType.Bool01, null, "Checks"),
                new N64Field("InitPerspectiveMode", N64FieldType.Bool01, null, "Checks"),
                new N64Field("NeedPreParse", N64FieldType.Bool01, null, "Checks"),
                new N64Field("NeedTileSizeCheck", N64FieldType.Bool01, null, "Checks"),
                new N64Field("PreparseTMEMBlock", N64FieldType.Bool01, null, "Checks"),
                new N64Field("RendererReset", N64FieldType.Bool01, null, "Checks"),
                new N64Field("TileSizeCheckSpecial", N64FieldType.Bool01, null, "Checks"),
                new N64Field("TLUTCheck", N64FieldType.Bool01, null, "Checks"),
                new N64Field("FogVertexAlpha", N64FieldType.Bool01, null, "Checks"),
                new N64Field("FirstFrameAt", N64FieldType.IntDec, null, "Checks"),
                new N64Field("useViewportXScale", N64FieldType.Bool01, null, "Viewport"),
                new N64Field("useViewportYScale", N64FieldType.Bool01, null, "Viewport"),
                new N64Field("useViewportZScale", N64FieldType.Bool01, null, "Viewport"),
                // ConstValue[] and list types are kept for future; available via Advanced tab.
            },

            ["Sound"] = new List<N64Field>
            {
                new N64Field("BufFull", N64FieldType.IntHex),
                new N64Field("BufHalf", N64FieldType.IntHex),
                new N64Field("BufHave", N64FieldType.IntHex),
                new N64Field("FillAfterVCM", N64FieldType.IntDec),
                new N64Field("Resample", N64FieldType.IntDec),
            },

            ["Input"] = new List<N64Field>
            {
                new N64Field("AlwaysHave", N64FieldType.Bool01, null, "General"),
                new N64Field("STICK_CLAMP", N64FieldType.Bool01, null, "Clamp"),
                new N64Field("VPAD_STICK_CLAMP", N64FieldType.Bool01, null, "Clamp"),
                new N64Field("StickLimit", N64FieldType.IntDec, null, "Stick"),
                new N64Field("StickModify", N64FieldType.IntDec, null, "Stick"),
            },

            ["RSPG"] = new List<N64Field>
            {
                new N64Field("GTaskDelay", N64FieldType.IntDec),
                new N64Field("RDPDelay", N64FieldType.IntDec),
                new N64Field("RDPInt", N64FieldType.Bool01),
                new N64Field("RIntAfterGTask", N64FieldType.Bool01),
                new N64Field("RSPGWaitOnlyFirstGTaskDelay", N64FieldType.Bool01),
                new N64Field("Skip", N64FieldType.Bool01),
                new N64Field("WaitDelay", N64FieldType.Bool01),
                new N64Field("WaitOnlyFirst", N64FieldType.Bool01),
            },

            ["Cmp"] = new List<N64Field>
            {
                new N64Field("BlockSize", N64FieldType.IntHex),
                new N64Field("CmpLimit", N64FieldType.Bool01),
                new N64Field("FrameBlockLimit", N64FieldType.IntHex),
                new N64Field("OptEnable", N64FieldType.Bool01),
                new N64Field("W32OverlayCheck", N64FieldType.Bool01),
            },

            ["TempConfig"] = new List<N64Field>
            {
                new N64Field("g_nN64CpuPC", N64FieldType.Bool01),
                new N64Field("n64MemAcquireForground", N64FieldType.Bool01),
                new N64Field("n64MemDefaultRead32MemTest", N64FieldType.Bool01),
                new N64Field("n64MemReleaseForground", N64FieldType.Bool01),
                new N64Field("RSPGDCFlush", N64FieldType.Bool01),
            },

            ["SI"] = new List<N64Field>
            {
                new N64Field("SIDelay", N64FieldType.IntHex),
            },

            ["VI"] = new List<N64Field>
            {
                new N64Field("ScanReadTime", N64FieldType.IntDec),
            },
        };
    }
}
