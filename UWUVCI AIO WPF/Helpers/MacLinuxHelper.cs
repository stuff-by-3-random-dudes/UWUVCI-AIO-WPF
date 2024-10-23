using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class MacLinuxHelper
    {
        private static readonly string[] UWUVCIHelperMessage = {
            "Don't panic! I see you're trying to run UWUCVI V3 on something that isn't Windows. Sadly, some external tool seems to not be compatible, but that's where I, ZestyTS, comes in!" +
                    "\n\nGo to the folder where UWUVCI is, you should see a folder called 'macos' or 'linux' please go into the one meant for your system. In either folder you'll see a file called 'UWUVCI-V3-Helper' run that file." +
                    "\nDon't use Wine or any form of virtualization, that is a program that you can run natively." +
                    "\n\nOnce that program finishes running, it'll tell you, to click the 'OK' button on this MessageBox." +
                    "\nIf it's not clear, clicking 'OK' will continue with the Inject and clicking 'Cancel' will cancel out of the inject.",
            "UWUVCI V3 Helper Program Required To Continue!" };
        public static void WriteFailedStepToJson(string functionName, string toolName, string arguments, string currentDirectory)
        {
            // Get the base directory where the application is running
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string toolsJsonPath = Path.Combine(basePath, "tools.json");

            if (string.IsNullOrEmpty(currentDirectory))
                currentDirectory = basePath;

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
                MessageBox.Show("You have requested to cancel out of the inject.", "Cancel");
                throw new Exception("User canceled Injection early");
            }
        }

        public static void PrepareAndInformUserOnUWUVCIHelper(string functionName, string toolName, string arguments, string currentDirectory = "")
        {
            WriteFailedStepToJson(functionName, toolName, arguments, currentDirectory);
            DisplayMessageBoxAboutTheHelper();
        }

        public static bool IsRunningUnderWineOrSimilar()
        {
            try
            {
                // Check for Wine
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Wine"))
                {
                    if (key != null)
                        return true;
                }

                string winePrefix = Environment.GetEnvironmentVariable("WINEPREFIX");
                if (!string.IsNullOrEmpty(winePrefix))
                    return true;

                // Check for Proton
                string protonPrefix = Environment.GetEnvironmentVariable("STEAM_COMPAT_DATA_PATH");
                if (!string.IsNullOrEmpty(protonPrefix))
                    return true;

                // Check for CrossOver
                string crossoverPrefix = Environment.GetEnvironmentVariable("CROSSOVER_PREFIX");
                if (!string.IsNullOrEmpty(crossoverPrefix))
                    return true;

                // Check for BoxedWine
                string boxedWinePrefix = Environment.GetEnvironmentVariable("BOXEDWINE_PATH");
                if (!string.IsNullOrEmpty(boxedWinePrefix))
                    return true;

                // Check for Lutris
                string lutrisRuntime = Environment.GetEnvironmentVariable("LUTRIS_GAME_UUID");
                if (!string.IsNullOrEmpty(lutrisRuntime))
                    return true;

                // Check for PlayOnLinux
                string playOnLinux = Environment.GetEnvironmentVariable("PLAYONLINUX");
                if (!string.IsNullOrEmpty(playOnLinux))
                    return true;

                // Check for DXVK
                string dxvk = Environment.GetEnvironmentVariable("DXVK_LOG_LEVEL");
                if (!string.IsNullOrEmpty(dxvk))
                    return true;

                // Check for ReactOS
                if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                    Environment.OSVersion.VersionString.Contains("ReactOS"))
                    return true;

                // Check for Winetricks
                string winetricks = Environment.GetEnvironmentVariable("WINETRICKS");
                if (!string.IsNullOrEmpty(winetricks))
                    return true;

                // Check for Cedega (WineX)
                string cedega = Environment.GetEnvironmentVariable("CEDEGA_PATH");
                if (!string.IsNullOrEmpty(cedega))
                    return true;

                // Check for common Wine/Proton files
                string[] wineFiles = { "/usr/bin/wine", "/usr/local/bin/wine", "/usr/bin/proton", "/usr/local/bin/proton" };
                if (wineFiles.Any(File.Exists))
                    return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while checking for Wine or similar: {ex.Message}");
            }

            return false;
        }

        public static bool IsRunningInVirtualMachine()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                    foreach (var _ in from ManagementObject bios in searcher.Get()
                                      let manufacturer = bios["Manufacturer"]?.ToString() ?? string.Empty
                                      where manufacturer.Contains("VMware") || manufacturer.Contains("VirtualBox") || manufacturer.Contains("Parallels") || manufacturer.Contains("Xen") || manufacturer.Contains("KVM") || manufacturer.Contains("Bhyve")
                                      select new { })
                        return true;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                    foreach (var (manufacturer, model) in from ManagementObject cs in searcher.Get()
                                                          let manufacturer = cs["Manufacturer"]?.ToString() ?? string.Empty
                                                          let model = cs["Model"]?.ToString() ?? string.Empty
                                                          select (manufacturer, model))
                    {
                        if (manufacturer.Contains("Microsoft Corporation") && model.Contains("Virtual Machine"))
                            return true;

                        if (manufacturer.Contains("QEMU") || manufacturer.Contains("Bochs") || manufacturer.Contains("OpenStack"))
                            return true;
                    }

                string[] virtualizationIndicators = { "Parallels", "VMware", "VirtualBox", "QEMU", "Hyper-V", "Xen", "KVM", "Bhyve", "Bochs", "OpenStack", "ProxMox", "Virtuozzo" };
                foreach (string indicator in virtualizationIndicators)
                    if (Environment.OSVersion.VersionString.Contains(indicator))
                        return true;

                // Check for common VM files
                string[] vmFiles = { "/usr/bin/vmware", "/usr/bin/virtualbox", "/usr/bin/qemu", "/usr/bin/kvm", "/usr/bin/hyperv" };
                if (vmFiles.Any(File.Exists))
                    return true;

                // Check for Docker
                string dockerEnv = Environment.GetEnvironmentVariable("DOCKER_ENV");
                if (!string.IsNullOrEmpty(dockerEnv) || File.Exists("/.dockerenv"))
                    return true;

                // Check for common VM processes
                string[] vmProcesses = { "vmware", "virtualbox", "qemu", "kvm", "hyperv" };
                foreach (var processName in vmProcesses)
                {
                    if (Process.GetProcessesByName(processName).Length > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while checking for virtual machine: {ex.Message}");
            }

            return false;
        }
    }
}
