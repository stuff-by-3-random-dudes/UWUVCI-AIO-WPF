using System;
using System.Diagnostics;
using System.IO;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class MacLinuxHelper
    {
        public sealed class RuntimeEnv
        {
            public bool UnderWineLike { get; set; }  // Wine / CrossOver / Proton / Lutris
            public bool HostIsMac { get; set; }      // macOS host (detected via Wine Z:\ mapping)
            public string Flavor { get; set; }       // "CrossOver" | "Proton" | "Lutris" | "Wine" | null
        }

        public static class EnvDetect
        {
            private static string GetEnv(string name)
            {
                var v = Environment.GetEnvironmentVariable(name);
                return v ?? string.Empty;
            }

            public static RuntimeEnv Get()
            {
                // Single source of truth: ToolRunner.UnderWine()
                bool underWine = Helpers.ToolRunner.UnderWine();

                // On native Windows, never treat as Wine-like.
                if (!underWine)
                    return new RuntimeEnv { UnderWineLike = false, HostIsMac = false, Flavor = null };

                // We’re under Wine; refine flavor/host using the existing env hints.
                bool wineEnv =
                    GetEnv("WINEDLLPATH") != string.Empty ||
                    GetEnv("WINEPREFIX") != string.Empty ||
                    GetEnv("WINELOADERNOEXEC") != string.Empty ||
                    GetEnv("WINEESYNC") != string.Empty ||
                    GetEnv("WINEFSYNC") != string.Empty;

                bool isProton = GetEnv("STEAM_COMPAT_DATA_PATH") != string.Empty;
                bool isCrossOver = GetEnv("CX_BOTTLE_PATH") != string.Empty ||
                                   GetEnv("CROSSOVER_PREFIX") != string.Empty;
                bool isLutris = GetEnv("LUTRIS_GAME_UUID") != string.Empty;
                bool hasZ = false;
                bool wineSrv = false;

                try { hasZ = Directory.Exists(@"Z:\"); } catch { }
                try { wineSrv = Process.GetProcessesByName("wineserver").Length > 0; } catch { }

                // Only works when running under Wine (because Z:\ maps to /)
                bool hostIsMac = File.Exists(@"Z:\System\\Library\\CoreServices\\SystemVersion.plist");

                string flavor = null;
                if (isCrossOver) flavor = "CrossOver";
                else if (isProton) flavor = "Proton";
                else if (isLutris) flavor = "Lutris";
                else if (wineEnv || hasZ || wineSrv) flavor = "Wine";

                return new RuntimeEnv
                {
                    UnderWineLike = true,
                    HostIsMac = hostIsMac,
                    Flavor = flavor
                };
            }
        }
    }
}
