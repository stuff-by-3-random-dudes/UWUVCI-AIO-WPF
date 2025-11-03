using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UWUVCI_AIO_WPF; // access Injection class
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Services
{
    public static class WiiInjectService
    {
        public static void InjectStandard(string toolsPath, string tempPath, string baseRomPath, string romPath, MainViewModel mvm, IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;
            static void Log(string msg)
            {
                var line = $"[WII] {DateTime.Now:HH:mm:ss} {msg}";
                Console.WriteLine(line);
                try { Logger.Log(line); } catch { }
            }
            string SizeOf(string p) => File.Exists(p) ? new FileInfo(p).Length.ToString("N0") + " bytes" : "missing";

            // Normalize possible POSIX input to Windows view
            romPath = ToolRunner.ToWindowsView(romPath);
            Log($"Input ROM: {romPath}");

            var preIsoWin = Path.Combine(tempPath, "pre.iso");
            bool preIsoIsSourceRom = false;
            var tempDirWin = Path.Combine(tempPath, "TEMP");
            var gameIsoWin = Path.Combine(tempPath, "game.iso");
            var tikTmdWin = Path.Combine(tempPath, "TIKTMD");
            Directory.CreateDirectory(tempPath);

            // 1) Prepare pre.iso (copy or convert NKIT/WBFS)
            {
                var ext = (Path.GetExtension(romPath) ?? "").ToLowerInvariant();
                if (ext.Contains("iso"))
                {
                    // Use the source ISO directly to avoid a large copy.
                    preIsoWin = romPath;
                    preIsoIsSourceRom = true;
                    Log($"Using source ISO directly: {preIsoWin}");
                }
                else if (mvm.NKITFLAG || romPath.IndexOf("nkit", StringComparison.OrdinalIgnoreCase) >= 0 || ext.Contains("wbfs"))
                {
                    mvm.msg = (mvm.NKITFLAG || romPath.IndexOf("nkit", StringComparison.OrdinalIgnoreCase) >= 0)
                                ? "Converting NKIT to ISO"
                                : "Converting WBFS to ISO...";
                    var witArgs = $"copy --source \"{romPath}\" --dest \"{preIsoWin}\" -I";
                    Log($"Calling wit: {witArgs}");
                    runner.RunTool("wit", toolsPath, witArgs, showWindow: mvm.debug);
                    ToolRunner.LogFileVisibility("[after wit copy] pre.iso", preIsoWin);
                    if (!ToolRunner.WaitForWineVisibility(preIsoWin))
                        throw new FileNotFoundException("pre.iso not visible to Wine/.NET after wit copy.", preIsoWin);
                    if (!ext.Contains("wbfs") && !File.Exists(preIsoWin))
                        throw new Exception("nkit");
                }
                else
                {
                    Log($"Unsupported input extension '{ext}'. Continuing may fail later.");
                }
            }

            // 2) meta.xml reserved_flag2 from disc ID
            {
                mvm.msg = "Trying to change the Manual...";
                ToolRunner.LogFileVisibility("[step 2 pre-check] pre.iso", preIsoWin);
                if (!File.Exists(preIsoWin))
                    throw new FileNotFoundException("pre.iso missing before meta.xml edit. (Wine/.NET view)", preIsoWin);

                byte[] chars = new byte[4];
                using (var fstrm = new FileStream(preIsoWin, FileMode.Open, FileAccess.Read))
                    fstrm.Read(chars, 0, 4);

                string procod = Encoding.ASCII.GetString(chars);
                var metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
                var doc = new System.Xml.XmlDocument();
                doc.Load(metaXml);
                doc.SelectSingleNode("menu/reserved_flag2").InnerText = procod.ToHex();
                doc.Save(metaXml);
                mvm.Progress = 20;
            }

            // 3–10) Extract, patch, repack (unchanged from original flow)
            {
                // 3) Extract to TEMP
                var extractArgs = $"extract \"{preIsoWin}\" --DEST \"{tempDirWin}\" --psel data -vv1";
                Log($"[STEP 3] wit extract: {extractArgs}");
                runner.RunTool("wit", toolsPath, extractArgs, showWindow: mvm.debug);

                // 4–8) DOL patching/gct conversion – call existing helpers
                // Note: original flow uses Injection.PatchDol and related helpers
                mvm.msg = "Patching main.dol with gct file";
                mvm.Progress = 27;
                File.Delete(gameIsoWin); // ensure clean
                var mainDolPath = Directory.GetFiles(tempDirWin, "main.dol", SearchOption.AllDirectories).FirstOrDefault();
                Injection.PatchDol("Wii", mainDolPath, mvm);

                // 9) Video patch (wii-vmc) – keep original interactive execution
                if (mvm.Patch)
                {
                    mvm.msg = "Applying video patch...";
                    var sysDir = Path.Combine(tempDirWin, "DATA", "sys");
                    Directory.CreateDirectory(sysDir);
                    var vmcExe = Path.Combine(toolsPath, "wii-vmc.exe");
                    using (var vmc = new System.Diagnostics.Process())
                    {
                        string extra = "";
                        if (mvm.Index == 2) extra = "-horizontal ";
                        else if (mvm.Index == 3) extra = "-wiimote ";
                        else if (mvm.Index == 4) extra = "-instantcc ";
                        else if (mvm.Index == 5) extra = "-nocc ";
                        if (mvm.LR) extra += "-lrpatch ";
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
                        vmc.StandardInput.WriteLine(mvm.toPal ? "1" : "2");
                        System.Threading.Thread.Sleep(2000);
                        vmc.StandardInput.WriteLine();
                        vmc.WaitForExit();
                        // nothing to delete; executed from tools path
                    }
                }

                // 10) Repack via WIT
                string copyFlags = mvm.donttrim ? "--psel raw --iso" : "--psel whole --iso";
                // Repack directly into content\\game.iso to avoid a large file move
                var contentDirForRepack = Path.Combine(baseRomPath, "content");
                Directory.CreateDirectory(contentDirForRepack);
                var finalIsoForRepack = Path.Combine(contentDirForRepack, "game.iso");
                var repackArgs = $"copy \"{tempDirWin}\" --DEST \"{finalIsoForRepack}\" -ovv --links {copyFlags}";
                Log($"[STEP 10] wit repack: {repackArgs}");
                runner.RunTool("wit", toolsPath, repackArgs, showWindow: mvm.debug);
                Directory.Delete(tempDirWin, true);
                // Update gameIsoWin to point to final destination
                gameIsoWin = finalIsoForRepack;
                if (!preIsoIsSourceRom)
                {
                    try { File.Delete(preIsoWin); } catch { }
                }
            }

            // 11–12) TIK/TMD and nfs2iso2nfs (ToolRunner fallback)
            {
                mvm.Progress = 50;
                mvm.msg = "Replacing TIK and TMD...";
                if (Directory.Exists(tikTmdWin)) { try { Directory.Delete(tikTmdWin, true); } catch { } }
                var witArgs = $"extract \"{gameIsoWin}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{tikTmdWin}\" -vv1 -o";
                runner.RunTool("wit", toolsPath, witArgs, showWindow: mvm.debug);
                var toDelete = Directory.GetFiles(Path.Combine(baseRomPath, "code"), "rvlt.*");
                System.Threading.Tasks.Parallel.ForEach(toDelete, s => { try { File.Delete(s); } catch { } });
                File.Copy(Path.Combine(tikTmdWin, "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"), true);
                File.Copy(Path.Combine(tikTmdWin, "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"), true);
                try { Directory.Delete(tikTmdWin, true); } catch { }

                mvm.Progress = 60;
                mvm.msg = "Injecting ROM...";
                var contentDir = Path.Combine(baseRomPath, "content");
                // Remove old NFS files in parallel to reduce I/O wall time.
                var oldNfs = Directory.GetFiles(contentDir, "*.nfs");
                System.Threading.Tasks.Parallel.ForEach(oldNfs, s => { try { File.Delete(s); } catch { } });

                string extra = "";
                if (mvm.Index == 2) extra = "-horizontal ";
                if (mvm.Index == 3) extra = "-wiimote ";
                if (mvm.Index == 4) extra = "-instantcc ";
                if (mvm.Index == 5) extra = "-nocc ";
                if (mvm.LR) extra += "-lrpatch ";
                var args = $"-enc {extra}-iso game.iso";
                runner.RunToolWithFallback("nfs2iso2nfs", contentDir, args, showWindow: mvm.debug, workDirWin: contentDir);
                File.Delete(Path.Combine(contentDir, "game.iso"));
                mvm.Progress = 80;
            }
        }

        public static void InjectHomebrew(
            string toolsPath,
            string tempPath,
            string baseRomPath,
            string romPath,
            MainViewModel mvm,
            IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;

            mvm.msg = "Extracting Homebrew Base...";

            var tempBase = System.IO.Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBase)) Directory.Delete(tempBase, true);
            Directory.CreateDirectory(tempBase);

            // Use cached extraction of BASE.zip
            var extractedBase = BaseExtractor.GetOrExtractBase(toolsPath, "BASE.zip");
            UWUVCI_AIO_WPF.Helpers.IOHelpers.MoveOrCopyDirectory(extractedBase, tempBase);

            mvm.Progress = 20;
            mvm.msg = "Injecting DOL...";

            File.Copy(romPath, System.IO.Path.Combine(tempBase, "sys", "main.dol"));
            mvm.Progress = 30;
            mvm.msg = "Creating Injectable file...";

            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, "WiiHomebrew", mvm, runner);
        }

        public static void InjectForwarder(
            string toolsPath,
            string tempPath,
            string baseRomPath,
            string wadPath,
            MainViewModel mvm,
            IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;

            mvm.msg = "Extracting Forwarder Base...";
            var tempBase = System.IO.Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBase)) Directory.Delete(tempBase, true);
            Directory.CreateDirectory(tempBase);

            // Use cached extraction of BASE.zip
            var extractedBase = BaseExtractor.GetOrExtractBase(toolsPath, "BASE.zip");
            UWUVCI_AIO_WPF.Helpers.IOHelpers.MoveOrCopyDirectory(extractedBase, tempBase);

            mvm.Progress = 20;
            mvm.msg = "Setting up Forwarder...";
            byte[] test = new byte[4];
            using (FileStream fs = new FileStream(wadPath, FileMode.Open))
            {
                fs.Seek(0xC20, SeekOrigin.Begin);
                fs.Read(test, 0, 4);
            }

            string[] id = { new System.Text.ASCIIEncoding().GetString(test) };
            File.WriteAllLines(System.IO.Path.Combine(tempBase, "files", "title.txt"), id);
            mvm.Progress = 30;
            mvm.msg = "Copying Forwarder...";
            File.Copy(System.IO.Path.Combine(toolsPath, "forwarder.dol"), System.IO.Path.Combine(tempBase, "sys", "main.dol"));
            mvm.Progress = 40;
            mvm.msg = "Creating Injectable file...";

            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, "WiiForwarder", mvm, runner);
        }
    }
}

