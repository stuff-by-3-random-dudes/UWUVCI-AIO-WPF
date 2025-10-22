using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTests
{
    [TestClass]
    public class DeviceFingerprintTests
    {
        [TestMethod]
        public void GetHashedFingerprint_ReturnsStableAndValidValue()
        {
            // Act
            var first = DeviceFingerprint.GetHashedFingerprint();
            var second = DeviceFingerprint.GetHashedFingerprint();

            // Assert
            Assert.IsNotNull(first, "Fingerprint should not be null.");
            Assert.AreEqual(first, second, "Fingerprint should be deterministic.");
            Assert.IsTrue(first.Length > 10, "Fingerprint should be a reasonable length.");
            StringAssert.Matches(first, new System.Text.RegularExpressions.Regex("^[A-Za-z0-9+/=]+$"));
        }
    }
}
