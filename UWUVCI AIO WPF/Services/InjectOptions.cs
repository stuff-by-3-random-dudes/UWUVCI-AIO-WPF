using System;

namespace UWUVCI_AIO_WPF.Services
{
    // Injection services are UI-free. All user-facing concerns (messages, progress)
    // flow via these option objects (callbacks) provided by the orchestrator/controller.
    public enum InjectKind
    {
        WiiStandard,
        WiiHomebrew,
        WiiForwarder,
        GCN
    }

    public class NfsInjectOptions
    {
        public bool Debug { get; set; }
        public InjectKind Kind { get; set; }
        public bool Passthrough { get; set; }
        public int Index { get; set; }
        public bool LR { get; set; }
        // Optional progress reporter: (percent [0-100], message)
        public Action<int, string> Progress { get; set; }
    }

    public class WiiInjectOptions
    {
        public bool Debug { get; set; }
        public bool DontTrim { get; set; }
        public bool PatchVideo { get; set; }
        public bool ToPal { get; set; }
        public int Index { get; set; }
        public bool LR { get; set; }
        public bool ForceNkitConvert { get; set; }
        public bool Passthrough { get; set; }
        public Action<string> Log { get; set; }
        // Called with full path to main.dol after extract, before repack.
        public Func<string, bool> PatchDolCallback { get; set; }
        // Optional progress reporter: (percent [0-100], message)
        public Action<int, string> Progress { get; set; }
    }

    public class GcnInjectOptions
    {
        public bool Debug { get; set; }
        public bool DontTrim { get; set; }
        public string Disc2Path { get; set; }
        public bool Force43 { get; set; }
        public bool Passthrough { get; set; } = true; // default passthrough for GCN
        public int Index { get; set; } = 0;
        public bool LR { get; set; } = false;
        public Action<string> Log { get; set; }
        public Func<string, bool> PatchDolCallback { get; set; }
    }
}
