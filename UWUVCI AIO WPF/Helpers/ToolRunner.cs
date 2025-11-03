using System.Linq;

namespace UWUVCI_AIO_WPF.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public static class ToolRunner
    {
        // -------- environment + path helpers --------
        public static string Q(string s) => "\"" + (s ?? "").Replace("\"", "\\\"") + "\"";

        public static bool IsNativeWindows =>
            Environment.OSVersion.Platform == PlatformID.Win32NT ||
            Environment.OSVersion.Platform == PlatformID.Win32Windows;

        public static bool HostIsMac() => File.Exists(@"Z:\System\Library\CoreServices\SystemVersion.plist");
        public static bool HostIsLinux() => !HostIsMac() && File.Exists(@"Z:\etc\os-release");
        public static bool UnderWine() => File.Exists(@"C:\windows\command\start.exe");

        public static string PosixFromWindows(string p)
        {
            if (string.IsNullOrEmpty(p)) return "";

            // If it’s already a Z:\ path, we can safely map to root (/)
            if (p.StartsWith(@"Z:\", StringComparison.OrdinalIgnoreCase))
                return p.Replace(@"Z:\", "/").Replace('\\', '/');

            // Handle C:\users\<name>\... specially:
            // - macOS: /Users/<name>/...
            // - Linux: /home/<name>/...
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

            // Fallback: /c/Users/... style—rarely right for native tools, but keep as last resort
            if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                return "/" + p.Substring(2).TrimStart('\\', '/').Replace('\\', '/');

            return p.Replace('\\', '/');
        }


        private static string StartExe()
        {
            var sys = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(sys)) sys = @"C:\windows";
            return Path.Combine(sys, "command", "start.exe"); // within current Wine prefix
        }

        // -------- permissions (only once per native tool) --------
        public static readonly ConcurrentDictionary<string, bool> _execFixed = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

        private static void Log(string s)
        {
            var line = $"[ToolRunner] {DateTime.Now:HH:mm:ss} {s}";
            Console.WriteLine(line);
            try { Logger.Log(line); } catch { /* ignore */ }
        }

        private static void EnsureExecBitOnce(string toolPosix, bool isMac)
        {
            if (_execFixed.ContainsKey(toolPosix)) return;

            var cmd = new StringBuilder()
                .Append("set -e; chmod +x ").Append(Q(toolPosix)).Append("; ");

            if (isMac)
            {
                // Clear quarantine on the .app containing the tool (idempotent)
                cmd.Append("B=").Append(Q(toolPosix)).Append("; ")
                   .Append("while [ \"$B\" != / -a \"${B##*/}\" != \"Contents\" ]; do B=\"${B%/*}\"; done; ")
                   .Append("if [ \"${B##*/}\" = Contents ]; then APP=\"${B%/*}\"; else APP=$(dirname ").Append(Q(toolPosix)).Append("); fi; ")
                   .Append("command -v xattr >/dev/null 2>&1 && xattr -dr com.apple.quarantine \"$APP\" >/dev/null 2>&1 || true; ");
            }

            int rc = RunHostSh(cmd.ToString(), out _, out var se);
            if (rc != 0) throw new Exception("Failed to set exec bit / clear quarantine.\n" + se);

            _execFixed[toolPosix] = true;
        }

        // -------- process launchers --------
        public static int RunHostSh(string shCommand, out string so, out string se)
        {
            // Use unique temp files under the same prefix so permissions match the bundle/prefix.
            string baseDir = PosixFromWindows(Path.Combine(Environment.CurrentDirectory, "bin", "temp"));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"bin\temp")));
            string guid = Guid.NewGuid().ToString("N");
            string outF = $"{baseDir}/tr_{guid}.out";
            string errF = $"{baseDir}/tr_{guid}.err";
            string rcF = $"{baseDir}/tr_{guid}.rc";

            // We run the user's command in a subshell and capture stdout/stderr + rc to files.
            // Then we 'cat' them back on the Wine side (since start.exe doesn't pipe through well).
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
            // Output from start.exe is usually empty; just wait for completion.
            p.WaitForExit();

            // Now read back what the *native* process wrote
            int rc = 0;
            RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"[ -f {Q(outF)} ] && cat {Q(outF)}")}", "/", false, out so, out _);
            RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"[ -f {Q(errF)} ] && cat {Q(errF)}")}", "/", false, out se, out _);
            string rcStr;
            RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"[ -f {Q(rcF)} ] && cat {Q(rcF)} || echo 127")}", "/", false, out rcStr, out _);
            int.TryParse(rcStr?.Trim(), out rc);

            Log($"RunHostSh rc={rc}\nSTDOUT:\n{so}\nSTDERR:\n{se}");

            // Best-effort cleanup (ignore failures)
            RunWinExe(StartExe(), $"/unix /bin/sh -lc {Q($"rm -f {Q(outF)} {Q(errF)} {Q(rcF)}")}", "/", false, out _, out _);

            return rc;
        }

        // Try native tool if present; otherwise run the Windows .exe (works on Windows and under Wine).
        public static void RunToolWithFallback(
            string toolBaseName,
            string toolsPathWin,
            string argsWindowsPaths,
            bool showWindow,
            string workDirWin = @"C:\uwu")
        {
            bool underWine = UnderWine();
            bool mac = underWine && HostIsMac();
            bool lin = underWine && !mac && HostIsLinux();

            // If native is possible, only use it if the native binary actually exists.
            if (mac || lin)
            {
                string toolsPosix = PosixFromWindows(toolsPathWin).TrimEnd('/');
                string exePosix = toolsPosix + "/" + toolBaseName + (mac ? "-mac" : "-linux");
                string argsPosix = ConvertArgsToPosix(argsWindowsPaths);
                if (File.Exists(exePosix))
                {
                    Log($"Mode={(mac ? "mac" : "linux")} native (fallback-capable)\nexe={exePosix}\nargs={argsPosix}");
                    int rc = RunNative(exePosix, argsPosix, mac, out var so, out var se);
                    Log($"{toolBaseName} exit={rc}\nSTDOUT:\n{so}\nSTDERR:\n{se}");
                    if (rc != 0) throw new Exception($"{toolBaseName} (native) failed (exit {rc}).\n{se}");
                    return;
                }
                else
                {
                    Log($"Native binary not found: {exePosix} — falling back to Windows .exe");
                }
            }

            // Windows native OR Wine fallback to .exe
            string exeWin = Path.Combine(toolsPathWin, toolBaseName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? toolBaseName
                : toolBaseName + ".exe");

            Directory.CreateDirectory(workDirWin);
            var finalArgs = ReplaceArgsWithWindowsFlavor(argsWindowsPaths);
            Log($"Mode={(IsNativeWindows ? "Windows" : "Wine")}.exe\nexe={exeWin}\nargs={finalArgs}");
            int rc2 = RunWinExe(exeWin, finalArgs, workDirWin, showWindow, out var so2, out var se2);
            if (rc2 != 0) throw new Exception($"{Path.GetFileName(exeWin)} failed (exit {rc2}).\n{se2}");
        }

        public static int RunWinExe(string exeWin, string args, string workWin, bool show, out string so, out string se)
        {
            Log($"RunWinExe: exe={exeWin}\nargs={args}\nworkdir={workWin}");
            so = se = "";
            var psi = new ProcessStartInfo
            {
                FileName = exeWin,
                Arguments = args,
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
            Log($"RunWinExe exit={p.ExitCode}\nSTDOUT:\n{so}\nSTDERR:\n{se}");
            return p.ExitCode;
        }


        private static int RunNative(string exePosix, string args, bool mac, out string so, out string se)
        {
            EnsureExecBitOnce(exePosix, mac);
            return RunHostSh(Q(exePosix) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args), out so, out se);
        }

        // -------- public API --------

        // Runs "wstrt patch <mainDol> --add-sec <gct> ..."
        public static void RunWstrtPatch(string toolsPathWin, string mainDolPathWin, string[] gctFilesWin, bool showWindow, string workDirWin = @"C:\temp")
        {
            var args = "patch " + Q(SelectPath(mainDolPathWin)) + ConcatSections(gctFilesWin);

            RunTool("wstrt", toolsPathWin, args, showWindow, workDirWin);
        }

        // Runs arbitrary wit/wstrt subcommands with Windows-view args (we convert if native)
        public static void RunTool(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\uwu")
        {
            bool underWine = UnderWine();
            bool mac = underWine && HostIsMac();
            bool lin = underWine && !mac && HostIsLinux();

            if (!underWine && IsNativeWindows)
            {
                string exeWin = Path.Combine(toolsPathWin, toolBaseName + ".exe");
                Directory.CreateDirectory(workDirWin);
                var finalArgs = ReplaceArgsWithWindowsFlavor(argsWindowsPaths);
                Log($"Mode=Windows .exe\nexe={exeWin}\nargs={finalArgs}");
                int rc = RunWinExe(exeWin, finalArgs, workDirWin, showWindow, out _, out var se);
                if (rc != 0) throw new Exception($"{toolBaseName}.exe failed (exit {rc}).\n{se}");
                return;
            }

            if (mac || lin)
            {
                string toolsPosix = PosixFromWindows(toolsPathWin).TrimEnd('/');
                string exePosix = toolsPosix + "/" + toolBaseName + (mac ? "-mac" : "-linux");
                string argsPosix = ConvertArgsToPosix(argsWindowsPaths);
                Log($"Mode={(mac ? "mac" : "linux")} native\nexe={exePosix}\nargs={argsPosix}");
                int rc = RunNative(exePosix, argsPosix, mac, out var so, out var se);
                Log($"{toolBaseName} exit={rc}\nSTDOUT:\n{so}\nSTDERR:\n{se}");
                if (rc != 0) throw new Exception($"{toolBaseName} (native) failed (exit {rc}).\n{se}");
                return;
            }

            throw new Exception("Unsupported environment for tool execution.");
        }


        // ----- arg shaping helpers (convert all quoted paths safely) -----
        public static string ConcatSections(string[] gctFilesWin)
        {
            if (gctFilesWin == null || gctFilesWin.Length == 0) return "";
            var sb = new StringBuilder();
            foreach (var g in gctFilesWin) sb.Append(" --add-sect ").Append(Q(SelectPath(g)));
            return sb.ToString();
        }

        public static string SelectPath(string pWinOrPosix)
        {
            // At the call sites you pass Windows-view paths; this keeps quoting consistent.
            bool native = UnderWine() && (HostIsMac() || HostIsLinux());
            return native ? PosixFromWindows(pWinOrPosix) : pWinOrPosix.Replace('/', '\\');
        }

        public static string ReplaceArgsWithWindowsFlavor(string args)
        {
            // Just normalize slashes—call sites already built Windows paths
            return args?.Replace('/', '\\') ?? "";
        }

        public static string ConvertArgsToPosix(string args)
        {
            if (string.IsNullOrEmpty(args)) return "";
            // Convert any "C:\..." or "Z:\..." inside quoted segments to POSIX.
            // Cheap heuristic: split on quotes and convert odd segments (inside quotes).
            var parts = args.Split('"');
            for (int i = 1; i < parts.Length; i += 2) parts[i] = PosixFromWindows(parts[i]);
            return string.Join("\"", parts);
        }

        // Ensure a Windows-view path (so Windows runs are correct; Wine runs will be converted by ToolRunner)
        public static string ToWindowsView(string p)
        {
            if (string.IsNullOrEmpty(p)) return p ?? "";
            // Already C:\... ?
            if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                return p.Replace('/', '\\');
            // POSIX → Z:\...
            if (p[0] == '/')
                return @"Z:\" + p.TrimStart('/').Replace('/', '\\');
            return p.Replace('/', '\\');
        }

        public static string JoinWin(string a, string b) => ToWindowsView(Path.Combine(a, b));

        // ---------- visibility + stat helpers ----------
        public static void LogFileVisibility(string label, string winPath)
        {
            var (wineOk, wineBytes, hostOk, hostBytes, posix) = ProbeFile(winPath);
            var sb = new StringBuilder();
            sb.Append(label).Append(" vis: ");
            sb.Append("wine=").Append(wineOk ? $"exists,{wineBytes}B" : "missing");
            sb.Append(" | host=").Append(hostOk ? $"exists,{hostBytes}B" : "missing");
            sb.Append(" | posix=").Append(posix);
            Log(sb.ToString());
        }

        /// <summary>
        /// Opens a URL or file on the native host (macOS/Linux) when running under Wine; otherwise uses Windows shell.
        /// Returns true on best-effort success.
        /// </summary>
        public static bool OpenOnHost(string target)
        {
            try
            {
                bool native = UnderWine() && (HostIsMac() || HostIsLinux());
                if (!native)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });
                    return true;
                }

                // Host side
                bool isUrl = Uri.TryCreate(target, UriKind.Absolute, out var uri) &&
                             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "mailto");

                string toOpen = isUrl ? target : PosixFromWindows(target);
                string opener = HostIsMac() ? "open" : "xdg-open";
                // run detached; ignore stdout/stderr
                int rc = RunHostSh($"(command -v {opener} >/dev/null 2>&1 && {opener} {Q(toOpen)} >/dev/null 2>&1 & ) || true", out _, out _);
                return rc == 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Waits until the file is visible to .NET/Wine (File.Exists && size > 0),
        /// polling host and wine views for up to 'timeoutMs'.
        /// </summary>
        public static bool WaitForWineVisibility(string winPath, int timeoutMs = 8000, int pollMs = 200)
        {
            // If we are not under Wine, the Windows view is authoritative; return immediately.
            try { if (!UnderWine()) return true; } catch { /* best-effort */ }
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string lastNote = "";

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (File.Exists(winPath))
                    {
                        long size = new FileInfo(winPath).Length;
                        if (size > 0)
                        {
                            Log($"I/O fence: visible to Wine/.NET: {winPath} ({size} bytes)");
                            return true;
                        }
                    }
                }
                catch { /* transient */ }

                var (wineOk, wineBytes, hostOk, hostBytes, _) = ProbeFile(winPath);
                var note = $"wine:{(wineOk ? wineBytes.ToString() : "-")} host:{(hostOk ? hostBytes.ToString() : "-")}";
                if (note != lastNote)
                {
                    Log($"I/O fence probe -> {note}");
                    lastNote = note;
                }

                System.Threading.Thread.Sleep(pollMs);
            }

            Log($"I/O fence TIMEOUT: Wine can't see {winPath}");
            return false;
        }


        public static (bool wineOk, long wineBytes, bool hostOk, long hostBytes, string posixPath) ProbeFile(string winPath)
        {
            bool mac = HostIsMac();
            bool lin = HostIsLinux();
            long wineBytes = 0;
            bool wineOk = false;

            try
            {
                if (File.Exists(winPath))
                {
                    var fi = new FileInfo(winPath);
                    wineBytes = fi.Length;
                    wineOk = true;
                }
            }
            catch { /* ignore */ }

            var posix = PosixFromWindows(winPath);
            long hostBytes = 0;
            bool hostOk = false;

            // stat command differs mac vs linux
            var statCmd = mac ? $"[ -f {Q(posix)} ] && stat -f%z {Q(posix)} || echo MISS"
                              : $"[ -f {Q(posix)} ] && stat -c%s {Q(posix)} || echo MISS";

            try
            {
                int rc = RunHostSh(statCmd, out var so, out _);
                if (rc == 0 && so != null && so.Trim() != "MISS" && long.TryParse(so.Trim(), out var b))
                {
                    hostOk = true;
                    hostBytes = b;
                }
            }
            catch { /* ignore */ }

            return (wineOk, wineBytes, hostOk, hostBytes, posix);
        }

    }
}
