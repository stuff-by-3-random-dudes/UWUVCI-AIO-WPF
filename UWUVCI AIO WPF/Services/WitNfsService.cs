using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using UWUVCI_AIO_WPF.Helpers;

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

            // Prefer configured/resolved paths (PathResolver reads settings / env)
            toolsPath = !string.IsNullOrWhiteSpace(PathResolver.GetToolsPath()) ? PathResolver.GetToolsPath() : toolsPath;
            tempPath = !string.IsNullOrWhiteSpace(PathResolver.GetTempPath()) ? PathResolver.GetTempPath() : tempPath;

            var tempBaseWin = Path.Combine(tempPath, "TempBase");
            var gameIsoWin = Path.Combine(tempPath, "game.iso");
            var tikTmdWin = Path.Combine(tempPath, "TIKTMD");
            var contentDir = Path.Combine(baseRomPath, "content");
            var alignArg = BuildWitAlignArg(baseRomPath, tempPath);

            Directory.CreateDirectory(contentDir);
            Directory.CreateDirectory(Path.Combine(baseRomPath, "code"));
            Directory.CreateDirectory(Path.Combine(baseRomPath, "meta"));

            Directory.CreateDirectory(tempBaseWin);

            // KegWorks / sandboxed macOS handling (keep existing behavior)
            if ((ToolRunner.HostIsMac() || ToolRunner.HostIsLinux()) && ToolRunner.RunningInsideKegworksApp())
            {
                contentDir = Path.Combine(ToolRunner.GetUserUWUVCIDir(), "baserom", "content");
                Directory.CreateDirectory(contentDir);
                ToolRunner.LogFileVisibility("[KegWorks] Redirected contentDir", contentDir);
                ToolRunner.Log("[KegWorks] contentDir -> host view: " + ToolRunner.WindowsToHostPosix(contentDir));
            }

            // 1) wit copy (TempBase -> content\game.iso)
            try
            {
                var destIso = Path.Combine(contentDir, "game.iso");
                RunnerLog($"[WitNfs] Running wit copy -> dest={destIso}");
                runner.RunTool(
                    toolBaseName: "wit",
                    toolsPathWin: toolsPath,
                    argsWindowsPaths: $"copy \"{tempBaseWin}\" --DEST \"{destIso}\" -ovv --links --iso{(string.IsNullOrWhiteSpace(alignArg) ? string.Empty : " " + alignArg)}",
                    showWindow: debug
                );

                gameIsoWin = Path.Combine(contentDir, "game.iso");

                // Fence: wait for native tool -> wine/.NET visibility
                if (!ToolRunner.WaitForWineVisibility(gameIsoWin))
                {
                    var posix = ToolRunner.WindowsToHostPosix(gameIsoWin);
                    var rc = ToolRunner.RunHostSh($"[ -s {ToolRunner.Q(posix)} ]", out _, out _);
                    if (rc != 0)
                        throw new Exception($"WIT reported success but game.iso is not visible to Wine/host. posix={posix}");
                }
                ToolRunner.LogFileVisibility("[post wit copy] content/game.iso", gameIsoWin);
                LogIsoSize("[WitNfs] content/game.iso after copy", gameIsoWin);
                LogWitSize("[WitNfs] wit size after copy", toolsPath, gameIsoWin);
                WaitForWitSizeStability("[WitNfs] wit size stable after copy", toolsPath, gameIsoWin);
                // Compare against original source size if known; otherwise ensure wit can read non-zero ISO.
                bool VerifyWithRetry(double? expectedMiB = null)
                {
                    try
                    {
                        if (expectedMiB.HasValue)
                            ToolRunner.VerifyWitSize(toolsPath, gameIsoWin, expectedMiB: expectedMiB, toleranceMiB: 5.0);
                        else
                            ToolRunner.VerifyWitSize(toolsPath, gameIsoWin); // ensure wit can read non-zero ISO
                        return true;
                    }
                    catch (Exception ex)
                    {
                        RunnerLog($"[WitNfs] VerifyWitSize first attempt failed: {ex.Message}. Retrying after short delay...");
                        Thread.Sleep(JsonSettingsManager.Settings.UnixWaitDelayMs);
                        try
                        {
                            if (expectedMiB.HasValue)
                                ToolRunner.VerifyWitSize(toolsPath, gameIsoWin, expectedMiB: expectedMiB, toleranceMiB: 5.0);
                            else
                                ToolRunner.VerifyWitSize(toolsPath, gameIsoWin);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                if (options?.SourceMiB.HasValue == true)
                {
                    if (!VerifyWithRetry(options.SourceMiB))
                        throw new Exception("wit size failed or returned zero for " + gameIsoWin);
                }
                else
                {
                    if (!VerifyWithRetry(null) && options?.Kind == InjectKind.WiiStandard)
                        throw new Exception("wit size failed or returned zero for " + gameIsoWin);
                }

                if (!File.Exists(gameIsoWin))
                    throw new Exception("WIT: An error occurred while creating the ISO (game.iso missing).");
            }
            catch (Exception ex)
            {
                throw new Exception("WIT copy step failed: " + ex.Message, ex);
            }

            // 2) extract TIK/TMD
            try
            {
                WitTicketExtractionService.ExtractTickets(
                    toolsPath: toolsPath,
                    gameIso: gameIsoWin,
                    tikTmdDir: tikTmdWin,
                    debug: debug,
                    runner: runner
                );
                LogIsoSize("[WitNfs] content/game.iso after ticket extraction", gameIsoWin);
                LogWitSize("[WitNfs] wit size after ticket extraction", toolsPath, gameIsoWin);
                WaitForWitSizeStability("[WitNfs] wit size stable after ticket extraction", toolsPath, gameIsoWin);
            }
            catch (Exception ex)
            {
                throw new Exception("WIT extract step failed: " + ex.Message, ex);
            }

            // GCN meta tweak if needed
            try
            {
                if (options != null && options.Kind == InjectKind.GCN)
                {
                    byte[] chars = new byte[4];
                    using (var fstrm = new FileStream(gameIsoWin, FileMode.Open, FileAccess.Read))
                        fstrm.Read(chars, 0, 4);

                    string procod = BAToAscii(chars);
                    string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
                    if (File.Exists(metaXml))
                    {
                        var doc = new XmlDocument();
                        doc.Load(metaXml);
                        var node = doc.SelectSingleNode("menu/reserved_flag2");
                        if (node != null)
                        {
                            node.InnerText = procod.ToHex();
                            doc.Save(metaXml);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Non-fatal? You can decide; for now surface as exception so user sees the problem.
                throw new Exception("GCN meta patch failed: " + ex.Message, ex);
            }

            // Cleanup TempBase
            try
            {
                var tempBaseDir = Path.Combine(tempPath, "TempBase");
                if (Directory.Exists(tempBaseDir))
                    Directory.Delete(tempBaseDir, true);
            }
            catch { /* ignore cleanup errors */ }

            // Replace rvlt.* and copy TIK/TMD into code/
            try
            {
                options?.Progress?.Invoke(50, "Replacing TIK and TMD...");
                var codeDir = Path.Combine(baseRomPath, "code");
                var rvltFiles = Directory.GetFiles(codeDir, "rvlt.*");
                System.Threading.Tasks.Parallel.ForEach(rvltFiles, f => { try { File.Delete(f); } catch { } });

                File.Copy(Path.Combine(tikTmdWin, "tmd.bin"), Path.Combine(codeDir, "rvlt.tmd"), true);
                File.Copy(Path.Combine(tikTmdWin, "ticket.bin"), Path.Combine(codeDir, "rvlt.tik"), true);

                // remove extracted TIKTMD dir if exists
                try { if (Directory.Exists(tikTmdWin)) Directory.Delete(tikTmdWin, true); } catch { }
            }
            catch (Exception ex)
            {
                throw new Exception("Replacing TIK/TMD failed: " + ex.Message, ex);
            }

            // Prepare injection (nfs2iso2nfs)
            try
            {
                options?.Progress?.Invoke(60, "Injecting ROM...");

                // remove any previous .nfs files from content dir
                var oldNfs = Directory.GetFiles(contentDir, "*.nfs");
                System.Threading.Tasks.Parallel.ForEach(oldNfs, f => { try { File.Delete(f); } catch { } });

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

                // Important: pass a Windows-view working directory so RunToolWithFallback uses the correct work dir.
                var contentWinView = ToolRunner.ToWindowsView(contentDir);
                LogIsoSize("[WitNfs] content/game.iso before injection", Path.Combine(contentDir, "game.iso"));
                LogWitSize("[WitNfs] wit size before injection", toolsPath, Path.Combine(contentDir, "game.iso"));
                WaitForWitSizeStability("[WitNfs] wit size stable before injection", toolsPath, Path.Combine(contentDir, "game.iso"));
                if (options?.Kind == InjectKind.WiiStandard)
                {
                    // Verify against original source size if known
                    if (options?.SourceMiB.HasValue == true)
                        ToolRunner.VerifyWitSize(toolsPath, Path.Combine(contentDir, "game.iso"), expectedMiB: options.SourceMiB, toleranceMiB: 5.0);
                    else
                        ToolRunner.VerifyWitSize(toolsPath, Path.Combine(contentDir, "game.iso"));
                }

                runner.RunToolWithFallback(
                    toolBaseName: "nfs2iso2nfs",
                    toolsPathWin: toolsPath,
                    argsWindowsPaths: $"-enc -homebrew {extra}{pass}-iso game.iso",
                    showWindow: debug,
                    workDirWin: contentWinView
                );

                LogNfsOutput(contentDir);
                LogIsoSize("[WitNfs] content/game.iso after injection", Path.Combine(contentDir, "game.iso"));
                LogWitSize("[WitNfs] wit size after injection", toolsPath, Path.Combine(contentDir, "game.iso"));
                WaitForWitSizeStability("[WitNfs] wit size stable after injection", toolsPath, Path.Combine(contentDir, "game.iso"));

                // remove working iso, ensure to use Path.Combine
                var isoToDelete = Path.Combine(contentDir, "game.iso");
                try { if (File.Exists(isoToDelete)) File.Delete(isoToDelete); } catch { /* ignore */ }

                options?.Progress?.Invoke(80, "Injection complete");
            }
            catch (Exception ex)
            {
                throw new Exception("Injection (nfs2iso2nfs) failed: " + ex.Message, ex);
            }
        }

        // Simple wrapper log to central logger if available
        private static void RunnerLog(string msg)
        {
            try { Logger.Log("[WitNfs] " + msg); } catch { Console.WriteLine("[WitNfs] " + msg); }
        }

        private static void LogNfsOutput(string contentDir)
        {
            try
            {
                var files = Directory.GetFiles(contentDir, "*.nfs");
                if (files.Length == 0)
                {
                    RunnerLog("No .nfs files produced.");
                    return;
                }

                foreach (var file in files.OrderBy(f => f))
                {
                    var fi = new FileInfo(file);
                    var wineView = ToolRunner.ToWindowsView(file);
                    var hostView = ToolRunner.WindowsToHostPosix(wineView);
                    RunnerLog($"NFS output: {Path.GetFileName(file)} ({fi.Length:N0} bytes) | wine={wineView} | host={hostView}");
                }
            }
            catch (Exception ex)
            {
                RunnerLog("Unable to log NFS files: " + ex.Message);
            }
        }

        private static void LogIsoSize(string label, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    RunnerLog($"{label}: missing {path}");
                    return;
                }

                var fi = new FileInfo(path);
                var hostView = ToolRunner.WindowsToHostPosix(path);
                RunnerLog($"{label}: {Path.GetFileName(path)} {fi.Length:N0} bytes | wine={path} | host={hostView}");
            }
            catch (Exception ex)
            {
                RunnerLog($"{label}: size log failed ({ex.Message})");
            }
        }

        private static double? LogWitSize(string label, string toolsPathWin, string isoWinPath)
        {
            if (!(ToolRunner.HostIsMac() || ToolRunner.HostIsLinux()))
                return null;

            var (rc, so, se) = RunWitSizeCommand(toolsPathWin, isoWinPath);
            var stdoutText = string.IsNullOrWhiteSpace(so) ? "(none)" : so.Trim();
            var stderrText = string.IsNullOrWhiteSpace(se) ? "(none)" : se.Trim();
            RunnerLog($"{label}: exit={rc} stdout={stdoutText} stderr={stderrText}");
            return ParseWitSize(stdoutText);
        }

        private static (int rc, string stdout, string stderr) RunWitSizeCommand(string toolsPathWin, string isoWinPath)
        {
            var isoHost = ToolRunner.WindowsToHostPosix(isoWinPath);
            var toolName = ToolRunner.HostIsMac() ? "wit-mac" : "wit-linux";
            var toolHost = ToolRunner.WindowsToHostPosix(Path.Combine(toolsPathWin, toolName));
            var cmd = $"[ -f {ToolRunner.Q(isoHost)} ] && {ToolRunner.Q(toolHost)} size {ToolRunner.Q(isoHost)}";
            var rc = ToolRunner.RunHostSh(cmd, out var so, out var se);
            return (rc, so, se);
        }

        private static double? ParseWitSize(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return null;

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || !char.IsDigit(trimmed[0]))
                    continue;
                var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                if (double.TryParse(parts[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
                    return value;
            }

            return null;
        }

        private static void WaitForWitSizeStability(string label, string toolsPathWin, string isoWinPath, int maxAttempts = 6, double tolerance = 0.01)
        {
            if (!(ToolRunner.HostIsMac() || ToolRunner.HostIsLinux()))
                return;

            double? prevSize = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var size = LogWitSize($"{label} (attempt {attempt})", toolsPathWin, isoWinPath);
                if (!size.HasValue && !prevSize.HasValue)
                {
                    RunnerLog($"{label}: wit size missing output; skipping stability wait.");
                    return;
                }

                if (size.HasValue && prevSize.HasValue)
                {
                    var diff = Math.Abs(size.Value - prevSize.Value);
                    if (diff <= Math.Max(prevSize.Value * tolerance, 0.01))
                    {
                        RunnerLog($"{label}: stable at {size.Value:N3} MiB");
                        return;
                    }
                }

                prevSize = size;
                if (attempt < maxAttempts)
                    Thread.Sleep(250);
            }

            RunnerLog($"{label}: stability check ended (last={prevSize?.ToString("N3") ?? "n/a"} MiB)");
        }

        private static string BuildWitAlignArg(string baseRomPath, string tempPath)
        {
            if (string.IsNullOrWhiteSpace(baseRomPath) && string.IsNullOrWhiteSpace(tempPath))
                return string.Empty;

            var candidates = new[]
            {
                Path.Combine(baseRomPath ?? string.Empty, "align-files.txt"),
                Path.Combine(tempPath ?? string.Empty, "align-files.txt")
            };

            foreach (var candidate in candidates.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                if (File.Exists(candidate))
                {
                    RunnerLog($"Using wit align file: {candidate}");
                    return $"--align-files \"{candidate}\"";
                }
            }

            return string.Empty;
        }

    }
}
