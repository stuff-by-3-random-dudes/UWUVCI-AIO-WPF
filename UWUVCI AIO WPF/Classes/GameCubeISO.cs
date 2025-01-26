using System;
using System.Collections.Generic;
using System.IO;

namespace UWUVCI_AIO_WPF.Classes
{
    public class GameCubeISO
    {
        private TOCManager TOCManager { get; set; } = new TOCManager();
        public string IsoPath { get; private set; }
        public string ExtractedPath { get; private set; }

        public GameCubeISO(string isoPath)
        {
            if (string.IsNullOrWhiteSpace(isoPath) || !File.Exists(isoPath))
                throw new FileNotFoundException("ISO file not found.");
            IsoPath = isoPath;
        }

        /// <summary>
        /// Extracts the ISO into a directory.
        /// </summary>
        public void Extract(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty.");

            ExtractedPath = outputPath;
            Directory.CreateDirectory(ExtractedPath);

            // Path to the TOC file
            string tocPath = Path.Combine(ExtractedPath, "&&systemdata", "Game.toc");
            if (!File.Exists(tocPath))
                throw new FileNotFoundException("Game.toc not found in the extracted directory.");

            Console.WriteLine("Parsing TOC...");
            ParseTOCFromFile(tocPath);

            if (TOCManager.TOCEntries.Count == 0)
                throw new InvalidOperationException("No valid entries found in the TOC.");

            Console.WriteLine($"Found {TOCManager.TOCEntries.Count} entries in the TOC.");

            using var fs = new FileStream(IsoPath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs);

            Console.WriteLine("Extracting files...");
            TOCManager.ExtractFiles(reader, ExtractedPath);

            Console.WriteLine($"Extraction complete: {ExtractedPath}");
        }

        /// <summary>
        /// Rebuilds the ISO from the extracted directory.
        /// </summary>
        public void Rebuild(string extractedPath, string outputIsoPath)
        {
            if (!Directory.Exists(extractedPath))
                throw new DirectoryNotFoundException($"Extracted directory not found: {extractedPath}");

            // Paths for essential system files
            string systemDataPath = Path.Combine(extractedPath, "&&systemdata");
            if (!Directory.Exists(systemDataPath))
                throw new DirectoryNotFoundException($"System data directory not found: {systemDataPath}");

            string hdrFilePath = Path.Combine(systemDataPath, "ISO.hdr");
            string dolFilePath = Path.Combine(systemDataPath, "Start.dol");
            string apploaderFilePath = Path.Combine(systemDataPath, "AppLoader.ldr");
            string tocFilePath = Path.Combine(systemDataPath, "Game.toc");

            if (!File.Exists(hdrFilePath) || !File.Exists(dolFilePath) || !File.Exists(apploaderFilePath) || !File.Exists(tocFilePath))
                throw new FileNotFoundException("One or more essential system files are missing.");

            Console.WriteLine("Rebuilding ISO...");

            using var writer = new BinaryWriter(File.Create(outputIsoPath));

            // Write the ISO header
            Console.WriteLine("Writing ISO header...");
            WriteSystemFile(writer, hdrFilePath);

            // Write the DOL file
            Console.WriteLine("Writing Start.dol...");
            WriteSystemFile(writer, dolFilePath);

            // Write the Apploader
            Console.WriteLine("Writing AppLoader.ldr...");
            WriteSystemFile(writer, apploaderFilePath);

            // Parse the TOC
            Console.WriteLine("Parsing TOC...");
            List<TOCItem> tocEntries = ParseTOCFromFile(tocFilePath);

            // Write game files
            Console.WriteLine("Writing game files...");
            foreach (var entry in tocEntries)
            {
                // Directories are skipped
                if (entry.IsDirectory) continue;

                string filePath = Path.Combine(extractedPath, entry.Name.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}");

                WriteGameFile(writer, filePath, entry.Position);
            }

            Console.WriteLine($"Rebuild complete. ISO saved to: {outputIsoPath}");
        }

        /// <summary>
        /// Parses the TOC file to determine the list of files and directories in the ISO.
        /// </summary>
        private List<TOCItem> ParseTOCFromFile(string tocPath)
        {
            var tocEntries = new List<TOCItem>();

            using var reader = new BinaryReader(File.OpenRead(tocPath));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint fileOffset = reader.ReadUInt32(); // File position in ISO
                uint fileSize = reader.ReadUInt32();   // File size
                byte flags = reader.ReadByte();        // Flags (e.g., directory or file)
                reader.BaseStream.Seek(3, SeekOrigin.Current); // Skip padding
                uint nameOffset = reader.ReadUInt32(); // Offset to the name in the string table

                // Resolve file name
                long currentPos = reader.BaseStream.Position;
                reader.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
                string name = ReadNullTerminatedString(reader);
                reader.BaseStream.Seek(currentPos, SeekOrigin.Begin);

                tocEntries.Add(new TOCItem
                {
                    Position = (int)fileOffset,
                    Length = (int)fileSize,
                    IsDirectory = (flags & 0x02) != 0,
                    Name = name
                });
            }

            return tocEntries;
        }

        /// <summary>
        /// Writes system files (e.g., ISO.hdr, Start.dol) to the ISO.
        /// </summary>
        private void WriteSystemFile(BinaryWriter writer, string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            writer.Write(data);

            // Align to 0x8000-byte boundaries
            long padding = AlignToBlock(data.Length, 0x8000) - data.Length;
            writer.Write(new byte[padding]);
        }

        /// <summary>
        /// Writes game files based on their TOC entries.
        /// </summary>
        private void WriteGameFile(BinaryWriter writer, string filePath, long position)
        {
            byte[] data = File.ReadAllBytes(filePath);

            // Seek to the correct position in the ISO
            writer.Seek((int)position, SeekOrigin.Begin);
            writer.Write(data);

            // Add padding to align to the next block if needed
            long padding = AlignToBlock(data.Length, 0x8000) - data.Length;
            writer.Write(new byte[padding]);
        }

        /// <summary>
        /// Aligns a value to the next 0x8000-byte boundary.
        /// </summary>
        private long AlignToBlock(long value, int blockSize)
        {
            return (value + blockSize - 1) / blockSize * blockSize;
        }

        /// <summary>
        /// Reads a null-terminated string from the binary stream.
        /// </summary>
        private string ReadNullTerminatedString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
                bytes.Add(b);
            return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
        }
    }

    public class TOCManager
    {
        public List<TOCItem> TOCEntries { get; private set; } = new List<TOCItem>();

        public void ExtractFiles(BinaryReader reader, string outputDirectory)
        {
            foreach (var entry in TOCEntries)
            {
                var outputPath = Path.Combine(outputDirectory, entry.Name);

                // Create directories for output
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(outputPath);
                    continue;
                }

                // Ensure the parent directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

                // Seek to the file's position in the ISO
                reader.BaseStream.Seek(entry.Position, SeekOrigin.Begin);

                // Extract file in chunks
                using var outputFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                long remaining = entry.Length;
                const int bufferSize = 0x8000; // 32KB chunks
                byte[] buffer = new byte[bufferSize];

                while (remaining > 0)
                {
                    int bytesToRead = (int)Math.Min(bufferSize, remaining);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    outputFile.Write(buffer, 0, bytesRead);
                    remaining -= bytesRead;
                }
            }
        }
    }

    public class TOCItem
    {
        public int Position { get; set; }
        public int Length { get; set; }
        public bool IsDirectory { get; set; }
        public string Name { get; set; }
    }
}
