using System.Reflection;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class GitHubCompatServiceTests
    {
        private static MethodInfo GetPrivateMethod(string name)
        {
            return typeof(GitHubCompatService).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }

        [TestMethod]
        public void Token_Reconstruction_Returns_ValidString()
        {
            // Arrange
            var method = GetPrivateMethod("GetToken");

            // Act
            string token = (string)method.Invoke(null, null);

            // Assert
            Assert.IsNotNull(token, "Token should not be null");
            Assert.IsTrue(token.Length > 10, "Token should be of reasonable length");
            Assert.IsFalse(token.Contains("="), "Token should not be a base64 string itself");
        }

        [TestMethod]
        public void FileName_Mapping_Works_For_All_Consoles()
        {
            // Arrange
            var knownConsoles = new[]
            {
                "NES", "SNES", "GBA", "N64", "TG16", "MSX", "Wii", "NDS"
            };

            // Act & Assert
            foreach (var console in knownConsoles)
            {
                var fileName = new GitHubCompatService().GetFileNameForConsole(console);
                Assert.IsTrue(fileName.EndsWith("Compat.json"), $"Invalid filename for {console}");
            }
        }

        [TestMethod]
        public void BuildPrBody_Includes_Correct_Fields_For_Wii()
        {
            // Arrange
            var entry = new GameCompatEntry
            {
                GameName = "Mario Kart Wii",
                GameRegion = "USA",
                BaseName = "Rhythm Heaven Fever",
                BaseRegion = "USA",
                Status = 2,
                Notes = "Works perfectly."
            };

            var method = GetPrivateMethod("BuildPrBody");

            // Act
            string prBody = (string)method.Invoke(
                null,
                ["Wii", entry, 2, null, "2.1.0"]
            );

            // Assert
            Assert.IsTrue(prBody.Contains("GamePad"), "Wii PR should include GamePad info");
            Assert.IsTrue(prBody.Contains("Mario Kart Wii"), "PR body missing game name");
            Assert.IsTrue(prBody.Contains("2.1.0"), "App version missing");
        }

        [TestMethod]
        public void BuildPrBody_Includes_Correct_Fields_For_NDS()
        {
            // Arrange
            var entry = new GameCompatEntry
            {
                GameName = "Pokémon Platinum",
                GameRegion = "USA",
                BaseName = "Pokémon Diamond",
                BaseRegion = "USA",
                Status = 1,
                Notes = "Slight rendering issues."
            };

            var method = GetPrivateMethod("BuildPrBody");

            // Act
            string prBody = (string)method.Invoke(
                null,
                ["NDS", entry, null, "2x", "2.1.0"]
            );

            // Assert
            Assert.IsTrue(prBody.Contains("Render Size"), "NDS PR should include Render Size");
            Assert.IsTrue(prBody.Contains("Pokémon Platinum"), "PR body missing game name");
        }
    }
}
