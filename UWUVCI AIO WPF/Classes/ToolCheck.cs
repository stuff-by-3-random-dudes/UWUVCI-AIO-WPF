using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
{
    class ToolCheck
    {
        static string FolderName = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName + "\\bin\\Tools";
        public static string backupulr = @"https://github.com/Hotbrawl20/UWUVCI-Tools/raw/master/";
        public static string[] ToolNames =
        {
            "N64Converter.exe",
            "png2tga.exe",
            "psb.exe",
            "RetroInject.exe",
            "tga_verify.exe",
            "wiiurpxtool.exe",
            "INICreator.exe",
            "blank.ini",
            "FreeImage.dll",
            "BuildPcePkg.exe",
            "BuildTurboCdPcePkg.exe",
            "goomba.gba",
            "nintendont.dol",
            "nintendont_force.dol",
            "GetExtTypePatcher.exe",
            "wit.exe",
            "cygwin1.dll",
            "cygz.dll",
            "cyggcc_s-1.dll",
            "NintendontConfig.exe",
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
            "CNUSPACKER.exe"
        };

        public static bool DoesToolsFolderExist()
        {
            return Directory.Exists(FolderName);
        }

        public static async Task<bool> IsToolRightAsync(string name)
        {
            string md5Name = name + ".md5";
            string md5Path = FolderName + "\\" + md5Name;
            
            using (var httpClient = new HttpClient())
            {
                using var response = await httpClient.GetStreamAsync(backupulr + md5Name);
                using var fs = new FileStream(md5Path, FileMode.Create);
                await response.CopyToAsync(fs);
            }
            
            var md5 = "";
            using (var sr = new StreamReader(md5Path))
                md5 = await sr.ReadToEndAsync();

            return CalculateMD5(md5Path) == md5;
        }
        static string CalculateMD5(string filename)
        {
            var ret = "";
            using var md5 = MD5.Create();
            using (var stream = File.OpenRead(filename))
                ret = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();

            return ret;
        }
        public static List<MissingTool> CheckForMissingTools()
        {
            List<MissingTool> ret = new List<MissingTool>();
            foreach (string s in ToolNames)
            {
                string path = $@"{FolderName}\{s}";
                if (!DoesToolExist(path))
                {
                    ret.Add(new MissingTool(s, path));
                }
            }
            return ret;
        }

        private static bool DoesToolExist(string path)
        {
            if (!File.Exists(path))
                return false;

            if (path.ToLower().Contains("gba1.zip") || path.ToLower().Contains("gba2.zip"))
                if (!File.Exists(Path.Combine(FolderName, "MArchiveBatchTool.exe")) || !File.Exists(Path.Combine(FolderName, "ucrtbase.dll")))
                    try
                    {
                        ZipFile.ExtractToDirectory(path, FolderName);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(200);
                        DoesToolExist(path);
                    }

            return true;
        }

    }
    class MissingTool
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public MissingTool(string n, string p)
        {
            this.Name = n;
            FileInfo f = new FileInfo(p);
            this.Path = f.FullName;
        }
    }
}
