using System;
using System.IO;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Services
{
    public static class WitTicketExtractionService
    {
        public static void ExtractTickets(
            string toolsPath,
            string gameIso,
            string tikTmdDir,
            bool debug,
            IToolRunnerFacade runner = null,
            int waitTimeoutMs = 8000)
        {
            if (string.IsNullOrWhiteSpace(toolsPath)) throw new ArgumentException("Tools path required", nameof(toolsPath));
            if (string.IsNullOrWhiteSpace(gameIso)) throw new ArgumentException("Game ISO path required", nameof(gameIso));
            if (string.IsNullOrWhiteSpace(tikTmdDir)) throw new ArgumentException("Destination path required", nameof(tikTmdDir));

            runner ??= DefaultToolRunnerFacade.Instance;

            if (Directory.Exists(tikTmdDir))
            {
                try { Directory.Delete(tikTmdDir, true); } catch { }
            }

            var witArgs = $"extract \"{gameIso}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{tikTmdDir}\" -vv1";
            runner.RunTool("wit", toolsPath, witArgs, showWindow: debug);

            var tmdPath = Path.Combine(tikTmdDir, "tmd.bin");
            var tikPath = Path.Combine(tikTmdDir, "ticket.bin");
            if (!ToolRunner.WaitForWineVisibility(tmdPath, timeoutMs: waitTimeoutMs) ||
                !ToolRunner.WaitForWineVisibility(tikPath, timeoutMs: waitTimeoutMs))
            {
                throw new Exception($"WIT extract completed but extracted files not visible: {tmdPath}, {tikPath}");
            }

            ToolRunner.LogFileVisibility("[WitTicketExtraction] tmd.bin", tmdPath);
            ToolRunner.LogFileVisibility("[WitTicketExtraction] ticket.bin", tikPath);
        }
    }
}
