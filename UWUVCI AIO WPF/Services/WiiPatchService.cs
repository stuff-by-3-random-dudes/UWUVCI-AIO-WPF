using System;
using System.Diagnostics;
using System.IO;

namespace UWUVCI_AIO_WPF.Services
{
    public static class WiiPatchService
    {
        public static void ApplyRegionFrii(string preIsoWin, bool us, bool jp)
        {
            using var fs = new FileStream(preIsoWin, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(0x4E003, SeekOrigin.Begin);
            if (us)
            {
                fs.Write(new byte[] { 0x01 }, 0, 1);
                fs.Seek(0x4E010, SeekOrigin.Begin);
                fs.Write(new byte[] { 0x80, 0x06, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
            }
            else if (jp)
            {
                fs.Write(new byte[] { 0x00 }, 0, 1);
                fs.Seek(0x4E010, SeekOrigin.Begin);
                fs.Write(new byte[] { 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
            }
            else
            {
                fs.Write(new byte[] { 0x02 }, 0, 1);
                fs.Seek(0x4E010, SeekOrigin.Begin);
                fs.Write(new byte[] { 0x80, 0x80, 0x80, 0x00, 0x03, 0x03, 0x04, 0x03, 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
            }
        }

        public static void ApplyJpPatch(string tempPath)
        {
            var dolPath = Path.Combine(tempPath, "TEMP", "sys", "main.dol");
            using var writer = new BinaryWriter(new FileStream(dolPath, FileMode.Open, FileAccess.Write));
            writer.Seek(0x4CBDAC, SeekOrigin.Begin);
            writer.Write(new byte[] { 0x38, 0x60 });
            writer.Seek(0x4CBDAF, SeekOrigin.Begin);
            writer.Write((byte)0x00);
        }

        public static void ForceClassicController(string toolsPath, string targetDol, bool debug)
        {
            using var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(toolsPath, "GetExtTypePatcher.exe");
            proc.StartInfo.Arguments = $"\"{targetDol}\" -nc";
            if (!debug) proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.Start();
            System.Threading.Thread.Sleep(2000);
            proc.StandardInput.WriteLine();
            proc.WaitForExit();
        }
    }
}

