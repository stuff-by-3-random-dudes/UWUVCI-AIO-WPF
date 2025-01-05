using System;
using System.Collections.Generic;
using System.IO;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Classes
{
    public class Dol
    {
        public int MemoryToDolOffset(uint memoryAddress, List<DolSection> sections)
        {
            foreach (var section in sections)
                if (memoryAddress >= section.MemoryAddress && memoryAddress < section.MemoryAddress + section.Size)
                    return (int)(section.FileOffset + (memoryAddress - section.MemoryAddress));

            return -1; // Address not found
        }
        public List<GctCode> ParseGctFile(string gctFilePath)
        {
            var codes = new List<GctCode>();
            byte[] gctData = File.ReadAllBytes(gctFilePath);

            for (int i = 0; i < gctData.Length; i += 8)
            {
                if (i + 7 >= gctData.Length) break;
                uint address = BitConverter.ToUInt32(gctData, i);
                uint value = BitConverter.ToUInt32(gctData, i + 4);
                codes.Add(new GctCode { Address = address, Value = value });
            }
            return codes;
        }

        public void PatchDolFile(string dolFilePath, List<GctCode> gctCodes)
        {
            var sections = new DolSection().ReadDolHeader(dolFilePath);
            byte[] dolData = File.ReadAllBytes(dolFilePath);

            foreach (var code in gctCodes)
            {
                int offset = MemoryToDolOffset(code.Address, sections);

                if (offset >= 0 && offset < dolData.Length - 4) // Ensure offset is within bounds
                {
                    byte[] valueBytes = BitConverter.GetBytes(code.Value);
                    Array.Reverse(valueBytes); // Convert to big-endian if needed
                    Array.Copy(valueBytes, 0, dolData, offset, 4);
                }
                else
                {
                    Console.WriteLine($"Address {code.Address:X} out of bounds for DOL file.");
                }
            }

            // Save the patched DOL
            File.WriteAllBytes("patched_dol.dol", dolData);
        }

        public void ApplyGctToDol(string gctFilePath, string dolFilePath)
        {
            var gctCodes = ParseGctFile(gctFilePath);
            PatchDolFile(dolFilePath, gctCodes);
        }

    }
}
