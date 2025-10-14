using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Modules.Nintendont
{
    public class NintendontConfigService
    {
        // ---- Load / Save nincfg.bin ----
        public async Task<(NintendontConfig config, bool extraBytesSkipped)> LoadConfigAsync(string path)
        {
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);
            var cfg = new NintendontConfig();

            long fileLength = fs.Length;
            bool hasExtra = false;

            // Header
            cfg.Magic = ReadUInt32BE(br);
            cfg.Version = ReadUInt32BE(br);

            if (cfg.Magic != 0x01070CF6)
                throw new InvalidDataException("Invalid Nintendont config magic (expected 0x01070CF6).");

            uint configBits = ReadUInt32BE(br);
            uint videoBits = ReadUInt32BE(br);
            uint language = ReadUInt32BE(br);

            // Paths
            cfg.GamePathRaw = br.ReadBytes(256);
            cfg.CheatPathRaw = br.ReadBytes(256);

            // Body
            cfg.MaxPads = (int)ReadUInt32BE(br);
            cfg.GameId = ReadUInt32BE(br);
            cfg.MemcardBlocksIndex = br.ReadByte();
            cfg.VideoScale = (sbyte)br.ReadSByte();
            cfg.VideoOffset = (sbyte)br.ReadSByte();
            cfg.NetworkProfile = br.ReadByte();

            long bytesRead = fs.Position;

            // Optional Wii U Gamepad slot (newer 548-byte format)
            if (fileLength >= 548)
            {
                cfg.WiiUGamepadSlot = (int)ReadUInt32BE(br);
                bytesRead += 4;
            }
            else
            {
                cfg.WiiUGamepadSlot = 0;
            }

            // Any unknown trailing bytes → future format
            if (fileLength > bytesRead)
            {
                hasExtra = true;
                long remaining = fileLength - bytesRead;
                br.ReadBytes((int)remaining); // safely consume
            }

            // Translate fields
            cfg.LanguageIndex = (language == 0xFFFFFFFF) ? 0 : (int)(language + 1);
            cfg.FromBitfields(configBits, videoBits);

            // Auto width & derived width (no Math.Clamp)
            cfg.AutoVideoWidth = (cfg.VideoScale == 0);
            if (cfg.AutoVideoWidth)
            {
                cfg.VideoWidth = 0; // Auto
            }
            else
            {
                int w = 600 + cfg.VideoScale;
                cfg.VideoWidth = (w >= 640 && w <= 720) ? w : 0;
            }

            // Bounds
            cfg.MaxPads = Math.Max(1, Math.Min(4, cfg.MaxPads));

            await Task.CompletedTask;
            return (cfg, hasExtra);
        }

        public async Task SaveConfigAsync(string path, NintendontConfig cfg)
        {
            // Bitfields
            cfg.ToBitfields(out uint configBits, out uint videoBits);

            // Language (Auto = 0xFFFFFFFF, else 0..6 becomes 0..5)
            uint language = (cfg.LanguageIndex <= 0) ? 0xFFFFFFFF : (uint)(cfg.LanguageIndex - 1);

            // Video scale from width when Auto is OFF
            sbyte scale = 0;
            if (!cfg.AutoVideoWidth)
            {
                int width = (cfg.VideoWidth == 0) ? 640 : cfg.VideoWidth;
                width = Math.Max(640, Math.Min(720, width));
                scale = (sbyte)(width - 600);
            }

            // Paths
            var gamePath = cfg.GamePathRaw ?? new byte[256];
            var cheatPath = cfg.CheatPathRaw ?? new byte[256];

            using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var bw = new BinaryWriter(fs);

            // Header
            WriteUInt32BE(bw, 0x01070CF6);
            WriteUInt32BE(bw, cfg.Version == 0 ? 10u : cfg.Version);
            WriteUInt32BE(bw, configBits);
            WriteUInt32BE(bw, videoBits);
            WriteUInt32BE(bw, language);

            // Paths
            bw.Write(gamePath);   // 256
            bw.Write(cheatPath);  // 256

            // Body
            WriteUInt32BE(bw, (uint)Math.Max(1, Math.Min(4, cfg.MaxPads)));
            WriteUInt32BE(bw, cfg.GameId);
            bw.Write((byte)Math.Max(0, Math.Min(5, cfg.MemcardBlocksIndex)));
            bw.Write((sbyte)scale);
            bw.Write((sbyte)cfg.VideoOffset);
            bw.Write((byte)cfg.NetworkProfile);

            // Choose format length:
            // - If WiiUGamepadSlot is set (or Version≥10), write the 548-byte field
            bool writeWiiUSlot = (cfg.WiiUGamepadSlot > 0) || (cfg.Version >= 10);
            if (writeWiiUSlot)
            {
                WriteUInt32BE(bw, (uint)Math.Max(0, Math.Min(3, cfg.WiiUGamepadSlot))); // adds +4 bytes → 548
            }

            await fs.FlushAsync();
        }

        private static uint ReadUInt32BE(BinaryReader br)
        {
            var b = br.ReadBytes(4);

            if (b.Length != 4) 
                throw new EndOfStreamException();

            Array.Reverse(b);
            return BitConverter.ToUInt32(b, 0);
        }

        private static void WriteUInt32BE(BinaryWriter bw, uint value)
        {
            var b = BitConverter.GetBytes(value);
            Array.Reverse(b);
            bw.Write(b);
        }

        // ---- Download Nintendont (tiny; simple status text) ----
        public async Task DownloadNintendontAsync(string targetFolder)
        {
            Directory.CreateDirectory(targetFolder);

            var bootDolUrl = "https://raw.githubusercontent.com/FIX94/Nintendont/master/loader/loader.dol";
            var metaUrl = "https://raw.githubusercontent.com/FIX94/Nintendont/master/nintendont/meta.xml";
            var iconUrl = "https://raw.githubusercontent.com/FIX94/Nintendont/master/nintendont/icon.png";

            using (var http = new HttpClient())
            {
                // boot.dol
                var boot = await http.GetAsync(bootDolUrl);
                boot.EnsureSuccessStatusCode();
                var bootBytes = await boot.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(Path.Combine(targetFolder, "boot.dol"), bootBytes);

                // meta.xml
                var meta = await http.GetAsync(metaUrl);
                meta.EnsureSuccessStatusCode();
                var metaBytes = await meta.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(Path.Combine(targetFolder, "meta.xml"), metaBytes);

                // icon.png
                var icon = await http.GetAsync(iconUrl);
                icon.EnsureSuccessStatusCode();
                var iconBytes = await icon.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(Path.Combine(targetFolder, "icon.png"), iconBytes);
            }
        }
    }
}
