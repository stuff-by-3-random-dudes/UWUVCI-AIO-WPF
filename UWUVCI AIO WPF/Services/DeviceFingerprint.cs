using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UWUVCI_AIO_WPF.Services
{
    public static class DeviceFingerprint
    {
        /// <summary>
        /// Build a device fingerprint from first available MAC, machine name and user name.
        /// Returns a SHA256 hash (Base64) of the combined string.
        /// Falls back to machine name + user name if no MAC is available.
        /// </summary>
        public static string GetHashedFingerprint()
        {
            // Compose raw parts
            string machine = Environment.MachineName ?? "";
            string user = Environment.UserName ?? "";
            string mac = GetPrimaryMacAddress() ?? "";

            // Keep rawConcise deterministic ordering
            var raw = $"{mac}|{machine}|{user}";

            // Compute SHA256 and return Base64
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(hash);
        }

        private static string GetPrimaryMacAddress()
        {
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n =>
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        n.OperationalStatus == OperationalStatus.Up)
                    .OrderByDescending(n => n.Speed)
                    .ToArray();

                if (nics.Length == 0)
                {
                    // try again without OperationalStatus filter (some environments)
                    nics = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        .OrderByDescending(n => n.Speed)
                        .ToArray();
                }

                if (nics.Length > 0)
                {
                    var bytes = nics[0].GetPhysicalAddress().GetAddressBytes();
                    if (bytes != null && bytes.Length > 0)
                        return string.Concat(bytes.Select(b => b.ToString("x2")));
                }
            }
            catch
            {
                // swallow errors — fallback will be used
            }
            return null;
        }
    }

    public static class DeviceBlacklistService
    {
        /// <summary>
        /// Remote blacklist JSON format:
        /// { "blocked": ["Base64Hash1", "Base64Hash2", ...] }
        /// (where each entry is SHA256(base string) base64-encoded)
        /// </summary>
        public class BlacklistFile
        {
            [JsonProperty("blocked")]
            public List<string> Blocked { get; set; } = new List<string>();
        }

        /// <summary>
        /// Check whether the current device is in the remote blacklist.
        /// - blacklistUrl: the direct raw URL to a JSON file containing the list.
        /// - timeoutMs: how long to wait for the download (default 4s)
        /// Returns true if the device is explicitly found in the list.
        /// If the fetch fails (timeout/network) this returns false (fail-open).
        /// Change to fail-closed behavior if you prefer.
        /// </summary>
        public static async Task<bool> IsDeviceBlacklistedAsync(string blacklistUrl, int timeoutMs = 4000)
        {
            if (string.IsNullOrWhiteSpace(blacklistUrl))
                return false;

            var fingerprint = DeviceFingerprint.GetHashedFingerprint();
            if (string.IsNullOrWhiteSpace(fingerprint))
                return false;

            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

                var txt = await http.GetStringAsync(blacklistUrl).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(txt))
                    return false;

                var bl = JsonConvert.DeserializeObject<BlacklistFile>(txt) ?? new BlacklistFile();
                // normalize comparisons
                var set = new HashSet<string>(bl.Blocked ?? new List<string>(), StringComparer.Ordinal);
                return set.Contains(fingerprint);
            }
            catch (TaskCanceledException)
            {
                // timed out -> fail-open (not blacklisted)
                return false;
            }
            catch (Exception)
            {
                // network/parse error -> fail-open
                return false;
            }
        }
    }
}
