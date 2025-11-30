using System;
using System.IO;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class Logger
    {
        private static readonly string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI-V3", "Logs");
        private static readonly string logFilePath;

        static Logger()
        {
            try
            {
                // Ensure log directory exists
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                // Create a timestamped log file per app launch (single file per run)
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                logFilePath = Path.Combine(logDirectory, $"log_{timestamp}.txt");

                // Optional: Clean up old logs (e.g., older than 7 days)
                CleanupOldLogs(7);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger initialization failed: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            try
            {
                using StreamWriter sw = new StreamWriter(logFilePath, true);
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
            catch (Exception)
            {
                // If logging fails, there's nothing more to do
            }
        }

        private static void CleanupOldLogs(int daysToKeep)
        {
            try
            {
                // Clean up legacy per-tool logs and old legacy session logs
                foreach (var file in Directory.GetFiles(logDirectory, "tool-*.txt"))
                {
                    try { var fi = new FileInfo(file); if (fi.CreationTime < DateTime.Now.AddDays(-daysToKeep)) fi.Delete(); } catch { }
                }
                foreach (var file in Directory.GetFiles(logDirectory, "log_*.txt"))
                {
                    try { var fi = new FileInfo(file); if (fi.CreationTime < DateTime.Now.AddDays(-daysToKeep)) fi.Delete(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clean up old logs: {ex.Message}");
            }
        }
    }
}
