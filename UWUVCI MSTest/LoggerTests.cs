using Microsoft.VisualStudio.TestTools.UnitTesting;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Log_WritesToFile_Successfully()
        {
            // Act
            Logger.Log("Test log entry");

            // Assert
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "UWUVCI-V3", "Logs");

            Assert.IsTrue(Directory.Exists(dir), "Log directory should exist.");

            var latest = new DirectoryInfo(dir).GetFiles("log_*.txt")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            Assert.IsNotNull(latest, "A log file should have been created.");
            string content = File.ReadAllText(latest.FullName);
            StringAssert.Contains(content, "Test log entry");
        }
    }
}
