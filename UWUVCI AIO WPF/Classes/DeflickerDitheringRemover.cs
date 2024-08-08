using System;
using System.IO;

public static class DeflickerDitheringRemover
{
    private static readonly byte[] Ptr = { 0x3C, 0x80, 0xCC, 0x01, 0x38, 0xA0, 0x00, 0x61, 0x38, 0x00, 0x00, 0x00, 0x80, 0xC7, 0x02, 0x20,
                                           0x50, 0x66, 0x17, 0x7A, 0x98, 0xA4, 0x80, 0x00, 0x90, 0xC4, 0x80, 0x00, 0x90, 0xC7, 0x02, 0x20,
                                           0xB0, 0x07, 0x00, 0x02, 0x4E, 0x80, 0x00, 0x20 };

    private static readonly byte[] PtrNoDither = { 0x48, 0x00, 0x00, 0x28 };

    private static readonly byte[] PtrOldProgSf = { 0x08, 0x08, 0x0A, 0x0C, 0x0A, 0x08, 0x08 };
    private static readonly byte[] PtrOldProgAa = { 0x05, 0x08, 0x0C, 0x10, 0x0C, 0x08, 0x04 };
    private static readonly byte[] PtrOldMkwii1 = { 0x07, 0x07, 0x0C, 0x0C, 0x0C, 0x07, 0x07 };
    private static readonly byte[] PtrOldMkwii2 = { 0x05, 0x05, 0x0F, 0x0E, 0x0F, 0x05, 0x05 };
    private static readonly byte[] PtrOldGalaxy = { 0x20, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00 };
    private static readonly byte[] PtrNewNoFilt = { 0x00, 0x00, 0x15, 0x16, 0x15, 0x00, 0x00 };
    private static readonly byte[] PtrPreceding = { 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06 };

    private static readonly byte[][] DeflickerPatterns =
    {
        PtrOldProgSf,
        PtrOldProgAa,
        PtrOldMkwii1,
        PtrOldMkwii2,
        PtrOldGalaxy
    };

    public static void RemoveDeflicker(byte[] buffer)
    {
        for (int i = 7; i < buffer.Length - PtrNewNoFilt.Length; i++)
            if (IsAnyMatch(buffer, i, DeflickerPatterns) && IsMatch(buffer, i - 7, PtrPreceding))
                Array.Copy(PtrNewNoFilt, 0, buffer, i, PtrNewNoFilt.Length);
    }

    public static void RemoveDithering(byte[] buffer)
    {
        for (int i = 0; i < buffer.Length - Ptr.Length; i++)
            if (IsMatch(buffer, i, Ptr))
                Array.Copy(PtrNoDither, 0, buffer, i - 4, PtrNoDither.Length);
    }

    public static void ProcessFile(string inputFilePath, string outputFilePath, bool applyDeflicker, bool applyDithering)
    {
        if (string.IsNullOrEmpty(inputFilePath)) 
            throw new ArgumentException("Invalid input file path", nameof(inputFilePath));

        if (string.IsNullOrEmpty(outputFilePath)) 
            throw new ArgumentException("Invalid output file path", nameof(outputFilePath));

        byte[] fileBuffer = File.ReadAllBytes(inputFilePath);

        if (applyDeflicker)
            RemoveDeflicker(fileBuffer);

        if (applyDithering)
            RemoveDithering(fileBuffer);

        File.WriteAllBytes(outputFilePath, fileBuffer);
    }

    private static bool IsMatch(byte[] buffer, int position, byte[] pattern)
    {
        if (position < 0 || position + pattern.Length > buffer.Length)
            return false;

        for (int i = 0; i < pattern.Length; i++)
            if (buffer[position + i] != pattern[i])
                return false;

        return true;
    }

    private static bool IsAnyMatch(byte[] buffer, int position, byte[][] patterns)
    {
        foreach (var pattern in patterns)
            if (IsMatch(buffer, position, pattern))
                return true;

        return false;
    }
}
