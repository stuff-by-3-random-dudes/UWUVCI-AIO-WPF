using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Handles fetching, caching, and updating the ReadMe.txt file from GitHub.
    /// </summary>
    public static class ReadMeManager
    {
        // Remote location of ReadMe
        private static readonly string ReadMeUrl =
            "https://raw.githubusercontent.com/stuff-by-3-random-dudes/UWUVCI-AIO-WPF/master/UWUVCI%20AIO%20WPF/uwuvci_installer_creator/app/Readme.txt";

        // Local cached file path (in same directory as executable)
        private static readonly string LocalReadMePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReadMe.txt");

        // Where to store the previous hash for update detection
        private static readonly string HashFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReadMe.hash");

        /// <summary>
        /// Checks for an updated ReadMe online and replaces the local version if changed.
        /// Returns the latest text (online or local).
        /// </summary>
        public static async Task<string> GetLatestReadMeAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var remoteContent = await client.GetStringAsync(ReadMeUrl).ConfigureAwait(false);
                var remoteHash = ComputeSha256(remoteContent);

                var localHash = File.Exists(HashFilePath) ? File.ReadAllText(HashFilePath) : string.Empty;

                if (!string.Equals(remoteHash, localHash, StringComparison.OrdinalIgnoreCase))
                {
                    // Save new ReadMe and hash
                    File.WriteAllText(LocalReadMePath, remoteContent, Encoding.UTF8);
                    File.WriteAllText(HashFilePath, remoteHash, Encoding.UTF8);
                }

                return remoteContent;
            }
            catch
            {
                // Fallback to local cached version if offline or failed
                if (File.Exists(LocalReadMePath))
                    return File.ReadAllText(LocalReadMePath, Encoding.UTF8);

                // If nothing at all exists, return a basic message
                return "ReadMe not available.\n\nPlease check your internet connection or reinstall UWUVCI-V3.";
            }
        }

        /// <summary>
        /// Computes a SHA256 hash for the given text.
        /// </summary>
        private static string ComputeSha256(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
