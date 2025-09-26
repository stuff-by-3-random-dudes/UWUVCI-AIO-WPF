using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.Classes
{
    public class Dol
    {
        public void ApplyMultipleFilesToDol(IEnumerable<string> filePaths, string dolFilePath)
        {
            try
            {
                Logger.Log($"Starting to process {filePaths.Count()} files for DOL patching.");

                var allCodes = new List<GctCode>();

                // Load codes from all provided files
                foreach (var filePath in filePaths)
                {
                    Logger.Log($"Processing file: {filePath}");
                    var codes = GctCode.LoadFromFile(filePath);
                    Logger.Log($"Parsed {codes.Count} codes from {filePath}.");
                    allCodes.AddRange(codes);
                }

                // Validate combined codes
                if (!ValidateCodes(allCodes, dolFilePath))
                {
                    Logger.Log("Validation failed for the provided codes. Aborting patching.");
                    return;
                }

                Logger.Log($"Combined total of {allCodes.Count} codes from all files.");

                // Patch the DOL file
                PatchDolFile(dolFilePath, allCodes);

                // Inject the codehandler
                InjectCodehandler(dolFilePath, "path/to/codehandler.bin"); // Adjust the path if needed

                Logger.Log("All files applied and patched successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error applying files to DOL: {ex.Message}");
            }
        }

        public void PatchDolFile(string dolFilePath, List<GctCode> gctCodes)
        {
            try
            {
                Logger.Log($"Patching DOL file {dolFilePath} with GCT codes.");

                if (!File.Exists(dolFilePath))
                {
                    Logger.Log($"DOL file '{dolFilePath}' not found.");
                    throw new FileNotFoundException($"DOL file '{dolFilePath}' not found.");
                }

                var sections = DolSection.ReadDolHeader(dolFilePath);
                byte[] dolData = File.ReadAllBytes(dolFilePath);

                foreach (var code in gctCodes)
                {
                    int offset = MemoryToDolOffset(code.Address, sections);

                    if (offset >= 0 && offset < dolData.Length - 4) // Ensure offset is within bounds
                    {
                        byte[] valueBytes = BitConverter.GetBytes(code.Value);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(valueBytes); // Convert to big-endian

                        Array.Copy(valueBytes, 0, dolData, offset, 4);
                    }
                    else
                    {
                        Logger.Log($"Address {code.Address:X8} is out of bounds for the DOL file.");
                    }
                }

                File.WriteAllBytes(dolFilePath, dolData);

                Logger.Log($"Patched DOL saved to: {dolFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error patching DOL file: {ex.Message}");
                throw;
            }
        }

        private bool ValidateCodes(List<GctCode> codes, string dolFilePath)
        {
            Logger.Log("Validating codes before patching...");

            var sections = DolSection.ReadDolHeader(dolFilePath);

            foreach (var code in codes)
            {
                bool valid = sections.Any(section =>
                    code.Address >= section.MemoryAddress &&
                    code.Address < section.MemoryAddress + section.Size);

                if (!valid)
                {
                    Logger.Log($"Invalid code: Address {code.Address:X8} is not within any DOL section.");
                    return false;
                }
            }

            Logger.Log("All codes are valid.");
            return true;
        }

        public int MemoryToDolOffset(uint memoryAddress, List<DolSection> sections)
        {
            if (sections == null || sections.Count == 0)
            {
                Logger.Log("Sections list is null or empty.");
                throw new ArgumentException("Sections list cannot be null or empty.");
            }

            foreach (var section in sections)
            {
                if (memoryAddress >= section.MemoryAddress && memoryAddress < section.MemoryAddress + section.Size)
                {
                    return (int)(section.FileOffset + (memoryAddress - section.MemoryAddress));
                }
            }

            Logger.Log($"Memory address {memoryAddress:X} not found in any section.");
            return -1; // Address not found
        }

        public void InjectCodehandler(string dolFilePath, string codehandlerPath)
        {
            try
            {
                Logger.Log($"Starting codehandler injection for {dolFilePath}.");

                if (!File.Exists(dolFilePath) || !File.Exists(codehandlerPath))
                {
                    Logger.Log("DOL file or codehandler file not found.");
                    throw new FileNotFoundException("DOL file or codehandler file not found.");
                }

                byte[] dolData = File.ReadAllBytes(dolFilePath);
                byte[] codehandlerData = File.ReadAllBytes(codehandlerPath);

                int injectionOffset = FindFreeSpace(dolData, codehandlerData.Length);

                if (injectionOffset < 0)
                {
                    Logger.Log("No free space available for codehandler injection.");
                    throw new Exception("No free space available for codehandler injection.");
                }

                Array.Copy(codehandlerData, 0, dolData, injectionOffset, codehandlerData.Length);

                // Update entry point in DOL
                PatchDolEntryPoint(dolData, injectionOffset);

                string outputPath = Path.Combine(Path.GetDirectoryName(dolFilePath), "patched_dol.dol");
                File.WriteAllBytes(outputPath, dolData);

                Logger.Log($"Codehandler injected successfully. Patched DOL saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during codehandler injection: {ex.Message}");
                throw;
            }
        }

        private void PatchDolEntryPoint(byte[] dolData, int injectionOffset)
        {
            uint newEntryPoint = (uint)(0x80000000 + injectionOffset); // Convert offset to memory address
            Array.Copy(BitConverter.GetBytes(newEntryPoint), 0, dolData, 0xE0, 4); // Replace entry point
            Logger.Log($"Updated DOL entry point to: {newEntryPoint:X}");
        }

        private int FindFreeSpace(byte[] fileData, int requiredSize)
        {
            Logger.Log($"Searching for {requiredSize} bytes of free space in the file.");

            for (int i = 0; i < fileData.Length - requiredSize; i++)
            {
                bool isFree = true;
                for (int j = 0; j < requiredSize; j++)
                {
                    if (fileData[i + j] != 0x00)
                    {
                        isFree = false;
                        break;
                    }
                }
                if (isFree)
                {
                    Logger.Log($"Found free space at offset {i}.");
                    return i;
                }
            }

            Logger.Log("No free space found in the file.");
            return -1; // No free space found
        }
    }
}
