using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Encapsulates the common WIT + NFS2ISO2NFS sequence used for Wii/GCN injections.
    /// </summary>
    public static class WitNfsService
    {
        private static string BAToAscii(byte[] arr) => Encoding.ASCII.GetString(arr ?? Array.Empty<byte>());

        /// <summary>
        /// Builds game.iso from TempBase via wit, extracts TIK/TMD, optionally updates meta flag for GCN,
        /// and runs nfs2iso2nfs to inject. Updates MainViewModel for progress and messages.
        /// </summary>
        public static void BuildIsoExtractTicketsAndInject(
            string toolsPath,
            string tempPath,
            string baseRomPath,
            string functionName,
            MainViewModel mvm,
            IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;
            bool debug = mvm?.debug ?? false;

            var tempBaseWin = Path.Combine(tempPath, "TempBase");
            var gameIsoWin = Path.Combine(tempPath, "game.iso");
            var tikTmdWin = Path.Combine(tempPath, "TIKTMD");

            // 1) wit copy  (TempBase -> game.iso)
            runner.RunTool(
                toolBaseName: "wit",
                toolsPathWin: toolsPath,
                argsWindowsPaths: $"copy \"{tempBaseWin}\" --DEST \"{gameIsoWin}\" -ovv --links --iso",
                showWindow: debug
            );

            if (!File.Exists(gameIsoWin))
                throw new Exception("Wii: An error occured while Creating the ISO");
            if (mvm != null) { mvm.Progress = 50; mvm.msg = "Replacing TIK and TMD..."; }

            // 2) wit extract (TIK/TMD out of game.iso)
            runner.RunTool(
                toolBaseName: "wit",
                toolsPathWin: toolsPath,
                argsWindowsPaths: $"extract \"{gameIsoWin}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{tikTmdWin}\" -vv1",
                showWindow: debug
            );

            // If GCN, save rom code to meta.xml
            if (string.Equals(functionName, "GCN", StringComparison.OrdinalIgnoreCase))
            {
                if (mvm != null) mvm.msg = "Trying to save rom code...";
                byte[] chars = new byte[4];
                using (var fstrm = new FileStream(gameIsoWin, FileMode.Open, FileAccess.Read))
                    fstrm.Read(chars, 0, 4);

                string procod = BAToAscii(chars);
                string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
                var doc = new XmlDocument();
                doc.Load(metaXml);
                doc.SelectSingleNode("menu/reserved_flag2").InnerText = procod.ToHex();
                doc.Save(metaXml);
                if (mvm != null) mvm.Progress = 55;
            }

            // Cleanup TempBase
            var tempBaseDir = Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBaseDir))
                Directory.Delete(tempBaseDir, true);

            // Replace rvlt.* and copy TIK/TMD
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "code"), "rvlt.*"))
                File.Delete(sFile);

            File.Copy(Path.Combine(tikTmdWin, "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"), true);
            File.Copy(Path.Combine(tikTmdWin, "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"), true);
            Directory.Delete(tikTmdWin, true);

            // Inject via nfs2iso2nfs.exe (Windows exe; runs fine under Wine)
            if (mvm != null) { mvm.Progress = 60; mvm.msg = "Injecting ROM..."; }

            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "content"), "*.nfs"))
                File.Delete(sFile);

            var contentDir = Path.Combine(baseRomPath, "content");
            File.Move(gameIsoWin, Path.Combine(contentDir, "game.iso"));
            File.Copy(Path.Combine(toolsPath, "nfs2iso2nfs.exe"), Path.Combine(contentDir, "nfs2iso2nfs.exe"), true);

            // Run nfs2iso2nfs via ToolRunner in the content folder
            string pass = (!string.Equals(functionName, "GCN", StringComparison.OrdinalIgnoreCase) && (mvm?.passtrough ?? false))
                ? "-passthrough " : string.Empty;
            string extra = string.Empty;
            if (!string.Equals(functionName, "GCN", StringComparison.OrdinalIgnoreCase))
            {
                var idx = mvm?.Index ?? 0;
                if (idx == 2) extra = "-horizontal ";
                if (idx == 3) extra = "-wiimote ";
                if (idx == 4) extra = "-instantcc ";
                if (idx == 5) extra = "-nocc ";
                if (mvm?.LR == true) extra += "-lrpatch ";
            }
            else
            {
                pass = "-passthrough ";
                extra = string.Empty;
            }

            runner.RunToolWithFallback(
                toolBaseName: "nfs2iso2nfs",
                toolsPathWin: contentDir,
                argsWindowsPaths: $"-enc -homebrew {extra}{pass}-iso game.iso",
                showWindow: debug,
                workDirWin: contentDir
            );

            // Cleanup dropped executable and working ISO
            File.Delete(Path.Combine(contentDir, "nfs2iso2nfs.exe"));
            File.Delete(Path.Combine(contentDir, "game.iso"));

            if (mvm != null) mvm.Progress = 80;
        }
    }
}
