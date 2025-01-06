using System.Collections.Generic;
using System.IO;

namespace UWUVCI_AIO_WPF.Models
{
    public class DolSection
    {
        public uint MemoryAddress { get; set; }
        public uint FileOffset { get; set; }
        public uint Size { get; set; }

        public static List<DolSection> ReadDolHeader(string dolFilePath)
        {
            var sections = new List<DolSection>();

            if (!File.Exists(dolFilePath))
                throw new FileNotFoundException($"The file '{dolFilePath}' was not found.");

            using (FileStream fs = new FileStream(dolFilePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                if (fs.Length < 0x100) // Minimum header size
                    throw new InvalidDataException("The file is too small to be a valid DOL file.");

                fs.Seek(0x00, SeekOrigin.Begin); // Start of DOL file header

                for (int i = 0; i < 7; i++) // Text sections
                {
                    uint offset = br.ReadUInt32();
                    uint addr = br.ReadUInt32();
                    uint size = br.ReadUInt32();
                    if (size > 0)
                        sections.Add(new DolSection { FileOffset = offset, MemoryAddress = addr, Size = size });
                }

                for (int i = 0; i < 11; i++) // Data sections
                {
                    uint offset = br.ReadUInt32();
                    uint addr = br.ReadUInt32();
                    uint size = br.ReadUInt32();
                    if (size > 0)
                        sections.Add(new DolSection { FileOffset = offset, MemoryAddress = addr, Size = size });
                }
            }

            return sections;
        }
    }

}
