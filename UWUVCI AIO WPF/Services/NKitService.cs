using System;
using System.IO;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Thin wrapper around external ConvertToIso/ConvertToNKit tools.
    /// Uses ToolRunner so it works under native Windows and under Wine (macOS/Linux).
    /// </summary>
    public static class NKitService
    {
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

            // Build args: tool expects only the source path quoted
            string args = ToolRunner.Q(ToolRunner.SelectPath(sourcePath));

            // Execute tool via ToolRunner (cross-platform under Wine)
            runner.RunToolWithFallback("ConvertToIso", toolsPath, args, showWindow);

            // Produced file appears next to the tool (historical behavior of these helpers)
            string outPath = Path.Combine(toolsPath, outputFileName);
            if (!File.Exists(outPath))
                throw new Exception("ISO conversion failed: output file not found: " + outPath);

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

            // Build args: tool expects only the source path quoted
            string args = ToolRunner.Q(ToolRunner.SelectPath(sourcePath));

            // Execute tool via ToolRunner (cross-platform under Wine)
            runner.RunToolWithFallback("ConvertToNKit", toolsPath, args, showWindow);

            // Produced file appears next to the tool (historical behavior of these helpers)
            string outPath = Path.Combine(toolsPath, outputFileName);
            if (!File.Exists(outPath))
                throw new Exception("NKit conversion failed: output file not found: " + outPath);

            return outPath;
        }
    }
}
