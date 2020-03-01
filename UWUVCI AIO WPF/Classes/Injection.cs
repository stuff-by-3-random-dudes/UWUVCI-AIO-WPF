using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using UWUVCI_AIO_WPF.Properties;

namespace UWUVCI_AIO
{
    internal static class Injection
    {
        public enum Console { NDS, N64, GBA, NES, SNES }

        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        private static readonly string baseRomPath = Path.Combine(tempPath, "baserom");
        private static readonly string imgPath = Path.Combine(tempPath, "img");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "Tools");

        /*
         * Console: Can either be NDS, N64, GBA, NES or SNES
         * baseRom = Name of the BaseRom, which is the folder name too (example: Super Metroid EU will be saved at the BaseRom path under the folder SMetroidEU, so the BaseRom is in this case SMetroidEU).
         * customBasePath = Path to the custom Base. Is null if no custom base is used.
         * injectRomPath = Path to the Rom to be injected into the Base Game.
         * bootImages = String array containing the paths for
         *              bootTvTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 1280x720 and have a bit depth of 24. If null, the original BootImage will be used.
         *              bootDrcTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 854x480 and have a bit depth of 24. If null, the original BootImage will be used.
         *              iconTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 128x128 and have a bit depth of 32. If null, the original BootImage will be used.
         *              bootLogoTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 170x42 and have a bit depth of 32. If null, the original BootImage will be used.
         * gameName = The name of the final game to be entered into the .xml files.
         * iniPath = Only used for N64. Path to the INI configuration. If "blank", a blank ini will be used.
         * darkRemoval = Only used for N64. Indicates whether the dark filter should be removed.
         */
        public static void Inject(Console console, string baseRom, string customBasePath, string injectRomPath, string[] bootImages, string gameName, string iniPath = null, bool darkRemoval = false)
        {
            CopyBase(baseRom, customBasePath);
            switch (console)
            {
                case Console.NDS:
                    NDS(injectRomPath);
                    break;

                case Console.N64:
                    N64(injectRomPath, iniPath, darkRemoval);
                    break;

                case Console.GBA:
                    GBA(injectRomPath);
                    break;

                case Console.NES:
                    NESSNES(injectRomPath);
                    break;
                case Console.SNES:
                    NESSNES(RemoveHeader(injectRomPath));
                    break;
            }

            EditXML(gameName);
            Images(bootImages);
            //MessageBox.Show(Resources.InjectionFinishedText, Resources.InjectionFinishedCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Clean()
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }

        public static void Loadiine(string gameName)
        {
            //string outputPath = Path.Combine(Properties.Settings.Default.InjectionPath, gameName);
            string outputPath = string.Empty;
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                //outputPath = Path.Combine(Properties.Settings.Default.InjectionPath, $"{gameName}_{i}");
                i++;
            }

            //Directory.Move(baseRomPath,outputPath);
           // MessageBox.Show(string.Format(Resources.InjectCreatedText, outputPath), Resources.InjectCreatedCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            Clean();
        }

        public static void Packing(string gameName)
        {
            //string outputPath = Path.Combine(Properties.Settings.Default.InjectionPath, gameName);
            string outputPath = string.Empty;
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                //outputPath = Path.Combine(Properties.Settings.Default.InjectionPath, $"{gameName}_{i}");
                i++;
            }

            using (Process cnuspacker = new Process())
            {
                cnuspacker.StartInfo.UseShellExecute = false;
                cnuspacker.StartInfo.CreateNoWindow = true;
                cnuspacker.StartInfo.FileName = Path.Combine(toolsPath, "CNUSPACKER.exe");
                //cnuspacker.StartInfo.Arguments = $"-in \"{baseRomPath}\" -out \"{outputPath}\" -encryptKeyWith {Properties.Settings.Default.CommonKey}";

                cnuspacker.Start();
                cnuspacker.WaitForExit();
            }

            //MessageBox.Show(string.Format(Resources.InjectCreatedText, outputPath), Resources.InjectCreatedCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            Clean();
        }

        public static void Download(string baseRom)
        {
            string TID = null;
            //string TK = (string) Properties.Settings.Default[baseRom];

            switch (baseRom)
            {
                #region NDS
                case "ZSTEU":
                    TID = "00050000101b8d00";
                    break;
                case "ZSTUS":
                    TID = "00050000101b8c00";
                    break;
                case "ZPHEU":
                    TID = "00050000101c3800";
                    break;
                case "ZPHUS":
                    TID = "00050000101c3700";
                    break;
                case "WWEU":
                    TID = "00050000101a2000";
                    break;
                case "WWUS":
                    TID = "00050000101a1f00";
                    break;
                #endregion
                #region N64
                case "PMEU":
                    TID = "0005000010199800";
                    break;
                case "PMUS":
                    TID = "0005000010199700";
                    break;
                case "FZXUS":
                    TID = "00050000101ebc00";
                    break;
                case "FZXJP":
                    TID = "00050000101ebb00";
                    break;
                case "DK64EU":
                    TID = "0005000010199300";
                    break;
                case "DK64US":
                    TID = "0005000010199200";
                    break;
                #endregion
                #region GBA
                case "ZMCEU":
                    TID = "000500001015e500";
                    break;
                case "ZMCUS":
                    TID = "000500001015e400";
                    break;
                case "MKCEU":
                    TID = "000500001017d200";
                    break;
                case "MKCUS":
                    TID = "000500001017d300";
                    break;
                #endregion
                #region NES
                case "POEU":
                    TID = "0005000010108c00";
                    break;
                case "POUS":
                    TID = "0005000010108b00";
                    break;
                case "SMBEU":
                    TID = "0005000010106e00";
                    break;
                case "SMBUS":
                    TID = "0005000010106d00";
                    break;
                #endregion
                #region SNES
                case "SMetroidEU":
                    TID = "000500001010a700";
                    break;
                case "SMetroidUS":
                    TID = "000500001010a600";
                    break;
                case "SMetroidJP":
                    TID = "000500001010a500";
                    break;
                case "EarthboundEU":
                    TID = "0005000010133500";
                    break;
                case "EarthboundUS":
                    TID = "0005000010133400";
                    break;
                case "EarthboundJP":
                    TID = "0005000010133000";
                    break;
                case "DKCEU":
                    TID = "0005000010109600";
                    break;
                case "DKCUS":
                    TID = "0005000010109500";
                    break;
                #endregion
            }

            Directory.CreateDirectory(tempPath);
            using (Process download = new Process())
            {
                download.StartInfo.FileName = Path.Combine(toolsPath, "WiiUDownloader.exe");
                //download.StartInfo.Arguments = $"{TID} {TK} \"{Path.Combine(tempPath, "download")}\"";

                download.Start();
                download.WaitForExit();
            }

            using (Process decrypt = new Process())
            {
                decrypt.StartInfo.FileName = Path.Combine(toolsPath, "Cdecrypt.exe");
                //decrypt.StartInfo.Arguments = $"{Properties.Settings.Default.CommonKey} \"{Path.Combine(tempPath, "download")}\" \"{Path.Combine(Properties.Settings.Default.BaseRomPath, baseRom)}\"";

                decrypt.Start();
                decrypt.WaitForExit();
            }
        }

        // This function changes TitleID, ProductCode and GameName in app.xml (ID) and meta.xml (ID, ProductCode, Name)
        private static void EditXML(string gameName)
        {
            string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
            string appXml = Path.Combine(baseRomPath, "code", "app.xml");
            Random random = new Random();
            string ID = $"{random.Next(0x3000, 0x10000):X4}";

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(metaXml);
                doc.SelectSingleNode("menu/longname_ja").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_en").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_fr").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_de").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_it").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_es").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_zhs").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_ko").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_nl").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_pt").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_ru").InnerText = gameName;
                doc.SelectSingleNode("menu/longname_zht").InnerText = gameName;

                doc.SelectSingleNode("menu/product_code").InnerText = $"WUP-N-{ID}";
                doc.SelectSingleNode("menu/title_id").InnerText = $"0005000010{ID}00";
                doc.SelectSingleNode("menu/group_id").InnerText = $"0000{ID}";

                doc.SelectSingleNode("menu/shortname_ja").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_fr").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_de").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_en").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_it").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_es").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_zhs").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_ko").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_nl").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_pt").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_ru").InnerText = gameName;
                doc.SelectSingleNode("menu/shortname_zht").InnerText = gameName;
                doc.Save(metaXml);
            }
            catch (NullReferenceException)
            {
                //MessageBox.Show("Error when editing the meta.xml: Values seem to be missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                doc.Load(appXml);
                doc.SelectSingleNode("app/title_id").InnerText = $"0005000010{ID}00";
                doc.SelectSingleNode("app/group_id").InnerText = $"0000{ID}";
                doc.Save(appXml);
            }
            catch (NullReferenceException)
            {
               // MessageBox.Show("Error when editing the app.xml: Values seem to be missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //This function copies the custom or normal Base to the working directory
        private static void CopyBase(string baserom, string customPath)
        {
            if (Directory.Exists(baseRomPath)) // sanity check
            {
                Directory.Delete(baseRomPath, true);
            }
            if (baserom == "Custom")
            {
                DirectoryCopy(customPath, baseRomPath, true);
            }
            else
            {
                //DirectoryCopy(Path.Combine(Properties.Settings.Default.BaseRomPath, baserom), baseRomPath, true);
            }
        }

        private static void NESSNES(string injectRomPath)
        {
            string rpxFile = Directory.GetFiles(Path.Combine(baseRomPath, "code"), "*.rpx")[0]; //To get the RPX path where the NES/SNES rom needs to be Injected in

            RPXdecomp(rpxFile); //Decompresses the RPX to be able to write the game into it

            using (Process retroinject = new Process())
            {
                retroinject.StartInfo.UseShellExecute = false;
                retroinject.StartInfo.CreateNoWindow = true;
                retroinject.StartInfo.FileName = Path.Combine(toolsPath, "retroinject.exe");
                retroinject.StartInfo.Arguments = $"\"{rpxFile}\" \"{injectRomPath}\" \"{rpxFile}\"";

                retroinject.Start();
                retroinject.WaitForExit();
            }

            RPXcomp(rpxFile); //Compresses the RPX
        }

        private static void GBA(string injectRomPath)
        {
            using (Process psb = new Process())
            {
                psb.StartInfo.UseShellExecute = false;
                psb.StartInfo.CreateNoWindow = true;
                psb.StartInfo.FileName = Path.Combine(toolsPath, "psb.exe");
                psb.StartInfo.Arguments = $"\"{Path.Combine(baseRomPath, "content", "alldata.psb.m")}\" \"{injectRomPath}\" \"{Path.Combine(baseRomPath, "content", "alldata.psb.m")}\"";

                psb.Start();
                psb.WaitForExit();
            }
        }

        private static void NDS(string injectRomPath)
        {
            using (ZipArchive archive = ZipFile.Open(Path.Combine(baseRomPath, "content", "0010", "rom.zip"), ZipArchiveMode.Update))
            {
                string romname = archive.Entries[0].FullName;
                archive.Entries[0].Delete();
                archive.CreateEntryFromFile(injectRomPath, romname);
            }
            
        }

        private static void N64(string injectRomPath, string iniPath, bool darkRemoval)
        {
            string mainRomPath = Directory.GetFiles(Path.Combine(baseRomPath, "content", "rom"))[0];
            string mainIni = Path.Combine(baseRomPath, "content", "config", $"{Path.GetFileName(mainRomPath)}.ini");
            using (Process n64convert = new Process())
            {
                n64convert.StartInfo.UseShellExecute = false;
                n64convert.StartInfo.CreateNoWindow = true;
                n64convert.StartInfo.FileName = Path.Combine(toolsPath, "N64Converter.exe");
                n64convert.StartInfo.Arguments = $"\"{injectRomPath}\" \"{mainRomPath}\"";

                n64convert.Start();
                n64convert.WaitForExit();
            }

            if (iniPath != null)
            {
                File.Delete(mainIni);
                File.Copy((iniPath == "blank") ? Path.Combine(toolsPath, "blank.ini") : iniPath, mainIni);
            }

            if (darkRemoval)
            {
                string filePath = Path.Combine(baseRomPath, "content", "FrameLayout.arc");
                using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Open)))
                {
                    writer.Seek(0x1AD8, SeekOrigin.Begin);
                    writer.Write(0L);
                }
            }
        }

        //Compressed or decompresses the RPX using wiiurpxtool
        private static void RPXdecomp(string rpxpath)
        {
            using (Process rpxtool = new Process())
            {
                rpxtool.StartInfo.UseShellExecute = false;
                rpxtool.StartInfo.CreateNoWindow = true;
                rpxtool.StartInfo.FileName = Path.Combine(toolsPath, "wiiurpxtool.exe");
                rpxtool.StartInfo.Arguments = $"-d \"{rpxpath}\"";

                rpxtool.Start();
                rpxtool.WaitForExit();
            }
        }

        private static void RPXcomp(string rpxpath)
        {
            using (Process rpxtool = new Process())
            {
                rpxtool.StartInfo.UseShellExecute = false;
                rpxtool.StartInfo.CreateNoWindow = true;
                rpxtool.StartInfo.FileName = Path.Combine(toolsPath, "wiiurpxtool.exe");
                rpxtool.StartInfo.Arguments = $"-c \"{rpxpath}\"";

                rpxtool.Start();
                rpxtool.WaitForExit();
            }
        }

        private static void Images(string[] paths)
        {
            bool tv = false;
            bool drc = false;
            bool icon = false;
            bool logo = false;

            if (Directory.Exists(imgPath)) // sanity check
            {
                Directory.Delete(imgPath, true);
            }
            Directory.CreateDirectory(imgPath);

            if (paths[0] != null)
            {
                tv = true;
                CopyAndConvertImage(paths[0], Path.Combine(imgPath, "bootTvTex.tga"));
            }

            if (paths[1] != null)
            {
                drc = true;
                CopyAndConvertImage(paths[1], Path.Combine(imgPath, "bootDrcTex.tga"));
            }

            if (paths[2] != null)
            {
                icon = true;
                CopyAndConvertImage(paths[2], Path.Combine(imgPath, "iconTex.tga"));
            }

            if (paths[3] != null)
            {
                logo = true;
                CopyAndConvertImage(paths[3], Path.Combine(imgPath, "bootLogoTex.tga"));
            }

            if (tv || drc || icon || logo) {
                using (Process tgaverifyFixup = new Process())
                {
                    tgaverifyFixup.StartInfo.UseShellExecute = false;
                    tgaverifyFixup.StartInfo.CreateNoWindow = true;
                    tgaverifyFixup.StartInfo.FileName = Path.Combine(toolsPath, "tga_verify.exe");
                    tgaverifyFixup.StartInfo.Arguments = $"--fixup \"{imgPath}\"";

                    tgaverifyFixup.Start();
                    tgaverifyFixup.WaitForExit();
                }

                if (tv)
                {
                    File.Delete(Path.Combine(baseRomPath, "meta", "bootTvTex.tga"));
                    File.Move(Path.Combine(imgPath, "bootTvTex.tga"), Path.Combine(baseRomPath, "meta", "bootTvTex.tga"));
                }
                if (drc)
                {
                    File.Delete(Path.Combine(baseRomPath, "meta", "bootDrcTex.tga"));
                    File.Move(Path.Combine(imgPath, "bootDrcTex.tga"), Path.Combine(baseRomPath, "meta", "bootDrcTex.tga"));
                }
                if (icon)
                {
                    File.Delete(Path.Combine(baseRomPath, "meta", "iconTex.tga"));
                    File.Move(Path.Combine(imgPath, "iconTex.tga"), Path.Combine(baseRomPath, "meta", "iconTex.tga"));
                }
                if (logo)
                {
                    File.Delete(Path.Combine(baseRomPath, "meta", "bootLogoTex.tga"));
                    File.Move(Path.Combine(imgPath, "bootLogoTex.tga"), Path.Combine(baseRomPath, "meta", "bootLogoTex.tga"));
                }
            }
        }

        private static void CopyAndConvertImage(string inputPath, string outputPath)
        {
            if (inputPath.EndsWith(".tga"))
            {
                File.Copy(inputPath, outputPath);
            }
            else
            {
                using (Process png2tga = new Process())
                {
                    png2tga.StartInfo.UseShellExecute = false;
                    png2tga.StartInfo.CreateNoWindow = true;
                    png2tga.StartInfo.FileName = Path.Combine(toolsPath, "png2tga.exe");
                    png2tga.StartInfo.Arguments = $"\"{inputPath}\" \"{outputPath}\"";

                    png2tga.Start();
                    png2tga.WaitForExit();
                }
            }
        }

        private static string RemoveHeader(string filePath)
        {
            // logic taken from snesROMUtil
            using (FileStream inStream = new FileStream(filePath, FileMode.Open))
            {
                byte[] header = new byte[512];
                inStream.Read(header, 0, 512);
                string string1 = BitConverter.ToString(header, 8, 3);
                string string2 = Encoding.ASCII.GetString(header, 0, 11);
                string string3 = BitConverter.ToString(header, 30, 16);
                if (string1 != "AA-BB-04" && string2 != "GAME DOCTOR" && string3 != "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")
                    return filePath;

                string newFilePath = Path.Combine(tempPath, Path.GetFileName(filePath));
                using (FileStream outStream = new FileStream(newFilePath, FileMode.OpenOrCreate))
                {
                    inStream.CopyTo(outStream);
                }

                return newFilePath;
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            foreach (FileInfo file in dir.EnumerateFiles())
            {
                file.CopyTo(Path.Combine(destDirName, file.Name), false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dir.EnumerateDirectories())
                {
                    DirectoryCopy(subdir.FullName,  Path.Combine(destDirName, subdir.Name), copySubDirs);
                }
            }
        }
    }
}
