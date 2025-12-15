using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Classes
{
    internal static class NesPalettePatcher
    {
        private static readonly byte[] SearchPattern = HexToBytes("4A4A4AFF00006AFF080062FF29005AFF41004AFF4A0000FF410000FF291000FF182900FF003110FF003100FF002910FF002041FF000000FF000000FF000000FF737373FF003183FF3100ACFF4A0094FF62007BFF6A0039FF6A2000FF5A3100FF414A00FF185A00FF105A00FF005A31FF004A5AFF101010FF000000FF000000FFACACACFF4A73B4FF625AD5FF8352E6FFA452ACFFAC4A83FFB4624AFF947331FF7B7329FF5A8300FF398B31FF318B5AFF398B8BFF393939FF000000FF000000FFB4B4B4FF8B9CB4FF8B8BACFF9C8BBDFFA483BDFFAC8B9CFFAC948BFF9C8B7BFF9C9C73FF94A47BFF83A47BFF7B9C83FF73948BFF8B8B8BFF000000FF000000FF");

        private static readonly Dictionary<string, byte[]> PaletteMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["2C03 Palette"] = HexToBytes("B5 AD 80 92 80 1B B5 3B C8 0D D8 0D D8 80 C9 20 B5 20 91 20 81 A4 82 40 81 29 80 00 80 00 80 00 DA D6 81 BB 81 3F C8 1F D8 1F FC 12 FC 00 ED A0 C9 A0 92 40 82 40 82 CD 82 52 80 00 80 00 80 00 FF FF B6 DF CA 5F ED BF FC 1F FD BF FE 40 FE C0 EF 60 B7 60 83 E0 A7 FB 83 FF 80 00 80 00 80 00 FF FF DB 7F EE DF FE DF FE 5F FE D6 FF 72 FF E9 FF ED DB E9 CB ED A7 FB CB 7F 80 00 80 00 80 00"),
            ["Fx3 NES Palette"] = HexToBytes("BE 10 80 1F 80 18 A0 B8 C8 11 D4 05 D4 40 C4 60 A8 C0 81 E0 81 A0 81 60 81 0B 80 00 80 00 80 01 DF 18 81 FF 82 3F B5 3F EC 1A F0 0C FC E0 F1 83 D6 00 82 E0 82 A0 82 A9 82 32 94 A5 80 00 80 00 FF FF 9F 1F B6 3F CD FF FD FF FD 73 FD EB FE 89 FE E0 DF E3 AF 6B AF F3 83 BC B1 8C 80 00 80 00 FF FF D3 BF DE FF EE FF FE FF FB 1C FB 56 FF 96 FF 70 EF EF DF EF DB DB 83 FF E7 18 80 00 80 00"),
            ["RGBSource NESCAP"] = HexToBytes("B1 8C 80 50 8C 12 9C 10 A8 0B AC 03 A4 20 9C 60 8C C0 80 E0 81 00 80 E3 80 AA 80 00 80 00 80 00 D6 B5 89 39 A0 BC B4 7B C8 56 CC 6B C8 C0 BD 20 AD 80 91 E0 81 E0 81 E7 81 B1 80 00 80 00 80 00 FF FF B2 9F C6 1F D9 DF F1 BF F9 B8 F5 EC EE 44 DE C0 C3 20 AB 47 A3 2F A7 19 A5 29 80 00 80 00 FF FF E3 9F EB 5F F3 3F FB 3F FF 1D FF 38 FB 55 F7 93 EB B3 E3 D6 DF D9 DF BD DE F7 80 00 80 00"),
            ["NES Palette"] = HexToBytes("BD EF 80 1F 80 17 A0 B7 C8 10 D4 04 D4 40 C4 40 A8 C0 81 E0 81 A0 81 60 81 0B 80 00 80 00 80 00 DE F7 81 FF 81 7F B5 1F EC 19 F0 0B FC E0 F1 62 D5 E0 82 E0 82 A0 82 A8 82 31 80 00 80 00 80 00 FF FF 9E FF B6 3F CD FF FD FF FD 73 FD EB FE 88 FE E0 DF E3 AF 6A AF F3 83 BB BD EF 80 00 80 00 FF FF D3 9F DE FF EE FF FE FF FE 98 FB 56 FF 95 FF 6F EF EF DF F7 DF FB 83 FF FF 7F 80 00 80 00"),
            ["PAL NES Palette"] = HexToBytes("C2 10 80 17 98 17 C0 14 DC 0D D8 03 D8 00 C8 80 BC A0 80 E0 81 21 80 E4 80 AC 80 00 80 00 80 00 E7 39 81 7F A0 FF D8 D9 FC D5 FC CB FC C3 E9 20 E1 80 9D E0 8E 02 82 4C 82 18 88 42 84 21 84 21 FF FF 82 5F B6 1F E9 BF FD D9 FD B3 FD EB FE 4B FE 86 D2 E0 AB 6D A7 55 83 7F B1 8C 84 21 84 21 FF FF C2 FF DE FF EA FF FE FD FE F9 FF 16 FF 35 FF 74 E7 93 D7 B6 D7 FD DB BF EF 7B 88 42 88 42"),
            ["Wavebeam (Alt)"] = HexToBytes("B5 AD 80 70 84 13 A0 11 B4 0D B8 04 B4 00 A8 40 94 C0 81 20 81 41 81 24 80 CB 80 00 80 00 80 00 DA D6 89 7A 9C BD B4 9B D0 58 DC 2C D8 A0 C9 20 B1 A0 8E 00 82 40 82 28 81 F2 88 42 80 00 80 00 FF FF B2 DF BE 7F E1 FF F5 DF F9 D8 FA 2C F2 86 E3 00 C7 41 AF 67 A7 71 A3 5B A5 29 80 00 80 00 FF FF DB 7F E7 3F EB 1F F6 FF FF 1B FF 38 FB 75 FB B3 EB D4 DF D6 DB D9 D7 BE DE F7 80 00 80 00"),
            ["Nestopia YUV"] = HexToBytes("B1 8C 80 B1 88 54 9C 14 AC 0F B4 08 B4 00 A8 60 98 C0 85 20 81 40 81 21 81 09 80 00 80 00 80 00 D6 B5 89 7B A1 1F B8 9F D0 79 D8 6F D8 C4 CD 20 B5 A0 9E 00 86 40 82 26 81 F1 80 00 80 00 80 00 FF FF B2 DF CA 5F E1 DF F9 BF FD B9 FE 0E F6 64 DE E0 C7 60 AF 86 A3 90 A7 3B A5 29 80 00 80 00 FF FF E3 7F EB 5F F7 3F FF 1F FF 1D FF 38 FB 74 F3 92 E7 B2 DF D5 DB D9 DB BE DE F7 80 00 80 00"),
            ["Nestopia RGB"] = HexToBytes("B5 AD 80 92 80 1B B5 3B C8 0D D8 0D D8 80 C9 20 B5 20 91 20 81 A4 82 40 81 29 80 00 80 00 80 00 DA D6 81 BB 81 3F C8 1F D8 1F FC 12 FC 00 ED A0 C9 A0 92 40 82 40 82 CD 82 52 90 84 80 00 80 00 FF FF B6 DF CA 5F ED BF FC 1F FD BF FE 40 FE C0 EF 60 B7 60 83 E0 A7 FB 83 FF A5 29 80 00 80 00 FF FF DB 7F EE DF FE DF FE 5F FE D6 FF 72 FF E9 FF ED DB E9 CB ED A7 FB CB 7F CA 52 80 00 80 00"),
            ["Original Wii VC"] = HexToBytes("A5 29 80 0D 84 0C 94 0B A0 09 A4 00 A0 00 94 40 8C 80 80 C2 80 C0 80 A2 80 88 80 00 80 00 80 00 B9 CE 80 D0 98 15 A4 12 B0 0F B4 07 B4 80 AC A0 A1 20 8D 60 89 60 81 66 81 2B 88 42 80 00 80 00 D6 B5 A5 D6 B1 7A C1 5C D1 55 D5 30 D9 89 C9 C6 BD C5 AE 00 9E 26 9A 2B 9E 31 9C E7 80 00 80 00 DA D6 C6 76 C6 35 CE 37 D2 17 D6 33 D6 51 CE 2F CE 6E CA 8F C2 8F BE 70 BA 51 EF 7B 80 00 80 00"),
            ["Restored Wii VC (SuperrSonic)"] = HexToBytes("B5 AD 80 13 84 11 9C 0F AC 0D B4 00 AC 00 9C 60 90 E0 81 02 81 00 80 E2 80 AC 80 00 80 00 80 00 D2 94 81 17 A0 1E B4 1A C4 16 CC 0A CC A0 C1 00 AD A0 92 00 89 E0 81 E9 81 B0 88 42 80 00 80 00 FF FF B6 9F C5 FF DD DF F5 DF FD B7 FE 2D EA 89 DA 87 C3 00 AB 28 A3 30 AB 39 A9 4A 80 00 80 00 FF FF E7 9F E7 3F EF 3F F7 1F FF 3C FF 59 EF 36 EF 94 EB B6 DF B6 DB 97 D3 59 EF 7B 80 00 80 00"),
            ["FBX Composite Direct"] = HexToBytes("B1 8C 80 4F 8C 11 98 10 A8 0B AC 03 A4 00 9C 60 8C C0 80 E0 81 00 80 E2 80 AA 80 00 80 00 80 00 D6 B5 8D 39 A0 BC B4 7A C8 75 CC 6B CC C0 BD 20 AD 80 91 E0 82 00 81 E7 81 B1 80 00 80 00 80 00 FF FF B2 BF C6 3F D9 DF F1 BF F5 B8 FA 0D EE 65 DE C1 C3 21 AF 47 A7 4F A7 19 A5 29 80 00 80 00 FF FF E3 9F EF 7F F7 5F FF 3F FF 3E FF 59 FF 76 F7 B4 EB D4 E3 F7 DF DA DF DE DE F7 80 00 80 00"),
            ["FBX NES Classic Mini"] = HexToBytes("B1 8C 80 11 8C 33 98 4F A8 4C AC 02 A8 20 9C 81 90 C1 85 01 89 02 80 E3 80 AA 80 00 80 00 80 00 D6 B5 85 38 A4 9B B4 59 C8 55 CC 69 C8 C0 B9 40 AD A2 89 E2 8A 01 89 C9 8D 92 80 00 80 00 80 00 FF FF B2 7F C5 FF D9 BF ED BE F1 D5 F2 0B E6 64 D6 C0 BB 00 AF 29 9B 11 A6 F9 A1 08 80 00 80 00 FF FF DF 5F E7 3F EF 1F F7 1F FF 1C FB 38 F3 34 EF 73 E7 93 DF 97 DB B9 DB 9D D6 B5 80 00 80 00"),
            ["Wavebeam"] = HexToBytes("B5 AD 80 71 90 13 A0 11 B0 0C B0 03 AC 20 A4 40 94 C0 81 00 81 21 81 03 80 CB 80 00 80 00 80 00 DA D6 89 5A A0 DD B8 9B CC 77 D4 2C D4 A0 C5 20 B1 A0 92 00 82 20 82 08 81 D2 80 00 80 00 80 00 FF FF B2 DF BE 7F E1 FF F5 DF F9 D9 FA 2D EE 85 DE E1 C7 41 AF 67 A7 70 A7 3A A9 4A 80 00 80 00 FF FF DF 7F EB 5F F3 3F F7 1F FF 1C FF 38 FB 75 F7 94 EB B4 DF D6 DB D9 DB BE DE F7 80 00 80 00"),
            ["3DS VC (No Dark Filter)"] = HexToBytes("B9 CE 90 71 80 15 A0 13 C4 0E D4 02 D0 00 BC 20 A0 A0 81 00 81 40 80 E2 8C EB 80 00 80 00 80 00 DE F7 81 DD 90 FD C0 1E DC 17 F0 0B EC A0 E5 21 C5 C0 82 40 82 A0 82 47 82 11 88 42 80 00 80 00 FF FF 9E FF AE 5F D2 3F F9 FF FD D6 FD CC FE 67 FA E7 C3 42 A7 69 AF F3 83 BB 9C E7 80 00 80 00 FF FF D7 9F E3 5F EB 3F FF 1F FF 1B FE F6 FF 75 FF 94 F3 F4 D7 D7 DB F9 CF FE C6 31 80 00 80 00"),
            ["Animal Crossing Emulator"] = HexToBytes("C2 10 80 17 98 17 C0 14 DC 0D D8 03 D8 00 C8 80 BC A0 80 E0 81 21 80 E4 80 AC 80 00 80 00 80 00 E7 39 81 7F A0 FF D8 D9 FC D5 FC CB FC C3 E9 20 E1 80 9D E0 8E 02 82 4C 82 18 88 42 80 00 80 00 FF FF 82 5F B6 1F E9 BF FD D9 FD B3 FD EB FE 4B FE 86 D2 E0 AB 6D A7 55 83 7F B1 8C 80 00 80 00 FF FF C2 FF DE FF EA FF FE FD FE F9 FF 16 FF 35 FF 74 E7 93 D7 B6 D7 DD DB BF EF 7B 80 00 80 00"),
            ["FCEUX Colorful"] = HexToBytes("B9 CE 90 71 80 15 A0 13 C4 0E D4 02 D0 00 BC 20 A0 A0 81 00 81 40 80 E2 8C EB 80 00 80 00 80 00 DE F7 81 DD 90 FD C0 1E DC 17 F0 0B EC A0 E5 21 C5 C0 82 40 82 A0 82 47 82 11 80 00 80 00 80 00 FF FF 9E FF AE 5F E6 3F F9 FF FD D6 FD CC FE 67 FA E7 C3 42 A7 69 AF F3 83 BB BD EF 80 00 80 00 FF FF D7 9F E3 5F EB 3F FF 1F FF 1B FE F6 FF 75 FF 94 F3 F4 D7 D7 DB F9 CF FE E3 18 80 00 80 00"),
            ["NES Remix U"] = HexToBytes("B5 AD 94 53 94 35 AC 72 C4 8E CC 66 C8 C1 B9 00 A1 40 9D 81 9D 81 99 68 99 2E 80 00 80 00 80 00 D2 94 9D 5A 98 DB BC D9 DC D4 E0 CA E1 00 D1 60 BD E0 A2 20 A2 41 A2 2B 99 F2 B5 AD 80 00 80 00 EF 7B AE 3B A1 DB C5 9B E5 5B ED 52 ED A6 EE 20 E2 C3 C7 00 B3 27 AF 10 AA D7 D2 94 80 00 80 00 EF 7B C2 FB C6 9B D6 7B E2 7B EE 78 EE B3 EF 10 EF 2F DB 4E C7 51 C7 76 C7 79 EF 7B 80 00 80 00"),
            ["Original Mega Man"] = HexToBytes("B5 AD 80 14 8C 50 9C 12 C0 0D D0 01 CC 00 B8 00 9C 80 80 E0 81 20 80 C1 88 CA 80 00 80 00 80 00 DA D6 81 BC 8C DC BC 1D D8 16 EC 0A E8 80 E1 00 C1 A0 82 20 82 80 82 26 81 F0 80 00 80 00 80 00 FB DE 9A DE AA 3E CE 1E F5 DE F9 B5 F9 AB FA 46 F6 C6 BF 22 A3 48 AB D2 83 9A A5 29 80 00 80 00 FB DE D3 7E DF 3E E7 1E FA FE FA FA FA D5 FB 54 FB 73 EF D3 D3 B6 D7 D8 CB DD CA 52 80 00 80 00"),
            ["Rockman 9 Modern"] = HexToBytes("B9 CE 80 15 90 71 A0 13 C4 0E D4 02 D0 00 BC 20 A0 A0 81 00 81 40 80 E2 8C EB 80 00 80 00 80 00 DE F7 81 DD 90 FD C0 1E DC 17 F0 0B EC A0 E5 21 C5 C0 82 40 82 A0 82 47 82 11 80 00 80 00 80 00 FF FF 9E FF AE 5F D2 3F F9 FF FD D6 FD CC FE 67 FA E7 C3 42 A7 69 AF F3 9E FF A9 4A 80 00 80 00 FF FF D7 9F E3 5F EB 3F FF 1F FF 1B FE F6 FF 75 FF 94 F3 F4 D7 D7 DB F9 CF FE CE 73 80 00 80 00"),
            ["MM Legacy 3DS Modern"] = HexToBytes("B5 AD 80 B1 84 35 9C 15 B0 10 B4 05 B4 00 A8 60 94 C0 85 00 81 20 80 E4 80 CB 80 00 88 42 88 42 DE F7 85 5A 94 BF B8 BF D4 7A E0 90 DC C4 D1 40 B9 C0 92 20 8A 60 82 48 82 14 88 42 88 42 88 42 FF FF B6 FF CE 7F E2 5F F1 DF FD 9A FE 2E FE C6 F3 40 D3 84 B3 A6 9B B2 B6 FF B5 AD 88 42 88 42 FF FF E7 9F EF 7F F7 5F FF 3F FF 5E FF 9B FB 97 FB B6 F7 F7 EB F8 DF DA E3 DF DE F7 88 42 88 42")
        };

        public static void Apply(string rpxPath, string paletteName, string defaultPaletteName = "Default (Base RPX)")
        {
            if (string.IsNullOrWhiteSpace(rpxPath) || string.IsNullOrWhiteSpace(paletteName))
                return;
            if (string.Equals(paletteName, defaultPaletteName, StringComparison.OrdinalIgnoreCase))
                return;
            if (!File.Exists(rpxPath))
                return;
            if (!PaletteMap.TryGetValue(paletteName, out var paletteBytes))
                return;

            var rgbaPalette = EnsureRgba8(paletteBytes);

            var data = File.ReadAllBytes(rpxPath);
            int idx = IndexOfPattern(data, SearchPattern);
            if (idx < 0)
            {
                try { Logger.Log($"[NES Palette] Pattern not found in {rpxPath}"); } catch { }
                return;
            }

            Array.Copy(rgbaPalette, 0, data, idx, rgbaPalette.Length);
            File.WriteAllBytes(rpxPath, data);
        }

        private static byte[] EnsureRgba8(byte[] paletteBytes)
        {
            if (paletteBytes.Length == SearchPattern.Length)
                return paletteBytes;

            if (paletteBytes.Length * 2 == SearchPattern.Length)
                return ConvertRgb5A3ToRgba8(paletteBytes);

            if (paletteBytes.Length % 3 == 0 && (paletteBytes.Length / 3) * 4 == SearchPattern.Length)
                return ConvertRgb8ToRgba8(paletteBytes);

            throw new InvalidOperationException($"Palette byte length {paletteBytes.Length} does not match expected formats.");
        }

        private static int IndexOfPattern(byte[] buffer, byte[] pattern)
        {
            if (buffer == null || pattern == null || pattern.Length == 0 || buffer.Length < pattern.Length)
                return -1;

            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                int j = 0;
                for (; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                        break;
                }
                if (j == pattern.Length)
                    return i;
            }
            return -1;
        }

        private static byte[] HexToBytes(string hex)
        {
            hex = Regex.Replace(hex ?? string.Empty, "[^0-9A-Fa-f]", "");
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must contain an even number of characters", nameof(hex));

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static byte[] ConvertRgb5A3ToRgba8(byte[] rgb5a3)
        {
            if (rgb5a3 == null || rgb5a3.Length % 2 != 0)
                throw new ArgumentException("RGB5A3 palette must be even length.", nameof(rgb5a3));

            int colorCount = rgb5a3.Length / 2;
            var rgba = new byte[colorCount * 4];

            for (int i = 0; i < colorCount; i++)
            {
                int hi = rgb5a3[i * 2] & 0xFF;
                int lo = rgb5a3[i * 2 + 1] & 0xFF;
                int value = (hi << 8) | lo;

                byte a, r, g, b;
                if ((value & 0x8000) != 0)
                {
                    // 1rrrrr ggggg bbbbb (no alpha, 5 bits each)
                    a = 0xFF;
                    r = (byte)((value >> 10) & 0x1F);
                    g = (byte)((value >> 5) & 0x1F);
                    b = (byte)(value & 0x1F);

                    r = (byte)((r << 3) | (r >> 2));
                    g = (byte)((g << 3) | (g >> 2));
                    b = (byte)((b << 3) | (b >> 2));
                }
                else
                {
                    // 0aaa rrrr gggg bbbb (3-bit alpha, 4-bit RGB)
                    int a3 = (value >> 12) & 0x7;
                    a = (byte)((a3 << 5) | (a3 << 2) | (a3 >> 1));

                    r = (byte)((value >> 8) & 0x0F);
                    g = (byte)((value >> 4) & 0x0F);
                    b = (byte)(value & 0x0F);

                    r = (byte)((r << 4) | r);
                    g = (byte)((g << 4) | g);
                    b = (byte)((b << 4) | b);
                }

                int o = i * 4;
                rgba[o] = r;
                rgba[o + 1] = g;
                rgba[o + 2] = b;
                rgba[o + 3] = a;
            }

            return rgba;
        }

        private static byte[] ConvertRgb8ToRgba8(byte[] rgb)
        {
            if (rgb == null || rgb.Length % 3 != 0)
                throw new ArgumentException("RGB8 palette length must be divisible by 3.", nameof(rgb));

            int colorCount = rgb.Length / 3;
            var rgba = new byte[colorCount * 4];
            for (int i = 0, o = 0; i < rgb.Length; i += 3, o += 4)
            {
                rgba[o] = rgb[i];
                rgba[o + 1] = rgb[i + 1];
                rgba[o + 2] = rgb[i + 2];
                rgba[o + 3] = 0xFF;
            }
            return rgba;
        }
    }
}
