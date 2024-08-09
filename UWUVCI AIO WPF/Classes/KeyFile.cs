using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
{
    class KeyFile
    {
        public static List<TKeys> ReadBasesFromKeyFile(string keyPath)
        {
            List<TKeys> result = new List<TKeys>();

            try
            {
                FileInfo fileInfo = new FileInfo(keyPath);
                if (fileInfo.Extension.Contains("vck"))
                {
                    using (FileStream inputConfigStream = new FileStream(keyPath, FileMode.Open, FileAccess.Read))
                    using (GZipStream decompressedConfigStream = new GZipStream(inputConfigStream, CompressionMode.Decompress))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        result = (List<TKeys>)formatter.Deserialize(decompressedConfigStream);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the error appropriately
                Console.WriteLine($"An error occurred while reading the key file: {ex.Message}");
            }

            return result;
        }

        public static void ExportFile(List<TKeys> keys, GameConsoles console)
        {
            try
            {
                string folderPath = Path.Combine("bin", "keys");
                CheckAndFixFolder(folderPath);

                string filePath = Path.Combine(folderPath, $"{console.ToString().ToLower()}.vck");

                using (FileStream createConfigStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (GZipStream compressedStream = new GZipStream(createConfigStream, CompressionMode.Compress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(compressedStream, keys);
                }
            }
            catch (Exception ex)
            {
                // Handle or log the error appropriately
                Console.WriteLine($"An error occurred while exporting the key file: {ex.Message}");
            }
        }

        private static void CheckAndFixFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
    }
}
