using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class GitHubBaseServiceTests
    {
        [TestMethod]
        public void GetToken_ReturnsNonEmptyString_WhenDebugMode()
        {
            // Arrange
            var method = typeof(GitHubBaseService).GetMethod("GetToken", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "Could not find GetToken in GitHubBaseService.");

            var instance = new TestableGitHubBaseService(); // Dummy subclass

            // Act
            string token = (string)method.Invoke(instance, null);

            // Assert
            Assert.IsNotNull(token, "Token should not be null.");
            Assert.IsTrue(token.StartsWith("ghp_"), "Token should start with ghp_ in debug mode.");
        }

        private class TestableGitHubBaseService : GitHubBaseService { }
    }
}
