using System;
using System.Text;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class HashCompat
    {
        // Emulates .NET Framework non-randomized string.GetHashCode variant that
        // mixes 32-bit chunks (historically observed on x86 builds). This is not
        // the same as the char-by-char form and can differ from x64.
        public static int NetFxStringHash32_X86(string s)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;
                int length = s.Length;
                if (length == 0) return hash1;

                int intCount = (length + 1) / 2;
                int[] buffer = new int[intCount + 1];
                Buffer.BlockCopy(s.ToCharArray(), 0, buffer, 0, length * sizeof(char));

                int index = 0;
                int remaining = length;
                while (remaining > 3)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ buffer[index++];
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ buffer[index++];
                    remaining -= 4;
                }

                if (remaining > 0)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ buffer[index++];
                }
                if (remaining > 2)
                {
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ buffer[index];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static int LegacyClr32(string s)
        {
            // Mirrors legacy CLR string.GetHashCode (x86-era), deterministic across architectures
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;
                for (int i = 0; i < s.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ s[i];
                    if (i == s.Length - 1) break;
                    hash2 = ((hash2 << 5) + hash2) ^ s[i + 1];
                }
                return hash1 + (hash2 * 1566083941);
            }
        }

        public static int JavaHash32(string s)
        {
            unchecked
            {
                int h = 0;
                for (int i = 0; i < s.Length; i++) h = 31 * h + s[i];
                return h;
            }
        }

        public static int Crc32(string s)
        {
            uint[] table = _crcTable;
            if (table == null) table = _crcTable = BuildCrc32Table();
            uint crc = 0xFFFFFFFFu;
            var bytes = Encoding.ASCII.GetBytes(s);
            for (int i = 0; i < bytes.Length; i++)
            {
                uint idx = (crc ^ bytes[i]) & 0xFF;
                crc = (crc >> 8) ^ table[idx];
            }
            crc ^= 0xFFFFFFFFu;
            return unchecked((int)crc);
        }

        public static int Fnv1a32(string s)
        {
            unchecked
            {
                uint hash = 0x811C9DC5u;
                var bytes = Encoding.ASCII.GetBytes(s);
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= 0x01000193u;
                }
                return unchecked((int)hash);
            }
        }

        public static int Crc32(byte[] bytes)
        {
            uint[] table = _crcTable;
            if (table == null) table = _crcTable = BuildCrc32Table();
            uint crc = 0xFFFFFFFFu;
            for (int i = 0; i < bytes.Length; i++)
            {
                uint idx = (crc ^ bytes[i]) & 0xFF;
                crc = (crc >> 8) ^ table[idx];
            }
            crc ^= 0xFFFFFFFFu;
            return unchecked((int)crc);
        }

        public static int Fnv1a32(byte[] bytes)
        {
            unchecked
            {
                uint hash = 0x811C9DC5u;
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= 0x01000193u;
                }
                return unchecked((int)hash);
            }
        }

        public static int LegacyClr32(byte[] bytes)
        {
            // Interpret each byte as a char and apply the same algorithm
            var chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++) chars[i] = (char)bytes[i];
            return LegacyClr32(new string(chars));
        }

        public static int JavaHash32(byte[] bytes)
        {
            unchecked
            {
                int h = 0;
                for (int i = 0; i < bytes.Length; i++) h = 31 * h + bytes[i];
                return h;
            }
        }

        private static uint[] _crcTable;
        private static uint[] BuildCrc32Table()
        {
            var table = new uint[256];
            const uint poly = 0xEDB88320u;
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int k = 0; k < 8; k++)
                {
                    if ((crc & 1) != 0) crc = poly ^ (crc >> 1);
                    else crc >>= 1;
                }
                table[i] = crc;
            }
            return table;
        }

        public static bool MatchesAnyLegacyHash(string key, int expected)
        {
            // Try common 32-bit hash variants that may have been used historically
            if (expected == key.GetHashCode()) return true;
            string lower = key.ToLowerInvariant();
            string upper = key.ToUpperInvariant();
            if (expected == lower.GetHashCode() || expected == upper.GetHashCode()) return true;
            if (expected == LegacyClr32(key) || expected == LegacyClr32(lower) || expected == LegacyClr32(upper)) return true;
            // Explicitly try the historical x86 mixing form as observed in .NET Framework builds
            if (expected == NetFxStringHash32_X86(key) || expected == NetFxStringHash32_X86(lower) || expected == NetFxStringHash32_X86(upper)) return true;
            if (expected == JavaHash32(lower) || expected == JavaHash32(upper)) return true;
            if (expected == Crc32(lower) || expected == Crc32(upper)) return true;
            if (expected == Fnv1a32(lower) || expected == Fnv1a32(upper)) return true;

            // Also try interpreting hex string as bytes
            try
            {
                var bytes = HexToBytes(RemoveHexSeparators(key));
                if (bytes != null)
                {
                    if (expected == Crc32(bytes)) return true;
                    if (expected == Fnv1a32(bytes)) return true;
                    if (expected == LegacyClr32(bytes)) return true;
                    if (expected == JavaHash32(bytes)) return true;

                    try
                    {
                        using (var sha1 = System.Security.Cryptography.SHA1.Create())
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            var s1 = sha1.ComputeHash(bytes);
                            var m5 = md5.ComputeHash(bytes);

                            int sha1_le = unchecked((int)(s1[0] | (s1[1] << 8) | (s1[2] << 16) | (s1[3] << 24)));
                            int sha1_be = unchecked((int)((s1[0] << 24) | (s1[1] << 16) | (s1[2] << 8) | s1[3]));
                            int md5_le  = unchecked((int)(m5[0] | (m5[1] << 8) | (m5[2] << 16) | (m5[3] << 24)));
                            int md5_be  = unchecked((int)((m5[0] << 24) | (m5[1] << 16) | (m5[2] << 8) | m5[3]));

                            if (expected == sha1_le || expected == sha1_be || expected == md5_le || expected == md5_be)
                                return true;
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return false;
        }

        private static string RemoveHexSeparators(string s)
        {
            // remove spaces and dashes commonly used in hex formatting
            return s.Replace(" ", string.Empty).Replace("-", string.Empty);
        }

        private static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return null;
            if ((hex.Length & 1) == 1) return null;
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                if (!byte.TryParse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[i]))
                    return null;
            }
            return bytes;
        }
    }
}
