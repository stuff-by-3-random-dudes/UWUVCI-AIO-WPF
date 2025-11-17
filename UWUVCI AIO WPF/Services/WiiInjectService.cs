using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Services
{
    public static class WiiInjectService
    {
        // Thin wrapper; controller should typically call the step methods explicitly
        public static void InjectStandard(string toolsPath, string tempPath, string baseRomPath, string romPath, WiiInjectOptions opt, IToolRunnerFacade runner = null)
        {
            if (opt == null) throw new ArgumentNullException(nameof(opt));
            runner ??= DefaultToolRunnerFacade.Instance;
            romPath = ToolRunner.ToWindowsView(romPath);
            Directory.CreateDirectory(tempPath);
            RunStandardPipeline(toolsPath, tempPath, baseRomPath, romPath, opt, runner);
        }

        // Orchestrated standard flow composed of small steps
        internal static void RunStandardPipeline(string toolsPath, string tempPath, string baseRomPath, string romPath, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            var (preIso, usedSource) = PreparePreIso(toolsPath, tempPath, romPath, opt, runner);
            UpdateMetaReservedFlag(baseRomPath, preIso);
            var tempDir = Path.Combine(tempPath, "TEMP");
            WitExtractToTemp(toolsPath, preIso, tempDir, opt, runner);
            ApplyOptionalDolPatch(tempDir, opt);
            if (opt.PatchVideo) ApplyVideoPatch(toolsPath, tempDir, opt);
            var gameIso = RepackToContentIso(toolsPath, tempDir, baseRomPath, opt, runner);
            if (!usedSource) TryDelete(preIso);
            ExtractTicketsAndReplace(toolsPath, tempPath, baseRomPath, gameIso, opt, runner);
            var contentDir = Path.Combine(baseRomPath, "content");
            CleanContentNfs(contentDir);
            RunNfsConversion(toolsPath, contentDir, opt, runner);
            TryDelete(Path.Combine(contentDir, "game.iso"));
            opt.Progress?.Invoke(80, "Injection complete");
        }

        internal static (string preIso, bool usedSource) PreparePreIso(string toolsPath, string tempPath, string romPath, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            string preIso = Path.Combine(tempPath, "pre.iso");
            var ext = (Path.GetExtension(romPath) ?? string.Empty).ToLowerInvariant();
            if (ext.Contains("iso")) return (romPath, true);
            if (opt.ForceNkitConvert || romPath.IndexOf("nkit", StringComparison.OrdinalIgnoreCase) >= 0 || ext.Contains("wbfs"))
            {
                var witArgs = $"copy --source \"{romPath}\" --dest \"{preIso}\" -I";
                runner.RunTool("wit", toolsPath, witArgs, showWindow: opt.Debug);
                ToolRunner.LogFileVisibility("[after wit copy] pre.iso", preIso);
                if (!ToolRunner.WaitForWineVisibility(preIso)) throw new FileNotFoundException("pre.iso not visible after conversion.", preIso);
                if (!ext.Contains("wbfs") && !File.Exists(preIso)) throw new Exception("nkit");
                return (preIso, false);
            }
            return (preIso, false);
        }

        internal static void UpdateMetaReservedFlag(string baseRomPath, string isoPath)
        {
            if (!File.Exists(isoPath)) throw new FileNotFoundException("pre.iso missing before meta.xml edit.", isoPath);
            byte[] chars = new byte[4];
            using (var f = new FileStream(isoPath, FileMode.Open, FileAccess.Read)) f.Read(chars, 0, 4);
            string procod = Encoding.ASCII.GetString(chars);
            var metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
            var doc = new System.Xml.XmlDocument();
            doc.Load(metaXml);
            doc.SelectSingleNode("menu/reserved_flag2").InnerText = procod.ToHex();
            doc.Save(metaXml);
            // Progress is optional; controller may also track
        }

        internal static void WitExtractToTemp(string toolsPath, string preIso, string tempDir, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            var pselParam = opt.DontTrim ? "raw" : "whole";
            var extractArgs = $"extract \"{preIso}\" --DEST \"{tempDir}\" --psel {pselParam} -vv1";
            runner.RunTool("wit", toolsPath, extractArgs, showWindow: opt.Debug);
        }

        internal static void ApplyOptionalDolPatch(string tempDir, WiiInjectOptions opt)
        {
            if (opt.PatchDolCallback == null) 
                return;

            var mainDol = Directory.GetFiles(tempDir, "main.dol", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrEmpty(mainDol)) 
                return;

            try { opt.PatchDolCallback(mainDol); } catch { }
        }

        internal static void ApplyVideoPatch(string toolsPath, string tempDir, WiiInjectOptions opt)
        {
            var sysDir = Path.Combine(tempDir, "DATA", "sys");
            Directory.CreateDirectory(sysDir);
            var vmcExe = Path.Combine(toolsPath, "wii-vmc.exe");
            using var vmc = new System.Diagnostics.Process();
            string extra = string.Empty;
            if (opt.Index == 2) extra = "-horizontal ";
            else if (opt.Index == 3) extra = "-wiimote ";
            else if (opt.Index == 4) extra = "-instantcc ";
            else if (opt.Index == 5) extra = "-nocc ";
            if (opt.LR) extra += "-lrpatch ";
            vmc.StartInfo.FileName = vmcExe;
            vmc.StartInfo.Arguments = $"-enc {extra}-iso main.dol";
            vmc.StartInfo.UseShellExecute = false;
            vmc.StartInfo.CreateNoWindow = true;
            vmc.StartInfo.RedirectStandardOutput = true;
            vmc.StartInfo.RedirectStandardInput = true;
            vmc.StartInfo.WorkingDirectory = sysDir;
            vmc.Start();
            System.Threading.Thread.Sleep(1000);
            vmc.StandardInput.WriteLine("a");
            System.Threading.Thread.Sleep(2000);
            vmc.StandardInput.WriteLine(opt.ToPal ? "1" : "2");
            System.Threading.Thread.Sleep(2000);
            vmc.StandardInput.WriteLine();
            vmc.WaitForExit();
        }

        internal static string RepackToContentIso(string toolsPath, string tempDir, string baseRomPath, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            string copyFlags = opt.DontTrim ? "--psel raw --iso" : "--psel whole --iso";
            var contentDir = Path.Combine(baseRomPath, "content");
            Directory.CreateDirectory(contentDir);
            var finalIso = Path.Combine(contentDir, "game.iso");
            var repackArgs = $"copy \"{tempDir}\" --DEST \"{finalIso}\" -ovv --links {copyFlags}";
            runner.RunTool("wit", toolsPath, repackArgs, showWindow: opt.Debug);
            if (!ToolRunner.WaitForWineVisibility(finalIso, timeoutMs: 20000, pollMs: 250))
            {
                throw new Exception($"WIT repack completed but content/game.iso not visible: {finalIso}");
            }
            try { Directory.Delete(tempDir, true); } catch { }
            opt.Progress?.Invoke(45, "Repacked ISO to content");
            return finalIso;
        }

        internal static void ExtractTicketsAndReplace(string toolsPath, string tempPath, string baseRomPath, string gameIso, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            if (opt == null) throw new ArgumentNullException(nameof(opt));
            runner ??= DefaultToolRunnerFacade.Instance;

            var tikTmd = Path.Combine(tempPath, "TIKTMD");
            WitTicketExtractionService.ExtractTickets(
                toolsPath,
                gameIso,
                tikTmd,
                opt.Debug,
                runner: runner,
                waitTimeoutMs: 10000);
            var codeDir = Path.Combine(baseRomPath, "code");
            var toDelete = Directory.GetFiles(codeDir, "rvlt.*");

            Parallel.ForEach(toDelete, s => 
                { try { File.Delete(s); } catch { } 
            });

            File.Copy(Path.Combine(tikTmd, "tmd.bin"), Path.Combine(codeDir, "rvlt.tmd"), true);
            File.Copy(Path.Combine(tikTmd, "ticket.bin"), Path.Combine(codeDir, "rvlt.tik"), true);

            try { Directory.Delete(tikTmd, true); } catch { }

            opt.Progress?.Invoke(50, "Replacing TIK and TMD...");
        }

        internal static void CleanContentNfs(string contentDir)
        {
            var oldNfs = Directory.GetFiles(contentDir, "*.nfs");
            System.Threading.Tasks.Parallel.ForEach(oldNfs, s => { try { File.Delete(s); } catch { } });
        }

        internal static void RunNfsConversion(string toolsPath, string contentDir, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            string extra = string.Empty;
            if (opt.Index == 2) extra = "-horizontal ";
            if (opt.Index == 3) extra = "-wiimote ";
            if (opt.Index == 4) extra = "-instantcc ";
            if (opt.Index == 5) extra = "-nocc ";
            if (opt.LR) extra += "-lrpatch ";
            var pass = opt.Passthrough ? "-passthrough " : string.Empty;
            var args = $"-enc {pass}{extra}-iso game.iso";
            // Run the exe from the tools directory but with working directory set to content
            runner.RunToolWithFallback("nfs2iso2nfs", toolsPath, args, showWindow: opt.Debug, workDirWin: contentDir);
            opt.Progress?.Invoke(60, "Injecting ROM...");
        }

        internal static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        // Shared helpers for Homebrew/Forwarder flows
        internal static string PrepareTempBase(string toolsPath, string tempPath)
        {
            var tempBase = Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBase)) Directory.Delete(tempBase, true);
            Directory.CreateDirectory(tempBase);
            var extractedBase = BaseExtractor.GetOrExtractBase(toolsPath, "BASE.zip");
            IOHelpers.MoveOrCopyDirectory(extractedBase, tempBase);
            return tempBase;
        }

        internal static void CopyDolToBase(string tempBase, string dolSource)
        {
            File.Copy(dolSource, Path.Combine(tempBase, "sys", "main.dol"), true);
        }

        internal static void SetupForwarderTitle(string tempBase, string wadPath)
        {
            byte[] test = new byte[4];
            using (FileStream fs = new FileStream(wadPath, FileMode.Open))
            {
                fs.Seek(0xC20, SeekOrigin.Begin);
                fs.Read(test, 0, 4);
            }
            string[] id = { new ASCIIEncoding().GetString(test) };
            File.WriteAllLines(Path.Combine(tempBase, "files", "title.txt"), id);
        }

        internal static void CopyForwarderDol(string toolsPath, string tempBase)
        {
            File.Copy(Path.Combine(toolsPath, "forwarder.dol"), Path.Combine(tempBase, "sys", "main.dol"), true);
        }
    }
}

