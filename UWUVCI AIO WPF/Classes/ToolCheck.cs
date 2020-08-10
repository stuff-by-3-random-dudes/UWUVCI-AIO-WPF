using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
{
    class ToolCheck
    {
        static string FolderName = "bin\\Tools";
        public static string backupulr = @"https://github.com/Hotbrawl20/UWUVCI-Tools/raw/master/";
        public static string[] ToolNames =
        {
            "CDecrypt.exe",
            "CNUSPACKER.exe",
            "N64Converter.exe",
            "png2tga.exe",
            "psb.exe",
            "RetroInject.exe",
            "tga_verify.exe",
            "WiiUDownloader.exe",
            "wiiurpxtool.exe",
            "INICreator.exe",
            "7za.exe",
            "blank.ini",
            "FreeImage.dll",
            "BuildPcePkg.exe",
            "BuildTurboCdPcePkg.exe",
            "goomba.gba",
            "nfs2iso2nfs.exe",
            "nintendont.dol",
            "nintendont_force.dol",
            "GetExtTypePatcher.exe",
            "wbfs_file.exe",
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
            "forwarder.dol"
        };

        public static bool DoesToolsFolderExist()
        {
            if (Directory.Exists(FolderName))
            {
                return true;
            }
            return false;
        }

        public static bool IsToolRight(string name)
        {
            bool ret = false;
            WebClient client = new WebClient();
            client.DownloadFile(backupulr + name + ".md5", name + ".md5");
            StreamReader sr = new StreamReader(name + ".md5");
            var md5 = sr.ReadLine();
            if(CalculateMD5(name) == md5)
            {
                ret = true;
            }
            sr.Close();
            File.Delete(name + ".md5");
            return ret;
        }
        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    string ret = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    stream.Close();
                    return ret;
                }
            }
        }
        public static List<MissingTool> CheckForMissingTools()
        {
            List<MissingTool> ret = new List<MissingTool>();
            foreach(string s in ToolNames)
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
            if (File.Exists(path))
            {
                return true;
            }
            return false;
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
