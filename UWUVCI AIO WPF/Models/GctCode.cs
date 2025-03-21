using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                ".txt" => ParseOcarinaOrDolphinTxtFile(filePath).Item1,
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
        public static (List<GctCode>, string) ParseOcarinaOrDolphinTxtFile(string txtFilePath)
        {
            var codes = new List<GctCode>();
            string[] lines = File.ReadAllLines(txtFilePath);
            string gameId = null;
            string currentCodeName = null;
            bool insideGeckoSection = false;
            var codeLines = new List<string>();

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // Ignore empty lines or comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Detect Game ID (Ocarina)
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    if (!trimmedLine.Equals("[Gecko]", StringComparison.OrdinalIgnoreCase))
                    {
                        gameId = trimmedLine.Trim('[', ']');
                        Logger.Log($"Detected Game ID: {gameId}");
                    }
                    continue;
                }

                // Detect start of Gecko section (Dolphin)
                if (trimmedLine.Equals("[Gecko]", StringComparison.OrdinalIgnoreCase))
                {
                    insideGeckoSection = true;
                    continue;
                }

                // Detect new cheat code section
                if (trimmedLine.StartsWith("$"))
                {
                    // Process previous block if any
                    if (codeLines.Count > 0)
                    {
                        codes.AddRange(ParseCodeBlock(currentCodeName, codeLines));
                        codeLines.Clear();
                    }

                    currentCodeName = trimmedLine.Substring(1).Trim();
                    Logger.Log($"Detected Code Name: {currentCodeName}");
                    continue;
                }

                // Validate and collect cheat codes
                if (Regex.IsMatch(trimmedLine, @"^[0-9A-Fa-f]{8}\s+[0-9A-Fa-f]{8}$"))
                {
                    codeLines.Add(trimmedLine);
                }
                else if (insideGeckoSection)
                {
                    Logger.Log($"WARNING: Ignoring unrecognized line in Gecko section: {trimmedLine}");
                }
                else
                {
                    Logger.Log($"ERROR: Unrecognized line format in {txtFilePath}: {trimmedLine}");
                    throw new InvalidDataException($"Invalid format in {txtFilePath}: {trimmedLine}");
                }
            }

            // Process last block
            if (codeLines.Count > 0)
                codes.AddRange(ParseCodeBlock(currentCodeName, codeLines));

            if (codes.Count == 0)
                throw new InvalidDataException($"No valid cheat codes found in {txtFilePath}");

            return (codes, gameId);
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
        public static void WriteGctFile(string gctFilePath, List<GctCode> codes, string gameId = null)
        {
            if (codes == null || codes.Count == 0)
            {
                Logger.Log($"ERROR: No cheat codes available to write to {gctFilePath}");
                throw new InvalidDataException($"Cannot create {gctFilePath}: No valid codes found.");
            }

            using var fs = new FileStream(gctFilePath, FileMode.Create, FileAccess.Write);
            Logger.Log($"Writing {codes.Count} cheat codes to {gctFilePath}");

            // Optionally write Game ID if provided
            if (!string.IsNullOrEmpty(gameId))
            {
                byte[] gameIdBytes = Encoding.ASCII.GetBytes(gameId.PadRight(8, '\0')); // Ensure 8 bytes
                fs.Write(gameIdBytes, 0, 8);
            }

            foreach (var code in codes)
            {
                byte[] addressBytes = BitConverter.GetBytes(code.Address);
                byte[] valueBytes = BitConverter.GetBytes(code.Value);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(addressBytes);
                    Array.Reverse(valueBytes);
                }

                fs.Write(addressBytes, 0, 4);
                fs.Write(valueBytes, 0, 4);
            }

            // Write GCT terminator
            fs.Write(BitConverter.GetBytes(0xF0000000), 0, 4);
            fs.Write(BitConverter.GetBytes(0x00000000), 0, 4);

            Logger.Log($"GCT file successfully written to {gctFilePath}");
        }

    }
}
