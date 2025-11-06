using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF;

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
        /// and runs nfs2iso2nfs to inject. .
        /// </summary>
        public static void BuildIsoExtractTicketsAndInject(
            string toolsPath,
            string tempPath,
            string baseRomPath,
            NfsInjectOptions options,
            IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;
            bool debug = options?.Debug ?? false;

            var tempBaseWin = Path.Combine(tempPath, "TempBase");
            var gameIsoWin = Path.Combine(tempPath, "game.iso");
            var tikTmdWin = Path.Combine(tempPath, "TIKTMD");
            var contentDir = Path.Combine(baseRomPath, "content");
            Directory.CreateDirectory(contentDir);

            // 1) wit copy  (TempBase -> content\game.iso) to avoid a large move later
            runner.RunTool(
                toolBaseName: "wit",
                toolsPathWin: toolsPath,
                argsWindowsPaths: $"copy \"{tempBaseWin}\" --DEST \"{Path.Combine(contentDir, "game.iso")}\" -ovv --links --iso",
                showWindow: debug
            );

            gameIsoWin = Path.Combine(contentDir, "game.iso");
            if (!File.Exists(gameIsoWin))
                throw new Exception("Wii: An error occured while Creating the ISO");

            // 2) wit extract (TIK/TMD out of game.iso)
            runner.RunTool(
                toolBaseName: "wit",
                toolsPathWin: toolsPath,
                argsWindowsPaths: $"extract \"{gameIsoWin}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{tikTmdWin}\" -vv1",
                showWindow: debug
            );

            // If GCN, save rom code to meta.xml
            if (options != null && options.Kind == InjectKind.GCN)
            {
                byte[] chars = new byte[4];
                using (var fstrm = new FileStream(gameIsoWin, FileMode.Open, FileAccess.Read))
                    fstrm.Read(chars, 0, 4);

                string procod = BAToAscii(chars);
                string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
                var doc = new XmlDocument();
                doc.Load(metaXml);
                doc.SelectSingleNode("menu/reserved_flag2").InnerText = procod.ToHex();
                doc.Save(metaXml);
            }

            // Cleanup TempBase
            var tempBaseDir = Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBaseDir))
                Directory.Delete(tempBaseDir, true);

            // Replace rvlt.* and copy TIK/TMD
            options?.Progress?.Invoke(50, "Replacing TIK and TMD...");
            var codeDir = Path.Combine(baseRomPath, "code");
            var rvltFiles = Directory.GetFiles(codeDir, "rvlt.*");
            System.Threading.Tasks.Parallel.ForEach(rvltFiles, f => { try { File.Delete(f); } catch { } });

            File.Copy(Path.Combine(tikTmdWin, "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"), true);
            File.Copy(Path.Combine(tikTmdWin, "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"), true);
            Directory.Delete(tikTmdWin, true);

            // Inject via nfs2iso2nfs.exe (Windows exe; runs fine under Wine)
            options?.Progress?.Invoke(60, "Injecting ROM...");
            // Begin nfs conversion

            var oldNfs = Directory.GetFiles(contentDir, "*.nfs");
            System.Threading.Tasks.Parallel.ForEach(oldNfs, f => { try { File.Delete(f); } catch { } });


            // Run nfs2iso2nfs via ToolRunner in the content folder
            string pass = (options != null && options.Passthrough && options.Kind != InjectKind.GCN) ? "-passthrough " : string.Empty;
            string extra = string.Empty;
            if (options == null || options.Kind != InjectKind.GCN)
            {
                var idx = options?.Index ?? 0;
                if (idx == 2) extra = "-horizontal ";
                if (idx == 3) extra = "-wiimote ";
                if (idx == 4) extra = "-instantcc ";
                if (idx == 5) extra = "-nocc ";
                if (options?.LR == true) extra += "-lrpatch ";
            }
            else
            {
                pass = "-passthrough ";
                extra = string.Empty;
            }

            runner.RunToolWithFallback(
                toolBaseName: "nfs2iso2nfs",
                toolsPathWin: toolsPath,      // exe lives in Tools
                argsWindowsPaths: $"-enc -homebrew {extra}{pass}-iso game.iso",
                showWindow: debug,
                workDirWin: contentDir        // run in content dir so relative game.iso resolves
            );

            // Cleanup working ISO
            File.Delete(Path.Combine(contentDir, "game.iso"));
            options?.Progress?.Invoke(80, "Injection complete");
        }
    }
}
