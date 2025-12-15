using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTest
{
    [TestClass]
    public class BaseExtractorTests
    {
        [TestMethod]
        public void GetOrExtractBase_ExtractsAndCaches()
        {
            // Arrange: create a fake tools folder with BASE_UNITTEST.zip containing BASE/marker.txt
            var tools = Path.Combine(Path.GetTempPath(), "uwu_tools_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tools);
            var baseDir = Path.Combine(tools, "_work", "BASE");
            Directory.CreateDirectory(baseDir);
            File.WriteAllText(Path.Combine(baseDir, "marker.txt"), "ok");
            var zipPath = Path.Combine(tools, "BASE_UNITTEST.zip");
            System.IO.Compression.ZipFile.CreateFromDirectory(Path.Combine(tools, "_work"), zipPath);

            try
            {
                // Act
                var extracted = BaseExtractor.GetOrExtractBase(tools, "BASE_UNITTEST.zip");

                // Assert
                Assert.IsTrue(Directory.Exists(extracted), "Extracted BASE folder should exist");
                Assert.IsTrue(File.Exists(Path.Combine(extracted, "marker.txt")), "Marker file should be present in extracted cache");

                // Act again: should reuse cache (same path)
                var extracted2 = BaseExtractor.GetOrExtractBase(tools, "BASE_UNITTEST.zip");
                Assert.AreEqual(extracted, extracted2, "Cache key should be stable and reused for identical zip");
            }
            finally
            {
                try { Directory.Delete(tools, true); } catch { }
            }
        }
    }
}

