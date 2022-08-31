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
        static string FolderName = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName + "\\bin\\Tools";
        public static string backupulr = @"https://github.com/Hotbrawl20/UWUVCI-Tools/raw/master/" + (Environment.Is64BitProcess ? "x64/" : "");
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
            "c2w_patcher.exe"
        };

        public static bool DoesToolsFolderExist()
        {
            return Directory.Exists(FolderName);
        }

        public static bool IsToolRightAsync(string name)
        {
            var result = false;
            string md5Name = name + ".md5";
            string md5Path = FolderName + "\\" + md5Name;
            string filePath = FolderName + "\\" + name;

            if (!File.Exists(filePath))
                return result;

            using (var webClient = new WebClient())
                webClient.DownloadFile(backupulr + md5Name, md5Path);

            var md5 = "";
            using (var sr = new StreamReader(md5Path))
                md5 = sr.ReadLine();

            result = CalculateMD5(filePath) == md5.ToLower();

            File.Delete(md5Path);

            return result;
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
