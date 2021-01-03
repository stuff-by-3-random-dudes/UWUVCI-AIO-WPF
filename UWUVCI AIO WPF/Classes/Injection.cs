using GameBaseClassLibrary;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.UI.Windows;
using MessageBox = System.Windows.MessageBox;

namespace UWUVCI_AIO_WPF
{
    public static class StringExtensions
    {
        public static string ToHex(this string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }
    }
    internal static class Injection
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll")]
        public static extern int SendMessage(
    int hWnd,     // handle to destination window
    uint Msg,      // message
    long wParam,   // first message parameter
    long lParam    // second message parameter
  );
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, int Msg, System.Windows.Forms.Keys wParam, int lParam);
        private static Int32 WM_KEYUP = 0x101;
        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(),"bin", "temp");
        private static readonly string baseRomPath = Path.Combine(tempPath, "baserom");
        private static readonly string imgPath = Path.Combine(tempPath, "img");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(),"bin", "Tools");
        static string code = null;
        static MainViewModel mvvm;

        /*
         * GameConsole: Can either be NDS, N64, GBA, NES, SNES or TG16
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
        static List<int> fiind(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(i);
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }
        static void PokePatch(string rom)
        {
            byte[] search = { 0xD0, 0x88, 0x8D, 0x83, 0x42 };
            byte[] test;
            test = new byte[new FileInfo(rom).Length];
            using (var fs = new FileStream(rom,
                                 FileMode.Open,
                                 FileAccess.ReadWrite))
            {
                try
                {
                    fs.Read(test, 0, test.Length - 1);

                    var l = fiind(test, search, 0);
                    byte[] check = new byte[4];
                    fs.Seek(l[0] + 5, SeekOrigin.Begin);
                    fs.Read(check, 0, 4);

                    fs.Seek(0, SeekOrigin.Begin);
                    if (check[3] != 0x24)
                    {
                        fs.Seek(l[0] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);

                    }
                    else
                    {
                        fs.Seek(l[0] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);

                    }
                    check = new byte[4];
                    fs.Seek(l[1] + 5, SeekOrigin.Begin);
                    fs.Read(check, 0, 4);
                    fs.Seek(0, SeekOrigin.Begin);
                    if (check[3] != 0x24)
                    {
                        fs.Seek(l[1] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);

                    }
                    else
                    {
                        fs.Seek(l[1] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);

                    }
                }
                catch (Exception )
                {

                }


                fs.Close();
            }
        }
        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        [STAThread]
        public static bool Inject(GameConfig Configuration, string RomPath, MainViewModel mvm, bool force)
        {
         
            mvm.failed = false;
           
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tick;
            
            Clean();
            
            long freeSpaceInBytes = 0;
            if (!mvm.saveworkaround)
            {
                try
                {
                    long gamesize = new FileInfo(RomPath).Length;

                    var drive = new DriveInfo(tempPath);

                    done = true;
                    freeSpaceInBytes = drive.AvailableFreeSpace;
                }
                catch(Exception e)
                {
                    mvm.saveworkaround = true;
                }

               
                           
            }
            long neededspace = 0;
            
            mvvm = mvm;


          
            Directory.CreateDirectory(tempPath);
            

            
            mvm.msg = "Checking Tools...";
            mvm.InjcttoolCheck();
     
            mvm.Progress = 5;
           
            mvm.msg = "Copying Base...";
            try
            {
                if (!mvm.saveworkaround && (Configuration.Console == GameConsoles.WII || Configuration.Console == GameConsoles.GCN))
                {

                    if (mvm.GC)
                    {
                        neededspace = 10000000000;
                    }
                    else
                    {
                        neededspace = 15000000000;
                    }
                    if (freeSpaceInBytes < neededspace)
                    {
                        throw new Exception("12G");
                    }
                }

                if(Configuration.BaseRom == null || Configuration.BaseRom.Name == null)
                {
                    throw new Exception("BASE");
                }
                if (Configuration.BaseRom.Name != "Custom")
                {
                    //Normal Base functionality here
                    CopyBase($"{Configuration.BaseRom.Name.Replace(":", "")} [{Configuration.BaseRom.Region.ToString()}]", null);
                }
                else
                {
                    //Custom Base Functionality here
                    CopyBase($"Custom", Configuration.CBasePath);
                }
                if(!Directory.Exists(Path.Combine(baseRomPath, "code")) || !Directory.Exists(Path.Combine(baseRomPath, "content")) || !Directory.Exists(Path.Combine(baseRomPath, "meta")))
                {
                    throw new Exception("MISSINGF");
                }
                mvm.Progress = 10;
                mvm.msg = "Injecting ROM...";
                if (mvm.GC)
                {
                    RunSpecificInjection(Configuration, GameConsoles.GCN, RomPath, force, mvm);
                }
                else
                {
                    RunSpecificInjection(Configuration, Configuration.Console, RomPath, force, mvm);
                }
                mvm.msg = "Editing XML...";
                EditXML(Configuration.GameName, mvm.Index, code);
                mvm.Progress = 90;
                mvm.msg = "Changing Images...";
                Images(Configuration);
                if (File.Exists(mvm.BootSound))
                {
                    mvm.Progress = 95;
                    mvm.msg = "Adding BootSound...";
                    bootsound(mvm.BootSound);
                }
                
                
                mvm.Progress = 100;
               
                
                code = null;
                return true;
            }catch(Exception e)
            {
                mvm.Progress = 100;
                
                code = null;
                if(e.Message == "Failed this shit")
                {
                    Clean();
                    return false;
                }
                if (e.Message == "MISSINGF")
                {
                    MessageBox.Show("Injection Failed because there are base files missing. \nPlease redownload the base, or redump if you used a custom base! ", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                   
                }
                else if (e.Message.Contains("Images")){

                    MessageBox.Show("Injection Failed due to wrong BitDepth, please check if your Files are in a different bitdepth than 32bit or 24bit", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (e.Message.Contains("Size"))
                {
                    MessageBox.Show("Injection Failed due to Image Issues.Please check if your Images are made using following Information:\n\niconTex: \nDimensions: 128x128\nBitDepth: 32\n\nbootDrcTex: \nDimensions: 854x480\nBitDepth: 24\n\nbootTvTex: \nDimensions: 1280x720\nBitDepth: 24\n\nbootLogoTex: \nDimensions: 170x42\nBitDepth: 32", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                  
                }
                else if (e.Message.Contains("retro"))
                {
                    MessageBox.Show("The ROM you want to Inject is to big for selected Base!\nPlease try again with different Base", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (e.Message.Contains("BASE"))
                {
                    MessageBox.Show("If you import a config you NEED to reselect a base", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (e.Message.Contains("WII"))
                {
                    MessageBox.Show( $"{e.Message.Replace("WII", "")}\nPlease make sure that your ROM isn't flawed and that you have atleast 12 GB of free Storage left.", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (e.Message.Contains("12G"))
                {
                    MessageBox.Show($" Please make sure to have atleast {FormatBytes(15000000000)} of storage left on the drive where you stored the Injector.", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }else if (e.Message.Contains("nkit"))
                {
                    MessageBox.Show($"There is an issue with your NKIT.\nPlease try the original ISO, or redump your game and try again with that dump.", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else
                {
                    MessageBox.Show("Injection Failed due to unknown circumstances, please contact us on the UWUVCI discord", "Injection Failed", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                Clean();
                return false;
            }
            finally
            {
                
                mvm.Index = -1;
                mvm.LR = false;
                mvm.msg = "";

            }

        }
        private static bool done = false;
        private static void tick(object sender, EventArgs e)
        {
            if (!done)
            {
                mvvm.failed = true;
            }
            throw new Exception("Failed this shit");
        }

        public static void SendKey(IntPtr hWnd, System.Windows.Forms.Keys key)
        {
            PostMessage(hWnd, WM_KEYUP, key, 0);
        }
        static void bootsound(string sound)
        {
            string btsndPath = Path.Combine(baseRomPath, "meta", "bootSound.btsnd");
            FileInfo soundFile = new FileInfo(sound);
            if(soundFile.Extension.Contains("mp3") || soundFile.Extension.Contains("wav"))
            {
                // Convert input file to 6 second .wav
                using (Process sox = new Process())
                {
                    sox.StartInfo.UseShellExecute = false;
                    sox.StartInfo.CreateNoWindow = true;
                    sox.StartInfo.FileName = Path.Combine(toolsPath, "sox.exe");
                    sox.StartInfo.Arguments = $"\"{sound}\" -b 16 \"{Path.Combine(tempPath, "bootSound.wav")}\" channels 2 rate 48k trim 0 6";
                    sox.Start();
                    sox.WaitForExit();
                }
                //convert to btsnd
                wav2btsnd(Path.Combine(tempPath, "bootSound.wav"), btsndPath);
                File.Delete(Path.Combine(tempPath, "bootSound.wav"));
            }
            else
            {
                //Copy BootSound to location
                File.Delete(btsndPath);
                File.Copy(sound, btsndPath);
            }
        }

        private static void wav2btsnd(string inputWav, string outputBtsnd)
        {
            // credits to the original creator of wav2btsnd for the general logic
            byte[] buffer = File.ReadAllBytes(inputWav);
            using (FileStream output = new FileStream(outputBtsnd, FileMode.OpenOrCreate))
            using (BinaryWriter writer = new BinaryWriter(output))
            {
                writer.Write(new byte[] {0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0});
                for (int i = 0x2C; i < buffer.Length; i += 2)
                {
                    writer.Write(new[] {buffer[i+1], buffer[i]});
                }
            }
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            if(mvvm.Progress < 50)
            {
                mvvm.Progress += 1;
            }
            
        }
        private static void RunSpecificInjection(GameConfig cfg, GameConsoles console, string RomPath, bool force, MainViewModel mvm)
        {
            switch (console)
            {
                case GameConsoles.NDS:
                    NDS(RomPath);
                    break;

                case GameConsoles.N64:
                   N64(RomPath, cfg.N64Stuff);
                    break;

                case GameConsoles.GBA:
                    GBA(RomPath);
                    break;

                case GameConsoles.NES:
                    NESSNES(RomPath);
                    break;
                case GameConsoles.SNES:
                    NESSNES(RemoveHeader(RomPath));
                    break;
                case GameConsoles.TG16:
                    TG16(RomPath);
                    break;
                case GameConsoles.MSX:
                    MSX(RomPath);
                    break;
                case GameConsoles.WII:
                    if (RomPath.ToLower().EndsWith(".dol"))
                    {
                        WiiHomebrew(RomPath, mvm);
                    }else if (RomPath.ToLower().EndsWith(".wad"))
                    {
                        WiiForwarder(RomPath, mvm);
                    }
                    else
                    {
                        WII(RomPath, mvm);
                    }
                    
                    break;
                case GameConsoles.GCN:
                    GC(RomPath, mvm, force);
                    break;
            }
        }
        private static string ByteArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }
        private static void WiiForwarder(string romPath, MainViewModel mvm)
        {
            string savedir = Directory.GetCurrentDirectory();
            mvvm.msg = "Extracting Forwarder Base...";
            if (Directory.Exists(Path.Combine(tempPath, "TempBase"))) Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
            Directory.CreateDirectory(Path.Combine(tempPath, "TempBase"));
            using (Process zip = new Process())
            {
                if (!mvm.debug)
                {

                    zip.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                zip.StartInfo.FileName = Path.Combine(toolsPath, "7za.exe");
                zip.StartInfo.Arguments = $"x \"{Path.Combine(toolsPath, "BASE.zip")}\" -o\"{Path.Combine(tempPath)}\"";
                zip.Start();
                zip.WaitForExit();
            }

            DirectoryCopy(Path.Combine(tempPath, "BASE"), Path.Combine(tempPath, "TempBase"), true);
            mvvm.Progress = 20;
            mvvm.msg = "Setting up Forwarder...";
            byte[] test = new byte[4];
            using (FileStream fs = new FileStream(romPath, FileMode.Open))
            {
                fs.Seek(0xC20, SeekOrigin.Begin);
                fs.Read(test, 0, 4);
                fs.Close();

            }

            string[] id = { ByteArrayToString(test) };
            File.WriteAllLines(Path.Combine(tempPath, "TempBase", "files","title.txt"), id);
            mvm.Progress = 30;
            mvm.msg = "Copying Forwarder...";
            File.Copy(Path.Combine(toolsPath, "forwarder.dol"), Path.Combine(tempPath, "TempBase", "sys", "main.dol"));
            mvm.Progress = 40;
            mvvm.msg = "Creating Injectable file...";
            using (Process wit = new Process())
            {
                if (!mvm.debug)
                {

                    wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                wit.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                wit.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "TempBase")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --links --iso";
                wit.Start();
                wit.WaitForExit();
            }

            Thread.Sleep(6000);
            if (!File.Exists(Path.Combine(tempPath, "game.iso")))
            {
                Console.Clear();

                throw new Exception("WIIAn error occured while Creating the ISO");
            }
            Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
            romPath = Path.Combine(tempPath, "game.iso");
            mvvm.Progress = 50;

            mvm.msg = "Replacing TIK and TMD...";
            using (Process extract = new Process())
            {
                if (!mvm.debug)
                {
                    extract.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                extract.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                extract.StartInfo.Arguments = $"extract \"{Path.Combine(tempPath, "game.iso")}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{Path.Combine(tempPath, "TIKTMD")}\" -vv1";
                extract.Start();
                extract.WaitForExit();
                foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "code"), "rvlt.*"))
                {
                    File.Delete(sFile);
                }
                File.Copy(Path.Combine(tempPath, "TIKTMD", "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"));
                File.Copy(Path.Combine(tempPath, "TIKTMD", "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"));
                Directory.Delete(Path.Combine(tempPath, "TIKTMD"), true);
            }
            mvm.Progress = 60;
            mvm.msg = "Injecting ROM...";
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "content"), "*.nfs"))
            {
                File.Delete(sFile);
            }
            File.Move(Path.Combine(tempPath, "game.iso"), Path.Combine(baseRomPath, "content", "game.iso"));
            File.Copy(Path.Combine(toolsPath, "nfs2iso2nfs.exe"), Path.Combine(baseRomPath, "content", "nfs2iso2nfs.exe"));
            Directory.SetCurrentDirectory(Path.Combine(baseRomPath, "content"));
            using (Process iso2nfs = new Process())
            {
                if (!mvm.debug)
                {

                    iso2nfs.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                iso2nfs.StartInfo.FileName = "nfs2iso2nfs.exe";
                string extra = "";
                if (mvm.Index == 2)
                {
                    extra = "-horizontal ";
                }
                if (mvm.Index == 3) { extra = "-wiimote "; }
                if (mvm.Index == 4) { extra = "-instantcc "; }
                if (mvm.Index == 5) { extra = "-nocc "; }
                if (mvm.LR) { extra += "-lrpatch "; }
                iso2nfs.StartInfo.Arguments = $"-enc -homebrew {extra}-iso game.iso";
                iso2nfs.Start();
                iso2nfs.WaitForExit();
                File.Delete("nfs2iso2nfs.exe");
                File.Delete("game.iso");
            }
            Directory.SetCurrentDirectory(savedir);
            mvm.Progress = 80;


        }

        private static void WiiHomebrew(string romPath, MainViewModel mvm)
        {
            string savedir = Directory.GetCurrentDirectory();
            mvvm.msg = "Extracting Homebrew Base...";
            if (Directory.Exists(Path.Combine(tempPath, "TempBase"))) Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
            Directory.CreateDirectory(Path.Combine(tempPath, "TempBase"));
            using (Process zip = new Process())
            {
                if (!mvm.debug)
                {

                    zip.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                zip.StartInfo.FileName = Path.Combine(toolsPath, "7za.exe");
                zip.StartInfo.Arguments = $"x \"{Path.Combine(toolsPath, "BASE.zip")}\" -o\"{Path.Combine(tempPath)}\"";
                zip.Start();
                zip.WaitForExit();
            }

            DirectoryCopy(Path.Combine(tempPath, "BASE"), Path.Combine(tempPath, "TempBase"), true);
            mvvm.Progress = 20;
            mvvm.msg = "Injecting DOL...";

            File.Copy(romPath, Path.Combine(tempPath, "TempBase", "sys", "main.dol"));
            mvm.Progress = 30;
            mvvm.msg = "Creating Injectable file...";
            using (Process wit = new Process())
            {
                if (!mvm.debug)
                {

                    wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                wit.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                wit.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "TempBase")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --links --iso";
                wit.Start();
                wit.WaitForExit();
            }

            Thread.Sleep(6000);
            if (!File.Exists(Path.Combine(tempPath, "game.iso")))
            {
                Console.Clear();

                throw new Exception("WIIAn error occured while Creating the ISO");
            }
            Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
            romPath = Path.Combine(tempPath, "game.iso");
            mvvm.Progress = 50;

            mvm.msg = "Replacing TIK and TMD...";
            using (Process extract = new Process())
            {
                if (!mvm.debug)
                {
                    extract.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                extract.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                extract.StartInfo.Arguments = $"extract \"{Path.Combine(tempPath, "game.iso")}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{Path.Combine(tempPath, "TIKTMD")}\" -vv1";
                extract.Start();
                extract.WaitForExit();
                foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "code"), "rvlt.*"))
                {
                    File.Delete(sFile);
                }
                File.Copy(Path.Combine(tempPath, "TIKTMD", "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"));
                File.Copy(Path.Combine(tempPath, "TIKTMD", "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"));
                Directory.Delete(Path.Combine(tempPath, "TIKTMD"), true);
            }
            mvm.Progress = 60;
            mvm.msg = "Injecting ROM...";
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "content"), "*.nfs"))
            {
                File.Delete(sFile);
            }
            File.Move(Path.Combine(tempPath, "game.iso"), Path.Combine(baseRomPath, "content", "game.iso"));
            File.Copy(Path.Combine(toolsPath, "nfs2iso2nfs.exe"), Path.Combine(baseRomPath, "content", "nfs2iso2nfs.exe"));
            Directory.SetCurrentDirectory(Path.Combine(baseRomPath, "content"));
            using (Process iso2nfs = new Process())
            {
                if (!mvm.debug)
                {

                    iso2nfs.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                iso2nfs.StartInfo.FileName = "nfs2iso2nfs.exe";
                string pass = "-passthrough ";
                if(mvm.passtrough != true)
                {
                    pass = "";
                }
                iso2nfs.StartInfo.Arguments = $"-enc -homebrew {pass}-iso game.iso";
                iso2nfs.Start();
                iso2nfs.WaitForExit();
                File.Delete("nfs2iso2nfs.exe");
                File.Delete("game.iso");
            }
            Directory.SetCurrentDirectory(savedir);
            mvm.Progress = 80;

        
    }

        private static void WII(string romPath, MainViewModel mvm)
        {
            string savedir = Directory.GetCurrentDirectory();
            if (mvm.NKITFLAG || romPath.Contains("nkit"))
            {
                using (Process toiso = new Process())
                {
                    mvm.msg = "Converting NKIT to ISO";
                    if (!mvm.debug)
                    {
                        toiso.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        // toiso.StartInfo.CreateNoWindow = true;
                    }
                    toiso.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToIso.exe");
                    toiso.StartInfo.Arguments = $"\"{romPath}\"";

                    toiso.Start();
                    toiso.WaitForExit();
                    if(!File.Exists(Path.Combine(toolsPath, "out.iso")))
                    {
                        throw new Exception("nkit");
                    }
                    File.Move(Path.Combine(toolsPath, "out.iso"), Path.Combine(tempPath, "pre.iso"));
                    mvm.Progress = 15;
                }
            }
            else
            {
                if (new FileInfo(romPath).Extension.Contains("wbfs"))
                {
                    mvm.msg = "Converting WBFS to ISO...";
                    using (Process toiso = new Process())
                    {
                        if (!mvm.debug)
                        {
                            toiso.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            // toiso.StartInfo.CreateNoWindow = true;
                        }
                        toiso.StartInfo.FileName = Path.Combine(toolsPath, "wbfs_file.exe");
                        toiso.StartInfo.Arguments = $"\"{romPath}\" convert \"{Path.Combine(tempPath, "pre.iso")}\" -t";

                        toiso.Start();
                        toiso.WaitForExit();
                        mvm.Progress = 15;
                    }
                }
                else if (new FileInfo(romPath).Extension.Contains("iso"))
                {
                    mvm.msg = "Copying ROM...";
                    File.Copy(romPath, Path.Combine(tempPath, "pre.iso"));
                    mvm.Progress = 15;
                }
            }
            //GET ROMCODE and change it
            mvm.msg = "Trying to change the Manual...";
            //READ FIRST 4 BYTES
            byte[] chars = new byte[4];
            FileStream fstrm = new FileStream(Path.Combine(tempPath, "pre.iso"), FileMode.Open);
            fstrm.Read(chars, 0, 4);
            fstrm.Close();
            string procod = ByteArrayToString(chars);
            string neededformanual = procod.ToHex();
            string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(metaXml);
            doc.SelectSingleNode("menu/reserved_flag2").InnerText = neededformanual;
            doc.Save(metaXml);
            //edit emta.xml
            mvm.Progress = 20;

            if (!mvm.donttrim)
            {
                if (mvm.regionfrii)
                {
                    if (mvm.regionfriius)
                    {
                        using (FileStream fs = new FileStream(Path.Combine(tempPath, "pre.iso"), FileMode.Open))
                        {
                            fs.Seek(0x4E003, SeekOrigin.Begin);
                            fs.Write(new byte[] { 0x01 },0,1);
                            fs.Seek(0x4E010, SeekOrigin.Begin);
                            fs.Write(new byte[] { 0x80, 0x06, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 },0,16);
                            fs.Close();

                        }
                    }
                    else if(mvm.regionfriijp)
                    {
                        using (FileStream fs = new FileStream(Path.Combine(tempPath, "pre.iso"), FileMode.Open))
                        {
                            fs.Seek(0x4E003, SeekOrigin.Begin);
                            fs.Write(new byte[] { 0x00 }, 0, 1);
                            fs.Seek(0x4E010, SeekOrigin.Begin);
                            fs.Write(new byte[] { 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 },0, 16);
                            fs.Close();

                        }
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(Path.Combine(tempPath, "pre.iso"), FileMode.Open))
                        {
                            fs.Seek(0x4E003, SeekOrigin.Begin);
                            fs.Write(new byte[] { 0x02 }, 0, 1);
                            fs.Seek(0x4E010, SeekOrigin.Begin);
                            fs.Write(new byte[] { 0x80, 0x80, 0x80, 0x00, 0x03, 0x03, 0x04, 0x03, 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
                            fs.Close();

                        }
                    }
                }
                using (Process trimm = new Process())
                {
                    if (!mvm.debug)
                    {

                        trimm.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    }
                    mvm.msg = "Trimming ROM...";
                    trimm.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                    trimm.StartInfo.Arguments = $"extract \"{Path.Combine(tempPath, "pre.iso")}\" --DEST \"{Path.Combine(tempPath, "TEMP")}\" --psel data -vv1";
                    trimm.Start();
                    trimm.WaitForExit();
                    mvm.Progress = 30;
                }
                if (mvm.Index == 4)
                {
                    mvvm.msg = "Patching ROM (Force CC)...";
                    Console.WriteLine("Patching the ROM to force Classic Controller input");
                    using (Process tik = new Process())
                    {
                        tik.StartInfo.FileName = Path.Combine(toolsPath, "GetExtTypePatcher.exe");
                        tik.StartInfo.Arguments = $"\"{Path.Combine(tempPath, "TEMP", "sys", "main.dol")}\" -nc";
                        tik.StartInfo.UseShellExecute = false;
                        tik.StartInfo.CreateNoWindow = true;
                        tik.StartInfo.RedirectStandardOutput = true;
                        tik.StartInfo.RedirectStandardInput = true;
                        tik.Start();
                        Thread.Sleep(2000);
                        tik.StandardInput.WriteLine();
                        tik.WaitForExit();
                        mvm.Progress = 35;
                    }

                }
                if (mvm.jppatch)
                {
                    
                    mvm.msg = "Language Patching ROM...";
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(tempPath, "TEMP", "sys", "main.dol"), FileMode.Open)))
                    {
                        byte[] stuff = new byte[] { 0x38, 0x60};
                        writer.Seek(0x4CBDAC, SeekOrigin.Begin);
                        writer.Write(stuff);
                        writer.Seek(0x4CBDAF, SeekOrigin.Begin);
                        stuff = new byte[] { 0x00 };
                        writer.Write(stuff);
                        writer.Close();
                    }
                    mvm.Progress = 37;
                }
                if (mvm.Patch)
                {
                   
                    mvm.msg = "Video Patching ROM...";
                    using (Process vmc = new Process())
                    {

                        File.Copy(Path.Combine(toolsPath, "wii-vmc.exe"), Path.Combine(tempPath, "TEMP", "sys", "wii-vmc.exe"));

                        Directory.SetCurrentDirectory(Path.Combine(tempPath, "TEMP", "sys"));
                        vmc.StartInfo.FileName = "wii-vmc.exe";
                        vmc.StartInfo.Arguments = "main.dol";
                        vmc.StartInfo.UseShellExecute = false;
                        vmc.StartInfo.CreateNoWindow = true;
                        vmc.StartInfo.RedirectStandardOutput = true;
                        vmc.StartInfo.RedirectStandardInput = true;

                        vmc.Start();
                        Thread.Sleep(1000);
                        vmc.StandardInput.WriteLine("a");
                        Thread.Sleep(2000);
                        if (mvm.toPal) vmc.StandardInput.WriteLine("1");
                        else vmc.StandardInput.WriteLine("2");
                        Thread.Sleep(2000);
                        vmc.StandardInput.WriteLine();
                        vmc.WaitForExit();
                        File.Delete("wii-vmc.exe");


                        Directory.SetCurrentDirectory(savedir);
                        mvm.Progress = 40;
                    }

                }
                mvm.msg = "Creating ISO from trimmed ROM...";
                using (Process repack = new Process())
                {
                    if (!mvm.debug)
                    {

                        repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    }
                    repack.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                    repack.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "TEMP")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --links --iso";
                    repack.Start();
                    repack.WaitForExit();
                    Directory.Delete(Path.Combine(tempPath, "TEMP"), true);
                    File.Delete(Path.Combine(tempPath, "pre.iso"));
                }
            }
            else
            {
              if(mvm.Index == 4 || mvm.Patch)
                {
                    using (Process trimm = new Process())
                    {
                        if (!mvm.debug)
                        {

                            trimm.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        }
                        mvm.msg = "Trimming ROM...";
                        trimm.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                        trimm.StartInfo.Arguments = $"extract \"{Path.Combine(tempPath, "pre.iso")}\" --DEST \"{Path.Combine(tempPath, "TEMP")}\" --psel WHOLE -vv1";
                        trimm.Start();
                        trimm.WaitForExit();
                        mvm.Progress = 30;
                    }
                    if (mvm.Index == 4)
                    {
                        mvvm.msg = "Patching ROM (Force CC)...";
                        Console.WriteLine("Patching the ROM to force Classic Controller input");
                        using (Process tik = new Process())
                        {
                            tik.StartInfo.FileName = Path.Combine(toolsPath, "GetExtTypePatcher.exe");
                            tik.StartInfo.Arguments = $"\"{Path.Combine(tempPath, "TEMP","DATA", "sys", "main.dol")}\" -nc";
                            tik.StartInfo.UseShellExecute = false;
                            tik.StartInfo.CreateNoWindow = true;
                            tik.StartInfo.RedirectStandardOutput = true;
                            tik.StartInfo.RedirectStandardInput = true;
                            tik.Start();
                            Thread.Sleep(2000);
                            tik.StandardInput.WriteLine();
                            tik.WaitForExit();
                            mvm.Progress = 35;
                        }

                    }
                    if (mvm.Patch)
                    {
                        mvm.msg = "Video Patching ROM...";
                        using (Process vmc = new Process())
                        {

                            File.Copy(Path.Combine(toolsPath, "wii-vmc.exe"), Path.Combine(tempPath, "TEMP", "DATA", "sys", "wii-vmc.exe"));

                            Directory.SetCurrentDirectory(Path.Combine(tempPath, "TEMP", "DATA", "sys"));
                            vmc.StartInfo.FileName = "wii-vmc.exe";
                            vmc.StartInfo.Arguments = "main.dol";
                            vmc.StartInfo.UseShellExecute = false;
                            vmc.StartInfo.CreateNoWindow = true;
                            vmc.StartInfo.RedirectStandardOutput = true;
                            vmc.StartInfo.RedirectStandardInput = true;

                            vmc.Start();
                            Thread.Sleep(1000);
                            vmc.StandardInput.WriteLine("a");
                            Thread.Sleep(2000);
                            if (mvm.toPal) vmc.StandardInput.WriteLine("1");
                            else vmc.StandardInput.WriteLine("2");
                            Thread.Sleep(2000);
                            vmc.StandardInput.WriteLine();
                            vmc.WaitForExit();
                            File.Delete("wii-vmc.exe");


                            Directory.SetCurrentDirectory(savedir);
                            mvm.Progress = 40;
                        }

                    }
                    mvm.msg = "Creating ISO from patched ROM...";
                    using (Process repack = new Process())
                    {
                        if (!mvm.debug)
                        {

                            repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        }
                        repack.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                        repack.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "TEMP")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --psel WHOLE --iso";
                        repack.Start();
                        repack.WaitForExit();
                        Directory.Delete(Path.Combine(tempPath, "TEMP"), true);
                        File.Delete(Path.Combine(tempPath, "pre.iso"));
                    }
                }
                else
                {
                    File.Move(Path.Combine(tempPath, "pre.iso"), Path.Combine(tempPath, "game.iso"));
               }
               
            }
            
            mvm.Progress = 50;
            mvm.msg = "Replacing TIK and TMD...";
            using (Process extract = new Process())
            {
                if (!mvm.debug)
                {
                   
                    extract.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                extract.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                extract.StartInfo.Arguments = $"extract \"{Path.Combine(tempPath, "game.iso")}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{Path.Combine(tempPath, "TIKTMD")}\" -vv1";
                extract.Start();
                extract.WaitForExit();
                foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "code"), "rvlt.*"))
                {
                    File.Delete(sFile);
                }
                File.Copy(Path.Combine(tempPath, "TIKTMD", "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"));
                File.Copy(Path.Combine(tempPath, "TIKTMD", "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"));
                Directory.Delete(Path.Combine(tempPath, "TIKTMD"), true);
            }
            mvm.Progress = 60;
            mvm.msg = "Injecting ROM...";
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "content"), "*.nfs"))
            {
                File.Delete(sFile);
            }
            File.Move(Path.Combine(tempPath, "game.iso"), Path.Combine(baseRomPath, "content", "game.iso"));
            File.Copy(Path.Combine(toolsPath, "nfs2iso2nfs.exe"), Path.Combine(baseRomPath, "content", "nfs2iso2nfs.exe"));
            Directory.SetCurrentDirectory(Path.Combine(baseRomPath, "content"));
            using (Process iso2nfs = new Process())
            {
                if (!mvm.debug)
                {
                   
                    iso2nfs.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                iso2nfs.StartInfo.FileName = "nfs2iso2nfs.exe";
                string extra = "";
                if (mvm.Index == 2)
                {
                    extra = "-horizontal ";
                }
                if (mvm.Index == 3) { extra = "-wiimote "; }
                if (mvm.Index == 4) { extra = "-instantcc "; }
                if (mvm.Index == 5) { extra = "-nocc "; }
                if (mvm.LR) { extra += "-lrpatch "; }
                iso2nfs.StartInfo.Arguments = $"-enc {extra}-iso game.iso";
                iso2nfs.Start();
                iso2nfs.WaitForExit();
                File.Delete("nfs2iso2nfs.exe");
                File.Delete("game.iso");
            }
            Directory.SetCurrentDirectory(savedir);
            mvm.Progress = 80;
        }
        private static void GC(string romPath, MainViewModel mvm, bool force)
        {
            string savedir = Directory.GetCurrentDirectory();
            mvvm.msg = "Extracting Nintendont Base...";
            if (Directory.Exists(Path.Combine(tempPath, "TempBase"))) Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
            Directory.CreateDirectory(Path.Combine(tempPath, "TempBase"));
            using (Process zip = new Process()){
                if (!mvm.debug)
                {
                   
                   zip.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                zip.StartInfo.FileName = Path.Combine(toolsPath, "7za.exe");
                zip.StartInfo.Arguments = $"x \"{Path.Combine(toolsPath, "BASE.zip")}\" -o\"{Path.Combine(tempPath)}\"";
                zip.Start();
                zip.WaitForExit();
            }
           
            DirectoryCopy(Path.Combine(tempPath, "BASE"), Path.Combine(tempPath, "TempBase"), true);
            mvvm.Progress = 20;
            mvvm.msg = "Applying Nintendont";
            if (force)
            {
                mvvm.msg += " force 4:3...";
                File.Copy(Path.Combine(toolsPath, "nintendont_force.dol"), Path.Combine(tempPath, "TempBase", "sys", "main.dol"));
            }
            else
            {
                mvvm.msg += "...";
                File.Copy(Path.Combine(toolsPath, "nintendont.dol"), Path.Combine(tempPath, "TempBase", "sys", "main.dol"));
            }
            mvm.Progress = 40;
            mvvm.msg = "Injecting GameCube Game into NintendontBase...";
            if (mvm.donttrim)
            {
                if (romPath.ToLower().Contains("nkit.iso"))
                {
                    using (Process wit = new Process())
                    {
                        if (!mvm.debug)
                        {

                            wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        }
                        wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToIso.exe");
                        wit.StartInfo.Arguments = $"\"{romPath}\"";
                        wit.Start();
                        wit.WaitForExit();
                        if (!File.Exists(Path.Combine(toolsPath, "out.iso")))
                        {
                            throw new Exception("nkit");
                        }
                        File.Move(Path.Combine(toolsPath, "out.iso"), Path.Combine(tempPath, "TempBase", "files", "game.iso"));

                    }
                }
                else
                {
                    if (romPath.ToLower().Contains("gcz"))
                    {
                        //Convert to nkit.iso
                        using (Process wit = new Process())
                        {
                            if (!mvm.debug)
                            {

                                wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            }
                            wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToIso.exe");
                            wit.StartInfo.Arguments = $"\"{romPath}\"";
                            wit.Start();
                            wit.WaitForExit();
                            if (!File.Exists(Path.Combine(toolsPath, "out.iso")))
                            {
                                throw new Exception("nkit");
                            }
                            File.Move(Path.Combine(toolsPath, "out.iso"), Path.Combine(tempPath, "TempBase", "files", "game.iso"));

                        }
                    }
                    else
                    {
                        File.Copy(romPath, Path.Combine(tempPath, "TempBase", "files", "game.iso"));
                    }
                   
                }
            }
            else
            {
                if (romPath.ToLower().Contains("iso") || romPath.ToLower().Contains("gcm"))
                {
                    //convert to nkit
                    using (Process wit = new Process())
                    {
                        if (!mvm.debug)
                        {

                            wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        }
                        wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToNKit.exe");
                        wit.StartInfo.Arguments = $"\"{romPath}\"";
                        wit.Start();
                        wit.WaitForExit();
                        if (!File.Exists(Path.Combine(toolsPath, "out.nkit.iso")))
                        {
                            throw new Exception("nkit");
                        }
                        File.Move(Path.Combine(toolsPath, "out.nkit.iso"), Path.Combine(tempPath, "TempBase", "files", "game.iso"));

                    }
                    
                }
                else
                {
                    if (romPath.ToLower().Contains("gcz"))
                    {
                        //Convert to nkit.iso
                        using (Process wit = new Process())
                        {
                            if (!mvm.debug)
                            {

                                wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            }
                            wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToNKit.exe");
                            wit.StartInfo.Arguments = $"\"{romPath}\"";
                            wit.Start();
                            wit.WaitForExit();
                            if (!File.Exists(Path.Combine(toolsPath, "out.nkit.iso")))
                            {
                                throw new Exception("nkit");
                            }
                            File.Move(Path.Combine(toolsPath, "out.nkit.iso"), Path.Combine(tempPath, "TempBase", "files", "game.iso"));

                        }
                    }
                    else
                    {
                        File.Copy(romPath, Path.Combine(tempPath, "TempBase", "files", "game.iso"));
                    }
                    
                }

            }

            if (mvm.gc2rom != "" && File.Exists(mvm.gc2rom))
            {
                if (mvm.donttrim)
                {
                    if (mvm.gc2rom.Contains("nkit"))
                     {
                         using (Process wit = new Process())
                         {
                             if (!mvm.debug)
                             {

                                 wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                             }
                             wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToIso.exe");
                             wit.StartInfo.Arguments = $"\"{mvm.gc2rom}\"";
                             wit.Start();
                             wit.WaitForExit();
                             if (!File.Exists(Path.Combine(toolsPath, "out(Disc 1).iso")))
                             {
                                 throw new Exception("nkit");
                             }
                             File.Move(Path.Combine(toolsPath, "out(Disc 1).iso"), Path.Combine(tempPath, "TempBase", "files", "disc2.iso"));

                         }
                     }
                     else
                     {
                        
                        
                            File.Copy(mvm.gc2rom, Path.Combine(tempPath, "TempBase", "files", "disc2.iso"));
                        
                        
                    }
                }
                else{
                    if (mvm.gc2rom.ToLower().Contains("iso") || mvm.gc2rom.ToLower().Contains("gcm"))
                    {
                        //convert to nkit
                        using (Process wit = new Process())
                        {
                            if (!mvm.debug)
                            {

                                wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            }
                            wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToNKit.exe");
                            wit.StartInfo.Arguments = $"\"{mvm.gc2rom}\"";
                            wit.Start();
                            wit.WaitForExit();
                            if (!File.Exists(Path.Combine(toolsPath, "out(Disc 1).nkit.iso")))
                            {
                                throw new Exception("nkit");
                            }
                            File.Move(Path.Combine(toolsPath, "out(Disc 1).nkit.iso"), Path.Combine(tempPath, "TempBase", "files", "disc2.iso"));

                        }
                    }
                    else
                    {
                        if (romPath.ToLower().Contains("gcz"))
                        {
                            //Convert to nkit.iso
                            using (Process wit = new Process())
                            {
                                if (!mvm.debug)
                                {

                                    wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                }
                                wit.StartInfo.FileName = Path.Combine(toolsPath, "ConvertToNKit.exe");
                                wit.StartInfo.Arguments = $"\"{romPath}\"";
                                wit.Start();
                                wit.WaitForExit();
                                if (!File.Exists(Path.Combine(toolsPath, "out(Disc 1).nkit.iso")))
                                {
                                    throw new Exception("nkit");
                                }
                                File.Move(Path.Combine(toolsPath, "out(Disc 1).nkit.iso"), Path.Combine(tempPath, "TempBase", "files", "disc2.iso"));

                            }
                        }
                        else
                        {
                            File.Copy(romPath, Path.Combine(tempPath, "TempBase", "files", "disc2.iso"));
                        }
                    }
                    
                }
               
                
                
            }
            using(Process wit = new Process())
            {
                if (!mvm.debug)
                {
                   
                    wit.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                wit.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                wit.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "TempBase")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --links --iso";
                wit.Start();
                wit.WaitForExit();
            }

            Thread.Sleep(6000);
            if (!File.Exists(Path.Combine(tempPath, "game.iso")))
            {
                Console.Clear();

                throw new Exception("WIIAn error occured while Creating the ISO");
            }
            Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
            romPath = Path.Combine(tempPath, "game.iso");
            mvvm.Progress = 50;

            mvm.msg = "Replacing TIK and TMD...";
            using (Process extract = new Process())
            {
                if (!mvm.debug)
                {
                    extract.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                extract.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                extract.StartInfo.Arguments = $"extract \"{Path.Combine(tempPath, "game.iso")}\" --psel data --files +tmd.bin --files +ticket.bin --DEST \"{Path.Combine(tempPath, "TIKTMD")}\" -vv1";
                extract.Start();
                extract.WaitForExit();
                foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "code"), "rvlt.*"))
                {
                    File.Delete(sFile);
                }
                File.Copy(Path.Combine(tempPath, "TIKTMD", "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"));
                File.Copy(Path.Combine(tempPath, "TIKTMD", "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"));
                Directory.Delete(Path.Combine(tempPath, "TIKTMD"), true);
            }
            mvm.Progress = 60;
            mvm.msg = "Injecting ROM...";
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "content"), "*.nfs"))
            {
                File.Delete(sFile);
            }
            File.Move(Path.Combine(tempPath, "game.iso"), Path.Combine(baseRomPath, "content", "game.iso"));
            File.Copy(Path.Combine(toolsPath, "nfs2iso2nfs.exe"), Path.Combine(baseRomPath, "content", "nfs2iso2nfs.exe"));
            Directory.SetCurrentDirectory(Path.Combine(baseRomPath, "content"));
            using (Process iso2nfs = new Process())
            {
                if (!mvm.debug)
                {
                   
                    iso2nfs.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                iso2nfs.StartInfo.FileName = "nfs2iso2nfs.exe";
                iso2nfs.StartInfo.Arguments = $"-enc -homebrew -passthrough -iso game.iso";
                iso2nfs.Start();
                iso2nfs.WaitForExit();
                File.Delete("nfs2iso2nfs.exe");
                File.Delete("game.iso");
            }
            Directory.SetCurrentDirectory(savedir);
            mvm.Progress = 80;
            
        }
        private static void WIIold(string romPath, MainViewModel mvm, bool force)
        {
            mvvm.msg = "Removing unnecessary Files...";

            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "content"), "*.nfs"))
            {
                File.Delete(sFile);
            }
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "meta"), "*.jpg"))
            {
                File.Delete(sFile);
            }
            foreach (string sFile in Directory.GetFiles(Path.Combine(baseRomPath, "meta"), "*.bfma"))
            {
                File.Delete(sFile);
            }

            if (File.Exists(Path.Combine(baseRomPath, "code", "rvlt.tmd"))) File.Delete(Path.Combine(baseRomPath, "code", "rvlt.tmd"));
            if (File.Exists(Path.Combine(baseRomPath, "code", "rvlt.tik"))) File.Delete(Path.Combine(baseRomPath, "code", "rvlt.tik"));
            mvvm.Progress = 15;
            Console.WriteLine("Finished removing Files");

            using (Process tik = new Process())
            {
                if (!mvm.debug)
                {
                    tik.StartInfo.UseShellExecute = false;
                    tik.StartInfo.CreateNoWindow = true;
                }
                if (!mvm.GC)
                {
                    if (new FileInfo(romPath).Extension.Contains("wbfs"))
                    {
                        mvvm.msg = "Converting WBFS to ISO...";
                        Console.WriteLine("Converting WBFS to ISO...");
                        
                            tik.StartInfo.FileName = Path.Combine(toolsPath, "wbfs_file.exe");
                        tik.StartInfo.Arguments = $"\"{romPath}\" convert \"{Path.Combine(tempPath, "pre.iso")}\"";
                        tik.Start();
                        tik.WaitForExit();
                        if (!File.Exists(Path.Combine(tempPath, "pre.iso")))
                        {
                            throw new Exception("WIIAn error occured while converting WBFS to ISO");
                        }
                        if (File.Exists(Path.Combine(tempPath, "rom.wbfs"))) { File.Delete(Path.Combine(tempPath, "rom.wbfs")); }
                        romPath = Path.Combine(tempPath, "pre.iso");
                        Console.WriteLine("Finished Conversion");
                        mvvm.Progress = 20;
                    }
                    tik.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                    mvvm.msg = "Trimming ROM...";
                    Console.WriteLine("Trimming ROM...");

                    tik.StartInfo.Arguments = $"extract \"{romPath}\" --DEST \"{Path.Combine(tempPath, "IsoExt")}\" --psel data -vv1";
                    tik.Start();
                    tik.WaitForExit();
                    if (!Directory.Exists(Path.Combine(tempPath, "IsoExt")))
                    {

                        throw new Exception("WIIAn error occured while trimming the ROM");
                    }
                    mvvm.Progress = 40;
                    Console.WriteLine("Finished trimming");
                    if (mvm.Index == 4)
                    {
                        mvvm.msg = "Patching ROM (Force CC)...";
                        Console.WriteLine("Patching the ROM to force Classic Controller input");
                        tik.StartInfo.FileName = Path.Combine(toolsPath, "GetExtTypePatcher.exe");
                        tik.StartInfo.Arguments = $"\"{Path.Combine(tempPath, "IsoExt", "sys", "main.dol")}\"";
                        tik.Start();
                        tik.WaitForExit();
                        tik.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                        mvvm.Progress = 45;
                    }
                    if (mvm.Patch)
                    {
                        mvvm.msg = "Video Patching ROM...";
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = Path.Combine(toolsPath,"wii-vmc.exe");
                            process.StartInfo.Arguments = $"\"{Path.Combine(tempPath, "IsoExt", "sys", "main.dol")}\"";
                            //process.StartInfo.RedirectStandardInput = true;
                           // process.StartInfo.UseShellExecute = false;
                           // process.StartInfo.CreateNoWindow = true;
                            
                            process.Start();
                           /* Thread.Sleep(2000);
                            process.StandardInput.WriteLine("a");
                            Thread.Sleep(2000);
                            if (mvm.toPal)
                            {
                                process.StandardInput.WriteLine("1");
                            }
                            else
                            {
                                process.StandardInput.WriteLine("2");
                            }

                            Thread.Sleep(2000);
                            process.StandardInput.WriteLine();
                            */
                            process.WaitForExit();
                        }
                        mvvm.Progress = 50;
                    }
                   
                    mvvm.msg = "Creating ISO from trimmed ROM...";
                    Console.WriteLine("Creating ISO from trimmed ROM...");
                    tik.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "IsoExt")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --links --iso";
                    tik.Start();
                    tik.WaitForExit();

                    if (!File.Exists(Path.Combine(tempPath, "game.iso")))
                    {
                        throw new Exception("WIIAn error occured while Creating the ISO");
                    }
                    romPath = Path.Combine(tempPath, "game.iso");
                    mvvm.Progress = 60;
                }
                else
                {
                    mvvm.msg = "Extracting Nintendont Base...";
                    if (Directory.Exists(Path.Combine(tempPath, "TempBase"))) Directory.Delete(Path.Combine(tempPath, "TempBase"), true);
                    Directory.CreateDirectory(Path.Combine(tempPath, "TempBase"));
                    tik.StartInfo.FileName =  Path.Combine(toolsPath, "7za.exe");
                    tik.StartInfo.Arguments = $"x \"{Path.Combine(toolsPath, "BASE.zip")}\" -o\"{Path.Combine(tempPath)}\"";
                    tik.Start();
                    tik.WaitForExit();
                    DirectoryCopy(Path.Combine(tempPath, "BASE"), Path.Combine(tempPath, "TempBase"), true);
                    mvvm.Progress = 30;
                    mvvm.msg = "Applying Nintendont";
                    if (force)
                    {
                        mvvm.msg += " force 4:3...";
                        File.Copy(Path.Combine(toolsPath, "nintendont_force.dol"), Path.Combine(tempPath, "TempBase", "sys", "main.dol"));
                    }
                    else
                    {
                        mvvm.msg += "...";
                        File.Copy(Path.Combine(toolsPath, "nintendont.dol"), Path.Combine(tempPath, "TempBase", "sys", "main.dol"));
                    }
                    mvm.Progress = 40;
                    mvvm.msg = "Injecting GameCube Game into NintendontBase...";
                    File.Copy(romPath, Path.Combine(tempPath, "TempBase", "files", "game.iso"));
                    Thread.Sleep(6000);
                    tik.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                    tik.StartInfo.Arguments = $"copy \"{Path.Combine(tempPath, "TempBase")}\" --DEST \"{Path.Combine(tempPath, "game.iso")}\" -ovv --links --iso";
                    tik.Start();
                    tik.WaitForExit();
                    Thread.Sleep(6000);
                    if (!File.Exists(Path.Combine(tempPath, "game.iso")))
                    {
                        Console.Clear();

                        throw new Exception("WIIAn error occured while Creating the ISO");
                    }
                    romPath = Path.Combine(tempPath, "game.iso");
                    mvvm.Progress = 60;
                }

                mvvm.msg = "Extracting Ticket and TMD from ISO...";
                tik.StartInfo.FileName = Path.Combine(toolsPath, "wit.exe");
                tik.StartInfo.Arguments = $"extract \"{romPath}\" --psel data --files +tmd.bin --files +ticket.bin --dest \"{Path.Combine(tempPath, "tik")}\" -vv1";
                tik.Start();
                tik.WaitForExit();
                if (!Directory.Exists(Path.Combine(tempPath, "tik")) || !File.Exists(Path.Combine(tempPath, "tik", "tmd.bin")) || !File.Exists(Path.Combine(tempPath, "tik", "ticket.bin")))
                {
                    throw new Exception("WIIAn error occured while extracting the Ticket and TMD");
                }
                Console.WriteLine("Finished extracting");
                mvvm.Progress = 65;
                mvvm.msg = "Copying TIK and TMD...";
                Console.WriteLine("Copying TIK and TMD...");
                if (File.Exists(Path.Combine(baseRomPath, "code", "rvlt.tmd"))) { File.Delete(Path.Combine(baseRomPath, "code", "rvlt.tmd")); }
                File.Copy(Path.Combine(tempPath, "tik", "tmd.bin"), Path.Combine(baseRomPath, "code", "rvlt.tmd"));
                if (File.Exists(Path.Combine(baseRomPath, "code", "rvlt.tik"))) { File.Delete(Path.Combine(baseRomPath, "code", "rvlt.tik")); }
                File.Copy(Path.Combine(tempPath, "tik", "ticket.bin"), Path.Combine(baseRomPath, "code", "rvlt.tik"));
                if (!File.Exists(Path.Combine(baseRomPath, "code", "rvlt.tik")) || !File.Exists(Path.Combine(baseRomPath, "code", "rvlt.tmd")))
                {
                    Console.Clear();
                    throw new Exception("WIIAn error occured while copying the Ticket and TMD");
                }
                Console.WriteLine("Finished Copying");
                mvvm.Progress = 70;
                Thread.Sleep(6000);
                mvvm.msg = "Injecting ROM...";
                Console.WriteLine("Converting Game to NFS format...");
                string olddir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(Path.Combine(baseRomPath, "content"));
                tik.StartInfo.FileName = Path.Combine(toolsPath, "nfs2iso2nfs.exe");
                if (!mvm.GC)
                {
                    string extra = "";
                    if (mvm.Index == 2)
                    {
                        extra = "-horizontal ";
                    }
                    if (mvm.Index == 3) { extra = "-wiimote "; }
                    if (mvm.Index == 4) { extra = "-instantcc "; }
                    if (mvm.Index == 5) { extra = "-nocc "; }
                    if (mvm.LR) { extra += "-lrpatch "; }
                    Console.WriteLine(extra);
                    Console.ReadLine();
                    tik.StartInfo.Arguments = $"-enc {extra}-iso \"{romPath}\"";
                }
                else
                {
                    tik.StartInfo.Arguments = $"-enc -homebrew -passthrough -iso \"{romPath}\"";
                }
                tik.Start();
                tik.WaitForExit();
                Console.WriteLine("Finished Conversion");
                mvvm.Progress = 80;
                Directory.SetCurrentDirectory(olddir);
            }
        }

       
        public static void MSX(string injectRomPath)
        {
            mvvm.msg = "Reading Header from Base...";
            byte[] test = new byte[0x580B3];
            using (var fs = new FileStream(Path.Combine(baseRomPath, "content" , "msx", "msx.pkg"),
                                 FileMode.Open,
                                 FileAccess.ReadWrite))
            {


                fs.Read(test, 0, 0x580B3);
                fs.Close();
                File.Delete(Path.Combine(baseRomPath, "content", "msx", "msx.pkg"));
            }
            mvvm.Progress = 20;
            mvvm.msg = "Creating new PKG with read Header...";
            using (var fs = new FileStream(Path.Combine(baseRomPath, "content", "msx", "msx.pkg"),
                                 FileMode.OpenOrCreate,
                                 FileAccess.ReadWrite))
            {


                fs.Write(test, 0, 0x580B3);
                fs.Close();

            }
            mvvm.Progress = 30;
            mvvm.msg = "Reading ROM content...";
            using (var fs = new FileStream(injectRomPath,
                                 FileMode.OpenOrCreate,
                                 FileAccess.ReadWrite))
            {


                test = new byte[fs.Length];
                fs.Read(test, 0, test.Length - 1);

            }
            mvvm.Progress = 50;
            mvvm.msg = "Injecting ROM into new PKG...";
            using (var fs = new FileStream(Path.Combine(baseRomPath, "content", "msx", "msx.pkg"),
                                FileMode.Append))
            {

                fs.Write(test, 0, test.Length);

            }
            mvvm.Progress = 80;
        }
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Thread.Sleep(0);
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
        public static void Clean()
        {
            if (Directory.Exists(tempPath))
            {
                DeleteDirectory(tempPath);
            }
        }
        [STAThread]
        public static void Loadiine(string gameName)
        {
            if (gameName == null || gameName == string.Empty) gameName = "NoName";
            gameName = gameName.Replace("|", " ");
            Regex reg = new Regex("[^a-zA-Z0-9 é -]");
            //string outputPath = Path.Combine(Properties.Settings.Default.InjectionPath, gameName);
            string outputPath = Path.Combine(Properties.Settings.Default.OutPath, $"[LOADIINE]{reg.Replace(gameName,"")} [{mvvm.prodcode}]");
            mvvm.foldername = $"[LOADIINE]{reg.Replace(gameName, "")} [{mvvm.prodcode}]";
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                outputPath = Path.Combine(Properties.Settings.Default.OutPath, $"[LOADIINE]{reg.Replace(gameName, "")} [{mvvm.prodcode}]_{i}");
                mvvm.foldername = $"[LOADIINE]{reg.Replace(gameName, "")} [{mvvm.prodcode}]_{i}";
                i++;
            }
            
            DirectoryCopy(baseRomPath,outputPath, true);

            Custom_Message cm = new Custom_Message("Injection Complete", $"To Open the Location of the Inject press Open Folder.\nIf you want the inject to be put on your SD now, press Copy to SD.", Settings.Default.OutPath);
            try
            {
                cm.Owner = mvvm.mw;
            }catch(Exception )
            {

            }
            cm.ShowDialog();
            Clean();
        }
        [STAThread]
        public static void Packing(string gameName, MainViewModel mvm)
        {
            mvm.msg = "Checking Tools...";
            mvm.InjcttoolCheck();
            mvm.Progress = 20;
            mvm.msg = "Creating Outputfolder...";
            Regex reg = new Regex("[^a-zA-Z0-9 -]");
            if (gameName == null || gameName == string.Empty) gameName = "NoName";
           
            //string outputPath = Path.Combine(Properties.Settings.Default.InjectionPath, gameName);
            string outputPath = Path.Combine(Properties.Settings.Default.OutPath, $"[WUP]{reg.Replace(gameName,"").Replace("|", " ")}");
            outputPath = outputPath.Replace("|", " ");
            mvvm.foldername = $"[WUP]{reg.Replace(gameName, "").Replace("|"," ")}";
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                outputPath = Path.Combine(Properties.Settings.Default.OutPath, $"[WUP]{reg.Replace(gameName,"").Replace("|", " ")}_{i}");
                mvvm.foldername = $"[WUP]{reg.Replace(gameName, "").Replace("|", " ")}_{i}";
                i++;
            }
            var oldpath = Directory.GetCurrentDirectory();
            mvm.Progress = 40;
            mvm.msg = "Packing...";
            using (Process cnuspacker = new Process())
            {
                if (!mvm.debug)
                {
                    cnuspacker.StartInfo.UseShellExecute = false;
                    cnuspacker.StartInfo.CreateNoWindow = true;
                }
                if (Environment.Is64BitOperatingSystem)
                {
                    cnuspacker.StartInfo.FileName = Path.Combine(toolsPath, "CNUSPACKER.exe");
                    cnuspacker.StartInfo.Arguments = $"-in \"{baseRomPath}\" -out \"{outputPath}\" -encryptKeyWith {Properties.Settings.Default.Ckey}";
                }
                else
                {
                    cnuspacker.StartInfo.FileName = "java";
                    cnuspacker.StartInfo.Arguments = $"-jar \"{Path.Combine(toolsPath, "NUSPacker.jar")}\" -in \"{baseRomPath}\" -out \"{outputPath}\" -encryptKeyWith {Properties.Settings.Default.Ckey}";
                }
                cnuspacker.Start();
                cnuspacker.WaitForExit();
                Directory.SetCurrentDirectory(oldpath);
            }
            mvm.Progress = 90;
            mvm.msg = "Cleaning...";
            Clean();
            mvm.Progress = 100;
            
            mvm.msg = "";
        }

        public static void Download(MainViewModel mvm)

        {
            
                mvm.InjcttoolCheck();
                GameBases b = mvm.getBasefromName(mvm.SelectedBaseAsString);

                //GetKeyOfBase
                TKeys key = mvm.getTkey(b);
                if (mvm.GameConfiguration.Console == GameConsoles.WII || mvm.GameConfiguration.Console == GameConsoles.GCN)
                {
                    using (Process zip = new Process())
                    {

                        if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                        Directory.CreateDirectory(tempPath);
                        using (Process download = new Process())
                        {
                            if (!mvm.debug)
                            {
                                download.StartInfo.UseShellExecute = false;
                                download.StartInfo.CreateNoWindow = true;
                            }

                            download.StartInfo.FileName = Path.Combine(toolsPath, "WiiUDownloader.exe");
                            download.StartInfo.Arguments = $"{b.Tid} {key.Tkey} \"{Path.Combine(tempPath, "download")}\"";

                            download.Start();
                            download.WaitForExit();
                        }
                        mvm.Progress = 75;

                        using (Process decrypt = new Process())
                        {
                            if (!mvm.debug)
                            {
                                decrypt.StartInfo.UseShellExecute = false;
                                decrypt.StartInfo.CreateNoWindow = true;
                            }

                            decrypt.StartInfo.FileName = Path.Combine(toolsPath, "Cdecrypt.exe");
                            decrypt.StartInfo.Arguments = $"{Properties.Settings.Default.Ckey} \"{Path.Combine(tempPath, "download")}\" \"{Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]")}\"";

                            decrypt.Start();
                            decrypt.WaitForExit();
                        }
                        mvm.Progress += 10;
                        foreach (string sFile in Directory.GetFiles(Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]", "content"), "*.nfs"))
                        {
                            File.Delete(sFile);
                        }
                        /* File.Delete(Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]", "code", "fw.img"));

                        File.Delete(Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]", "code", "fw.tmd"));

                        if (Directory.Exists(Path.Combine(toolsPath, "IKVM"))) { Directory.Delete(Path.Combine(toolsPath, "IKVM"), true); }
                        if (!mvm.debug)
                        {
                            zip.StartInfo.UseShellExecute = false;
                            zip.StartInfo.CreateNoWindow = true;
                        }

                        zip.StartInfo.FileName = Path.Combine(toolsPath, "7za.exe");
                        zip.StartInfo.Arguments = $"x \"{Path.Combine(toolsPath, "IKVM.zip")}\" -o\"{Path.Combine(toolsPath, "IKVM")}\"";
                        zip.Start();
                        zip.WaitForExit();
                        mvm.Progress += 10;
                        string[] JNUSToolConfig = { "http://ccs.cdn.wup.shop.nintendo.net/ccs/download", Properties.Settings.Default.Ckey };
                        string savedir = Directory.GetCurrentDirectory();
                        File.WriteAllLines(Path.Combine(toolsPath, "IKVM", "config"), JNUSToolConfig);
                        Directory.SetCurrentDirectory(Path.Combine(toolsPath, "IKVM"));
                        zip.StartInfo.FileName = "JNUSTool.exe";
                        zip.StartInfo.Arguments = $"{b.Tid} {key.Tkey} -file /code/fw.img";
                        zip.Start();
                        zip.WaitForExit();

                        zip.StartInfo.Arguments = $"{b.Tid} {key.Tkey} -file /code/fw.tmd";
                        zip.Start();
                        zip.WaitForExit();

                        Directory.SetCurrentDirectory(savedir);
                        var directories = Directory.GetDirectories(Path.Combine(toolsPath, "IKVM"));
                        string name = "";
                        foreach (var s in directories)
                        {
                            if (s.Contains(b.Name))
                            {
                                var split = s.Split('\\');
                                name = split[split.Length - 1];

                            }

                        }
                        File.Copy(Path.Combine(toolsPath, "IKVM", name, "code", "fw.img"), Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]", "code", "fw.img"));

                        File.Copy(Path.Combine(toolsPath, "IKVM", name, "code", "fw.tmd"), Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]", "code", "fw.tmd"));

                        Directory.Delete(Path.Combine(toolsPath, "IKVM"), true);*/
                        mvm.Progress += 15;
                    }
                }
                else
                {



                    if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                    Directory.CreateDirectory(tempPath);
                    using (Process download = new Process())
                    {
                        if (!mvm.debug)
                        {
                            download.StartInfo.UseShellExecute = false;
                            download.StartInfo.CreateNoWindow = true;
                        }

                        download.StartInfo.FileName = Path.Combine(toolsPath, "WiiUDownloader.exe");
                        download.StartInfo.Arguments = $"{b.Tid} {key.Tkey} \"{Path.Combine(tempPath, "download")}\"";

                        download.Start();
                        download.WaitForExit();
                    }
                    mvm.Progress = 75;
                    using (Process decrypt = new Process())
                    {
                        if (!mvm.debug)
                        {
                            decrypt.StartInfo.UseShellExecute = false;
                            decrypt.StartInfo.CreateNoWindow = true;
                        }
                        decrypt.StartInfo.FileName = Path.Combine(toolsPath, "Cdecrypt.exe");
                        decrypt.StartInfo.Arguments = $"{Properties.Settings.Default.Ckey} \"{Path.Combine(tempPath, "download")}\" \"{Path.Combine(Properties.Settings.Default.BasePath, $"{b.Name.Replace(":", "")} [{b.Region.ToString()}]")}\"";

                        decrypt.Start();
                        decrypt.WaitForExit();
                    }
                    mvm.Progress = 100;
                }
            
           
            //GetCurrentSelectedBase
            
        }
        public static string ExtractBase(string path, GameConsoles console)
        {
            if(!Directory.Exists(Path.Combine(Properties.Settings.Default.BasePath, "CustomBases")))
            {
                Directory.CreateDirectory(Path.Combine(Properties.Settings.Default.BasePath, "CustomBases"));
            }
            string outputPath = Path.Combine(Properties.Settings.Default.BasePath, "CustomBases", $"[{console.ToString()}] Custom");
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                outputPath = Path.Combine(Properties.Settings.Default.BasePath, $"[{console.ToString()}] Custom_{i}");
                i++;
            }
            using (Process decrypt = new Process())
            {
                
                decrypt.StartInfo.UseShellExecute = false;
                decrypt.StartInfo.CreateNoWindow = true;
                decrypt.StartInfo.FileName = Path.Combine(toolsPath, "Cdecrypt.exe");
                decrypt.StartInfo.Arguments = $"{Properties.Settings.Default.Ckey} \"{path}\" \"{outputPath}";

                decrypt.Start();
                decrypt.WaitForExit();
            }
            return outputPath;
        }
        // This function changes TitleID, ProductCode and GameName in app.xml (ID) and meta.xml (ID, ProductCode, Name)
        private static void EditXML(string gameNameOr, int index, string code)
        {
            string gameName = string.Empty;
            if(gameNameOr != null || !String.IsNullOrWhiteSpace(gameNameOr))
            {

                gameName = gameNameOr;
                if (gameName.Contains('|'))
                {
                    var split = gameName.Split('|');
                    gameName = split[0] + "," + split[1];
                }
            }

            
            
            string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
            string appXml = Path.Combine(baseRomPath, "code", "app.xml");
            Random random = new Random();
            string ID = $"{random.Next(0x3000, 0x10000):X4}{random.Next(0x3000, 0x10000):X4}";

            string ID2 = $"{random.Next(0x3000, 0x10000):X4}";
            mvvm.prodcode = ID2;
            XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(metaXml);
                    if (gameName != null && gameName != string.Empty)
                    {
                        doc.SelectSingleNode("menu/longname_ja").InnerText = gameName.Replace(",", "\n" );
                        doc.SelectSingleNode("menu/longname_en").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_fr").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_de").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_it").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_es").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_zhs").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_ko").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_nl").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_pt").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_ru").InnerText = gameName.Replace(",", "\n");
                        doc.SelectSingleNode("menu/longname_zht").InnerText = gameName.Replace(",", "\n");
                    }

                   /* if(code != null)
                    {
                        doc.SelectSingleNode("menu/product_code").InnerText = $"WUP-N-{code}";
                    }
                    else
                    {*/
                        doc.SelectSingleNode("menu/product_code").InnerText = $"WUP-N-{ID2}";
                    //}
                     if (index > 0)
                    {
                    doc.SelectSingleNode("menu/drc_use").InnerText = "65537";
                    }
                doc.SelectSingleNode("menu/title_id").InnerText = $"00050002{ID}";
                    doc.SelectSingleNode("menu/group_id").InnerText = $"0000{ID2}";
                    if (gameName != null && gameName != string.Empty)
                    {
                        doc.SelectSingleNode("menu/shortname_ja").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_fr").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_de").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_en").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_it").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_es").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_zhs").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_ko").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_nl").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_pt").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_ru").InnerText = gameName.Split(',')[0];
                        doc.SelectSingleNode("menu/shortname_zht").InnerText = gameName.Split(',')[0];
                    }

                    doc.Save(metaXml);
                }
                catch (NullReferenceException)
                {
                   
                }

                try
                {
                    doc.Load(appXml);
                doc.SelectSingleNode("app/title_id").InnerText = $"00050002{ID}";
                //doc.SelectSingleNode("app/title_id").InnerText = $"0005000247414645";
                
                    doc.SelectSingleNode("app/group_id").InnerText = $"0000{ID2}";
                    doc.Save(appXml);
                }
                catch (NullReferenceException)
                {
                  
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
                DirectoryCopy(Path.Combine(Properties.Settings.Default.BasePath, baserom), baseRomPath, true);
            }
        }

        private static void TG16(string injectRomPath)
        {
            //checking if folder
            if (Directory.Exists(injectRomPath))
            {
                DirectoryCopy(injectRomPath, "test", true);
                //TurboGrafCD
                using (Process TurboInject = new Process())
                {
                    mvvm.msg = "Creating TurboCD Pkg...";
                    TurboInject.StartInfo.UseShellExecute = false;
                    TurboInject.StartInfo.CreateNoWindow = true;
                    TurboInject.StartInfo.FileName = Path.Combine(toolsPath, "BuildTurboCDPcePkg.exe");
                    TurboInject.StartInfo.Arguments = $"test";
                    TurboInject.Start();
                    TurboInject.WaitForExit();
                    mvvm.Progress = 70;
                }
                Directory.Delete("test", true);
            }
            else
            {
                //creating pkg file including the TG16 rom
                using (Process TurboInject = new Process())
                {
                    mvvm.msg = "Creating Turbo16 Pkg...";
                    TurboInject.StartInfo.UseShellExecute = false;
                    TurboInject.StartInfo.CreateNoWindow = true;
                    TurboInject.StartInfo.FileName = Path.Combine(toolsPath, "BuildPcePkg.exe");
                    TurboInject.StartInfo.Arguments = $"\"{injectRomPath}\"";
                    TurboInject.Start();
                    TurboInject.WaitForExit();
                    mvvm.Progress = 70;
                }
            }
            mvvm.msg = "Injecting ROM...";
            //replacing tg16 rom
            File.Delete(Path.Combine(baseRomPath, "content", "pceemu", "pce.pkg"));
            File.Copy("pce.pkg", Path.Combine(baseRomPath, "content", "pceemu", "pce.pkg"));
            File.Delete("pce.pkg");
            mvvm.Progress = 80;
        }

        private static void NESSNES(string injectRomPath)
        {
            string rpxFile = Directory.GetFiles(Path.Combine(baseRomPath, "code"), "*.rpx")[0]; //To get the RPX path where the NES/SNES rom needs to be Injected in
            mvvm.msg = "Decompressing RPX...";
            RPXdecomp(rpxFile); //Decompresses the RPX to be able to write the game into it
            mvvm.Progress = 20;
            if (mvvm.pixelperfect)
            {
                using (Process retroinject = new Process())
                {
                    mvvm.msg = "Applying Pixel Perfect Patches...";
                    retroinject.StartInfo.UseShellExecute = false;
                    retroinject.StartInfo.CreateNoWindow = true;
                    retroinject.StartInfo.RedirectStandardOutput = true;
                    retroinject.StartInfo.RedirectStandardError = true;
                    retroinject.StartInfo.FileName = Path.Combine(toolsPath, "ChangeAspectRatio.exe");
                    retroinject.StartInfo.Arguments = $"\"{rpxFile}\"";

                    retroinject.Start();
                    retroinject.WaitForExit();
                    mvvm.Progress = 30;


                }
            }
            using (Process retroinject = new Process())
            {
                mvvm.msg = "Injecting ROM...";
                retroinject.StartInfo.UseShellExecute = false;
                retroinject.StartInfo.CreateNoWindow = true;
                retroinject.StartInfo.RedirectStandardOutput = true;
                retroinject.StartInfo.RedirectStandardError = true;
                retroinject.StartInfo.FileName = Path.Combine(toolsPath, "retroinject.exe");
                retroinject.StartInfo.Arguments = $"\"{rpxFile}\" \"{injectRomPath}\" \"{rpxFile}\"";

                retroinject.Start();
                retroinject.WaitForExit();
                mvvm.Progress = 70;
                var s = retroinject.StandardOutput.ReadToEnd();
                var e = retroinject.StandardError.ReadToEnd();
                if (e.Contains("is too large") || s.Contains("is too large"))
                {
                    mvvm.Progress = 100;
                    throw new Exception("retro");
                }

            }
            mvvm.msg = "Compressing RPX...";
            RPXcomp(rpxFile); //Compresses the RPX
            mvvm.Progress = 80;
        }

        private static void GBA(string injectRomPath)
        {
            bool delete = false;
            if(!new FileInfo(injectRomPath).Extension.Contains("gba"))
            {
                //it's a GBC or GB rom so it needs to be copied into goomba.gba and then padded to 32Mb (16 would work too but just ot be save)
                using (Process goomba = new Process())
                {
                    mvvm.msg = "Injecting GB/GBC ROM into goomba...";
                    goomba.StartInfo.UseShellExecute = false;
                    goomba.StartInfo.CreateNoWindow = true;
                    goomba.StartInfo.FileName = "cmd.exe";
                    goomba.StartInfo.Arguments = $"/c copy /b \"{Path.Combine(toolsPath, "goomba.gba")}\"+\"{injectRomPath}\" \"{Path.Combine(toolsPath, "goombamenu.gba")}\"";

                    goomba.Start();
                    goomba.WaitForExit();
                    mvvm.Progress = 20;
                }
                mvvm.msg = "Padding goomba ROM...";
                //padding
                byte[] rom = new byte[33554432];
                FileStream fs = new FileStream(Path.Combine(toolsPath, "goombamenu.gba"), FileMode.Open);
                fs.Read(rom, 0, (int)fs.Length);
                fs.Close();
                File.WriteAllBytes(Path.Combine(toolsPath, "goombaPadded.gba"), rom);
                Console.ReadLine();
                injectRomPath = Path.Combine(toolsPath, "goombaPadded.gba");
                delete = true;
                mvvm.Progress = 40;
            }
            if (mvvm.PokePatch)
            {
                mvvm.msg = "Applying PokePatch";
                File.Copy(injectRomPath, Path.Combine(tempPath, "rom.gba"));
                injectRomPath = Path.Combine(tempPath, "rom.gba");
                PokePatch(injectRomPath);
                delete = true;
                mvvm.PokePatch = false;
                mvvm.Progress = 40;
            }

            using (Process psb = new Process())
            {
                mvvm.msg = "Injecting ROM...";
                psb.StartInfo.UseShellExecute = false;
                psb.StartInfo.CreateNoWindow = true;
                psb.StartInfo.FileName = Path.Combine(toolsPath, "psb.exe");
                psb.StartInfo.Arguments = $"\"{Path.Combine(baseRomPath, "content", "alldata.psb.m")}\" \"{injectRomPath}\" \"{Path.Combine(baseRomPath, "content", "alldata.psb.m")}\"";

                psb.Start();
                psb.WaitForExit();
                mvvm.Progress = 80;
            }
            if (delete)
            {
                File.Delete(injectRomPath);
                if(File.Exists(Path.Combine(toolsPath, "goombamenu.gba")))File.Delete(Path.Combine(toolsPath, "goombamenu.gba"));
            }
        }
        private static void DownloadSysTitle(MainViewModel mvm)
        {
            if (mvm.SysKeyset() && mvm.SysKey1set())
            {
                using (Process download = new Process())
                {
                    download.StartInfo.FileName = Path.Combine(toolsPath, "WiiUDownloader.exe");
                    download.StartInfo.Arguments = $"0005001010004001 {Properties.Settings.Default.SysKey} \"{Path.Combine(tempPath, "download")}\"";

                    download.Start();
                    download.WaitForExit();
                }
                using (Process decrypt = new Process())
                {
                    decrypt.StartInfo.FileName = Path.Combine(toolsPath, "Cdecrypt.exe");
                    decrypt.StartInfo.Arguments = $"{Properties.Settings.Default.Ckey} \"{Path.Combine(tempPath, "download")}\" \"{Path.Combine(Properties.Settings.Default.BasePath, $"vwiisys")}\"";

                    decrypt.Start();
                    decrypt.WaitForExit();
                }
                using (Process download = new Process())
                {
                    Directory.Delete(Path.Combine(tempPath, "download"), true);
                    download.StartInfo.FileName = Path.Combine(toolsPath, "WiiUDownloader.exe");
                    download.StartInfo.Arguments = $"0005001010004000 {Properties.Settings.Default.SysKey1} \"{Path.Combine(tempPath, "download")}\"";

                    download.Start();
                    download.WaitForExit();
                }
                using (Process decrypt = new Process())
                {
                    decrypt.StartInfo.FileName = Path.Combine(toolsPath, "Cdecrypt.exe");
                    decrypt.StartInfo.Arguments = $"{Properties.Settings.Default.Ckey} \"{Path.Combine(tempPath, "download")}\" \"{Path.Combine(tempPath, "tempd")}\"";

                    decrypt.Start();
                    decrypt.WaitForExit();
                    File.Copy(Path.Combine(tempPath, "tempd", "code", "font.bin"), Path.Combine(Properties.Settings.Default.BasePath, $"vwiisys", "code", "font.bin"));
                    File.Copy(Path.Combine(tempPath, "tempd", "code", "deint.txt"), Path.Combine(Properties.Settings.Default.BasePath, $"vwiisys", "code", "deint.txt"));
                    File.Delete(Path.Combine(Properties.Settings.Default.BasePath, $"vwiisys", "code", "app.xml"));
                }
            }
        }
        private static void NDS(string injectRomPath)
        {
            
            string RomName = string.Empty;
            using (Process getRomName = new Process())
            {
                mvvm.msg = "Getting BaseRom Name...";
                getRomName.StartInfo.UseShellExecute = false;
                getRomName.StartInfo.CreateNoWindow = false;
                getRomName.StartInfo.RedirectStandardOutput = true;
                getRomName.StartInfo.FileName = "cmd.exe";
                Console.WriteLine(Directory.GetCurrentDirectory());
                //getRomName.StartInfo.Arguments = $"/c \"Tools\\7za.exe\" l \"temp\\baserom\\content\\0010\\rom.zip\" | findstr \"WUP\"";
                getRomName.StartInfo.Arguments = "/c bin\\Tools\\7za.exe l bin\\temp\\baserom\\content\\0010\\rom.zip | findstr WUP";
                getRomName.Start();
                getRomName.WaitForExit();
                var s = getRomName.StandardOutput.ReadToEnd();
                var split = s.Split(' ');
                RomName = split[split.Length - 1].Replace("\r\n", "");
                mvvm.Progress = 15;
            }
            using (Process RomEdit = new Process())
            {
                mvvm.msg = "Removing BaseRom...";
                RomEdit.StartInfo.UseShellExecute = false;
                RomEdit.StartInfo.CreateNoWindow = true;
                RomEdit.StartInfo.RedirectStandardOutput = true;
                RomEdit.StartInfo.FileName = Path.Combine(toolsPath, "7za.exe");
                //d Path.Combine(baseRomPath, "content", "0010", "rom.zip")
                RomEdit.StartInfo.Arguments = $"d bin\\temp\\baserom\\content\\0010\\rom.zip";
                RomEdit.Start();
                RomEdit.WaitForExit();
                mvvm.Progress = 40;
                mvvm.msg = "Injecting ROM...";
                File.Copy(injectRomPath, $"{RomName}");
                RomEdit.StartInfo.Arguments = $"u bin\\temp\\baserom\\content\\0010\\rom.zip {RomName}";
                RomEdit.Start();
                RomEdit.WaitForExit();
                mvvm.Progress = 80;
            }
            File.Delete(RomName);

        }

    
        private static void N64(string injectRomPath, N64Conf config)
        {
            string mainRomPath = Directory.GetFiles(Path.Combine(baseRomPath, "content", "rom"))[0];
            string mainIni = Path.Combine(baseRomPath, "content", "config", $"{Path.GetFileName(mainRomPath)}.ini");
            using (Process n64convert = new Process())
            {
                mvvm.msg = "Injecting ROM...";
                n64convert.StartInfo.UseShellExecute = false;
                n64convert.StartInfo.CreateNoWindow = true;
                n64convert.StartInfo.FileName = Path.Combine(toolsPath, "N64Converter.exe");
                n64convert.StartInfo.Arguments = $"\"{injectRomPath}\" \"{mainRomPath}\"";

                n64convert.Start();
                n64convert.WaitForExit();
                mvvm.Progress = 60;

            }
            if (config.WideScreen)
            {
                mvvm.msg = "Removing Dark Filter...";
                string filePath = Path.Combine(baseRomPath, "content", "FrameLayout.arc");
                using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Open)))
                {
                    writer.Seek(0x1B0D, SeekOrigin.Begin);
                    writer.Write(new byte[] { 0xF0 },0,1);
                }
                mvvm.Progress = 65;
            }
            if (config.DarkFilter)
            {
                mvvm.msg = "Removing Dark Filter...";
                string filePath = Path.Combine(baseRomPath, "content", "FrameLayout.arc");
                using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.Open)))
                {
                    writer.Seek(0x1AD8, SeekOrigin.Begin);
                    writer.Write(0L);
                }
                mvvm.Progress = 70;
            }
            mvvm.msg = "Copying INI...";
            if(config.INIBin == null)
            {
                if (config.INIPath == null)
                {
                    File.Delete(mainIni);
                    File.Copy(Path.Combine(toolsPath, "blank.ini"), mainIni);
                }
                else
                {
                    File.Delete(mainIni);
                    File.Copy(config.INIPath, mainIni);
                }
            }
            else
            {
                ReadFileFromBin(config.INIBin, "custom.ini");
                File.Delete(mainIni);
                File.Move("custom.ini", mainIni);
            }
            mvvm.Progress = 80;
            

            
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

        private static void ReadFileFromBin(byte[] bin, string output)
        {
            File.WriteAllBytes(output, bin);
        }
        private static void Images(GameConfig config)
        {
            bool usetemp = false;
            bool readbin = false;
            try
            {


                //is an image embedded? yes => export them and check for issues
                //no => using path
                if (Directory.Exists(imgPath)) // sanity check
                {
                    Directory.Delete(imgPath, true);
                }
                Directory.CreateDirectory(imgPath);
                //ICON
                List<bool> Images = new List<bool>();
                if (config.TGAIco.ImgBin == null)
                {
                    //use path
                    if (config.TGAIco.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGAIco.ImgPath, Path.Combine(imgPath), false, 128,128,32, "iconTex.tga");
                    }
                    else
                    {
                        if(File.Exists(Path.Combine(toolsPath, "iconTex.tga")))
                        {
                            CopyAndConvertImage(Path.Combine(toolsPath, "iconTex.tga"), Path.Combine(imgPath), false, 128, 128, 32, "iconTex.tga");
                           
                            Images.Add(true);
                        }
                        else
                        {
                            Images.Add(false);
                        }
                      
                    }
                }
                else
                {
                    ReadFileFromBin(config.TGAIco.ImgBin, $"iconTex.{config.TGAIco.extension}");
                    CopyAndConvertImage($"iconTex.{config.TGAIco.extension}", Path.Combine(imgPath), true, 128, 128, 32, "iconTex.tga");
                    Images.Add(true);
                }
                if (config.TGATv.ImgBin == null)
                {
                    //use path
                    if (config.TGATv.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGATv.ImgPath, Path.Combine(imgPath), false, 1280, 720, 24, "bootTvTex.tga");
                        config.TGATv.ImgPath = Path.Combine(imgPath, "bootTvTex.tga");
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(toolsPath, "bootTvTex.png")))
                        {
                            CopyAndConvertImage(Path.Combine(toolsPath, "bootTvTex.png"), Path.Combine(imgPath), false, 1280, 720, 24, "bootTvTex.tga");
                            usetemp = true;
                            Images.Add(true);
                            
                        }
                        else
                        {
                            Images.Add(false);
                        }
                    }
                }
                else
                {
                    ReadFileFromBin(config.TGATv.ImgBin, $"bootTvTex.{config.TGATv.extension}");
                    CopyAndConvertImage($"bootTvTex.{config.TGATv.extension}", Path.Combine(imgPath), true, 1280, 720, 24, "bootTvTex.tga");
                    config.TGATv.ImgPath = Path.Combine(imgPath, "bootTvTex.tga");
                    Images.Add(true);
                    readbin = true;
                }

                //Drc
                if (config.TGADrc.ImgBin == null)
                {
                    //use path
                    if (config.TGADrc.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGADrc.ImgPath, Path.Combine(imgPath), false, 854,480,24, "bootDrcTex.tga");
                    }
                    else
                    {
                        if (Images[1])
                        {
                            using(Process conv = new Process())
                            {
                               
                               if (!mvvm.debug)
                                {
                                    conv.StartInfo.UseShellExecute = false;
                                    conv.StartInfo.CreateNoWindow = true;
                                }
                                if (usetemp)
                                {
                                    File.Copy(Path.Combine(toolsPath, "bootTvTex.png"), Path.Combine(tempPath, "bootDrcTex.png"));
                                }
                                else
                                {

                                    conv.StartInfo.FileName = Path.Combine(toolsPath, "tga2png.exe");
                                    if (!readbin)
                                    {
                                        conv.StartInfo.Arguments = $"-i \"{config.TGATv.ImgPath}\" -o \"{Path.Combine(tempPath)}\"";
                                    }
                                    else
                                    {
                                        if (config.TGATv.extension.Contains("tga"))
                                            {
                                            ReadFileFromBin(config.TGATv.ImgBin, $"bootTvTex.{config.TGATv.extension}");
                                            conv.StartInfo.Arguments = $"-i \"bootTvTex.{config.TGATv.extension}\" -o \"{Path.Combine(tempPath)}\"";
                                        }
                                        else
                                        {
                                            ReadFileFromBin(config.TGATv.ImgBin, Path.Combine(tempPath, "bootTvTex.png"));
                                        }
                                       
                                    }
                                    if (!readbin || config.TGATv.extension.Contains("tga"))
                                    {
                                        conv.Start();
                                        conv.WaitForExit();
                                    }
                                    
                                    File.Copy(Path.Combine(tempPath, "bootTvTex.png"), Path.Combine(tempPath, "bootDrcTex.png"));
                                    if(File.Exists(Path.Combine(tempPath, "bootTvTex.png"))) File.Delete(Path.Combine(tempPath, "bootTvTex.png"));
                                    if (File.Exists($"bootTvTex.{config.TGATv.extension}")) File.Delete($"bootTvTex.{config.TGATv.extension}");
                                }
                                
                                
                                CopyAndConvertImage(Path.Combine(tempPath, "bootDrcTex.png"), Path.Combine(imgPath), false, 854, 480, 24, "bootDrcTex.tga");
                                Images.Add(true);
                            }
                        }
                        else
                        {
                            Images.Add(false);
                        }
                        
                    }
                }
                else
                {
                    ReadFileFromBin(config.TGADrc.ImgBin, $"bootDrcTex.{config.TGADrc.extension}");
                    CopyAndConvertImage($"bootDrcTex.{config.TGADrc.extension}", Path.Combine(imgPath), true,854,480,24, "bootDrcTex.tga");
                    Images.Add(true);
                }

                //tv
               
               

                //logo
                if (config.TGALog.ImgBin == null)
                {
                    //use path
                    if (config.TGALog.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGALog.ImgPath, Path.Combine(imgPath), false, 170,42,32, "bootLogoTex.tga");
                    }
                    else
                    {
                        Images.Add(false);
                    }
                }
                else
                {
                    ReadFileFromBin(config.TGALog.ImgBin, $"bootLogoTex.{config.TGALog.extension}");
                    CopyAndConvertImage($"bootLogoTex.{config.TGALog.extension}", Path.Combine(imgPath), true, 170, 42, 32, "bootLogoTex.tga");
                    Images.Add(true);
                }

                //Fixing Images + Injecting them
                if (Images[0] || Images[1] || Images[2] || Images[3])
                {
                    using (Process checkIfIssue = new Process())
                    {
                        checkIfIssue.StartInfo.UseShellExecute = false;
                        checkIfIssue.StartInfo.CreateNoWindow = false;
                        checkIfIssue.StartInfo.RedirectStandardOutput = true;
                        checkIfIssue.StartInfo.RedirectStandardError = true;
                        checkIfIssue.StartInfo.FileName = $"{Path.Combine(toolsPath,"tga_verify.exe")}";
                        Console.WriteLine(Directory.GetCurrentDirectory());
                        checkIfIssue.StartInfo.Arguments = $"\"{imgPath}\"";
                        checkIfIssue.Start();
                        checkIfIssue.WaitForExit();
                        var s = checkIfIssue.StandardOutput.ReadToEnd();
                        if (s.Contains("width") || s.Contains("height") || s.Contains("depth"))
                        {
                            throw new Exception("Size");
                        }
                        var e = checkIfIssue.StandardError.ReadToEnd();
                        if (e.Contains("width") || e.Contains("height") || e.Contains("depth"))
                        {
                            throw new Exception("Size");
                        }
                        if (e.Contains("TRUEVISION") || s.Contains("TRUEVISION"))
                        {
                            checkIfIssue.StartInfo.UseShellExecute = false;
                            checkIfIssue.StartInfo.CreateNoWindow = false;
                            checkIfIssue.StartInfo.RedirectStandardOutput = true;
                            checkIfIssue.StartInfo.RedirectStandardError = true;
                            checkIfIssue.StartInfo.FileName = $"{Path.Combine(toolsPath, "tga_verify.exe")}";
                            Console.WriteLine(Directory.GetCurrentDirectory());
                            checkIfIssue.StartInfo.Arguments = $"--fixup \"{imgPath}\"";
                            checkIfIssue.Start();
                            checkIfIssue.WaitForExit();
                        }
                        Console.ReadLine();
                    }

                    if (Images[1])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "bootTvTex.tga"));
                        File.Move(Path.Combine(imgPath, "bootTvTex.tga"), Path.Combine(baseRomPath, "meta", "bootTvTex.tga"));
                    }
                    if (Images[2])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "bootDrcTex.tga"));
                        File.Move(Path.Combine(imgPath, "bootDrcTex.tga"), Path.Combine(baseRomPath, "meta", "bootDrcTex.tga"));
                    }
                    if (Images[0])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "iconTex.tga"));
                        File.Move(Path.Combine(imgPath, "iconTex.tga"), Path.Combine(baseRomPath, "meta", "iconTex.tga"));
                    }
                    if (Images[3])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "bootLogoTex.tga"));
                        File.Move(Path.Combine(imgPath, "bootLogoTex.tga"), Path.Combine(baseRomPath, "meta", "bootLogoTex.tga"));
                    }
                }
            }
            catch(Exception e)
            {
                if (e.Message.Contains("Size"))
                {
                    throw e;
                }
                throw new Exception("Images");
            }

        }
        
        private static void CopyAndConvertImage(string inputPath, string outputPath, bool delete, int widht, int height, int bit, string newname)
        {
            if (inputPath.EndsWith(".tga"))
            {
                File.Copy(inputPath, Path.Combine(outputPath,newname));
            }
            else
            {
                using (Process png2tga = new Process())
                {
                    png2tga.StartInfo.UseShellExecute = false;
                    png2tga.StartInfo.CreateNoWindow = true;
                    if(new FileInfo(inputPath).Extension.Contains("png"))
                    {
                        png2tga.StartInfo.FileName = Path.Combine(toolsPath, "png2tga.exe");
                    }else if (new FileInfo(inputPath).Extension.Contains("jpg"))
                    {
                        png2tga.StartInfo.FileName = Path.Combine(toolsPath, "jpg2tga.exe");
                    }else if (new FileInfo(inputPath).Extension.Contains("bmp"))
                    {
                        png2tga.StartInfo.FileName = Path.Combine(toolsPath, "bmp2tga.exe");
                    }
                    
                    png2tga.StartInfo.Arguments = $"-i \"{inputPath}\" -o \"{outputPath}\" --width={widht} --height={height} --tga-bpp={bit} --tga-compression=none";

                    png2tga.Start();
                    png2tga.WaitForExit();
                }
                string name = Path.GetFileNameWithoutExtension(inputPath);
                if(File.Exists(Path.Combine(outputPath , name + ".tga")))
                {
                    File.Move(Path.Combine(outputPath, name + ".tga"), Path.Combine(outputPath, newname));
                }
            }
            if (delete)
            {
                File.Delete(inputPath);
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

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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
