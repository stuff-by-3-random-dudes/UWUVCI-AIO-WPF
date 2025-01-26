using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Models
{
    public class GctCode
    {
        public uint Address { get; set; }
        public uint Value { get; set; }

        public GctCode(uint address, uint value)
        {
            Address = address;
            Value = value;
        }

        // Unified loader for GCT and TXT files
        public static List<GctCode> LoadFromFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".gct" => ParseGctFile(filePath),
                ".txt" => ParseOcarinaOrDolphinTxtFile(filePath),
                _ => throw new NotSupportedException($"Unsupported file format: {extension}")
            };
        }

        // Parse binary GCT files
        public static List<GctCode> ParseGctFile(string gctFilePath)
        {
            var codes = new List<GctCode>();
            byte[] gctData = File.ReadAllBytes(gctFilePath);

            for (int i = 0; i < gctData.Length - 8; i += 8)
            {
                uint address = BitConverter.ToUInt32(gctData, i);
                uint value = BitConverter.ToUInt32(gctData, i + 4);

                // Check for terminator
                if (address == 0xF0000000 && value == 0x00000000)
                    break;

                codes.Add(new GctCode(address, value));
            }

            Logger.Log($"Parsed {codes.Count} codes from GCT file.");
            return codes;
        }

        // Parse textual codelists from Ocarina Manager or Dolphin Emulator
        public static List<GctCode> ParseOcarinaOrDolphinTxtFile(string txtFilePath)
        {
            var codes = new List<GctCode>();
            string[] lines = File.ReadAllLines(txtFilePath);
            string currentGameId = null;
            string currentCodeName = null;
            var codeLines = new List<string>();

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // Skip empty lines or comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Detect game ID line
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentGameId = trimmedLine.Trim('[', ']');
                    Logger.Log($"Detected Game ID: {currentGameId}");
                    continue;
                }

                // Detect code name line
                if (trimmedLine.StartsWith("$"))
                {
                    // Process the previous code block if exists
                    if (codeLines.Count > 0)
                    {
                        codes.AddRange(ParseCodeBlock(currentCodeName, codeLines));
                        codeLines.Clear();
                    }

                    currentCodeName = trimmedLine.Substring(1).Trim();
                    Logger.Log($"Detected Code Name: {currentCodeName}");
                    continue;
                }

                // Collect code lines
                if (Regex.IsMatch(trimmedLine, @"^[0-9A-Fa-f]{8}\s+[0-9A-Fa-f]{8}$"))
                {
                    codeLines.Add(trimmedLine);
                }
                else
                {
                    Logger.Log($"Unrecognized line format: {trimmedLine}");
                    throw new InvalidDataException($"Unrecognized line format: {trimmedLine}");
                }
            }

            // Process the last code block
            if (codeLines.Count > 0)
            {
                codes.AddRange(ParseCodeBlock(currentCodeName, codeLines));
            }

            Logger.Log($"Parsed {codes.Count} codes from TXT file.");
            return codes;
        }

        // Helper method to parse a block of code lines into GctCode objects
        private static List<GctCode> ParseCodeBlock(string codeName, List<string> codeLines)
        {
            var parsedCodes = new List<GctCode>();

            foreach (var line in codeLines)
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Logger.Log($"Invalid code line: {line}");
                    throw new InvalidDataException($"Invalid code line: {line}");
                }

                uint address = Convert.ToUInt32(parts[0], 16);
                uint value = Convert.ToUInt32(parts[1], 16);
                parsedCodes.Add(new GctCode(address, value));
            }

            Logger.Log($"Parsed {parsedCodes.Count} codes for {codeName}.");
            return parsedCodes;
        }

        // Convert a list of GctCode to a GCT binary file
        public static void WriteGctFile(string gctFilePath, List<GctCode> codes)
        {
            using (var fs = new FileStream(gctFilePath, FileMode.Create, FileAccess.Write))
            {
                foreach (var code in codes)
                {
                    byte[] addressBytes = BitConverter.GetBytes(code.Address);
                    byte[] valueBytes = BitConverter.GetBytes(code.Value);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(addressBytes); // Convert to big-endian
                        Array.Reverse(valueBytes);
                    }

                    fs.Write(addressBytes, 0, 4);
                    fs.Write(valueBytes, 0, 4);
                }

                // Write GCT terminator
                fs.Write(BitConverter.GetBytes(0xF0000000), 0, 4);
                fs.Write(BitConverter.GetBytes(0x00000000), 0, 4);

                Logger.Log($"GCT file written to {gctFilePath}.");
            }
        }
    }
}
