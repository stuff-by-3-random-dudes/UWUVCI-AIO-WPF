using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.IO;

namespace UWUVCI_AIO_WPF.Models
{
    public class GctCode
    {
        public uint Address { get; set; }
        public uint Value { get; set; }

        public List<GctCode> ParseTxtFile(string txtFilePath)
        {
            var codes = new List<GctCode>();
            string[] lines = File.ReadAllLines(txtFilePath);
            var regex = new Regex(@"Address:\s*0x(?<address>[0-9A-Fa-f]+)\s+Value:\s*0x(?<value>[0-9A-Fa-f]+)");

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    uint address = Convert.ToUInt32(match.Groups["address"].Value, 16);
                    uint value = Convert.ToUInt32(match.Groups["value"].Value, 16);
                    codes.Add(new GctCode { Address = address, Value = value });
                }
            }

            return codes;
        }
        // Helper method to validate the TXT file format
        public bool IsTxtFileFormatValid(string txtFilePath)
        {
            var regex = new Regex(@"^Address:\s*0x[0-9A-Fa-f]{8}\s+Value:\s*0x[0-9A-Fa-f]{8}$");

            foreach (string line in File.ReadLines(txtFilePath))
                if (!regex.IsMatch(line.Trim()))
                    return false; // If any line doesn't match, the format is invalid

            return true; // All lines match the expected format
        }
    }

}
