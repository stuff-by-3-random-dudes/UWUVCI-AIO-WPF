using System;
using System.IO;
using System.Linq;
using System.Text;
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
            var (preIso, usedSource, sourceMiB) = PreparePreIso(toolsPath, tempPath, romPath, opt, runner);
            UpdateMetaReservedFlag(baseRomPath, preIso);
            var tempDir = Path.Combine(tempPath, "TEMP");
            WitExtractToTemp(toolsPath, preIso, tempDir, opt, runner);
            ApplyOptionalDolPatch(tempDir, opt);
            if (opt.PatchVideo) ApplyVideoPatch(toolsPath, tempDir, opt);
            PromoteTempDirToTempBase(tempDir, tempPath);
            if (!usedSource) TryDelete(preIso);
            var nfsOptions = new NfsInjectOptions
            {
                Debug = opt.Debug,
                Kind = InjectKind.WiiStandard,
                Passthrough = opt.Passthrough,
                Index = opt.Index,
                LR = opt.LR,
                SourceMiB = sourceMiB,
                Progress = opt.Progress
            };
            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, nfsOptions, runner);
        }

        internal static (string preIso, bool usedSource, double? sourceMiB) PreparePreIso(string toolsPath, string tempPath, string romPath, WiiInjectOptions opt, IToolRunnerFacade runner)
        {
            string preIso = Path.Combine(tempPath, "pre.iso");
            var ext = (Path.GetExtension(romPath) ?? string.Empty).ToLowerInvariant();
            // Capture source size (wit view) upfront when possible
            double? srcSize = null;
            try { srcSize = ToolRunner.GetWitSizeMiB(toolsPath, romPath); } catch { srcSize = null; }
            if (ext.Contains("iso")) 
                return (romPath, true, srcSize);

            if (opt.ForceNkitConvert || romPath.IndexOf("nkit", StringComparison.OrdinalIgnoreCase) >= 0 || ext.Contains("wbfs"))
            {
                var witArgs = $"copy --source \"{romPath}\" --dest \"{preIso}\" -I";
                runner.RunTool("wit", toolsPath, witArgs, showWindow: opt.Debug);
                ToolRunner.LogFileVisibility("[after wit copy] pre.iso", preIso);
                if (!ToolRunner.WaitForWineVisibility(preIso)) 
                    throw new FileNotFoundException("pre.iso not visible after conversion.", preIso);
                ToolRunner.WaitForStableFileSize(preIso);

                // Verify the converted ISO matches source size per wit
                if (srcSize.HasValue)
                    ToolRunner.VerifyWitSize(toolsPath, preIso, expectedMiB: srcSize, toleranceMiB: 1.0);
                else
                    ToolRunner.VerifyWitSize(toolsPath, preIso); // at least ensure wit can read it

                if (!ext.Contains("wbfs") && !File.Exists(preIso)) 
                    throw new Exception("nkit");

                return (preIso, false, srcSize);
            }
            return (preIso, false, srcSize);
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

            // Under Wine the native wit binary can finish before Wine/.NET sees the extracted files.
            // Fence on directory visibility so later steps don't run against an empty TEMP folder.
            if (!ToolRunner.WaitForWineVisibility(tempDir, file: false))
                throw new IOException($"wit extract completed but TEMP directory not visible: {tempDir}");
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

        private static void PromoteTempDirToTempBase(string tempDir, string tempPath)
        {
            if (!Directory.Exists(tempDir))
                return;
            var tempBase = Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBase))
                Directory.Delete(tempBase, true);
            var moved = IOHelpers.MoveOrCopyDirectory(tempDir, tempBase);
            if (!moved && Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
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

