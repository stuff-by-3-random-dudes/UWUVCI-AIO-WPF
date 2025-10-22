using System.Reflection;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class GitHubFeedbackServiceTests
    {
        private static MethodInfo GetPrivateMethod(string name)
        {
            return typeof(GitHubFeedbackService).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }

        [TestMethod]
        public void BuildIssueBody_ContainsCollapsibleLog_AndFingerprint()
        {
            // Arrange
            var method = GetPrivateMethod("BuildIssueBody");
            string log = "Example log line\nAnother line";

            // Act
            string result = (string)method.Invoke(
                null,
                ["Bug Report", "Test Desc", "3.0.0", "FP123", "2025-10-21", "SysInfo", log]
            );

            // Assert
            StringAssert.Contains(result, "<details>", "Should include collapsible log section.");
            StringAssert.Contains(result, "FP123", "Should include fingerprint string.");
            StringAssert.Contains(result, "Test Desc", "Description text missing.");
        }

        [TestMethod]
        public void BuildIssueBody_TruncatesLongTitleCorrectly()
        {
            // Arrange
            var truncate = typeof(GitHubFeedbackService).GetMethod("TruncateTitle", BindingFlags.NonPublic | BindingFlags.Static);
            string longText = new('A', 200);

            // Act
            string result = (string)truncate.Invoke(null, [longText]);

            // Assert
            Assert.IsTrue(result.EndsWith("..."), "Truncated title should end with ellipsis.");
            Assert.IsTrue(result.Length <= 75, "Truncated title should be within reasonable length.");
        }
    }
}
