using System;
using System.IO;
using System.Reflection;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class FileHelpers
    {
        public static int FindFreeSpace(byte[] fileData, int requiredSize)
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
        public static void MoveOverwrite(string source, string destination)
        {
            if (File.Exists(destination))
                File.Delete(destination);

            File.Move(source, destination);
        }
    }
}
