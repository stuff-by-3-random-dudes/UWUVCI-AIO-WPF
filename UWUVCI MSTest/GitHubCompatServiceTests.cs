using System.Reflection;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class GitHubCompatServiceTests
    {
        private static MethodInfo GetPrivateInstanceMethod(string name)
        {
            var method = typeof(GitHubCompatService).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy
            );
            Assert.IsNotNull(method, $"Private instance method '{name}' not found.");
            return method;
        }

        [TestMethod]
        public void FileName_Mapping_Works_For_All_Consoles()
        {
            var svc = new GitHubCompatService();
            var known = new[] { "NES", "SNES", "GBA", "N64", "TG16", "MSX", "Wii", "NDS" };

            foreach (var console in known)
            {
                var file = svc.GetFileNameForConsole(console);
                Assert.IsTrue(file.EndsWith("Compat.json"), $"Invalid mapping for {console}");
            }
        }

        [TestMethod]
        public void BuildPrBody_ProducesExpectedFormat_For_Wii()
        {
            var entry = new GameCompatEntry
            {
                GameName = "Mario Kart Wii",
                GameRegion = "USA",
                BaseName = "Rhythm Heaven Fever",
                BaseRegion = "USA",
                Status = 2,
                Notes = "Works perfectly."
            };

            var svc = new GitHubCompatService();
            var method = GetPrivateInstanceMethod("BuildPrBody");

            string result = (string)method.Invoke(svc,
                new object[] { "Wii", entry, 2, null, "2.1.0", "FakeFingerprintABC" });

            StringAssert.Contains(result, "GamePad", "Wii PR body should list GamePad.");
            StringAssert.Contains(result, "Mario Kart Wii", "Missing game name.");
            StringAssert.Contains(result, "FakeFingerprintABC", "Missing fingerprint section.");
        }

        [TestMethod]
        public void BuildPrBody_ProducesExpectedFormat_For_NDS()
        {
            var entry = new GameCompatEntry
            {
                GameName = "Pokémon Platinum",
                GameRegion = "USA",
                BaseName = "Pokémon Diamond",
                BaseRegion = "USA",
                Status = 1,
                Notes = "Minor rendering issues."
            };

            var svc = new GitHubCompatService();
            var method = GetPrivateInstanceMethod("BuildPrBody");

            string result = (string)method.Invoke(svc,
                new object[] { "NDS", entry, null, "2x", "3.5.0", "FakeFingerprintXYZ" });

            StringAssert.Contains(result, "Render Size", "NDS PR body should list Render Size.");
            StringAssert.Contains(result, "Pokémon Platinum", "Missing game name.");
            StringAssert.Contains(result, "FakeFingerprintXYZ", "Missing fingerprint section.");
        }
    }
}
