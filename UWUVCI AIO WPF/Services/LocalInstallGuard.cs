using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// LocalInstallGuard
    /// - Uses AppData install file + encrypted marker near EXE.
    /// - Integrates with JsonSettingsManager.Settings.SysKey (and SysKey1 as backup if desired).
    /// - No hardware or NIC checks.
    /// 
    /// Behavior summary:
    /// - If marker exists but no install/settings -> suspicious, block.
    /// - If settings exist but files missing -> recreate them.
    /// - If install exists but marker missing -> rebuild marker.
    /// - If all exist -> verify marker matches install/settings.
    /// </summary>
    public static class LocalInstallGuard
    {
        private const string MarkerFileName = "uwuvci.marker";
        private const string InstallFileName = "uwuvci.install";

        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UWUVCI-V3");

        private static readonly string InstallPath = Path.Combine(AppDataPath, InstallFileName);
        private static readonly string MarkerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MarkerFileName);

        private const bool EnforceProtection = true;
        private static readonly byte[] AesKey = BuildAesKey();

        public static bool EnsureInstalled()
        {
#if DEBUG
            return true; // skip in debug
#endif
            if (!EnforceProtection)
                return true;

            try
            {
                Directory.CreateDirectory(AppDataPath);

                string settingsKey = JsonSettingsManager.Settings.SysKey?.Trim();
                bool installExists = File.Exists(InstallPath);
                bool markerExists = File.Exists(MarkerPath);

                // Case 1: Marker exists but no install/settings — suspicious
                if (markerExists && !installExists && string.IsNullOrWhiteSpace(settingsKey))
                {
                    Trace.WriteLine("[LocalInstallGuard] Marker present but no install/settings — suspicious.");
                    return false;
                }

                // Case 2: Settings exist but files missing — recreate both
                if (!installExists && !markerExists && !string.IsNullOrWhiteSpace(settingsKey))
                {
                    Trace.WriteLine("[LocalInstallGuard] Settings present but files missing — recreating install & marker.");
                    WriteInstall(settingsKey);
                    CreateMarker(settingsKey);
                    return true;
                }

                // Case 3: Nothing exists anywhere — new legitimate install
                if (!installExists && !markerExists && string.IsNullOrWhiteSpace(settingsKey))
                {
                    Trace.WriteLine("[LocalInstallGuard] First-time setup — creating new GUID + marker + settings.");
                    var guid = Guid.NewGuid().ToString("N");
                    WriteInstall(guid);
                    CreateMarker(guid);
                    JsonSettingsManager.Settings.SysKey = guid;
                    JsonSettingsManager.SaveSettings();
                    return true;
                }

                // Case 4: Install exists but marker missing — rebuild marker
                if (installExists && !markerExists)
                {
                    var guid = File.ReadAllText(InstallPath, Encoding.UTF8).Trim();
                    Trace.WriteLine("[LocalInstallGuard] Marker missing — regenerating marker.");
                    CreateMarker(guid);
                    JsonSettingsManager.Settings.SysKey = guid;
                    JsonSettingsManager.SaveSettings();
                    return true;
                }

                // Case 5: Everything exists — verify integrity
                if (installExists)
                {
                    string installGuid = File.ReadAllText(InstallPath, Encoding.UTF8).Trim();

                    // Sync SysKey if mismatched
                    if (!string.Equals(settingsKey, installGuid, StringComparison.Ordinal))
                    {
                        Trace.WriteLine("[LocalInstallGuard] Syncing settings SysKey to match install.");
                        JsonSettingsManager.Settings.SysKey = installGuid;
                        JsonSettingsManager.SaveSettings();
                    }

                    if (!markerExists)
                    {
                        CreateMarker(installGuid);
                        return true;
                    }

                    bool verified = VerifyMarker(installGuid);
                    if (!verified)
                    {
                        Trace.WriteLine("[LocalInstallGuard] Marker verification failed — blocking run.");
                        return false;
                    }

                    return true;
                }

                Trace.WriteLine("[LocalInstallGuard] Unexpected state — failing safe.");
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[LocalInstallGuard] Error: {ex}");
                return false;
            }
        }

        // ---------------------------
        // File helpers
        // ---------------------------
        private static void WriteInstall(string guid)
        {
            File.WriteAllText(InstallPath, guid, Encoding.UTF8);
        }

        private static void CreateMarker(string guid)
        {
            string payload = $"{guid}|{DateTime.UtcNow:O}";
            string encrypted = EncryptToBase64(payload);
            File.WriteAllText(MarkerPath, encrypted, Encoding.UTF8);
            try
            {
                var att = File.GetAttributes(MarkerPath);
                File.SetAttributes(MarkerPath, att | FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
            catch { /* ignore */ }
        }

        private static bool VerifyMarker(string expectedGuid)
        {
            try
            {
                var content = File.ReadAllText(MarkerPath, Encoding.UTF8).Trim();
                var decrypted = DecryptFromBase64(content);
                if (string.IsNullOrWhiteSpace(decrypted)) return false;

                var parts = decrypted.Split('|');
                if (parts.Length < 1) return false;

                string guid = parts[0];
                return string.Equals(guid, expectedGuid, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        // ---------------------------
        // AES encryption helpers
        // ---------------------------
        private static string EncryptToBase64(string text)
        {
            using var aes = Aes.Create();
            aes.Key = AesKey;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var enc = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(text);
            var encBytes = enc.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encBytes);
        }

        private static string DecryptFromBase64(string text)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = AesKey;
                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using var dec = aes.CreateDecryptor();
                var bytes = Convert.FromBase64String(text);
                var decBytes = dec.TransformFinalBlock(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(decBytes);
            }
            catch
            {
                return null;
            }
        }

        private static byte[] BuildAesKey()
        {
            // Short split parts (rotated per release by build script or manually)
            string p1 = "dOo";
            string p2 = "YJp";
            string p3 = "sH6";
            string p4 = "hpK";
            var combined = p1 + p2 + p3 + p4;
            var key = Encoding.UTF8.GetBytes(combined);

            if (key.Length < 16)
            {
                var padded = new byte[16];
                Array.Copy(key, padded, key.Length);
                for (int i = key.Length; i < 16; i++) padded[i] = 0x42;
                return padded;
            }
            if (key.Length > 16)
            {
                var trimmed = new byte[16];
                Array.Copy(key, trimmed, 16);
                return trimmed;
            }
            return key;
        }
    }
}










