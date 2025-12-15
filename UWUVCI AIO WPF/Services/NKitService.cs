using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Thin wrapper around external ConvertToIso/ConvertToNKit tools.
    /// Uses ToolRunner so it works under native Windows and under Wine (macOS/Linux).
    /// </summary>
    public static class NKitService
    {
        private static void CleanOldOutputs(string dir, string baseName)
        {
            try
            {
                if (!Directory.Exists(dir)) return;
                foreach (var f in Directory.GetFiles(dir, baseName + "*", SearchOption.TopDirectoryOnly))
                {
                    try { File.Delete(f); } catch { /* ignore */ }
                }
            }
            catch { /* ignore */ }
        }

        private static void TryMove(string src, string dest)
        {
            try
            {
                if (string.Equals(src, dest, StringComparison.OrdinalIgnoreCase)) return;
                if (File.Exists(dest)) File.Delete(dest);
                File.Move(src, dest);
            }
            catch { /* best-effort */ }
        }

        private static string PickNewest(string[] files)
        {
            try
            {
                if (files == null || files.Length == 0) return null;
                return files
                    .Select(f => new { f, t = File.GetLastWriteTimeUtc(f) })
                    .OrderByDescending(x => x.t)
                    .Select(x => x.f)
                    .FirstOrDefault();
            }
            catch { return null; }
        }
        /// <summary>
        /// Converts a GameCube/Wii image to ISO using the bundled ConvertToIso tool.
        /// Returns the absolute Windows-view path to the produced file (in toolsPath).
        /// </summary>
        public static string ConvertToIso(string toolsPath, string sourcePath, string outputFileName, bool showWindow, IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;
            if (string.IsNullOrWhiteSpace(toolsPath)) throw new ArgumentNullException(nameof(toolsPath));
            if (string.IsNullOrWhiteSpace(sourcePath)) throw new ArgumentNullException(nameof(sourcePath));
            if (string.IsNullOrWhiteSpace(outputFileName)) throw new ArgumentNullException(nameof(outputFileName));

            Directory.CreateDirectory(toolsPath);
            try { Logger.Log($"NKitService.ConvertToIso: src={sourcePath} out={outputFileName} tools={toolsPath}"); } catch { }

            // Build args: tool expects only the source path quoted
            string args = ToolRunner.Q(ToolRunner.SelectPath(sourcePath));

            // Ensure we don't get suffixed outputs by cleaning previous leftovers only in expected dirs
            var outBase = Path.GetFileName(outputFileName);
            CleanOldOutputs(toolsPath, outBase);
            CleanOldOutputs(Directory.GetCurrentDirectory(), outBase);

            // Execute tool via ToolRunner (cross-platform under Wine)
            // Use toolsPath as working directory so bundled INI/data files resolve correctly
            Exception firstError = null;
            try { runner.RunToolWithFallback("ConvertToIso", toolsPath, args, showWindow, toolsPath); }
            catch (Exception ex) { firstError = ex; /* we will check outputs anyway */ }

            // If ToolRunner failed, try legacy shell-execute style but don't fail solely on exit code
            if (firstError != null)
            {
                try
                {
                    var exe = Path.Combine(toolsPath, "ConvertToISO.exe"); // historical casing
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = "\"" + sourcePath + "\"",
                        WindowStyle = showWindow ? System.Diagnostics.ProcessWindowStyle.Normal : System.Diagnostics.ProcessWindowStyle.Hidden,
                        UseShellExecute = true
                    };
                    using (var p = System.Diagnostics.Process.Start(psi)) { p.WaitForExit(); }
                }
                catch (Exception) { /* ignore; we will validate outputs next */ }
            }

            // Produced file appears next to the tool (historical behavior of these helpers)
            string outPath = Path.Combine(toolsPath, outputFileName);
            if (!File.Exists(outPath))
            {
                // Fallback checks for legacy/default work dirs used previously, including NKit's suffix renames
                try
                {
                    // 1) Look for exact or suffixed output under toolsPath
                    var baseName = Path.GetFileName(outputFileName);
                    var candidates = Directory.GetFiles(toolsPath, baseName + "*", SearchOption.TopDirectoryOnly);
                    var picked = PickNewest(candidates);
                    if (!string.IsNullOrEmpty(picked))
                    {
                        TryMove(picked, outPath);
                        if (File.Exists(outPath)) return outPath;
                    }

                    // 2) Current working directory
                    var cwd = Directory.GetCurrentDirectory();
                    candidates = Directory.GetFiles(cwd, baseName + "*", SearchOption.TopDirectoryOnly);
                    picked = PickNewest(candidates);
                    if (!string.IsNullOrEmpty(picked))
                    {
                        TryMove(picked, outPath);
                        if (File.Exists(outPath)) return outPath;
                    }
                }
                catch { /* best-effort */ }

                // If we reach here and had an execution error, append its message to aid debugging
                if (firstError != null) throw new Exception("ISO conversion failed: output file not found: " + outPath + "\n" + firstError.Message);
                throw new Exception("ISO conversion failed: output file not found: " + outPath);
            }

            return outPath;
        }

        /// <summary>
        /// Converts a GameCube/Wii image to NKIT using the bundled ConvertToNKit tool.
        /// Returns the absolute Windows-view path to the produced file (in toolsPath).
        /// </summary>
        public static string ConvertToNKit(string toolsPath, string sourcePath, string outputFileName, bool showWindow, IToolRunnerFacade runner = null)
        {
            runner ??= DefaultToolRunnerFacade.Instance;
            if (string.IsNullOrWhiteSpace(toolsPath)) throw new ArgumentNullException(nameof(toolsPath));
            if (string.IsNullOrWhiteSpace(sourcePath)) throw new ArgumentNullException(nameof(sourcePath));
            if (string.IsNullOrWhiteSpace(outputFileName)) throw new ArgumentNullException(nameof(outputFileName));

            Directory.CreateDirectory(toolsPath);
            try { Logger.Log($"NKitService.ConvertToNKit: src={sourcePath} out={outputFileName} tools={toolsPath}"); } catch { }

            // Clean old outputs
            var outBase2 = Path.GetFileName(outputFileName);
            CleanOldOutputs(toolsPath, outBase2);
            CleanOldOutputs(Directory.GetCurrentDirectory(), outBase2);

            try
            {
                var exe = Path.Combine(toolsPath, "ConvertToNKit.exe");
                var psi = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = "\"" + sourcePath + "\"",
                    WorkingDirectory = toolsPath,            // ✅ force correct working dir
                    WindowStyle = showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                    UseShellExecute = false,                 // ✅ must be false for WorkingDirectory to take effect
                    CreateNoWindow = !showWindow
                };

                using var p = Process.Start(psi);
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                Logger.Log($"[ConvertToNKit] Exception: {ex.Message}");
                // ignore, we’ll check output next
            }

            // Produced file appears next to the tool (now guaranteed)
            string outPath = Path.Combine(toolsPath, outputFileName);
            if (!File.Exists(outPath))
            {
                try
                {
                    // Fallback: look anywhere this tool might have written to
                    var baseName = Path.GetFileName(outputFileName);
                    string[] candidates = Directory.GetFiles(toolsPath, baseName + "*", SearchOption.TopDirectoryOnly);
                    string picked = PickNewest(candidates);
                    if (!string.IsNullOrEmpty(picked))
                    {
                        TryMove(picked, outPath);
                        if (File.Exists(outPath)) return outPath;
                    }

                    var cwd = Directory.GetCurrentDirectory();
                    candidates = Directory.GetFiles(cwd, baseName + "*", SearchOption.TopDirectoryOnly);
                    picked = PickNewest(candidates);
                    if (!string.IsNullOrEmpty(picked))
                    {
                        TryMove(picked, outPath);
                        if (File.Exists(outPath)) return outPath;
                    }
                }
                catch { }

                throw new Exception("NKit conversion failed: output file not found: " + outPath);
            }

            return outPath;
        }

    }
}
