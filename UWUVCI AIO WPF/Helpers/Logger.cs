using System;
using System.IO;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class Logger
    {
        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI-V3", "log.txt");

        public static void Log(string message)
        {
            try
            {
                using StreamWriter sw = new StreamWriter(logFilePath, true);
                sw.WriteLine($"{DateTime.Now}: {message}");
            }
            catch (Exception)
            {
                // If logging fails, there's nothing more to do
            }
        }
    }
}
