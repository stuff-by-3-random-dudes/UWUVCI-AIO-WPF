using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

namespace UWUVCI_AIO_WPF.Classes
{
    class ToolCheck
    {
        static string FolderName = "bin\\Tools";
        public static string backupulr = @"https://github.com/Hotbrawl20/UWUVCI-Tools/raw/master/";
        public static string[] ToolNames =
        {
            "N64Converter.exe",
            "png2tga.exe",
            "psb.exe",
            "RetroInject.exe",
            "tga_verify.exe",
            "wiiurpxtool.exe",
            "blank.ini",
            "FreeImage.dll",
            "BuildPcePkg.exe",
            "BuildTurboCdPcePkg.exe",
            "goomba.gba",
            "nfs2iso2nfs.exe",
            "nintendont.dol",
            "nintendont_force.dol",
            "GetExtTypePatcher.exe",
            "wit.exe",
            "wit-mac",
            "wit-linux",
            "wstrt.exe",
            "wstrt-mac",
            "wstrt-linux",
            "cygwin1.dll",
            "cygz.dll",
            "cyggcc_s-1.dll",
            "BASE.zip",
            "tga2png.exe",
            "iconTex.tga",
            "wii-vmc.exe",
            "bootTvTex.png",
            "ConvertToISO.exe",
            "NKit.dll",
            "SharpCompress.dll",
            "NKit.dll.config",
            "sox.exe",
            "jpg2tga.exe",
            "bmp2tga.exe",
            "ConvertToNKit.exe",
            "wglp.exe",
            "font.otf",
            "ChangeAspectRatio.exe",
            "font2.ttf",
            "forwarder.dol",
            "gba1.zip",
            "gba2.zip",
            "c2w_patcher.exe",
            "DSLayoutScreens.zip",
            "cygcrypto-1.1.dll",
            "cygncursesw-10.dll"
        };

        public static bool DoesToolsFolderExist()
        {
            try
            {
                return Directory.Exists(FolderName);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsToolRight(string name)
        {
            bool ret = false;
            var md5 = "";
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(backupulr + name + ".md5", name + ".md5");
                }

                using StreamReader sr = new StreamReader(name + ".md5");
                md5 = sr.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading MD5 file for {name}: {ex.Message}");
                return false;  // Early return if MD5 file cannot be downloaded
            }

            ret = CalculateMD5(name) == md5;
            File.Delete(name + ".md5");

            return ret;
        }

        /// <summary>
        /// Verifies the MD5 of a tool at a specific path (thread-safe; no CWD changes).
        /// </summary>
        public static bool IsToolRightAtPath(string fullPath)
        {
            try
            {
                var dir = Path.GetDirectoryName(fullPath);
                var name = Path.GetFileName(fullPath);
                if (string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(name)) return false;

                string md5Path = Path.Combine(dir, name + ".md5");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(backupulr + name + ".md5", md5Path);
                }
                string md5;
                using (StreamReader sr = new StreamReader(md5Path))
                {
                    md5 = sr.ReadLine();
                }
                bool ok = CalculateMD5(fullPath) == md5;
                try { File.Delete(md5Path); } catch { }
                return ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying MD5 for {fullPath}: {ex.Message}");
                return false;
            }
        }


        public static string CalculateMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            string ret = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
            stream.Close();
            return ret;
        }

        public static List<MissingTool> CheckForMissingTools()
        {
            List<MissingTool> missingTools = new List<MissingTool>();

            foreach (string toolName in ToolNames)
            {
                string path = Path.Combine(FolderName, toolName);

                // Check if the tool exists and has the right MD5 hash
                if (!DoesToolExist(path))
                    missingTools.Add(new MissingTool(toolName, path));
            }

            return missingTools;
        }


        private static bool DoesToolExist(string path, int retryCount = 0)
        {
            const int MaxRetries = 3;  // Define a maximum number of retries

            if (!File.Exists(path))
                return false;

            if (path.ToLower().Contains("gba1.zip") || path.ToLower().Contains("gba2.zip"))
            {
                if (!File.Exists(Path.Combine(FolderName, "MArchiveBatchTool.exe")) || !File.Exists(Path.Combine(FolderName, "ucrtbase.dll")))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(path, FolderName);
                    }
                    catch (Exception)
                    {
                        if (retryCount < MaxRetries)
                        {
                            Thread.Sleep(200);
                            return DoesToolExist(path, retryCount + 1);  // Recursively retry
                        }
                        else
                        {
                            Console.WriteLine($"Failed to extract {path} after {MaxRetries} attempts.");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }

    public class MissingTool
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public MissingTool(string n, string p)
        {
            Name = n;
            FileInfo f = new FileInfo(p);
            Path = f.FullName;
        }
    }
}
