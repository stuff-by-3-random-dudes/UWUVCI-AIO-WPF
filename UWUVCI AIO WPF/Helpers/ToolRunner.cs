using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class ToolRunner
    {
        // -------------------
        // Basic helpers
        // -------------------

        public static string Q(string s) => "\"" + (s ?? "").Replace("\"", "\\\"") + "\"";

        public static bool IsNativeWindows =>
            Environment.OSVersion.Platform == PlatformID.Win32NT ||
            Environment.OSVersion.Platform == PlatformID.Win32Windows;

        public static bool UnderWine() => File.Exists(@"C:\windows\command\start.exe");

        // Host OS detection – via Z: mapping inside Wine
        public static bool HostIsMac() => File.Exists(@"Z:\System\Library\CoreServices\SystemVersion.plist");
        public static bool HostIsLinux() => !HostIsMac() && File.Exists(@"Z:\etc\os-release");

        // -------------------
        // Internal state
        // -------------------

        private static string _wineToolsPath;
        private static string _wineTempPath;

        [ThreadStatic] private static bool _inWinePathConv;
        [ThreadStatic] private static bool _inHostShell;

        private static readonly ConcurrentDictionary<string, bool> _execFixed =
            new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

        public static void Log(string s)
        {
            var line = $"[ToolRunner] {DateTime.Now:HH:mm:ss} {s}";
            Console.WriteLine(line);
            try { Logger.Log(line); } catch { /* ignore */ }
        }

        // -------------------
        // Public path access
        // -------------------

        /// <summary>Windows/Wine tools path (e.g., C:\users\..., Z:\..., etc).</summary>
        public static string WineToolsPath => _wineToolsPath;

        /// <summary>Windows/Wine temp path.</summary>
        public static string WineTempPath => _wineTempPath;

        /// <summary>Host-native tools path, derived from WineToolsPath when under Wine.</summary>
        public static string HostNativeToolsPath => WindowsToHostPosix(_wineToolsPath ?? string.Empty);

        /// <summary>Host-native temp path, derived from WineTempPath when under Wine.</summary>
        public static string HostNativeTempPath => WindowsToHostPosix(_wineTempPath ?? string.Empty);

        /// <summary>
        /// Simple central initializer for Tools/Temp paths.  
        /// Stores them in Windows form and ensures directories exist.
        /// Called from JsonSettingsManager.LoadSettings.
        /// </summary>
        public static void InitializePaths(string toolsPath, string tempPath)
        {
            if (string.IsNullOrWhiteSpace(toolsPath) || string.IsNullOrWhiteSpace(tempPath))
                return;

            // Normalize and store as Windows-style (for exe tools)
            toolsPath = Path.GetFullPath(toolsPath);
            tempPath = Path.GetFullPath(tempPath);

            Directory.CreateDirectory(toolsPath);
            Directory.CreateDirectory(tempPath);

            _wineToolsPath = toolsPath;
            _wineTempPath = tempPath;

            Log($"Init: Tools={_wineToolsPath}, Temp={_wineTempPath}");
        }

        /// <summary>
        /// For KegWorks we just use the temp path from settings as the base user dir.
        /// </summary>
        public static string GetUserUWUVCIDir() => JsonSettingsManager.Settings.TempPath;

        // -------------------
        // Path conversion
        // -------------------

        public static string PosixFromWindows(string p)
        {
            if (string.IsNullOrEmpty(p)) return string.Empty;

            // Z:\ → /
            if (p.StartsWith(@"Z:\", StringComparison.OrdinalIgnoreCase))
                return p.Replace(@"Z:\", "/").Replace('\\', '/');

            // C:\users\<name>\... → /Users/<name>/... (mac) or /home/<name>/... (linux)
            if (p.Length > 9 &&
                char.ToUpperInvariant(p[0]) == 'C' &&
                p[1] == ':' &&
                (p[2] == '\\' || p[2] == '/') &&
                p.Substring(3).StartsWith("users", StringComparison.OrdinalIgnoreCase))
            {
                var rest = p.Substring(3).TrimStart('\\', '/');
                var parts = rest.Split('\\', '/');
                if (parts.Length >= 2)
                {
                    var user = parts[1];
                    var tail = string.Join("/", parts.Skip(2));
                    if (HostIsMac())
                        return "/Users/" + user + (tail.Length > 0 ? "/" + tail : "");
                    if (HostIsLinux())
                        return "/home/" + user + (tail.Length > 0 ? "/" + tail : "");
                }
            }

            // Fallback: /c/Users/... style
            if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                return "/" + p.Substring(2).TrimStart('\\', '/').Replace('\\', '/');

            return p.Replace('\\', '/');
        }

        /// <summary>
        /// Convert a Windows path in the Wine prefix to host POSIX, using winepath when possible.
        /// </summary>
        public static string WindowsToHostPosix(string winPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(winPath))
                    return string.Empty;

                if (_inWinePathConv)
                    return PosixFromWindows(winPath);

                _inWinePathConv = true;

                // Z:\ is usually a direct host mapping
                if (winPath.StartsWith(@"Z:\", StringComparison.OrdinalIgnoreCase))
                    return winPath.Replace(@"Z:\", "/").Replace('\\', '/');

                if (UnderWine())
                {
                    int rc = RunWinExe("winepath.exe", "-u " + Q(winPath),
                                       Environment.CurrentDirectory, false, out var so, out _);
                    if (rc == 0 && !string.IsNullOrWhiteSpace(so))
                        return so.Trim();
                }
            }
            catch
            {
                // ignore, fall back
            }
            finally
            {
                _inWinePathConv = false;
            }

            return PosixFromWindows(winPath);
        }

        private static string NormalizeDosDevicesPosix(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return path ?? string.Empty;
                int i = path.IndexOf("/dosdevices/", StringComparison.OrdinalIgnoreCase);
                if (i >= 0)
                {
                    var rest = path.Substring(i + "/dosdevices/".Length);
                    if (rest.Length >= 2 && char.IsLetter(rest[0]) && rest[1] == ':')
                    {
                        char drive = char.ToLowerInvariant(rest[0]);
                        string tail = rest.Substring(2);
                        if (drive == 'z')
                            return path.Substring(0, i) + "/" + tail.TrimStart('/');
                        return path.Substring(0, i) + "/drive_" + drive + tail;
                    }
                }
            }
            catch { }
            return path;
        }

        /// <summary>
        /// Ensure a Windows-view path: C:\..., Z:\... etc.  
        /// POSIX paths are mapped to Z:\.
        /// </summary>
        public static string ToWindowsView(string p)
        {
            if (string.IsNullOrEmpty(p)) return p ?? string.Empty;

            // Already drive-based
            if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                return p.Replace('/', '\\');

            // POSIX → Z:\...
            if (p[0] == '/')
                return @"Z:\" + p.TrimStart('/').Replace('/', '\\');

            return p.Replace('/', '\\');
        }

        public static string JoinWin(string a, string b) => ToWindowsView(Path.Combine(a, b));

        // -------------------
        // Wine / host shell
        // -------------------

        private static string StartExe()
        {
            var sys = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(sys)) sys = @"C:\windows";
            return Path.Combine(sys, "command", "start.exe");
        }

        public static int RunHostSh(string shCommand, out string so, out string se)
        {
            if (_inHostShell)
            {
                so = se = "";
                return 1;
            }

            _inHostShell = true;
            try
            {
                // Simple temp under current prefix
                string winTempDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"bin\temp"));
                Directory.CreateDirectory(winTempDir);

                string baseDir = NormalizeDosDevicesPosix(WindowsToHostPosix(winTempDir));
                if (string.IsNullOrWhiteSpace(baseDir))
                    baseDir = PosixFromWindows(winTempDir);

                string guid = Guid.NewGuid().ToString("N");
                string outF = $"{baseDir}/tr_{guid}.out";
                string errF = $"{baseDir}/tr_{guid}.err";
                string rcF = $"{baseDir}/tr_{guid}.rc";

                string wrapped =
                    "set -e; " +
                    $"( {shCommand} ) >{Q(outF)} 2>{Q(errF)}; " +
                    "rc=$?; echo $rc > " + Q(rcF) + "; true";

                Log($"RunHostSh: /bin/sh -lc {Q(wrapped)}");

                so = se = "";
                var psi = new ProcessStartInfo
                {
                    FileName = StartExe(),
                    Arguments = "/unix /bin/sh -lc " + Q(wrapped),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p.WaitForExit();

                // Read back outputs from host
                int rc = 0;
                RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"[ -f {Q(outF)} ] && cat {Q(outF)}")}", "/", false, out so, out _);
                RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"[ -f {Q(errF)} ] && cat {Q(errF)}")}", "/", false, out se, out _);
                string rcStr;
                RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"[ -f {Q(rcF)} ] && cat {Q(rcF)} || echo 127")}", "/", false, out rcStr, out _);
                int.TryParse(rcStr?.Trim(), out rc);

                Log($"RunHostSh rc={rc}");
                // Best-effort cleanup
                RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"rm -f {Q(outF)} {Q(errF)} {Q(rcF)}")}", "/", false, out _, out _);

                return rc;
            }
            finally
            {
                _inHostShell = false;
            }
        }

        private static bool IsHostMacUnderWine()
        {
            if (HostIsMac()) return true;
            if (!UnderWine()) return false;
            try
            {
                int rc = RunHostSh("uname -s", out var so, out _);
                return rc == 0 && !string.IsNullOrWhiteSpace(so) &&
                       so.Trim().StartsWith("Darwin", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsHostLinuxUnderWine()
        {
            if (HostIsLinux()) return true;
            if (!UnderWine()) return false;
            try
            {
                int rc = RunHostSh("uname -s", out var so, out _);
                return rc == 0 && !string.IsNullOrWhiteSpace(so) &&
                       so.Trim().StartsWith("Linux", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // -------------------
        // Process runners
        // -------------------

        public static int RunWinExe(string exeWin, string args, string workWin, bool show, out string so, out string se)
        {
            exeWin = ToWindowsView(exeWin);
            workWin = ToWindowsView(workWin);

            so = se = "";
            var psi = new ProcessStartInfo
            {
                FileName = exeWin,
                Arguments = args ?? string.Empty,
                WorkingDirectory = workWin,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = !show,
                WindowStyle = show ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
            };

            using var p = Process.Start(psi);
            so = p.StandardOutput.ReadToEnd();
            se = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string hint = ComposeWinErrorHint(p.ExitCode, exeWin);
                if (!string.IsNullOrEmpty(hint))
                    se = (se ?? string.Empty) + (string.IsNullOrEmpty(se) ? "" : "\n") + hint;
            }

            Log($"RunWinExe exit={p.ExitCode} exe={exeWin}");
            return p.ExitCode;
        }

        private static string ComposeWinErrorHint(int exitCode, string exe)
        {
            try
            {
                if (exitCode == -1073741502)
                {
                    return $"Hint: {Path.GetFileName(exe)} failed to initialize (0xC0000142). " +
                           "This usually indicates missing Visual C++ dependencies.";
                }
                if (exitCode == -1073741515)
                {
                    return $"Hint: {Path.GetFileName(exe)} could not find a required DLL (0xC0000135).";
                }
                if (exitCode == -1073741701)
                {
                    return $"Hint: {Path.GetFileName(exe)} failed to start due to a 32/64-bit mismatch (0xC000007B).";
                }
            }
            catch { }
            return string.Empty;
        }

        private static void EnsureExecBitOnce(string toolPosix, bool isMac)
        {
            if (_execFixed.ContainsKey(toolPosix)) return;

            var cmd = new StringBuilder()
                .Append("set -e; chmod +x ").Append(Q(toolPosix)).Append("; ");

            if (isMac)
            {
                cmd.Append("B=").Append(Q(toolPosix)).Append("; ")
                   .Append("while [ \"$B\" != / -a \"${B##*/}\" != \"Contents\" ]; do B=\"${B%/*}\"; done; ")
                   .Append("if [ \"${B##*/}\" = Contents ]; then APP=\"${B%/*}\"; else APP=$(dirname ").Append(Q(toolPosix)).Append("); fi; ")
                   .Append("command -v xattr >/dev/null 2>&1 && xattr -dr com.apple.quarantine \"$APP\" >/dev/null 2>&1 || true; ");
            }

            int rc = RunHostSh(cmd.ToString(), out _, out var se);
            if (rc != 0) throw new Exception("Failed to set exec bit / clear quarantine.\n" + se);

            _execFixed[toolPosix] = true;
        }

        private static int RunNative(string exePosix, string args, bool mac, out string so, out string se)
        {
            EnsureExecBitOnce(exePosix, mac);
            return RunHostSh(Q(exePosix) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args), out so, out se);
        }

        // -------------------
        // Native tool decision
        // -------------------

        /// <summary>
        /// Only these are host-native tools on mac/Linux; everything else uses Windows exe.
        /// </summary>
        public static bool IsHostNativeTool(string toolBaseName)
        {
            if (string.IsNullOrEmpty(toolBaseName)) return false;
            toolBaseName = toolBaseName.ToLowerInvariant();
            return toolBaseName is "wit" or "wstrt";
        }

        // -------------------
        // Main public APIs
        // -------------------

        /// <summary>
        /// Runs "wstrt patch &lt;mainDol&gt; --add-sec &lt;gct&gt; ..." using the correct mode.
        /// </summary>
        public static void RunWstrtPatch(string toolsPathWin, string mainDolPathWin, string[] gctFilesWin, bool showWindow, string workDirWin = null)
        {
            var args = "patch " + Q(SelectPath(mainDolPathWin)) + ConcatSections(gctFilesWin);
            RunTool("wstrt", toolsPathWin, args, showWindow, workDirWin);
        }

        /// <summary>
        /// Core tool runner.  
        /// Windows: always exe.  
        /// mac/Linux under Wine: wit/wstrt use native binaries if present; everything else uses exe with Windows paths.
        /// </summary>
        public static void RunTool(
            string toolBaseName,
            string toolsPathWin,
            string argsWindowsPaths,
            bool showWindow,
            string workDirWin = null)
        {
            bool underWine = UnderWine();
            bool hostMac = IsHostMacUnderWine();
            bool hostLinux = IsHostLinuxUnderWine();
            bool nativeWindows = IsNativeWindows && !underWine;

            // Choose base tools path
            if (string.IsNullOrWhiteSpace(toolsPathWin))
                toolsPathWin = _wineToolsPath ?? Directory.GetCurrentDirectory();

            // Choose working dir
            if (string.IsNullOrWhiteSpace(workDirWin))
                workDirWin = toolsPathWin;

            Log($"RunTool: tool={toolBaseName}, toolsPath={toolsPathWin}, workDir={workDirWin}");

            // Case 1: Pure Windows → always exe
            if (nativeWindows)
            {
                string exeWin = Path.Combine(toolsPathWin, toolBaseName + ".exe");
                Directory.CreateDirectory(workDirWin);
                string finalArgs = ReplaceArgsWithWindowsFlavor(argsWindowsPaths);
                int rc = RunWinExe(exeWin, finalArgs, workDirWin, showWindow, out _, out var se);
                if (rc != 0)
                    throw new Exception($"{toolBaseName}.exe failed (exit {rc}).\n{se}");
                return;
            }

            // Case 2: mac/Linux under Wine → native wit/wstrt, else exe
            if (underWine && (hostMac || hostLinux) && IsHostNativeTool(toolBaseName))
            {
                string toolsPosix = WindowsToHostPosix(toolsPathWin).TrimEnd('/');
                string exePosix = toolsPosix + "/" + toolBaseName + (hostMac ? "-mac" : "-linux");
                string argsPosix = ConvertArgsToPosix(argsWindowsPaths);

                exePosix = NormalizeDosDevicesPosix(exePosix);

                if (!HostFileExistsPosix(exePosix))
                    throw new Exception($"Native {toolBaseName} not found at: {exePosix}");

                int rc = RunNative(exePosix, argsPosix, hostMac, out var so, out var se);
                Log($"{toolBaseName} (native) exit={rc}");
                if (rc != 0)
                    throw new Exception($"{toolBaseName} (native) failed (exit {rc}).\n{se}");
                return;
            }

            // Case 3: Everything else → Windows exe
            {
                string exeWinPath = Path.Combine(toolsPathWin, toolBaseName + ".exe");
                Directory.CreateDirectory(workDirWin);
                string finalArgs2 = ReplaceArgsWithWindowsFlavor(argsWindowsPaths);
                int rc2 = RunWinExe(exeWinPath, finalArgs2, workDirWin, showWindow, out var so2, out var se2);
                Log($"{toolBaseName} exit={rc2}");
                if (rc2 != 0)
                    throw new Exception($"{toolBaseName}.exe failed (exit {rc2}).\n{se2}");
            }
        }

        /// <summary>
        /// Fallback variant – currently just forwards to RunTool.  
        /// (Kept for API compatibility; if you ever want native→exe fallback you can extend here.)
        /// </summary>
        public static void RunToolWithFallback(
            string toolBaseName,
            string toolsPathWin,
            string argsWindowsPaths,
            bool showWindow,
            string workDirWin = null)
        {
            RunTool(toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin);
        }

        // -------------------
        // Arg helpers
        // -------------------

        public static string ConcatSections(string[] gctFilesWin)
        {
            if (gctFilesWin == null || gctFilesWin.Length == 0) return "";
            var sb = new StringBuilder();
            foreach (var g in gctFilesWin)
                sb.Append(" --add-sect ").Append(Q(SelectPath(g)));
            return sb.ToString();
        }

        /// <summary>
        /// For your call sites, you normally pass Windows-view paths; this keeps them canonical.
        /// </summary>
        public static string SelectPath(string pWinOrPosix)
        {
            if (string.IsNullOrWhiteSpace(pWinOrPosix))
                return "";

            bool nativeHost = (HostIsMac() || HostIsLinux()) && !UnderWine();
            if (nativeHost)
                return WindowsToHostPosix(pWinOrPosix);

            return ToWindowsView(pWinOrPosix);
        }

        /// <summary>
        /// Convert quoted POSIX paths to Windows Z:\ form, keeping existing Windows paths intact.
        /// </summary>
        public static string ReplaceArgsWithWindowsFlavor(string args)
        {
            if (string.IsNullOrEmpty(args)) return string.Empty;

            var parts = args.Split('"');
            for (int i = 1; i < parts.Length; i += 2)
            {
                var seg = parts[i];
                if (string.IsNullOrWhiteSpace(seg)) continue;

                // Already Windows path?
                if (seg.Length > 2 && char.IsLetter(seg[0]) && seg[1] == ':' && (seg[2] == '/' || seg[2] == '\\'))
                {
                    parts[i] = seg.Replace('/', '\\');
                    continue;
                }

                if (seg.StartsWith("/"))
                    parts[i] = @"Z:\" + seg.TrimStart('/').Replace('/', '\\');
                else
                    parts[i] = seg.Replace('/', '\\');
            }
            return string.Join("\"", parts);
        }

        /// <summary>
        /// Turns quoted C:\ or Z:\ segments into host POSIX for native tools (wit/wstrt).
        /// </summary>
        public static string ConvertArgsToPosix(string args)
        {
            if (string.IsNullOrEmpty(args)) return "";

            var parts = args.Split('"');
            for (int i = 1; i < parts.Length; i += 2)
            {
                var seg = parts[i];
                if (string.IsNullOrWhiteSpace(seg)) continue;

                string p = WindowsToHostPosix(seg);
                p = NormalizeDosDevicesPosix(p);
                parts[i] = p;
            }
            return string.Join("\"", parts);
        }

        private static bool HostFileExistsPosix(string posixPath)
        {
            if (string.IsNullOrWhiteSpace(posixPath)) return false;
            try
            {
                int rc = RunHostSh($"[ -e {Q(posixPath)} ]", out _, out _);
                return rc == 0;
            }
            catch
            {
                return false;
            }
        }

        // -------------------
        // Visibility / probing
        // -------------------

        public static void LogFileVisibility(string label, string winPath)
        {
            var (wineOk, wineBytes, hostOk, hostBytes, posix) = ProbeFile(winPath, true);
            var sb = new StringBuilder();
            sb.Append(label).Append(" vis: ");
            sb.Append("wine=").Append(wineOk ? $"exists,{wineBytes}B" : "missing");
            sb.Append(" | host=").Append(hostOk ? $"exists,{hostBytes}B" : "missing");
            sb.Append(" | posix=").Append(posix);
            Log(sb.ToString());
        }

        public static (bool wineOk, long wineBytes, bool hostOk, long hostBytes, string posixPath)
            ProbeFile(string winPath, bool typeFile)
        {
            long wineBytes = 0;
            bool wineOk = false;
            string flag = typeFile ? " -f " : " -d ";

            try
            {
                if (File.Exists(winPath))
                {
                    var fi = new FileInfo(winPath);
                    wineBytes = fi.Length;
                    wineOk = true;
                }
            }
            catch { }

            string posix = NormalizeDosDevicesPosix(WindowsToHostPosix(winPath));
            long hostBytes = 0;
            bool hostOk = false;

            bool hostMac = IsHostMacUnderWine();
            var statCmd = hostMac
                ? $"[ {flag} {Q(posix)} ] && stat -f%z {Q(posix)} || echo MISS"
                : $"[ {flag} {Q(posix)} ] && stat -c%s {Q(posix)} || echo MISS";

            try
            {
                int rc = RunHostSh(statCmd, out var so, out _);
                if (rc == 0 && so != null && so.Trim() != "MISS" && long.TryParse(so.Trim(), out var b))
                {
                    hostOk = true;
                    hostBytes = b;
                }
            }
            catch { }

            return (wineOk, wineBytes, hostOk, hostBytes, posix);
        }

        /// <summary>
        /// Wait until a file or directory is visible (non-zero size for files) under Wine/host.
        /// Under Wine we allow a longer timeout (up to several minutes) and log if we time out.
        /// </summary>
        public static bool WaitForWineVisibility(string winPath, bool file = true)
        {
            try { if (!UnderWine()) return true; } catch { }

            var pollMs = JsonSettingsManager.Settings.UnixWaitDelayMs;
            // Scale up timeout for Wine; Wii ISOs can take a while to appear in Wine's view.
            var timeoutMs = Math.Max(pollMs * 10, 180000); // minimum 3 minutes

            var sw = Stopwatch.StartNew();
            string lastNote = "";

            System.Threading.Thread.Sleep(pollMs);

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (file && File.Exists(winPath))
                    {
                        long size = new FileInfo(winPath).Length;
                        if (size > 0)
                        {
                            Log($"I/O fence: visible to Wine/.NET: {winPath} ({size} bytes)");
                            return true;
                        }
                    }
                    else if (!file && Directory.Exists(winPath))
                    {
                        Log($"I/O fence: directory visible to Wine/.NET: {winPath}");
                        return true;
                    }
                }
                catch { }

                var (wineOk, wineBytes, hostOk, hostBytes, _) = ProbeFile(winPath, file);
                var note = $"wine:{(wineOk ? wineBytes.ToString() : "-")} host:{(hostOk ? hostBytes.ToString() : "-")}";
                if (note != lastNote)
                {
                    Log($"I/O fence probe -> {note}");
                    lastNote = note;
                }

                if (hostOk && (!file || hostBytes > 0))
                {
                    Log($"I/O fence: visible on host, bytes={hostBytes}");
                    return true;
                }

                System.Threading.Thread.Sleep(pollMs);
            }

            Log($"I/O fence TIMEOUT: Wine can't see {winPath} after {sw.ElapsedMilliseconds}ms");
            // Soft-fail: allow caller to continue, they can still rely on size checks / fallbacks
            return true;
        }

        public static void DeleteDirectorySafe(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return;
            }
            catch { }

            // Fallback with \\?\ prefix to handle trailing dots or other oddities
            try
            {
                if (Directory.Exists(path))
                {
                    var prefixPath = path.StartsWith(@"\\?\") ? path : @"\\?\" + path.TrimStart('\\');
                    Directory.Delete(prefixPath, true);
                }
            }
            catch { }
        }

        /// <summary>
        /// Polls a file size until it stops changing (useful under Wine where native tools finish before Wine/.NET see final bytes).
        /// </summary>
        public static void WaitForStableFileSize(string winPath, int maxAttempts = 10, int delayMs = 500, long toleranceBytes = 1024)
        {
            if (string.IsNullOrWhiteSpace(winPath))
                return;

            long? last = null;
            int stableCount = 0;

            for (int i = 0; i < maxAttempts; i++)
            {
                long size = 0;
                try { if (File.Exists(winPath)) size = new FileInfo(winPath).Length; } catch { }

                if (size > 0 && last.HasValue && Math.Abs(size - last.Value) <= toleranceBytes)
                {
                    stableCount++;
                    if (stableCount >= 2) // two consecutive stable reads
                        return;
                }
                else
                {
                    stableCount = 0;
                }

                last = size > 0 ? size : last;
                Thread.Sleep(delayMs);
            }
        }

        /// <summary>
        /// Runs "wit size" on the given ISO/path and returns the reported MiB value (or null on failure).
        /// Uses host-native wit under Wine/mac/Linux; wit.exe on Windows.
        /// </summary>
        public static double? GetWitSizeMiB(string toolsPathWin, string isoWinPath)
        {
            if (string.IsNullOrWhiteSpace(toolsPathWin) || string.IsNullOrWhiteSpace(isoWinPath))
                return null;

            try
            {
                // Always prefer the native Windows path when we are actually running on Windows.
                // Some Wine detection heuristics (e.g., presence of start.exe) can be true on real Windows,
                // which would wrongly send us down the host (posix) code path and make verification fail.
                bool isNativeWindows = IsNativeWindows && !HostIsMac() && !HostIsLinux();
                bool nativeHost = HostIsMac() || HostIsLinux();
                bool underWine = UnderWine();

                // Choose binary and command
                if (!isNativeWindows && (nativeHost || underWine))
                {
                    // host-native wit (mac/linux) path and args
                    string toolName = HostIsMac() ? "wit-mac" : "wit-linux";
                    string toolHost = WindowsToHostPosix(Path.Combine(toolsPathWin, toolName));
                    string isoHost = WindowsToHostPosix(isoWinPath);
                    if (string.IsNullOrWhiteSpace(toolHost) || string.IsNullOrWhiteSpace(isoHost))
                        return FallbackFileSizeMiB(isoWinPath);

                    var cmd = $"[ -f {Q(isoHost)} ] && {Q(toolHost)} size {Q(isoHost)}";
                    int rc = RunHostSh(cmd, out var so, out _);
                    if (rc == 0 && !string.IsNullOrWhiteSpace(so))
                        return ParseWitSize(so);

                    // Fallback: force stdout into a temp file and read it
                    var tmpText = RunWitSizeToTempFile(toolHost, isoHost);
                    if (!string.IsNullOrWhiteSpace(tmpText))
                    {
                        var parsed = ParseWitSize(tmpText);
                        if (parsed.HasValue)
                            return parsed;
                    }

                    return FallbackFileSizeMiB(isoWinPath);
                }
                else
                {
                    // Native Windows wit.exe
                    string exeWin = Path.Combine(toolsPathWin, "wit.exe");
                    int rc = RunWinExe(exeWin, $"size \"{isoWinPath}\"", toolsPathWin, false, out var so, out _);
                    if (rc != 0 || string.IsNullOrWhiteSpace(so))
                        return FallbackFileSizeMiB(isoWinPath);
                    return ParseWitSize(so);
                }
            }
            catch
            {
                return null;
            }
        }

        // Best-effort fallback when wit size output is missing: use file length if visible.
        private static double? FallbackFileSizeMiB(string isoWinPath)
        {
            try
            {
                if (File.Exists(isoWinPath))
                    return new FileInfo(isoWinPath).Length / (1024.0 * 1024.0);
            }
            catch { }
            return null;
        }

        // Run wit size and redirect output into a temp file (host side), then read it.
        private static string RunWitSizeToTempFile(string toolHost, string isoHost)
        {
            try
            {
                string tmpWin = Path.Combine(Path.GetTempPath(), $"wit_size_{Guid.NewGuid():N}.txt");
                string tmpPosix = WindowsToHostPosix(tmpWin);
                if (string.IsNullOrWhiteSpace(tmpPosix))
                    return null;

                var cmd = $"{Q(toolHost)} size {Q(isoHost)} > {Q(tmpPosix)} 2>&1";
                int rc = RunHostSh(cmd, out _, out _);
                if (rc != 0)
                    return null;

                string text = File.Exists(tmpWin) ? File.ReadAllText(tmpWin) : null;
                try { if (File.Exists(tmpWin)) File.Delete(tmpWin); } catch { }
                return text;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Verify that "wit size" succeeds and (optionally) matches an expected size within tolerance.
        /// Throws if size cannot be read or if it differs beyond tolerance.
        /// </summary>
        public static void VerifyWitSize(string toolsPathWin, string isoWinPath, double? expectedMiB = null, double toleranceMiB = 1.0)
        {
            var size = GetWitSizeMiB(toolsPathWin, isoWinPath);
            if (!size.HasValue || size.Value <= 0)
                throw new InvalidDataException($"wit size failed or returned zero for {isoWinPath}");

            if (expectedMiB.HasValue)
            {
                var diff = Math.Abs(size.Value - expectedMiB.Value);
                if (diff > toleranceMiB)
                    throw new InvalidDataException($"wit size mismatch: got {size.Value:N1} MiB, expected ~{expectedMiB.Value:N1} MiB");
            }
        }

        // Extracts the leading MiB value from "wit size" output.
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

        // -------------------
        // OpenOnHost
        // -------------------

        public static bool OpenOnHost(string target)
        {
            try
            {
                bool native = UnderWine() && (IsHostMacUnderWine() || IsHostLinuxUnderWine());
                if (!native)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });
                    return true;
                }

                bool isUrl = Uri.TryCreate(target, UriKind.Absolute, out var uri) &&
                             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "mailto");

                string toOpen = target;
                if (!isUrl)
                {
                    string converted = WindowsToHostPosix(target);
                    if (string.IsNullOrWhiteSpace(converted))
                        converted = PosixFromWindows(target);
                    toOpen = NormalizeDosDevicesPosix(converted);
                }

                bool hostMac = IsHostMacUnderWine();
                string opener = hostMac ? "open" : "xdg-open";

                int rc = RunHostSh($"command -v {opener} >/dev/null 2>&1 && {opener} {Q(toOpen)} >/dev/null 2>&1", out _, out _);
                return rc == 0;
            }
            catch
            {
                return false;
            }
        }

        // -------------------
        // KegWorks detection (lightweight)
        // -------------------

        public static bool RunningInsideKegworksApp()
        {
            try
            {
                if (!UnderWine()) return false;

                var hostCwd = WindowsToHostPosix(Environment.CurrentDirectory);
                if (!string.IsNullOrEmpty(hostCwd) &&
                    hostCwd.IndexOf("/Contents/SharedSupport/prefix/", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch { }

            return false;
        }
    }
}
