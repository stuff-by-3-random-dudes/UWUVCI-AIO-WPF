using System;
using System.IO;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Services
{
    public static class GCNInjectService
    {
        public static void InjectGCN(
            string toolsPath,
            string tempPath,
            string baseRomPath,
            string romPath,
            GcnInjectOptions opt,
            IToolRunnerFacade runner = null)
        {
            if (opt == null) throw new ArgumentNullException(nameof(opt));
            runner ??= DefaultToolRunnerFacade.Instance;
            // Normalize potential POSIX input to Windows-view once.
            romPath = ToolRunner.ReplaceArgsWithWindowsFlavor(ToolRunner.Q(romPath)).Trim('"');

            var tempBase = PrepareTempBase(toolsPath, tempPath);
            ApplyNintendontDol(toolsPath, tempBase, opt.Force43);
            PlacePrimaryGame(toolsPath, tempBase, romPath, opt.DontTrim, opt.Debug, runner);
            PlaceDisc2IfAny(toolsPath, tempBase, romPath, opt.Disc2Path, opt.DontTrim, opt.Debug, runner);
            var nfs = new NfsInjectOptions { Debug = opt.Debug, Kind = InjectKind.GCN, Passthrough = opt.Passthrough, Index = opt.Index, LR = opt.LR };
            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, nfs, runner);
        }

        // Composable helpers for controller use
        internal static string PrepareTempBase(string toolsPath, string tempPath)
        {
            var tempBaseWin = Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBaseWin)) 
                Directory.Delete(tempBaseWin, true);

            var extracted = BaseExtractor.GetOrExtractBase(toolsPath, "BASE.zip");
            IOHelpers.MoveOrCopyDirectory(extracted, tempBaseWin);
            return tempBaseWin;
        }

        internal static void ApplyNintendontDol(string toolsPath, string tempBase, bool force43)
        {
            var mainDol = Path.Combine(tempBase, "sys", "main.dol");
            File.Copy(Path.Combine(toolsPath, force43 ? "nintendont_force.dol" : "nintendont.dol"), mainDol, true);
        }

        internal static void PlacePrimaryGame(string toolsPath, string tempBase, string romPath, bool dontTrim, bool debug, IToolRunnerFacade runner)
        {
            var targetGameInBase = Path.Combine(tempBase, "files", "game.iso");
            Directory.CreateDirectory(Path.GetDirectoryName(targetGameInBase));
            if (dontTrim)
            {
                if (romPath.ToLowerInvariant().Contains("nkit.iso") || romPath.ToLower().Contains("gcz"))
                {
                    var outIso = NKitService.ConvertToIso(toolsPath, romPath, "out.iso", debug, runner);
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
                if (romPath.ToLowerInvariant().Contains("iso") || romPath.ToLower().Contains("gcm") || romPath.ToLower().Contains("gcz"))
                {
                    var outNkit = NKitService.ConvertToNKit(toolsPath, romPath, "out.nkit.iso", debug, runner);
                    if (!File.Exists(outNkit)) throw new Exception("nkit");
                    FileHelpers.MoveOverwrite(outNkit, targetGameInBase);
                }
                else
                {
                    File.Copy(romPath, targetGameInBase, true);
                }
            }
        }

        internal static void PlaceDisc2IfAny(string toolsPath, string tempBase, string primaryRom, string disc2, bool dontTrim, bool debug, IToolRunnerFacade runner)
        {
            if (string.IsNullOrEmpty(disc2) || !File.Exists(disc2)) return;
            var disc2Out = Path.Combine(tempBase, "files", "disc2.iso");
            if (dontTrim)
            {
                if (disc2.ToLower().Contains("nkit"))
                {
                    var outIso1 = NKitService.ConvertToIso(toolsPath, disc2, "out(Disc 1).iso", debug, runner);
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
                    var outNkit1 = NKitService.ConvertToNKit(toolsPath, disc2, "out(Disc 1).nkit.iso", debug, runner);
                    if (!File.Exists(outNkit1)) throw new Exception("nkit");
                    FileHelpers.MoveOverwrite(outNkit1, disc2Out);
                }
                else if (primaryRom.ToLower().Contains("gcz"))
                {
                    var outNkit1 = NKitService.ConvertToNKit(toolsPath, primaryRom, "out(Disc 1).nkit.iso", debug, runner);
                    if (!File.Exists(outNkit1)) throw new Exception("nkit");
                    FileHelpers.MoveOverwrite(outNkit1, disc2Out);
                }
                else
                {
                    File.Copy(primaryRom, disc2Out, true);
                }
            }
        }
    }
}
