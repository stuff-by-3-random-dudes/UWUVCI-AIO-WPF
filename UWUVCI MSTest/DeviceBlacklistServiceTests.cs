using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Services;
using Newtonsoft.Json;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class DeviceBlacklistServiceTests
    {
        private class MockHttpHandler : HttpMessageHandler
        {
            private readonly string _response;
            private readonly HttpStatusCode _status;

            public MockHttpHandler(string response, HttpStatusCode status = HttpStatusCode.OK)
            {
                _response = response;
                _status = status;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var msg = new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_response)
                };
                return Task.FromResult(msg);
            }
        }

        [TestMethod]
        public async Task Blacklist_ReturnsTrue_WhenFingerprintMatches()
        {
            // Arrange
            var fakeHash = DeviceFingerprint.GetHashedFingerprint();
            var json = JsonConvert.SerializeObject(new { blocked = new[] { fakeHash } });

            var handler = new MockHttpHandler(json);
            using var http = new HttpClient(handler);

            // Act
            bool result;
            using (var replace = new HttpClient(handler))
            {
                // Will always fail here if the URL isn't a real one hosted somewhere.
                result = await DeviceBlacklistService.IsDeviceBlacklistedAsync("https://example.com");
            }

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Blacklist_ReturnsFalse_WhenFingerprintNotListed()
        {
            // Arrange
            var json = JsonConvert.SerializeObject(new { blocked = new[] { "SomeOtherHash" } });
            var handler = new MockHttpHandler(json);
            using var http = new HttpClient(handler);

            bool result = await DeviceBlacklistService.IsDeviceBlacklistedAsync("https://example.com");
            Assert.IsFalse(result);
        }
    }
}
