using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (p.StartsWith(@"Z:\")) return p.Replace(@"Z:\", "/").Replace('\\', '/');
            if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                return "/" + p.Substring(2).TrimStart('\\', '/').Replace('\\', '/');
            return p.Replace('\\', '/');
        }

        public static string StartExe()
        {
            var sys = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(sys)) sys = @"C:\windows";
            return Path.Combine(sys, "command", "start.exe"); // within current Wine prefix
        }

        // -------- permissions (only once per native tool) --------
        public static readonly ConcurrentDictionary<string, bool> _execFixed = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

        public static void EnsureExecBitOnce(string toolPosix, bool isMac)
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
                   .Append("command -v xattr >/dev/null 2>&1 && xattr -dr com.apple.quarantine \"$APP\" || true; ");
            }

            int rc = RunHostSh(cmd.ToString(), out _, out var se);
            if (rc != 0) throw new Exception("Failed to set exec bit / clear quarantine.\n" + se);

            _execFixed[toolPosix] = true;
        }

        // -------- process launchers --------
        public static int RunHostSh(string shCommand, out string so, out string se)
        {
            so = se = "";
            var psi = new ProcessStartInfo
            {
                FileName = StartExe(),
                Arguments = "/unix /bin/sh -lc " + Q(shCommand),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            so = p.StandardOutput.ReadToEnd();
            se = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode;
        }

        private static int RunWinExe(string exeWin, string args, string workWin, bool show, out string so, out string se)
        {
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
            return p.ExitCode;
        }

        private static int RunNative(string exePosix, string args, bool mac, out string so, out string se)
        {
            EnsureExecBitOnce(exePosix, mac);
            return RunHostSh(Q(exePosix) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args), out so, out se);
        }

        // -------- public API --------

        // Runs "wstrt patch <mainDol> --add-section <gct> ..."
        public static void RunWstrtPatch(string toolsPathWin, string mainDolPathWin, string[] gctFilesWin, bool showWindow, string workDirWin = @"C:\temp")
        {
            var args = "patch " + Q(SelectPath(mainDolPathWin)) + ConcatSections(gctFilesWin);

            RunTool("wstrt", toolsPathWin, args, showWindow, workDirWin);
        }

        // Runs arbitrary wit/wstrt subcommands with Windows-view args (we convert if native)
        public static void RunTool(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\temp")
        {
            // Decide environment
            bool underWine = UnderWine();
            bool mac = underWine && HostIsMac();
            bool lin = underWine && !mac && HostIsLinux();

            // Build executable path for this environment
            if (!underWine && IsNativeWindows)
            {
                // Pure Windows / VM / ReactOS → use .exe
                string exeWin = Path.Combine(toolsPathWin, toolBaseName + ".exe");
                Directory.CreateDirectory(workDirWin);
                int rc = RunWinExe(exeWin, ReplaceArgsWithWindowsFlavor(argsWindowsPaths), workDirWin, showWindow, out _, out var se);
                if (rc != 0) throw new Exception($"{toolBaseName}.exe failed.\n" + se);
                return;
            }

            if (mac || lin)
            {
                // Native host tool (Mach-O/ELF) named wit-mac / wit-linux / wstrt-mac / wstrt-linux
                string toolsPosix = PosixFromWindows(toolsPathWin).TrimEnd('/');
                string exePosix = toolsPosix + "/" + toolBaseName + (mac ? "-mac" : "-linux");

                // Convert any Windows-style paths in args to POSIX
                string argsPosix = ConvertArgsToPosix(argsWindowsPaths);

                int rc = RunNative(exePosix, argsPosix, mac, out _, out var se);
                if (rc != 0) throw new Exception($"{toolBaseName} (native) failed.\n" + se);
                return;
            }

            // Fallback (very rare): if not Wine and not Windows, error
            throw new Exception("Unsupported environment for tool execution.");
        }

        // ----- arg shaping helpers (convert all quoted paths safely) -----
        public static string ConcatSections(string[] gctFilesWin)
        {
            if (gctFilesWin == null || gctFilesWin.Length == 0) return "";
            var sb = new StringBuilder();
            foreach (var g in gctFilesWin) sb.Append(" --add-section ").Append(Q(SelectPath(g)));
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
    }
}
