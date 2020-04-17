using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
{
    class KeyFile
    {
        public static List<TKeys> ReadBasesFromKeyFile(string Keypath)
        {
            List<TKeys> lRet = new List<TKeys>();

            FileInfo fn = new FileInfo(Keypath);
            if (fn.Extension.Contains("vck"))
            {
                FileStream inputConfigStream = new FileStream(Keypath, FileMode.Open, FileAccess.Read);
                GZipStream decompressedConfigStream = new GZipStream(inputConfigStream, CompressionMode.Decompress);
                IFormatter formatter = new BinaryFormatter();
                lRet = (List<TKeys>)formatter.Deserialize(decompressedConfigStream);
                inputConfigStream.Close();
                decompressedConfigStream.Close();
            }

            return lRet;
        }
        public static void ExportFile(List<TKeys> precomp, GameConsoles console)
        {
            CheckAndFixFolder("bin\\keys");
            Stream createConfigStream = new FileStream($@"bin\keys\{console.ToString().ToLower()}.vck", FileMode.Create, FileAccess.Write);
            GZipStream compressedStream = new GZipStream(createConfigStream, CompressionMode.Compress);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(compressedStream, precomp);
            compressedStream.Close();
            createConfigStream.Close();
        }
        private static void CheckAndFixFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }
}
