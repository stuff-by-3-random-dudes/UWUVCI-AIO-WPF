using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class MacLinuxHelper
    {
        private static readonly string[] UWUVCIHelperMessage = {
            "Don't panic! I see you're trying to run UWUCVI V3 on something that isn't Windows. Sadly, some external tool seems to not be compatible, but that's where I, ZestyTS, comes in!" +
                    "\n\nGo to the folder where UWUVCI is, you should see a folder called 'macOS' or 'linux' please go into the one meant for your system. In either folder you'll see a file called 'UWUVCI-V3-Helper' run that file." +
                    "\nDon't use Wine or any form of virtualization, that is a console app that you can run natively. Since it's a console app, make sure to run it via the terminal!" +
                    "\n\nOnce that program finishes running, it'll tell you, to click the 'OK' button on this MessageBox. The console app has it's own ReadMe, so make sure to check it out!" +
                    "\nIf it's not clear, clicking 'OK' will continue with the Inject and clicking 'Cancel' will cancel out of the inject.",
            "UWUVCI V3 Helper Program Required To Continue!" };
        public static void WriteFailedStepToJson(string functionName, string toolName, string arguments, string currentDirectory)
        {
            // Get the base directory where the application is running
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string toolsJsonPath = Path.Combine(basePath, "tools.json");

            var step = new ToolStep
            {
                ToolName = toolName,
                Arguments = arguments,
                CurrentDirectory = currentDirectory,
                Function = functionName
            };

            List<ToolStep> steps;

            if (File.Exists(toolsJsonPath))
                steps = JsonConvert.DeserializeObject<List<ToolStep>>(File.ReadAllText(toolsJsonPath)) ?? new List<ToolStep>();
            else
                steps = new List<ToolStep>();

            steps.Add(step);
            File.WriteAllText(toolsJsonPath, JsonConvert.SerializeObject(steps));
        }

        public static void DisplayMessageBoxAboutTheHelper()
        {
            var result = MessageBox.Show(UWUVCIHelperMessage[0], UWUVCIHelperMessage[1], MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

            if (result != MessageBoxResult.OK)
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string toolsJsonPath = Path.Combine(basePath, "tools.json");

                if (File.Exists(toolsJsonPath))
                    File.Delete(toolsJsonPath);

                MessageBox.Show("You have requested to cancel out of the inject.", "Cancel");
                Logger.Log("User canceled Injection early");
                throw new Exception("User canceled Injection early");
            }
        }

        public static void PrepareAndInformUserOnUWUVCIHelper(string functionName, string toolName, string arguments, string realPath = "")
        {
            WriteFailedStepToJson(functionName, toolName, arguments, realPath);
            DisplayMessageBoxAboutTheHelper();
        }

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

                bool underWineLike = wineEnv || isProton || isCrossOver || isLutris || hasZ || wineSrv;

                // Only works when running under Wine (because Z:\ maps to /)
                bool hostIsMac = File.Exists(@"Z:\System\Library\CoreServices\SystemVersion.plist");

                string flavor = null;
                if (isCrossOver) flavor = "CrossOver";
                else if (isProton) flavor = "Proton";
                else if (isLutris) flavor = "Lutris";
                else if (underWineLike) flavor = "Wine";

                return new RuntimeEnv
                {
                    UnderWineLike = underWineLike,
                    HostIsMac = hostIsMac,
                    Flavor = flavor
                };
            }
        }
    }
}
