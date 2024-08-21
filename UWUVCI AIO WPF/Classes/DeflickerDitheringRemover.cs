using System;
using System.IO;

public static class DeflickerDitheringRemover
{
    // I'm going based on this post in gbatemp
    // https://gbatemp.net/threads/possible-to-disable-the-wiis-de-flicker-filter.477163/page-6#post-9473338
    // The only thing is I'm not sure if I need to also remove Dithering or not, so I'll leave it how it is.
    private static readonly byte[] DeflickerPattern = { 0x41, 0x82, 0x00, 0x40 }; 
    private static readonly byte[] DeflickerReplacement = { 0x48, 0x00, 0x00, 0x40 };

    private static readonly byte[] DitheringPattern = { 0x08, 0x08, 0x0A, 0x0C, 0x0A, 0x08, 0x08 };
    private static readonly byte[] DitheringReplacement = { 0x00, 0x01, 0x51, 0x61, 0x50, 0x00, 0x00 };

    public static void ApplyDeflickerPatch(byte[] buffer)
    {
        ReplacePattern(buffer, DeflickerPattern, DeflickerReplacement);
    }

    public static void ApplyDitheringPatch(byte[] buffer)
    {
        ReplacePattern(buffer, DitheringPattern, DitheringReplacement);
    }

    public static void ProcessFile(string inputFilePath, string outputFilePath, bool applyDeflicker, bool applyDithering)
    {
        if (string.IsNullOrEmpty(inputFilePath))
            throw new ArgumentException("Invalid input file path", nameof(inputFilePath));

        if (string.IsNullOrEmpty(outputFilePath))
            throw new ArgumentException("Invalid output file path", nameof(outputFilePath));

        byte[] fileBuffer = File.ReadAllBytes(inputFilePath);

        if (applyDeflicker)
            ApplyDeflickerPatch(fileBuffer);

        if (applyDithering)
            ApplyDitheringPatch(fileBuffer);

        File.WriteAllBytes(outputFilePath, fileBuffer);
    }

    private static void ReplacePattern(byte[] buffer, byte[] pattern, byte[] replacement)
    {
        for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            if (IsMatch(buffer, i, pattern))
            {
                Array.Copy(replacement, 0, buffer, i, replacement.Length);
                // Assuming only one pattern is what needs to get modified
                break;
            }
    }

    private static bool IsMatch(byte[] buffer, int position, byte[] pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
            if (buffer[position + i] != pattern[i])
                return false;
        return true;
    }
}
