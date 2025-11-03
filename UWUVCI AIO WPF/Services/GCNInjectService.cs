using System;
using System.IO;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Services
{
    public static class GCNInjectService
    {
        public static void InjectGCN(
            string toolsPath,
            string tempPath,
            string baseRomPath,
            string romPath,
            MainViewModel mvm,
            bool force,
            IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;

            // Normalize potential POSIX input to Windows-view once.
            romPath = ToolRunner.ReplaceArgsWithWindowsFlavor(ToolRunner.Q(romPath)).Trim('"');
            if (mvm != null) mvm.msg = "Extracting Nintendont Base...";

            var tempBaseWin = Path.Combine(tempPath, "TempBase");
            var baseZipWin  = Path.Combine(toolsPath, "BASE.zip");
            var baseDirWin  = Path.Combine(tempPath, "BASE");

            if (Directory.Exists(tempBaseWin))
                Directory.Delete(tempBaseWin, true);

            // Use cached extraction and copy/move into TempBase
            var extractedBase = BaseExtractor.GetOrExtractBase(toolsPath, "BASE.zip");
            IOHelpers.MoveOrCopyDirectory(extractedBase, tempBaseWin);

            if (mvm != null)
            {
                mvm.Progress = 20;
                mvm.msg = "Applying Nintendont" + (force ? " force 4:3..." : "...");
            }

            var mainDol = Path.Combine(tempBaseWin, "sys", "main.dol");
            File.Copy(Path.Combine(toolsPath, force ? "nintendont_force.dol" : "nintendont.dol"), mainDol, true);
            if (mvm != null) mvm.Progress = 40;

            // Inject primary game to TempBase/files/game.iso
            var targetGameInBase = Path.Combine(tempBaseWin, "files", "game.iso");
            Directory.CreateDirectory(Path.GetDirectoryName(targetGameInBase));

            bool dontTrim = mvm?.donttrim ?? false;
            if (dontTrim)
            {
                // Keep full ISO. If NKIT or GCZ -> convert to ISO first.
                if (romPath.ToLowerInvariant().Contains("nkit.iso") || romPath.ToLower().Contains("gcz"))
                {
                    var outIso = NKitService.ConvertToIso(toolsPath, romPath, "out.iso", mvm?.debug ?? false, runner);
                    if (!File.Exists(outIso)) throw new Exception("nkit");
                    FileHelpers.MoveOverwrite(outIso, targetGameInBase);
                }
                else
                {
                    File.Copy(romPath, targetGameInBase, true);
                }
            }
            else
            {
                // Trim: convert ISO/GCM/GCZ → NKIT (then stored as game.iso in base)
                if (romPath.ToLowerInvariant().Contains("iso") || romPath.ToLower().Contains("gcm") || romPath.ToLower().Contains("gcz"))
                {
                    var outNkit = NKitService.ConvertToNKit(toolsPath, romPath, "out.nkit.iso", mvm?.debug ?? false, runner);
                    if (!File.Exists(outNkit)) throw new Exception("nkit");
                    FileHelpers.MoveOverwrite(outNkit, targetGameInBase);
                }
                else
                {
                    File.Copy(romPath, targetGameInBase, true);
                }
            }

            // Optional Disc 2
            var disc2 = mvm?.gc2rom;
            if (!string.IsNullOrEmpty(disc2) && File.Exists(disc2))
            {
                var disc2Out = Path.Combine(tempBaseWin, "files", "disc2.iso");
                if (dontTrim)
                {
                    if (disc2.Contains("nkit"))
                    {
                        var outIso1 = NKitService.ConvertToIso(toolsPath, disc2, "out(Disc 1).iso", mvm?.debug ?? false, runner);
                        if (!File.Exists(outIso1)) throw new Exception("nkit");
                        FileHelpers.MoveOverwrite(outIso1, disc2Out);
                    }
                    else
                    {
                        File.Copy(disc2, disc2Out, true);
                    }
                }
                else
                {
                    if (disc2.ToLower().Contains("iso") || disc2.ToLower().Contains("gcm") || disc2.ToLower().Contains("gcz"))
                    {
                        var outNkit1 = NKitService.ConvertToNKit(toolsPath, disc2, "out(Disc 1).nkit.iso", mvm?.debug ?? false, runner);
                        if (!File.Exists(outNkit1)) throw new Exception("nkit");
                        FileHelpers.MoveOverwrite(outNkit1, disc2Out);
                    }
                    else if (romPath.ToLower().Contains("gcz"))
                    {
                        var outNkit1 = NKitService.ConvertToNKit(toolsPath, romPath, "out(Disc 1).nkit.iso", mvm?.debug ?? false, runner);
                        if (!File.Exists(outNkit1)) throw new Exception("nkit");
                        FileHelpers.MoveOverwrite(outNkit1, disc2Out);
                    }
                    else
                    {
                        File.Copy(romPath, disc2Out, true);
                    }
                }
            }

            // Finalize via shared WIT+NFS flow
            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, "GCN", mvm, runner);
        }
    }
}
