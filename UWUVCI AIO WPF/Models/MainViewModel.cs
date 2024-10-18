using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Bases;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations;
using UWUVCI_AIO_WPF.UI.Windows;
using AutoUpdaterDotNET;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;
using NAudio.Wave;
using System.Timers;
using NAudio.Utils;
using System.Security.Cryptography;
using UWUVCI_AIO_WPF.Helpers;
using Microsoft.Win32;

namespace UWUVCI_AIO_WPF
{
    public class MainViewModel : BaseModel
    {
        public bool saveworkaround = false;

        private bool Injected2 = false;
        public bool injected2
        {

            get { return Injected2; }
            set
            {
                Injected2 = value;
                OnPropertyChanged();
            }

        }
        public string prodcode = "";
        //public GameConfig GameConfiguration { get; set; }
        private GameConfig gameConfiguration = new GameConfig();

        public bool addi = false;
        public GameConfig GameConfiguration
        {
            get { return gameConfiguration; }
            set
            {
                gameConfiguration = value;
                OnPropertyChanged();
            }
        }
        private string romPath;
        public bool regionfrii = false;
        public bool regionfriius = false;
        public bool regionfriijp = false;

        public string RomPath
        {
            get { return romPath; }
            set
            {
                romPath = value;
                OnPropertyChanged();
            }
        }

        public bool jppatch = false;
        public bool pixelperfect = false;

        private GameBases gbTemp;

        public GameBases GbTemp
        {
            get { return gbTemp; }
            set { gbTemp = value; }
        }

        private string selectedBaseAsString;

        public string SelectedBaseAsString
        {
            get { return selectedBaseAsString; }
            set { selectedBaseAsString = value; }
        }


        private List<string> lGameBasesString = new List<string>();

        public List<string> LGameBasesString
        {
            get { return lGameBasesString; }
            set
            {
                lGameBasesString = value;
                OnPropertyChanged();
            }
        }
        private bool pathsSet { get; set; } = false;

        public bool PathsSet
        {
            get { return pathsSet; }
            set
            {
                pathsSet = value;
                OnPropertyChanged();
            }
        }

        private string baseStore;

        public string BaseStore
        {
            get { return baseStore; }
            set
            {
                baseStore = value;
                OnPropertyChanged();
            }
        }

        private string injectStore;

        public string InjectStore
        {
            get { return injectStore; }
            set
            {
                injectStore = value;
                OnPropertyChanged();
            }
        }

        private bool injected = false;

        public bool Injected
        {
            get { return injected; }
            set
            {
                injected = value;
                OnPropertyChanged();
            }
        }

        private Page thing;

        public Page Thing
        {
            get { return thing; }
            set { thing = value; }
        }

        public int OldIndex { get; set; }

        public bool RomSet { get; set; }

        private List<GameBases> lBases = new List<GameBases>();

        public List<GameBases> LBases
        {
            get { return lBases; }
            set
            {
                lBases = value;
                OnPropertyChanged();
            }
        }
        private int progress = 0;

        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }
        public BaseContainerFrame bcf = null;
        #region TKLIST
        private List<GameBases> lNDS = new List<GameBases>();

        public List<GameBases> LNDS
        {
            get { return lNDS; }
            set { lNDS = value; OnPropertyChanged(); }
        }

        private List<GameBases> lN64 = new List<GameBases>();

        public List<GameBases> LN64
        {
            get { return lN64; }
            set { lN64 = value; OnPropertyChanged(); }
        }

        private List<GameBases> lNES = new List<GameBases>();

        public List<GameBases> LNES
        {
            get { return lNES; }
            set { lNES = value; OnPropertyChanged(); }
        }
        private List<GameBases> lGBA = new List<GameBases>();

        public List<GameBases> LGBA
        {
            get { return lGBA; }
            set { lGBA = value; OnPropertyChanged(); }
        }

        private List<GameBases> lSNES = new List<GameBases>();

        public List<GameBases> LSNES
        {
            get { return lSNES; }
            set { lSNES = value; OnPropertyChanged(); }
        }

        private List<GameBases> lTG16 = new List<GameBases>();

        public string ReadCkeyFromOtp()
        {
            string ret = "";
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "OTP.bin | otp.bin";
                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    var filepath = dialog.FileName;
                    using (var fs = new FileStream(filepath,
                                 FileMode.Open,
                                 FileAccess.Read))
                    {
                        byte[] test = new byte[16];
                        fs.Seek(0xE0, SeekOrigin.Begin);

                        fs.Read(test, 0, 16);
                        fs.Close();
                        foreach (var b in test)
                        {
                            ret += string.Format("{0:X2}", b);
                        }
                    }
                }
            }
            return ret;
        }

        public List<GameBases> LTG16
        {
            get { return lTG16; }
            set { lTG16 = value; OnPropertyChanged(); }
        }

        private List<GameBases> lMSX = new List<GameBases>();

        public List<GameBases> LMSX
        {
            get { return lMSX; }
            set { lMSX = value; OnPropertyChanged(); }
        }

        public void RemoveCreatedIMG()
        {
            if (Directory.Exists(@"bin\createdIMG"))
                Directory.Delete(@"bin\createdIMG", true);
        }

        private List<GameBases> lWii = new List<GameBases>();

        public void IsIsoNkit()
        {
            using var fs = new FileStream(RomPath, FileMode.Open, FileAccess.Read);
            byte[] procode = new byte[4];
            fs.Seek(0x200, SeekOrigin.Begin);
            fs.Read(procode, 0, 4);
            var s = ByteArrayToString(procode);

            fs.Close();
            NKITFLAG = s.ToLower().Contains("nkit");
        }

        public bool CheckTime(DateTime creationTime)
        {
            DateTime curr = DateTime.Now;

            // Calculate the time difference and return true if within 62 minutes, false otherwise
            return (curr - creationTime).TotalMinutes >= 0 && (curr - creationTime).TotalMinutes <= 62;
        }


        public List<GameBases> LWII
        {
            get { return lWii; }
            set { lWii = value; OnPropertyChanged(); }
        }

        private List<GameBases> ltemp = new List<GameBases>();

        public List<GameBases> Ltemp
        {
            get { return ltemp; }
            set { ltemp = value; OnPropertyChanged(); }
        }
        #endregion

        public bool BaseDownloaded { get; set; } = false;

        private bool removeDeflicker = false;

        public bool RemoveDeflicker
        {
            get { return removeDeflicker; }
            set
            {
                removeDeflicker = value;
                OnPropertyChanged();
            }
        }

        private bool rendererScale = false;
        private int brightness = 80;
        private int pixelArtUpscaler = 0;
        private bool dsLayout = false;
        private bool stLayout = false;

        public bool RendererScale
        {
            get { return rendererScale; }
            set
            {
                rendererScale = value;
                OnPropertyChanged();
            }
        }
        public bool DSLayout
        {
            get { return dsLayout; }
            set
            {
                dsLayout = value;
                OnPropertyChanged();
            }
        }
        public bool STLayout
        {
            get { return stLayout; }
            set
            {
                stLayout = value;
                OnPropertyChanged();
            }
        }
        public int Brightness
        {
            get { return brightness; }
            set
            {
                brightness = value;
                OnPropertyChanged();
            }
        }

        public int PixelArtUpscaler
        {
            get { return pixelArtUpscaler; }
            set
            {
                pixelArtUpscaler = value;
                OnPropertyChanged();
            }
        }
        private bool removeDithering = false;
        public bool RemoveDithering
        {
            get { return removeDithering; }
            set
            {
                removeDithering = value;
                OnPropertyChanged();
            }
        }

        private bool halfVFilter = false;
        public bool HalfVFilter
        {
            get { return halfVFilter; }
            set
            {
                halfVFilter = value;
                OnPropertyChanged();
            }
        }

        private bool canInject = false;

        public bool CanInject
        {
            get { return canInject; }
            set
            {
                canInject = value;
                OnPropertyChanged();
            }
        }

        private string cBasePath;

        public string CBasePath
        {
            get { return cBasePath; }
            set
            {
                cBasePath = value;
                OnPropertyChanged();
            }
        }

        public int Index = -1;
        public bool LR = false;
        public bool GC = false;
        public bool debug = false;
        public string doing = "";
        public bool Patch = false;
        public bool toPal = false;
        public string GctPath = "";
        private string Msg;

        private string Gc2rom = "";

        public string gctPath
        {
            get { return GctPath; }
            set
            {
                GctPath = value;
                OnPropertyChanged();
            }
        }

        public string gc2rom
        {
            get { return Gc2rom; }
            set
            {
                Gc2rom = value;
                OnPropertyChanged();
            }
        }

        public string foldername = "";

        public string msg
        {
            get { return Msg; }
            set
            {
                Msg = value;
                OnPropertyChanged();
            }
        }
        private string bootsound;

        public string BootSound
        {
            get { return bootsound; }
            set
            {
                bootsound = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Controls.ListViewItem curr = null;

        private bool ckeys;

        public bool Ckeys
        {
            get { return ckeys; }
            set
            {
                ckeys = value;
                OnPropertyChanged();
            }
        }

        public bool NKITFLAG { get; set; } = false;

        public MainWindow mw;
        private CustomBaseFrame cb = null;
        DispatcherTimer timer = new DispatcherTimer();
        public bool PokePatch = false;
        public void Update(bool button)
        {
            if (CheckForInternetConnection())
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;

                if (button)
                {
                    var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("UWUVCI-AIO-WPF"));
                    var releases = Task.Run(() => client.Repository.Release.GetAll("ZestyTS", "UWUVCI-AIO-WPF")).GetAwaiter().GetResult();
                    int comparison;
                    try
                    {
                        var latestString = Regex.Replace(releases[0].TagName, "[^0-9.]", "");
                        var latestLength = latestString.Split('.').Length;
                        var localLength = version.Split('.').Length;

                        for (var i = 0; i < localLength - latestLength; i++)
                            latestString += ".0";

                        var latestVersion = new Version(latestString);
                        var localVersion = new Version(version);
                        comparison = localVersion.CompareTo(latestVersion);
                    }
                    catch
                    {
                        //Someone messed up versioning, so eff it just don't even bother then
                        return;
                    }
                    //You idiot, when tf did you flip this back?
                    if (comparison < 0)
                    {
                        var cm = new Custom_Message("Update Available!", "You can get it from: https://github.com/ZestyTS/UWUVCI-AIO-WPF/releases/latest");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }
                    else
                    {
                        var cm = new Custom_Message("No Update Available", "This is currently the latest version.");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }
                }
            }
        }

        private int GetNewVersion()
        {
            try
            {
                WebRequest request;
                //get download link from uwuvciapi

                request = WebRequest.Create("https://uwuvciapi.azurewebsites.net/GetVersionNum");

                var response = request.GetResponse();
                using Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                // Display the content.  
                return Convert.ToInt32(responseFromServer);

            }
            catch (Exception)
            {
                return 100000;
            }
        }

        public bool ConfirmRiffWave(string path)
        {
            using var reader = new BinaryReader(File.OpenRead(path));
            reader.BaseStream.Position = 0x00;
            long WAVHeader1 = reader.ReadInt32();
            reader.BaseStream.Position = 0x08;
            long WAVHeader2 = reader.ReadInt32();

            return WAVHeader1 == 1179011410 & WAVHeader2 == 1163280727;
        }

        public void OpenDialog(string title, string msg)
        {
            Custom_Message cm = new Custom_Message(title, msg);
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();
        }
        public MainViewModel()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                List<string> Tools = ToolCheck.ToolNames.ToList();
                Tools.Remove("CNUSPACKER.exe");
                Tools.Add("NUSPacker.jar");
                ToolCheck.ToolNames = Tools.ToArray();
            }

            //if (Directory.Exists(@"Tools")) Directory.Delete(@"Tools", true);
            if (Directory.Exists(@"bases")) Directory.Delete(@"bases", true);
            if (Directory.Exists(@"temp")) Directory.Delete(@"temp", true);

            if (Directory.Exists(@"keys"))
            {
                if (Directory.Exists(@"bin\keys")) Directory.Delete(@"bin\keys", true);
                Injection.DirectoryCopy("keys", "bin/keys", true);
                Directory.Delete("keys", true);
            }
            if (!Directory.Exists("InjectedGames")) 
                Directory.CreateDirectory("InjectedGames");

            if (!Directory.Exists("SourceFiles")) 
                Directory.CreateDirectory("SourceFiles");

            if (!Directory.Exists("bin\\BaseGames")) 
                Directory.CreateDirectory("bin\\BaseGames");

            if (Settings.Default.OutPath == "" || Settings.Default.OutPath == null)
                Settings.Default.OutPath = Path.Combine(Directory.GetCurrentDirectory(), "InjectedGames");

            if (Settings.Default.BasePath == "" || Settings.Default.BasePath == null)
                Settings.Default.BasePath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "BaseGames");

            Settings.Default.Save();
            ArePathsSet();

            Update(false);

            toolCheck();
            BaseCheck();

            GameConfiguration = new GameConfig();
            if (!ValidatePathsStillExist() && Settings.Default.SetBaseOnce && Settings.Default.SetOutOnce)
            {
                Custom_Message cm = new Custom_Message("Issue", " One of your added Paths seems to not exist anymore. \n The Tool is now using it's default Paths \n Please check the paths in the Path menu! ");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception)
                {

                }
                cm.ShowDialog();
            }
            UpdatePathSet();

            GetAllBases();
        }
        public string turbocd()
        {
            string ret = string.Empty;
            Custom_Message cm = new Custom_Message("Information", " Please put a TurboGrafX CD ROM into a folder and select said folder. \n\n The Folder should at least contain: \n EXACTLY ONE *.hcd file \n One or more *.ogg files \n One or More *.bin files \n\n Not doing so will result in a faulty Inject. You have been warned! ");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();

            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        if (DirectoryIsEmpty(dialog.FileName))
                        {
                            cm = new Custom_Message("Issue", " The folder is Empty. Please choose another folder ");
                            try
                            {
                                cm.Owner = mw;
                            }
                            catch (Exception) { }
                            cm.ShowDialog();
                        }
                        else
                        {
                            if (Directory.GetDirectories(dialog.FileName).Length > 0)
                            {
                                cm = new Custom_Message("Issue", " This folder mustn't contain any subfolders. ");
                                try
                                {
                                    cm.Owner = mw;
                                }
                                catch (Exception) { }
                                cm.ShowDialog();

                            }
                            else
                            {
                                //WUP
                                if (Directory.GetFiles(dialog.FileName, "*.hcd").Length == 1 && Directory.GetFiles(dialog.FileName, "*.ogg").Length > 0 && Directory.GetFiles(dialog.FileName, "*.bin").Length > 0)
                                    ret = dialog.FileName;
                                else
                                {
                                    cm = new Custom_Message("Issue", " This Folder does not contain needed minimum of Files ");
                                    try
                                    {
                                        cm.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    cm.ShowDialog();

                                }

                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

            }

            return ret;
        }
        public GameConfig saveconf = null;
        public void resetCBASE()
        {
            cb?.Reset();
        }
        public void removeCBASE()
        {
            cb = null;
        }
        public void setThing(Page T)
        {
            Thing = T;
        }
        public void SetCBASE(CustomBaseFrame cbs)
        {
            cb = cbs;
        }
        public void setMW(MainWindow mwi)
        {
            mw = mwi;
        }
        public bool cd = false;
        public void ExportFile()
        {
            string drcp = null;
            string tvcp = null;
            string iccp = null;
            string lgcp = null;
            string incp = null;
            if (GameConfiguration.TGADrc.ImgPath != null || GameConfiguration.TGADrc.ImgPath == "") drcp = string.Copy(GameConfiguration.TGADrc.ImgPath);
            if (GameConfiguration.TGATv.ImgPath != null || GameConfiguration.TGATv.ImgPath == "") tvcp = string.Copy(GameConfiguration.TGATv.ImgPath);
            if (GameConfiguration.TGAIco.ImgPath != null || GameConfiguration.TGAIco.ImgPath == "") iccp = string.Copy(GameConfiguration.TGAIco.ImgPath);
            if (GameConfiguration.TGALog.ImgPath != null || GameConfiguration.TGALog.ImgPath == "") lgcp = string.Copy(GameConfiguration.TGALog.ImgPath);
            GameConfiguration.pixelperfect = pixelperfect;
            GameConfiguration.lr = LR;
            GameConfiguration.pokepatch = PokePatch;
            GameConfiguration.tgcd = cd;
            GameConfiguration.donttrim = donttrim;
            GameConfiguration.motepass = passtrough;
            GameConfiguration.jppatch = jppatch;
            GameConfiguration.vm = Patch;
            GameConfiguration.vmtopal = toPal;
            GameConfiguration.rf = regionfrii;
            GameConfiguration.rfjp = regionfriijp;
            GameConfiguration.rfus = regionfriius;
            if (Index != -1)
            {
                GameConfiguration.disgamepad = false;
            }
            else
            {
                GameConfiguration.disgamepad = true;
            }
            GameConfiguration.fourbythree = cd;
            if (GameConfiguration.N64Stuff.INIPath != null || GameConfiguration.N64Stuff.INIPath == "") incp = string.Copy(GameConfiguration.N64Stuff.INIPath);
            ReadBootSoundIntoConfig();
            ReadImagesIntoConfig();
            if (GameConfiguration.Console == GameConsoles.N64)
            {
                ReadIniIntoConfig();
            }
            GameConfig backup = GameConfiguration;
            if (test == GameConsoles.GCN) backup.Console = GameConsoles.GCN;
            if (GameConfiguration.TGADrc.ImgBin != null && GameConfiguration.TGADrc.ImgBin.Length > 0) backup.TGADrc.ImgPath = "Added via Config";
            if (GameConfiguration.TGATv.ImgBin != null && GameConfiguration.TGATv.ImgBin.Length > 0) backup.TGATv.ImgPath = "Added via Config";
            if (GameConfiguration.TGALog.ImgBin != null && GameConfiguration.TGALog.ImgBin.Length > 0) backup.TGALog.ImgPath = "Added via Config";
            if (GameConfiguration.TGAIco.ImgBin != null && GameConfiguration.TGAIco.ImgBin.Length > 0) backup.TGAIco.ImgPath = "Added via Config";
            if (GameConfiguration.N64Stuff.INIBin != null && GameConfiguration.N64Stuff.INIBin.Length > 0) backup.N64Stuff.INIPath = "Added via Config";
            if (GameConfiguration.GameName == "" || GameConfiguration.GameName == null) backup.GameName = "NoName";
            GameConfiguration.Index = Index;
            CheckAndFixConfigFolder();
            var sanitizedGameName = backup.GameName;
            Array.ForEach(Path.GetInvalidFileNameChars(),
                  c => sanitizedGameName = sanitizedGameName.Replace(c.ToString(), string.Empty));
            string outputPath = $@"configs\[{backup.Console}]{sanitizedGameName}.uwuvci";
            int i = 1;
            while (File.Exists(outputPath))
            {
                outputPath = $@"configs\[{backup.Console}]{sanitizedGameName}_{i}.uwuvci";
                i++;
            }

            Stream createConfigStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            GZipStream compressedStream = new GZipStream(createConfigStream, CompressionMode.Compress);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(compressedStream, backup);
            compressedStream.Close();
            createConfigStream.Close();
            Custom_Message cm = new Custom_Message("Export success", " The Config was successfully exported.\n Click the Open Folder Button to open the Location where the Config is stored. ", Path.Combine(Directory.GetCurrentDirectory(), outputPath));
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();

            GameConfiguration.TGADrc.ImgPath = drcp;
            GameConfiguration.TGATv.ImgPath = tvcp;
            GameConfiguration.TGAIco.ImgPath = iccp;
            GameConfiguration.TGALog.ImgPath = lgcp;
            GameConfiguration.TGADrc.ImgBin = null;
            GameConfiguration.TGATv.ImgBin = null;
            GameConfiguration.TGAIco.ImgBin = null;
            GameConfiguration.TGALog.ImgBin = null;
            if (incp != null)
            {
                GameConfiguration.N64Stuff.INIBin = null;
                GameConfiguration.N64Stuff.INIPath = incp;
            }
            /*if (GameConfiguration.Console == GameConsoles.N64)
            {
                (thing as N64Config).reset();
            }
            else if (gameConfiguration.Console == GameConsoles.TG16)
            {
                (thing as TurboGrafX).reset();
            }
            else if (gameConfiguration.Console == GameConsoles.WII && test != GameConsoles.GCN)
            {
                (thing as WiiConfig).reset();
            }
            else if (test == GameConsoles.GCN)
            {
                (thing as GCConfig).reset();
            }
            else
            {
                try
                {
                    (thing as OtherConfigs).reset();
                }
                catch (Exception e)
                {
                    (thing as GCConfig).reset();
                }
            }*/
        }
        public void ImportConfig(string configPath)
        {
            FileInfo fn = new FileInfo(configPath);
            if (Directory.Exists(@"bin\cfgBoot"))
            {
                Directory.Delete(@"bin\cfgBoot", true);
            }
            if (fn.Extension.Contains("uwuvci"))
            {
                FileStream inputConfigStream = new FileStream(configPath, FileMode.Open, FileAccess.Read);
                GZipStream decompressedConfigStream = new GZipStream(inputConfigStream, CompressionMode.Decompress);
                IFormatter formatter = new BinaryFormatter();
                GameConfiguration = (GameConfig)formatter.Deserialize(decompressedConfigStream);
                pixelperfect = GameConfiguration.pixelperfect;
                LR = GameConfiguration.lr;
                cd = GameConfiguration.tgcd;
                PokePatch = GameConfiguration.pokepatch;
                passtrough = GameConfiguration.motepass;
                jppatch = GameConfiguration.jppatch;
                Patch = GameConfiguration.vm;
                toPal = GameConfiguration.vmtopal;
                regionfrii = GameConfiguration.rf;
                regionfriijp = GameConfiguration.rfjp;
                regionfriius = GameConfiguration.rfus;
            }
            if (GameConfiguration.Console == GameConsoles.N64)
            {
                (thing as N64Config).getInfoFromConfig();
            }
            else if (gameConfiguration.Console == GameConsoles.TG16)
            {
                (thing as TurboGrafX).getInfoFromConfig();
            }
            else if (gameConfiguration.Console == GameConsoles.WII && test != GameConsoles.GCN)
            {
                (thing as WiiConfig).getInfoFromConfig();
            }
            else if (test == GameConsoles.GCN)
            {
                (thing as GCConfig).getInfoFromConfig();
            }
            else if (gameConfiguration.Console == GameConsoles.GBA)
            {
                (thing as GBA).getInfoFromConfig();
            }
            else
            {
                try
                {
                    (thing as OtherConfigs).getInfoFromConfig();
                }
                catch (Exception)
                {
                    (thing as GCConfig).getInfoFromConfig();
                }
            }
        }
        public void ReadBootSoundIntoConfig()
        {
            ReadFileAsBin(GameConfiguration, bootsound, 6);
        }

        public void ReadImagesIntoConfig()
        {
            ReadFileAsBin(GameConfiguration, GameConfiguration.TGAIco.ImgPath, 1);
            ReadFileAsBin(GameConfiguration, GameConfiguration.TGADrc.ImgPath, 2);
            ReadFileAsBin(GameConfiguration, GameConfiguration.TGATv.ImgPath, 3);
            ReadFileAsBin(GameConfiguration, GameConfiguration.TGALog.ImgPath, 4);
        }
        public void ReadIniIntoConfig()
        {
            ReadFileAsBin(GameConfiguration, GameConfiguration.N64Stuff.INIPath, 5);
        }

        private void ReadFileAsBin(GameConfig file, string FilePath, int scase)
        {
            if (FilePath != null)
            {
                try
                {
                    var filedata = new FileStream(FilePath, FileMode.Open);
                    var len = (int)filedata.Length;
                    switch (scase)
                    {
                        case 1:
                            file.TGAIco.ImgBin = new byte[len];
                            filedata.Read(file.TGAIco.ImgBin, 0, len);
                            break;
                        case 2:
                            file.TGADrc.ImgBin = new byte[len];
                            filedata.Read(file.TGADrc.ImgBin, 0, len);
                            break;
                        case 3:
                            file.TGATv.ImgBin = new byte[len];
                            filedata.Read(file.TGATv.ImgBin, 0, len);
                            break;
                        case 4:
                            file.TGALog.ImgBin = new byte[len];
                            filedata.Read(file.TGALog.ImgBin, 0, len);
                            break;
                        case 5:
                            file.N64Stuff.INIBin = new byte[len];
                            filedata.Read(file.N64Stuff.INIBin, 0, len);
                            break;
                        case 6:

                            file.bootsound = new byte[len];
                            filedata.Read(file.bootsound, 0, len);
                            file.extension = new FileInfo(FilePath).Extension.Replace(".", "");
                            break;
                    }

                    filedata.Close();
                }
                catch (Exception)
                {
                    switch (scase)
                    {
                        case 1:
                            file.TGAIco.ImgBin = null;
                            break;
                        case 2:
                            file.TGADrc.ImgBin = null;

                            break;
                        case 3:
                            file.TGATv.ImgBin = null;

                            break;
                        case 4:
                            file.TGALog.ImgBin = null;
                            break;
                        case 5:
                            file.N64Stuff.INIBin = null;
                            break;
                    }
                }
            }


        }
        public bool donttrim = false;
        private static void CheckAndFixConfigFolder()
        {
            if (!Directory.Exists(@"configs"))
                Directory.CreateDirectory(@"configs");
        }
        public void Pack(bool loadiine)
        {
            string consoleName =  GameConfiguration.Console.ToString();

            if (GC)
                consoleName = GameConsoles.GCN.ToString();

            ValidatePathsStillExist();
            if (loadiine)
                Injection.Loadiine(GameConfiguration.GameName, consoleName);

            else
            {
                if (gameConfiguration.GameName != null)
                {
                    Regex reg = new Regex("[^a-zA-Z0-9 é -]");
                    gameConfiguration.GameName = gameConfiguration.GameName.Replace("|", " ");
                    gameConfiguration.GameName = reg.Replace(gameConfiguration.GameName, "");
                }

                Task.Run(() => { Injection.Packing(GameConfiguration.GameName, consoleName, this); });

                DownloadWait dw = new DownloadWait("Packing Inject - Please Wait", "", this);
                try
                {
                    dw.changeOwner(mw);
                }
                catch (Exception) { }
                dw.ShowDialog();

                Progress = 0;
                string extra = "";
                string names = "Copy to SD";
                if (GameConfiguration.Console == GameConsoles.WII) extra = "\n Some games cannot reboot into the WiiU Menu. Shut down via the GamePad. \n If Stuck in a BlackScreen, you need to unplug your WiiU.";
                if (GameConfiguration.Console == GameConsoles.WII && romPath.ToLower().Contains(".wad")) extra += "\n Make sure that the chosen WAD is installed in your vWii!";
                if (GC)
                {
                    extra = "\n Make sure to have Nintendon't + config on your SD.\n You can add them by pressing the \"SD Setup\" button or using the \"Start Nintendont Config Tool\" button under Settings. ";
                    names = "SD Setup";
                }
                gc2rom = "";

                Custom_Message cm = new Custom_Message("Injection Complete", $" You need CFW (ex: haxchi, mocha, tiramisu, or aroma) to run and install this inject! \n It's recommended to install onto USB to avoid brick risks.{extra}\n To Open the Location of the Inject press Open Folder.\n If you want the inject to be put on your SD now, press {names}. ", Settings.Default.OutPath); try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
            LGameBasesString.Clear();
            CanInject = false;
            RomSet = false;
            RomPath = null;
            Injected = false;
            GameConfiguration.CBasePath = null;
            GC = false;
            bootsound = "";
            NKITFLAG = false;
            CBasePath = null;
            prodcode = "";
            ClearImage();
            foldername = "";
            mw.ListView_Click(mw.listCONS, null);
        }

        private void ClearImage()
        {
            switch (GameConfiguration.Console)
            {
                case GameConsoles.NDS:
                    (thing as OtherConfigs).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).Injection.IsEnabled = false;

                    break;
                case GameConsoles.GBA:
                    (thing as GBA).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as GBA).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as GBA).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as GBA).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as GBA).Injection.IsEnabled = false;
                    break;
                case GameConsoles.N64:
                    (thing as N64Config).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as N64Config).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as N64Config).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as N64Config).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as N64Config).Injection.IsEnabled = false;
                    break;
                case GameConsoles.NES:
                    (thing as OtherConfigs).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).Injection.IsEnabled = false;
                    break;
                case GameConsoles.SNES:
                    (thing as OtherConfigs).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).Injection.IsEnabled = false;
                    break;
                case GameConsoles.TG16:
                    (thing as TurboGrafX).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as TurboGrafX).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as TurboGrafX).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as TurboGrafX).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as TurboGrafX).Injection.IsEnabled = false;
                    break;
                case GameConsoles.MSX:
                    (thing as OtherConfigs).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).logIMG.Visibility = System.Windows.Visibility.Hidden;
                    (thing as OtherConfigs).Injection.IsEnabled = false;
                    break;
                case GameConsoles.WII:
                    if (test == GameConsoles.GCN)
                    {
                        (thing as GCConfig).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as GCConfig).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as GCConfig).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as GCConfig).logIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as GCConfig).Injection.IsEnabled = false;
                    }
                    else
                    {
                        (thing as WiiConfig).icoIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as WiiConfig).tvIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as WiiConfig).drcIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as WiiConfig).logIMG.Visibility = System.Windows.Visibility.Hidden;
                        (thing as WiiConfig).Injection.IsEnabled = false;
                    }
                    break;
            }
        }

        DownloadWait Injectwait;
        public void runInjectThread(bool force)
        {

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick2;
            timer.Start();
            var thread = new Thread(() =>
            {
                Injectwait = new DownloadWait("Injecting Game - Please Wait", "", this);

                try
                {
                    Injectwait.changeOwner(mw);
                }
                catch (Exception) { }
                Injectwait.Topmost = true;
                Injectwait.ShowDialog();

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();


        }
        public bool failed = false;

        public void Inject(bool force)
        {
            ValidatePathsStillExist();
            /* var task = new Task(() => runInjectThread(true));
              task.Start();*/
            Task.Run(() =>
            {
                if (Injection.Inject(GameConfiguration, RomPath, this, force))
                {
                    Injected = true;
                    injected2 = true;
                    if (GameConfiguration.Console == GameConsoles.WII || GameConfiguration.Console == GameConsoles.GCN)
                        injected2 = false;

                }
                else { Injected = false; injected2 = false; }
            });
            DownloadWait dw = new DownloadWait("Injecting Game - Please Wait", "", this);
            try
            {
                dw.changeOwner(mw);
            }
            catch (Exception) { }
            dw.ShowDialog();
            Progress = 0;
            if (Injected)
            {
                Custom_Message cm = new Custom_Message("Finished Injection Part", " Injection Finished, please choose how you want to export the Inject next. ");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
            else
            {
                if (failed)
                {
                    MessageBox.Show("In here");
                    mw.allowBypass();
                    if (debug)
                    {
                        mw.setDebug(true);
                    }
                    Inject(force);
                }
            }


        }
        private void BaseCheck()
        {
            if (Directory.Exists(@"bin\bases"))
            {
                var test = GetMissingVCBs();
                if (test.Count > 0)
                {
                    if (CheckForInternetConnection())
                    {
                        Progress = 0;
                        Task.Run(() =>
                        {
                            double stuff = 100 / test.Count;
                            foreach (string s in test)
                            {
                                DownloadBase(s, this);
                                Progress += Convert.ToInt32(stuff);
                            }
                            Progress = 100;
                        });
                        DownloadWait dw = new DownloadWait("Downloading needed Data - Please Wait", "", this);
                        try
                        {
                            dw.changeOwner(mw);
                        }
                        catch (Exception) { }
                        dw.ShowDialog();

                        BaseCheck();
                    }
                    else
                    {
                        Custom_Message dw = new Custom_Message("No Internet connection", " You have files missing, which need to be downloaded but you dont have an Internet Connection. \n The Program will now terminate ");
                        try
                        {
                            dw.Owner = mw;
                        }
                        catch (Exception) { }
                        dw.ShowDialog();
                        Environment.Exit(1);
                    }



                }
            }
            else
            {
                if (CheckForInternetConnection())
                {
                    Directory.CreateDirectory(@"bin\bases");
                    var test = GetMissingVCBs();
                    Progress = 0;
                    Task.Run(() =>
                    {
                        double stuff = 100 / test.Count;
                        foreach (string s in test)
                        {
                            DownloadBase(s, this);
                            Progress += Convert.ToInt32(stuff);
                        }
                        Progress = 100;
                    });
                    DownloadWait dw = new DownloadWait("Downloading needed Data - Please Wait", "", this);
                    try
                    {
                        dw.changeOwner(mw);
                    }
                    catch (Exception) { }
                    dw.ShowDialog();
                    Progress = 0;
                    BaseCheck();
                }
                else
                {
                    Custom_Message dw = new Custom_Message("No Internet connection", " You have files missing, which need to be downloaded but you dont have an Internet Connection. \n The Program will now terminate ");
                    try
                    {
                        dw.Owner = mw;
                    }
                    catch (Exception) { }
                    dw.ShowDialog();
                    Environment.Exit(1);
                }
            }
        }
        public void UpdateTools()
        {
            if (CheckForInternetConnection())
            {
                string[] bases = ToolCheck.ToolNames;
                Task.Run(() =>
                {
                    Progress = 0;
                    double l = 100 / bases.Length;
                    foreach (string s in bases)
                    {
                        DeleteTool(s);
                        DownloadTool(s, this);
                        Progress += Convert.ToInt32(l);
                    }
                    Progress = 100;
                });

                DownloadWait dw = new DownloadWait("Updating Tools - Please Wait", "", this);
                try
                {
                    dw.changeOwner(mw);
                }
                catch (Exception)
                {

                }
                dw.ShowDialog();
                toolCheck();
                Custom_Message cm = new Custom_Message("Finished Update", " Finished Updating Tools! Restarting UWUVCI AIO ");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
                Process p = new Process();
                p.StartInfo.FileName = System.Windows.Application.ResourceAssembly.Location;
                if (debug)
                {
                    if (saveworkaround)
                    {
                        p.StartInfo.Arguments = "--debug --skip --spacebypass";
                    }
                    else
                    {
                        p.StartInfo.Arguments = "--debug --skip";
                    }

                }
                else
                {
                    if (saveworkaround)
                    {
                        p.StartInfo.Arguments = "--skip --spacebypass";
                    }
                    else
                    {
                        p.StartInfo.Arguments = "--skip";
                    }
                }
                p.Start();
                Environment.Exit(0);
            }

        }
        public void ResetTKQuest()
        {
            Custom_Message cm = new Custom_Message("Resetting TitleKeys", " This Option will reset all entered TitleKeys meaning you will need to reenter them again! \n Do you still wish to continue?");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();
            cm.Close();

        }
        public void ResetTitleKeys()
        {
            File.Delete("bin/keys/gba.vck");
            File.Delete("bin/keys/nds.vck");
            File.Delete("bin/keys/nes.vck");
            File.Delete("bin/keys/n64.vck");
            File.Delete("bin/keys/msx.vck");
            File.Delete("bin/keys/tg16.vck");
            File.Delete("bin/keys/snes.vck");
            File.Delete("bin/keys/wii.vck");
            Custom_Message cm = new Custom_Message("Reset Successful", " The TitleKeys are now reset. \n The Program will now restart.");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();
            mw.Close();
            Process p = new Process();
            p.StartInfo.FileName = System.Windows.Application.ResourceAssembly.Location;
            if (debug)
            {
                p.StartInfo.Arguments = "--debug --skip";
            }
            else
            {
                p.StartInfo.Arguments = "--skip";
            }
            p.Start();
            Environment.Exit(0);


        }
        public void UpdateBases()
        {
            if (CheckForInternetConnection())
            {
                string[] bases = { "bases.vcbnds", "bases.vcbn64", "bases.vcbgba", "bases.vcbsnes", "bases.vcbnes", "bases.vcbtg16", "bases.vcbmsx", "bases.vcbwii" };
                Task.Run(() => {
                    Progress = 0;
                    double l = 100 / bases.Length;
                    foreach (string s in bases)
                    {
                        DeleteBase(s);
                        DownloadBase(s, this);

                        GameConsoles g = new GameConsoles();
                        if (s.Contains("nds")) g = GameConsoles.NDS;
                        if (s.Contains("nes")) g = GameConsoles.NES;
                        if (s.Contains("snes")) g = GameConsoles.SNES;
                        if (s.Contains("n64")) g = GameConsoles.N64;
                        if (s.Contains("gba")) g = GameConsoles.GBA;
                        if (s.Contains("tg16")) g = GameConsoles.TG16;
                        if (s.Contains("msx")) g = GameConsoles.MSX;
                        if (s.Contains("wii")) g = GameConsoles.WII;
                        UpdateKeyFile(VCBTool.ReadBasesFromVCB($@"bin/bases/{s}"), g);
                        Progress += Convert.ToInt32(l);
                    }
                    Progress = 100;
                });
                DownloadWait dw = new DownloadWait("Updating Base Files - Please Wait", "", this);
                try
                {
                    dw.changeOwner(mw);
                }
                catch (Exception)
                {

                }
                dw.ShowDialog();

                Custom_Message cm = new Custom_Message("Finished Updating", " Finished Updating Bases! Restarting UWUVCI AIO ");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
                Process p = new Process();
                p.StartInfo.FileName = System.Windows.Application.ResourceAssembly.Location;
                if (debug)
                {
                    p.StartInfo.Arguments = "--debug --skip";
                }
                else
                {
                    p.StartInfo.Arguments = "--skip";
                }
                p.Start();
                Environment.Exit(0);
            }


        }
        public static int GetDeterministicHashCode(string str)
        { 
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        public bool checkSysKey(string key)
        {
            if (key.GetHashCode() == -589797700 || GetDeterministicHashCode(key) == -589797700)
            {
                Settings.Default.SysKey = key;
                Settings.Default.Save();
                return true;
            }
            return false;
        }
        public bool SysKey1set()
        {
            return checkSysKey1(Settings.Default.SysKey1);
        }
        public bool checkSysKey1(string key)
        {
            if (key.GetHashCode() == -1230232583 || (GetDeterministicHashCode(key) == -1230232583))
            {
                Settings.Default.SysKey1 = key;
                Settings.Default.Save();
                return true;
            }
            return false;
        }
        public bool SysKeyset()
        {
            return checkSysKey(Settings.Default.SysKey);
        }
        public bool GetConsoleOfConfig(string configPath, GameConsoles console)
        {
            FileInfo fn = new FileInfo(configPath);
            if (fn.Extension.Contains("uwuvci"))
            {
                FileStream inputConfigStream = new FileStream(configPath, FileMode.Open, FileAccess.Read);
                GZipStream decompressedConfigStream = new GZipStream(inputConfigStream, CompressionMode.Decompress);
                IFormatter formatter = new BinaryFormatter();
                GameConfig check = (GameConfig)formatter.Deserialize(decompressedConfigStream);
                if (check.Console == console) return true;

            }
            return false;
        }
        public void selectConfig(GameConsoles console)
        {
            string ret = string.Empty;
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "configs");
                dialog.Filter = "UWUVCI Config (*.uwuvci) | *.uwuvci";
                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    ret = dialog.FileName;
                    if (GetConsoleOfConfig(ret, console))
                    {
                        ImportConfig(ret);
                        Custom_Message cm = new Custom_Message("Import Complete", " Importing of Config completed. \n Please reselect a Base!");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }
                    else
                    {
                        Custom_Message cm = new Custom_Message("Import Failed", $" The config you are trying to import is not made for {console.ToString()} Injections. \n Please choose a config made for these kind of Injections or choose a different kind of Injection");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }
                }
            }

        }
        private bool RemoteFileExists(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        public string GetFilePath(bool ROM, bool INI)
        {
            Custom_Message cm;
            string ret = string.Empty;
            if (ROM && !INI)
            {
                switch (GameConfiguration.Console)
                {
                    case GameConsoles.NDS:
                        cm = new Custom_Message("Information", " You can only inject NDS ROMs that are not DSi Enhanced (example for not working: Pokémon Black & White) \n\n If attempting to inject a DSi Enhanced ROM, we will not give you any support with fixing said injection. ");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        if (!Settings.Default.ndsw)
                        {
                            cm.ShowDialog();
                        }


                        break;
                    case GameConsoles.SNES:
                        cm = new Custom_Message("Information", " You can only inject SNES ROMs that are not using any Co-Processors (example for not working: Star Fox) \n\n If attempting to inject a ROM in need of a Co-Processor, we will not give you any support with fixing said injection. ");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        if (!Settings.Default.snesw)
                        {
                            cm.ShowDialog();
                        }

                        break;
                }
            }

            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                if (ROM)
                {
                    if (INI)
                    {
                        dialog.Filter = "BootSound Files (*.mp3; *.wav; *.btsnd) | *.mp3;*.wav;*.btsnd";
                    }
                    else if (GC)
                    {
                        dialog.Filter = "GCN ROM (*.iso; *.gcm) | *.iso; *.gcm";
                    }
                    else
                    {
                        switch (GameConfiguration.Console)
                        {
                            case GameConsoles.NDS:
                                dialog.Filter = "Nintendo DS ROM (*.nds; *.srl) | *.nds;*.srl";
                                break;
                            case GameConsoles.N64:
                                dialog.Filter = "Nintendo 64 ROM (*.n64; *.v64; *.z64) | *.n64;*.v64;*.z64";
                                break;
                            case GameConsoles.GBA:
                                dialog.Filter = "GameBoy Series ROM (*.gba;*.gbc;*.gb) | *.gba;*.gbc;*.gb";
                                break;
                            case GameConsoles.NES:
                                dialog.Filter = "Nintendo Entertainment System ROM (*.nes) | *.nes";
                                break;
                            case GameConsoles.SNES:
                                dialog.Filter = "Super Nintendo Entertainment System ROM (*.sfc; *.smc) | *.sfc;*.smc";
                                break;
                            case GameConsoles.TG16:
                                dialog.Filter = "TurboGrafX-16 ROM (*.pce) | *.pce";
                                break;
                            case GameConsoles.MSX:
                                dialog.Filter = "MSX/MSX2 ROM (*.ROM) | *.ROM";
                                break;
                            case GameConsoles.WII:
                                if (test == GameConsoles.GCN)
                                    dialog.Filter = "GC ROM (*.iso; *.gcm; *.nkit.iso; *.nkit.gcz) | *.iso; *.gcm; *.nkit.iso; *.nkit.gcz";
                                else
                                    dialog.Filter = "All Supported Types (*.*) | *.iso; *.wbfs; *.nkit.gcz; *.nkit.iso; *.dol; *.wad|Wii ROM (*.iso; *.wbfs; *.nkit.gcz; *.nkit.iso) | *.iso; *.wbfs; *.nkit.gcz; *.nkit.iso|Wii Homebrew (*.dol) | *.dol|Wii Channel (*.wad) | *.wad";
                                    // dialog.Filter = "Wii ROM (*.iso; *.wbfs; *.nkit.gcz; *.nkit.iso) | *.iso; *.wbfs; *.nkit.gcz; *.nkit.iso|Wii Homebrew (*.dol) | *.dol|Wii Channel (*.wad) | *.wad";

                                break;
                            case GameConsoles.GCN:
                                dialog.Filter = "GC ROM (*.iso; *.gcm; *.nkit.iso; *.nkit.gcz) | *.iso; *.gcm; *.nkit.iso; *.nkit.gcz";
                                break;
                        }
                    }

                }
                else if (!INI)
                    dialog.Filter = "Images (*.png; *.jpg; *.bmp; *.tga; *jpeg) | *.png;*.jpg;*.bmp;*.tga;*jpeg";
                else if (INI)
                    dialog.Filter = "N64 VC Configuration (*.ini) | *.ini";

                if (Directory.Exists("SourceFiles"))
                    dialog.InitialDirectory = "SourceFiles";

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (dialog.FileName.ToLower().Contains(".gcz"))
                    {
                        Custom_Message cm1 = new Custom_Message("Information", " Using a GameCube GCZ Nkit for a Wii Inject or vice versa will break things. \n You will not be able to grab the BootImages or GameName using this type of ROM. ");
                        try
                        {
                            cm1.Owner = mw;
                        }
                        catch (Exception)
                        {

                        }
                        if (!Settings.Default.gczw)
                            cm1.ShowDialog();
                    }
                    ret = dialog.FileName;
                }
                else
                    if (dialog.Filter.Contains("BootImages") || dialog.Filter.Contains("BootSound") || dialog.Filter.Contains("GCT"))
                        ret = "";
            }
            return ret;
        }
        public GameConsoles test;
        private static void CopyBase(string console)
        {
            File.Copy(console, $@"bin\bases\{console}");
            File.Delete(console);
        }

        private static void DeleteTool(string tool)
        {
            File.Delete($@"bin\Tools\{tool}");
        }
        private static void DeleteBase(string console)
        {
            File.Delete($@"bin\bases\{console}");
        }
        public static List<string> GetMissingVCBs()
        {
            List<string> ret = new List<string>();
            string path = @"bin\bases\bases.vcb";

            if (!File.Exists(path + "nds"))
                ret.Add(path + "nds");

            if (!File.Exists(path + "nes"))
                ret.Add(path + "nes");

            if (!File.Exists(path + "n64"))
                ret.Add(path + "n64");

            if (!File.Exists(path + "snes"))
                ret.Add(path + "snes");

            if (!File.Exists(path + "gba"))
                ret.Add(path + "gba");

            if (!File.Exists(path + "tg16"))
                ret.Add(path + "tg16");

            if (!File.Exists(path + "msx"))
                ret.Add(path + "msx");

            if (!File.Exists(path + "wii"))
                ret.Add(path + "wii");

            return ret;
        }
        public static void DownloadBase(string name, MainViewModel mvm)
        {
            string olddir = Directory.GetCurrentDirectory();
            try
            {
                string basePath = $@"bin\bases\";
                Directory.SetCurrentDirectory(basePath);
                using var client = new WebClient();
                var fixname = name.Split('\\');

                if (MacLinuxHelper.IsRunningInVirtualMachine() || MacLinuxHelper.IsRunningUnderWineOrSimilar())
                    name = "Net6/" + name;

                client.DownloadFile(getDownloadLink(name, false), fixname[fixname.Length - 1]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Custom_Message cm = new Custom_Message("Error 005: \"Unable to Download VCB Base\"", " There was an Error downloading the VCB Base File. \n The Programm will now terminate.");
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
                Environment.Exit(1);
            }
            Directory.SetCurrentDirectory(olddir);
        }
        public static void DownloadTool(string name, MainViewModel mvm)
        {
            string olddir = Directory.GetCurrentDirectory();
            try
            {
                if (Directory.GetCurrentDirectory().Contains("bin") && Directory.GetCurrentDirectory().Contains("Tools"))
                    olddir = Directory.GetCurrentDirectory().Replace("bin\\Tools", "");
                else
                {
                    string basePath = $@"bin\Tools\";
                    Directory.SetCurrentDirectory(basePath);
                }
                do
                {
                    if (File.Exists(name))
                        File.Delete(name);

                    using var client = new WebClient();
                    client.DownloadFile(getDownloadLink(name, true), name);
                } while (!ToolCheck.IsToolRight(name));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Custom_Message cm = new Custom_Message("Error 006: \"Unable to Download Tool\"", " There was an Error downloading the Tool. \n The Programm will now terminate.");
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception) { }
                cm.ShowDialog();

                Environment.Exit(1);
            }
            Directory.SetCurrentDirectory(olddir);
        }
        private static string getDownloadLink(string toolname, bool tool)
        {
            try
            {
                bool ok = false;
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string result = client.DownloadString("https://uwuvciapi.azurewebsites.net/api/values");
                    }

                    ok = true;
                }
                catch (WebException)
                {
                }
                if (ok)
                {
                    WebRequest request;
                    //get download link from uwuvciapi
                    request = WebRequest.Create("https://uwuvciapi.azurewebsites.net/GetToolLink?" + (tool ? "tool=" : "vcb=") + toolname);

                    var response = request.GetResponse();
                    using Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    if (responseFromServer == "")
                        if (tool)
                            return $"{ToolCheck.backupulr}{toolname}";
                        else
                            return $@"https://github.com/Hotbrawl20/UWUVCI-VCB/raw/master/" + toolname;

                    return responseFromServer;
                }
                else
                {
                    if (tool)
                        return $"{ToolCheck.backupulr}{toolname}";
                    else
                        return $@"https://github.com/Hotbrawl20/UWUVCI-VCB/raw/master/" + toolname.Replace("bin\\bases\\", "");
                }


            }
            catch (Exception)
            {
                if (tool)
                    return $"{ToolCheck.backupulr}{toolname}";
                else
                    return $@"https://github.com/Hotbrawl20/UWUVCI-VCB/raw/master/" + toolname.Replace("bin\\bases\\", "");

            }
        }
        public void InjcttoolCheck()
        {
            if (ToolCheck.DoesToolsFolderExist())
            {
                List<MissingTool> missingTools = new List<MissingTool>();
                missingTools = ToolCheck.CheckForMissingTools();
                if (missingTools.Count > 0)
                {
                    foreach (MissingTool m in missingTools)
                        DownloadTool(m.Name, this);

                    InjcttoolCheck();
                }
            }
            else
            {
                Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}bin\\Tools");
                InjcttoolCheck();
            }
        }
        private void ThreadDownload(List<MissingTool> missingTools)
        {
            var thread = new Thread(() =>
            {
                double l = 100 / missingTools.Count;

                foreach (MissingTool m in missingTools)
                {
                    if (m.Name == "blank.ini")
                    {
                        StreamWriter sw = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools", "blank.ini"));
                        sw.Close();
                    }
                    else
                        DownloadTool(m.Name, this);

                    Progress += Convert.ToInt32(l);
                }
                Progress = 100;
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        private void timer_Tick2(object sender, EventArgs e)
        {
            if (Progress == 100)
            {
                Injectwait.Close();
                timer.Stop();
                Progress = 0;
            }
        }
        private void toolCheck()
        {
            if (ToolCheck.DoesToolsFolderExist())
            {
                List<MissingTool> missingTools = new List<MissingTool>();
                missingTools = ToolCheck.CheckForMissingTools();

                if (missingTools.Count > 0)
                {
                    if (CheckForInternetConnection())
                    {
                        Task.Run(() => ThreadDownload(missingTools));
                        DownloadWait dw = new DownloadWait("Downloading Tools - Please Wait", "", this);
                        try
                        {
                            dw.changeOwner(mw);
                        }
                        catch (Exception)
                        {

                        }
                        dw.ShowDialog();
                        Thread.Sleep(200);
                        //Download Tools
                        Progress = 0;
                        toolCheck();
                    }
                    else
                    {
                        Custom_Message dw = new Custom_Message("No Internet connection", " You have files missing, which need to be downloaded but you dont have an Internet Connection. \n The Program will now terminate");
                        try
                        {
                            dw.Owner = mw;
                        }
                        catch (Exception) { }
                        dw.ShowDialog();
                        Environment.Exit(1);
                    }
                }
            }
            else
            {
                if (!Directory.GetCurrentDirectory().Contains("bin/tools"))
                    Directory.CreateDirectory("bin/Tools");
                
                toolCheck();
            }
        }

        public void UpdatePathSet()
        {
            PathsSet = Settings.Default.PathsSet;

            if (BaseStore != Settings.Default.BasePath)
                BaseStore = Settings.Default.BasePath;

            if (InjectStore != Settings.Default.BasePath)
                InjectStore = Settings.Default.OutPath;
        }

        public bool ValidatePathsStillExist()
        {
            string basePath = Settings.Default.BasePath;
            string outPath = Settings.Default.OutPath;

            bool baseExists = EnsureDirectoryExists(ref basePath, "bin/BaseGames");
            bool injectExists = EnsureDirectoryExists(ref outPath, "InjectedGames");

            if (baseExists && injectExists)
            {
                Settings.Default.BasePath = basePath;
                Settings.Default.OutPath = outPath;
                Settings.Default.PathsSet = true;
                Settings.Default.Save();
                return true;
            }

            return false;
        }

        private bool EnsureDirectoryExists(ref string path, string defaultSubDir)
        {
            if (Directory.Exists(path))
                return true;

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), defaultSubDir);
            Directory.CreateDirectory(fullPath);
            path = fullPath;
            return false;
        }

        public void GetBases(GameConsoles console)
        {
            string baseFilePath = $@"bin/bases/bases.vcb{console.ToString().ToLower()}";
            var tempBases = VCBTool.ReadBasesFromVCB(baseFilePath);

            LBases.Clear();
            LBases.Add(new GameBases { Name = "Custom", Region = Regions.EU });
            LBases.AddRange(tempBases);

            LGameBasesString.Clear();
            LGameBasesString.AddRange(LBases.Select(b => b.Name == "Custom" ? b.Name : $"{b.Name} {b.Region}"));
        }

        public GameBases getBasefromName(string name)
        {
            string nameWithoutRegion = name.Substring(0, name.Length - 3);
            string region = name.Substring(name.Length - 2);

            return LNDS.Concat(LN64).Concat(LNES).Concat(LSNES).Concat(LGBA).Concat(LTG16).Concat(LMSX).Concat(LWII)
                       .FirstOrDefault(b => b.Name == nameWithoutRegion && b.Region.ToString() == region);
        }


        private void GetAllBases()
        {
            LN64.Clear();
            LNDS.Clear();
            LNES.Clear();
            LSNES.Clear();
            LGBA.Clear();
            LTG16.Clear();
            LMSX.Clear();
            LWII.Clear();

            lNDS = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbnds");
            lNES = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbnes");
            lSNES = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbsnes");
            lN64 = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbn64");
            lGBA = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbgba");
            lTG16 = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbtg16");
            lMSX = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbmsx");
            lWii = VCBTool.ReadBasesFromVCB($@"bin/bases/bases.vcbwii");

            CreateSettingIfNotExist(lNDS, GameConsoles.NDS);
            CreateSettingIfNotExist(lNES, GameConsoles.NES);
            CreateSettingIfNotExist(lSNES, GameConsoles.SNES);
            CreateSettingIfNotExist(lGBA, GameConsoles.GBA);
            CreateSettingIfNotExist(lN64, GameConsoles.N64);
            CreateSettingIfNotExist(lTG16, GameConsoles.TG16);
            CreateSettingIfNotExist(lMSX, GameConsoles.MSX);
            CreateSettingIfNotExist(lWii, GameConsoles.WII);
        }
        private void CreateSettingIfNotExist(List<GameBases> basesList, GameConsoles console)
        {
            string keyFilePath = $@"bin\keys\{console.ToString().ToLower()}.vck";
            if (!File.Exists(keyFilePath))
            {
                var keyList = basesList.Select(baseGame => new TKeys { Base = baseGame }).ToList();
                KeyFile.ExportFile(keyList, console);
            }
            else
                FixupKeys(basesList, console);
        }

        private void FixupKeys(List<GameBases> basesList, GameConsoles console)
        {
            string keyFilePath = $@"bin\keys\{console.ToString().ToLower()}.vck";
            var savedKeys = KeyFile.ReadBasesFromKeyFile(keyFilePath);
            var updatedKeys = savedKeys.Concat(
                basesList.Where(baseGame => !savedKeys.Any(savedKey => savedKey.Base.Name == baseGame.Name && savedKey.Base.Region == baseGame.Region))
                         .Select(baseGame => new TKeys { Base = baseGame })
            ).ToList();

            File.Delete(keyFilePath);
            KeyFile.ExportFile(updatedKeys, console);
        }

        private void UpdateKeyFile(List<GameBases> basesList, GameConsoles console)
        {
            string keyFilePath = $@"bin\keys\{console.ToString().ToLower()}.vck";
            if (File.Exists(keyFilePath))
            {
                var savedKeys = KeyFile.ReadBasesFromKeyFile(keyFilePath);
                var updatedKeys = basesList.Select(baseGame =>
                {
                    var existingKey = savedKeys.FirstOrDefault(savedKey => savedKey.Base.Name == baseGame.Name && savedKey.Base.Region == baseGame.Region);
                    return existingKey ?? new TKeys { Base = baseGame, Tkey = null };
                }).ToList();

                File.Delete(keyFilePath);
                KeyFile.ExportFile(updatedKeys, console);
            }
        }

        public void getTempList(GameConsoles console)
        {
            Ltemp = console switch
            {
                GameConsoles.NDS => LNDS,
                GameConsoles.N64 => LN64,
                GameConsoles.GBA => LGBA,
                GameConsoles.NES => LNES,
                GameConsoles.SNES => LSNES,
                GameConsoles.TG16 => LTG16,
                GameConsoles.MSX => LMSX,
                GameConsoles.WII => LWII,
                _ => throw new ArgumentOutOfRangeException(nameof(console), console, null)
            };
        }


        public void EnterKey(bool ck)
        {
            EnterKey ek = new EnterKey(ck);
            try
            {
                ek.Owner = mw;
            }
            catch (Exception) { }
            ek.ShowDialog();
        }
        public bool checkcKey(string key)
        {
            string lowerKey = key.ToLower();
            int keyHash = lowerKey.GetHashCode();

            if (keyHash == 1274359530 || GetDeterministicHashCode(lowerKey) == -485504051)
            {
                Settings.Default.Ckey = lowerKey;
                ckeys = true;
                Settings.Default.Save();
                return true;
            }
            ckeys = false;
            return false;
        }

        public bool isCkeySet()
        {
            string lowerCKey = Settings.Default.Ckey.ToLower();
            ckeys = lowerCKey.GetHashCode() == 1274359530 || GetDeterministicHashCode(lowerCKey) == -485504051;
            return ckeys;
        }

        public bool checkKey(string key)
        {
            string lowerKey = key.ToLower();
            if (GbTemp.KeyHash == lowerKey.GetHashCode() || GbTemp.KeyHash == GetDeterministicHashCode(lowerKey))
            {
                string consoleName = GetConsoleOfBase(gbTemp).ToString().ToLower();
                string keyFilePath = $@"bin\keys\{consoleName}.vck";
                UpdateKeyInFile(lowerKey, keyFilePath, GbTemp, GetConsoleOfBase(gbTemp));
                return true;
            }
            return false;
        }

        public void UpdateKeyInFile(string key, string file, GameBases baseGame, GameConsoles console)
        {
            if (File.Exists(file))
            {
                var keyEntries = KeyFile.ReadBasesFromKeyFile(file);
                foreach (var entry in keyEntries)
                    if (entry.Base.Name == baseGame.Name && entry.Base.Region == baseGame.Region)
                        entry.Tkey = key;

                File.Delete(file);
                KeyFile.ExportFile(keyEntries, console);
            }
        }

        public bool isKeySet(GameBases baseGame)
        {
            var keyEntries = KeyFile.ReadBasesFromKeyFile($@"bin\keys\{GetConsoleOfBase(baseGame).ToString().ToLower()}.vck");
            return keyEntries.Any(entry => entry.Base.Name == baseGame.Name && entry.Base.Region == baseGame.Region && entry.Tkey != null);
        }

        public void ImageWarning()
        {
            Custom_Message cm = new Custom_Message("Image Warning", " Images need to either be in a Bit Depth of 32bit or 24bit. \n If using Tools like paint.net do not choose the Auto function.");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();

        }
        public bool choosefolder = false;
        public bool CBaseConvertInfo()
        {
            Custom_Message cm = new Custom_Message("NUS Custom Base", " You seem to have added a NUS format Custom Base. \n Do you want it to be converted to be used with the Injector?");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();

            if (choosefolder)
            {
                choosefolder = false;
                return true;
            }
            return false;
        }
        public TKeys getTkey(GameBases baseGame)
        {
            var keyEntries = KeyFile.ReadBasesFromKeyFile($@"bin\keys\{GetConsoleOfBase(baseGame).ToString().ToLower()}.vck");
            return keyEntries.FirstOrDefault(entry => entry.Base.Name == baseGame.Name && entry.Base.Region == baseGame.Region && entry.Tkey != null);
        }

        public void Download()
        {
            ValidatePathsStillExist();
            if (CheckForInternetConnection())
            {
                DownloadWait dw;
                
                if (GameConfiguration.Console == GameConsoles.WII || GameConfiguration.Console == GameConsoles.GCN)
                {
                    double speed = TestDownloadSpeed();  // in MB/s
                    TimeSpan estimatedTime = CalculateEstimatedTime(speed);

                    // Start the actual download
                    Task.Run(() => { Injection.Download(this); });

                    // Display the waiting dialog with the estimated time
                    dw = new DownloadWait("Downloading Base - Please Wait", estimatedTime, this);
                }
                else
                {
                    Task.Run(() => { Injection.Download(this); });
                    dw = new DownloadWait("Downloading Base - Please Wait", "", this);
                }
                try
                {
                    dw.changeOwner(mw);
                }
                catch (Exception) { }
                dw.ShowDialog();
                Progress = 0;
            }
        }

        private double TestDownloadSpeed()
        {
            Stopwatch sw = Stopwatch.StartNew();

            //Using this file as a test file, it's about 16MB which should be small enough to not impact anything.
            string url = "https://github.com/NicoAICP/UWUVCI-Tools/raw/master/gba2.zip";
            byte[] data;
            try
            {
                using var webClient = new WebClient();
                data = webClient.DownloadData(url);
            }
            catch
            {
                return 0;
            }

            sw.Stop();
            double timeTaken = sw.Elapsed.TotalSeconds;
            double sizeOfData = data.Length / (1024.0 * 1024.0); // size in MB

            return sizeOfData / timeTaken; // returns speed in MB/s
        }

        private TimeSpan CalculateEstimatedTime(double speedInMBps)
        {
            const double fileSize = 8.5 * 1024;  // file size in MB

            if (speedInMBps <= 0) 
                return TimeSpan.MaxValue;

            double estimatedTimeInSec = fileSize / speedInMBps;
            return TimeSpan.FromSeconds(estimatedTimeInSec);
        }


        public GameConsoles GetConsoleOfBase(GameBases gb)
        {
            var consoleMappings = new Dictionary<GameConsoles, List<GameBases>>
            {
                { GameConsoles.NDS, lNDS },
                { GameConsoles.N64, lN64 },
                { GameConsoles.NES, lNES },
                { GameConsoles.SNES, lSNES },
                { GameConsoles.GBA, lGBA },
                { GameConsoles.TG16, lTG16 },
                { GameConsoles.MSX, lMSX },
                { GameConsoles.WII, lWii }
            };

            foreach (var mapping in consoleMappings)
                if (mapping.Value.Any(b => b.Name == gb.Name && b.Region == gb.Region))
                    return mapping.Key;

            throw new Exception("Console of base is not one of the listed ones to work with UWUVCI, what you do?");
        }
        public List<bool> getInfoOfBase(GameBases gb)
        {
            string basePath = $@"{Settings.Default.BasePath}\{gb.Name.Replace(":", "")} [{gb.Region}]";
            return new List<bool>
            {
                Directory.Exists(basePath),
                isKeySet(gb),
                isCkeySet()
            };
        }

        public void SetInjectPath()
        {
            SetFolderPath(
                folderPath => Settings.Default.OutPath = folderPath,
                Settings.Default.SetOutOnce,
                "Inject Folder");
        }

        private void SetFolderPath(Action<string> setPathAction, bool setOnceFlag, string folderDescription)
        {
            using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    try
                    {
                        if (DirectoryIsEmpty(dialog.FileName))
                        {
                            setPathAction(dialog.FileName);
                            setOnceFlag = true;
                            Settings.Default.Save();
                            UpdatePathSet();
                        }
                        else
                            PromptUserForFolderSelection(dialog.FileName, setPathAction, setOnceFlag, folderDescription);
                    }
                    catch (Exception e)
                    {
                        HandleError(e);
                    }
            }
            ArePathsSet();
        }

        private void PromptUserForFolderSelection(string folderPath, Action<string> setPathAction, bool setOnceFlag, string folderDescription)
        {
            Custom_Message cm = new Custom_Message("Information", $" Folder contains Files or Subfolders, do you really want to use this folder as the {folderDescription}? ");
            try { cm.Owner = mw; } catch (Exception) { }
            cm.ShowDialog();
            if (choosefolder)
            {
                choosefolder = false;
                setPathAction(folderPath);
                setOnceFlag = true;
                Settings.Default.Save();
                UpdatePathSet();
            }
            else
            {
                SetFolderPath(setPathAction, setOnceFlag, folderDescription);
            }
        }

        private void HandleError(Exception e)
        {
            Console.WriteLine(e.Message);
            Custom_Message cm = new Custom_Message("Error", " An Error occured, please try again! ");
            try { cm.Owner = mw; } catch (Exception) { }
            cm.ShowDialog();
        }

        public void SetBasePath()
        {
            SetFolderPath(
                folderPath => Settings.Default.BasePath = folderPath,
                Settings.Default.SetBaseOnce,
                "Bases Folder");
        }

        public void ArePathsSet()
        {
            if (ValidatePathsStillExist())
            {
                Settings.Default.PathsSet = true;
                Settings.Default.Save();
            }
            UpdatePathSet();
        }

        public bool DirectoryIsEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public void getBootIMGGBA(string rom)
        {
            try
            {
                string repoid = "";
                string SystemType = "gba/";
                using (var fs = new FileStream(rom,
                                     FileMode.Open,
                                     FileAccess.Read))
                {

                    byte[] procode = new byte[4];
                    fs.Seek(0xAC, SeekOrigin.Begin);
                    fs.Read(procode, 0, 4);
                    repoid = ByteArrayToString(procode);
                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                    repoid = rgx.Replace(repoid, "");
                    Console.WriteLine("prodcode before scramble: " + repoid);

                    fs.Close();
                    Console.WriteLine("prodcode after scramble: " + repoid);
                }
                List<string> repoids = new List<string>
                {
                    SystemType + repoid,
                    SystemType + repoid.Substring(0, 3) + "E",
                    SystemType + repoid.Substring(0, 3) + "P",
                    SystemType + repoid.Substring(0, 3) + "J"
                };

                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.GBA);
            }
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
        }
        public void getBootIMGSNES(string rom)
        {
            try
            {
                string SystemType = "snes/";
                var repoid = GetFakeSNESProdcode(rom);
                List<string> repoids = new List<string>
                {
                    SystemType + repoid
                };
                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.SNES);
            } 
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }

        }
        public void getBootIMGMSX(string rom)
        {
            try
            {
                string SystemType = "msx/";
                var repoid = GetFakeMSXTGProdcode(rom, true);
                List<string> repoids = new List<string>
                {
                    SystemType + repoid
                };
                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.MSX);
            }
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
        }
        public void getBootIMGTG(string rom)
        {
            try
            {
                string SystemType = "tg16/";
                var repoid = GetFakeMSXTGProdcode(rom, false);
                List<string> repoids = new List<string>
                {
                    SystemType + repoid
                };
                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.TG16);
            }
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
        }
        private string GetFakeMSXTGProdcode(string v, bool msx)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            Regex rgx2 = new Regex("[^0-9]");
            byte[] procode = new byte[0x210];
            using var md5 = MD5.Create();
            using (var fs = new FileStream(v,
                         FileMode.Open,
                         FileAccess.Read))
            {

                fs.Read(procode, 0, 0x210);

                fs.Close();
            }

            string hash = GetMd5Hash(md5, procode);
            //var number = /*hash.GetHashCode();*/ gamename.GetHashCode();
            if (msx) Console.Write("MSX");
            else Console.Write("TG16");
            Console.WriteLine(" PRODCODE:");
            Console.WriteLine("File Name: " + new FileInfo(v).Name);
            Console.WriteLine("MD5 of Code Snippet: " + hash);
            string hashonlynumbers = rgx2.Replace(hash, "");
            do
            {
                if (hashonlynumbers.Length < 10)
                    hashonlynumbers += 0;
            } while (hashonlynumbers.Length < 10);

            string first10 = new string(new char[] { hashonlynumbers[0], hashonlynumbers[1], hashonlynumbers[2], hashonlynumbers[3], hashonlynumbers[4], hashonlynumbers[5], hashonlynumbers[6], hashonlynumbers[7], hashonlynumbers[8] });
            string prodcode = getCodeOfNumbers(Convert.ToInt32(first10));
            if (msx) prodcode += "SX";
            else prodcode += "TG";
            //Console.WriteLine("NumberHash of GameName: "+ number);
            Console.WriteLine("Fake ProdCode: " + prodcode);
            Console.WriteLine("---------------------------------------------------");
            return prodcode;
        }
        private string GetFakeSNESProdcode(string path)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            Regex rgx2 = new Regex("[^0-9]");
            using var md5 = MD5.Create();
            var name = new byte[] { };
            using (var fs = new FileStream(path,
                         FileMode.Open,
                         FileAccess.Read))
            {
                byte[] procode = new byte[4];
                fs.Seek(0x7FB2, SeekOrigin.Begin);
                fs.Read(procode, 0, 4);

                string repoid = ByteArrayToString(procode);


                repoid = rgx.Replace(repoid, "");
                if (repoid.Length < 4)
                {
                    fs.Seek(0xFFC0, SeekOrigin.Begin);
                    procode = new byte[21];
                    fs.Read(procode, 0, 21);
                    name = procode;

                    repoid = ByteArrayToString(procode);
                    repoid = rgx.Replace(repoid, "");
                }

                if (repoid.Length < 4)
                {
                    fs.Seek(0x7FC0, SeekOrigin.Begin);
                    procode = new byte[21];
                    fs.Read(procode, 0, 21);
                    name = procode;
                }
            }
            string gamenameo = ByteArrayToString(name);
            string gamename = rgx.Replace(gamenameo, "");
            string hash = GetMd5Hash(md5, gamename);
            //var number = /*hash.GetHashCode();*/ gamename.GetHashCode();
            Console.WriteLine("SNES PRODCODE:");
            Console.WriteLine("GameName: " + gamename);
            Console.WriteLine("MD5 of Name: " + hash);
            string hashonlynumbers = rgx2.Replace(hash, "");
            do
            {
                if (hashonlynumbers.Length < 10)
                    hashonlynumbers += 0;
            } while (hashonlynumbers.Length < 10);

            string first10 = new string(new char[] { hashonlynumbers[0], hashonlynumbers[1], hashonlynumbers[2], hashonlynumbers[3], hashonlynumbers[4], hashonlynumbers[5], hashonlynumbers[6], hashonlynumbers[7], hashonlynumbers[8] });

            //Console.WriteLine("NumberHash of GameName: "+ number);
            Console.WriteLine("Fake ProdCode: " + getCodeOfNumbers(Convert.ToInt32(first10)));
            Console.WriteLine("---------------------------------------------------");
            return getCodeOfNumbers(Convert.ToInt32(first10));
            // Console.WriteLine(md5.ComputeHash(name));
            // Console.WriteLine("NumberCode: "+hash.GetHashCode());

        }
        private string GetFakeNESProdcode(string path)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            Regex rgx2 = new Regex("[^0-9]");
            byte[] procode = new byte[0xB0];
            using var md5 = MD5.Create();
            using (var fs = new FileStream(path,
                         FileMode.Open,
                         FileAccess.Read))
            {

                fs.Seek(0x8000, SeekOrigin.Begin);
                fs.Read(procode, 0, 0xB0);

                fs.Close();
            }
            string hash = GetMd5Hash(md5, procode);
            //var number = /*hash.GetHashCode();*/ gamename.GetHashCode();
            Console.WriteLine("NES PRODCODE:");
            Console.WriteLine("File Name: " + new FileInfo(path).Name);
            Console.WriteLine("MD5 of Code Snippet: " + hash);
            string hashonlynumbers = rgx2.Replace(hash, "");
            do
            {
                if (hashonlynumbers.Length < 10)
                    hashonlynumbers += 0;
            } while (hashonlynumbers.Length < 10);

            string first10 = new string(new char[] { hashonlynumbers[0], hashonlynumbers[1], hashonlynumbers[2], hashonlynumbers[3], hashonlynumbers[4], hashonlynumbers[5], hashonlynumbers[6], hashonlynumbers[7], hashonlynumbers[8] });

            //Console.WriteLine("NumberHash of GameName: "+ number);
            Console.WriteLine("Fake ProdCode: " + getCodeOfNumbers(Convert.ToInt32(first10)));
            Console.WriteLine("---------------------------------------------------");
            return getCodeOfNumbers(Convert.ToInt32(first10));
        }

        private void FetchAndProcessRepoImages(string systemType, string repoid, List<string> repoids, GameConsoles console)
        {
            if (CheckForInternetConnectionWOWarning())
            {
                GetRepoImages(systemType, repoid, repoids);
                checkForAdditionalFiles(console, repoids);
            }
        }

        public void getBootIMGNES(string rom)
        {
            try
            {
                string SystemType = "nes/";
                string repoid = GetFakeNESProdcode(rom);
                List<string> repoids = new List<string> { SystemType + repoid };
                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.NES);
            }
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
        }
        public void getBootIMGNDS(string rom)
        {
            try
            {
                string repoid = "";
                string SystemType = "nds/";
                using (var fs = new FileStream(rom,
                                     FileMode.Open,
                                     FileAccess.Read))
                {

                    byte[] procode = new byte[4];
                    fs.Seek(0xC, SeekOrigin.Begin);
                    fs.Read(procode, 0, 4);
                    repoid = ByteArrayToString(procode);
                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                    repoid = rgx.Replace(repoid, "");
                    Console.WriteLine("prodcode before scramble: " + repoid);

                    fs.Close();
                    Console.WriteLine("prodcode after scramble: " + repoid);
                }
                List<string> repoids = new List<string>
                {
                    SystemType + repoid,
                    SystemType + repoid.Substring(0, 3) + "E",
                    SystemType + repoid.Substring(0, 3) + "P",
                    SystemType + repoid.Substring(0, 3) + "J"
                };

                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.NDS);
            }
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
        }

        public void getBootIMGN64(string rom)
        {
            try
            {
                string repoid = "";
                string SystemType = "n64/";
                List<string> repoids = new List<string>();
                using var fs = new FileStream(rom,
                                     FileMode.Open,
                                     FileAccess.Read);
                byte[] procode = new byte[6];
                fs.Seek(0x3A, SeekOrigin.Begin);
                fs.Read(procode, 0, 6);
                repoid = ByteArrayToString(procode);
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                repoid = rgx.Replace(repoid, "");
                Console.WriteLine("prodcode before scramble: " + repoid);

                fs.Close();
                Console.WriteLine("prodcode after scramble: " + repoid);

                repoids.Add(SystemType + repoid);
                repoids.Add(SystemType + new string(new char[] { repoid[0], repoid[2], repoid[1], repoid[3] }));
                FetchAndProcessRepoImages(SystemType, repoid, repoids, GameConsoles.N64);
            }
            catch (Exception e)
            {
                var cm = new Custom_Message("Missing Required Header Data", "Rom has missing some binary in the header used to determine the name, fetching images and other configuration files will not be possible.\nError Message: " + e.Message);
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
        }

        static string GetMd5Hash(MD5 md5Hash, byte[] input)
        {
            // Compute the hash from the byte array input.
            byte[] hashData = md5Hash.ComputeHash(input);
            // Convert the byte array to a hexadecimal string.
            return ConvertByteArrayToHexString(hashData);
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Compute the hash from the string input.
            byte[] hashData = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Convert the byte array to a hexadecimal string.
            return ConvertByteArrayToHexString(hashData);
        }

        private static string ConvertByteArrayToHexString(byte[] data)
        {
            StringBuilder hexString = new StringBuilder(data.Length * 2);

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            foreach (byte b in data)
                hexString.Append(b.ToString("x2"));

            return hexString.ToString();
        }

        static string getCodeOfNumbers(int number)
        {
            string ts = number.ToString();
            int n1 = Convert.ToInt32(ts[0] + ts[1]);
            int n2 = Convert.ToInt32(ts[2] + ts[3]);
            int n3 = Convert.ToInt32(ts[4] + ts[5]);
            int n4;
            try
            {
                n4 = Convert.ToInt32(ts[6] + ts[7]);
            }
            catch (Exception)
            {
                n4 = Convert.ToInt32(ts[6]);
            }

            char[] letters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            while (n1 > 23)
            {
                n1 -= 23;
            }
            while (n2 > 23)
            {
                n2 -= 23;
            }
            while (n3 > 23)
            {
                n3 -= 23;
            }
            while (n4 > 23)
            {
                n4 -= 23;
            }
            var toret = new char[] { letters[n1], letters[n2], letters[n3], letters[n4] };
            return new string(toret).ToUpper();
        }

        private string ByteArrayToString(byte[] arr)
        {
            return new ASCIIEncoding().GetString(arr);
        }

        public string getInternalWIIGCNName(string OpenGame, bool gc)
        {
            string ret = "";
            try
            {
                string TempString = "";
                string SystemType = (gc ? "gcn" : "wii") + "/";

                var repoid = "";
                    
                char TempChar;
                //WBFS Check
                List<string> repoids = new List<string>();
                using (var reader = new BinaryReader(File.OpenRead(OpenGame)))
                {
                    reader.BaseStream.Position = 0x00;
                    if (new FileInfo(OpenGame).Extension.Contains("wbfs")) //Performs actions if the header indicates a WBFS file
                    {
                        reader.BaseStream.Position = 0x200;
                        reader.BaseStream.Position = 0x218;

                        reader.BaseStream.Position = 0x220;
                        while ((TempChar = reader.ReadChar()) != 0) ret += TempChar;
                        reader.BaseStream.Position = 0x200;
                        while ((TempChar = reader.ReadChar()) != 0) TempString += TempChar;
                        repoid = TempString;
                    }
                    else
                    {
                        reader.BaseStream.Position = 0x18;

                        reader.BaseStream.Position = 0x20;
                        while ((TempChar = reader.ReadChar()) != 0) ret += TempChar;
                        reader.BaseStream.Position = 0x00;
                        while ((TempChar = reader.ReadChar()) != 0) TempString += TempChar;
                        repoid = TempString;
                    }
                }
                repoids.Add(SystemType + repoid);
                repoids.Add(SystemType + repoid.Substring(0, 3) + "E" + repoid.Substring(4, 2));
                repoids.Add(SystemType + repoid.Substring(0, 3) + "P" + repoid.Substring(4, 2));
                repoids.Add(SystemType + repoid.Substring(0, 3) + "J" + repoid.Substring(4, 2));

                FetchAndProcessRepoImages(SystemType, repoid, repoids, gc ? GameConsoles.GCN : GameConsoles.WII);
            }
            catch (Exception)
            {
                Custom_Message cm = new Custom_Message("Unknown ROM", " It seems that you inserted an unknown ROM as a Wii or GameCube game. \n It is not recommended continuing with said ROM!");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
            }
            return ret;
        }
        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Proxy = null;
                    client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                    client.DownloadString("http://google.com/generate_204");
                }
                return true;
            }
            catch (WebException)
            {
                ShowNoInternetConnectionMessage();
                Environment.Exit(1);
                return false;
            }
            catch (Exception ex)
            {
                // Optionally log the unexpected exception
                Console.WriteLine($"Unexpected error: {ex.Message}");
                ShowNoInternetConnectionMessage();
                Environment.Exit(1);
                return false;
            }
        }

        private void ShowNoInternetConnectionMessage()
        {
            var cm = new Custom_Message("No Internet Connection",
                "To download tools, bases, or required files, you need to be connected to the Internet. The program will now terminate.");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception ex)
            {
                // Optionally log the exception if setting the owner fails
                Console.WriteLine($"Failed to set message owner: {ex.Message}");
            }
            cm.ShowDialog();
        }

        public bool CheckForInternetConnectionWOWarning()
        {
            try
            {
                using var client = new WebClient();
                client.Proxy = null;
                client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                client.DownloadString("http://google.com/generate_204");
                return true;
            }
            catch (WebException)
            {
                return false;
            }
            catch (Exception ex)
            {
                // Optionally log the unexpected exception
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks for additional files like INI and BootSound for the given console and repository IDs.
        /// </summary>
        /// <param name="console">The game console type.</param>
        /// <param name="repoids">List of repository IDs to check for additional files.</param>
        private void checkForAdditionalFiles(GameConsoles console, List<string> repoids)
        {
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo");
            if (!Directory.Exists(repoPath))
                Directory.CreateDirectory(repoPath);

            string linkbase = "https://raw.githubusercontent.com/UWUVCI-PRIME/UWUVCI-IMAGES/master/";
            bool iniFound = false;
            bool bootSoundFound = false;
            string iniUrl = "";
            string bootSoundUrl = "";
            string bootSoundExtension = "btsnd";

            // Check for INI file
            if (console == GameConsoles.N64)
            {
                iniFound = TryFindFileInRepo(repoids, linkbase, "/game.ini", out iniUrl);
            }

            // Check for BootSound file
            bootSoundFound = TryFindFileInRepo(repoids, linkbase, $"/BootSound.{bootSoundExtension}", out bootSoundUrl);

            // Prompt user and download additional files if found
            if (iniFound || bootSoundFound)
            {
                string message = GetAdditionalFilesMessage(iniFound, bootSoundFound);
                var customMessage = new Custom_Message("Found additional Files", message);
                SetWindowOwner(customMessage);

                customMessage.ShowDialog();

                if (addi)
                {
                    DownloadAdditionalFiles(iniFound, iniUrl, bootSoundFound, bootSoundUrl, console, bootSoundExtension);
                    addi = false;
                }
            }
        }

        /// <summary>
        /// Tries to find a specific file in the repository.
        /// </summary>
        /// <param name="repoids">List of repository IDs to search.</param>
        /// <param name="linkbase">Base URL for the repository.</param>
        /// <param name="filePath">Specific file path to look for.</param>
        /// <param name="fileUrl">The found file URL.</param>
        /// <returns>True if the file is found, otherwise false.</returns>
        private bool TryFindFileInRepo(List<string> repoids, string linkbase, string filePath, out string fileUrl)
        {
            foreach (var repoid in repoids)
            {
                fileUrl = linkbase + repoid + filePath;
                if (RemoteFileExists(fileUrl))
                    return true;
            }
            fileUrl = string.Empty;
            return false;
        }

        /// <summary>
        /// Generates the appropriate message for additional files found.
        /// </summary>
        /// <param name="iniFound">Whether an INI file was found.</param>
        /// <param name="bootSoundFound">Whether a BootSound file was found.</param>
        /// <returns>A message string detailing the additional files found.</returns>
        private string GetAdditionalFilesMessage(bool iniFound, bool bootSoundFound)
        {
            if (iniFound && bootSoundFound)
                return "There is an additional INI and BootSound file available for download. Do you want to download those?";
            if (iniFound)
                return "There is an additional INI file available for download. Do you want to download it?";
            if (bootSoundFound)
                return "There is an additional BootSound file available for download. Do you want to download it?";

            return "There are more additional files found. Do you want to download those?";
        }

        /// <summary>
        /// Sets the owner of the window to the main window if possible.
        /// </summary>
        /// <param name="window">The window to set the owner for.</param>
        private void SetWindowOwner(Custom_Message window)
        {
            try
            {
                window.Owner = mw;
            }
            catch (Exception)
            {
                // Suppress exception when setting owner fails
            }
        }

        /// <summary>
        /// Downloads additional files (INI and BootSound) if they exist.
        /// </summary>
        /// <param name="iniFound">Whether an INI file was found.</param>
        /// <param name="iniUrl">The URL of the INI file.</param>
        /// <param name="bootSoundFound">Whether a BootSound file was found.</param>
        /// <param name="bootSoundUrl">The URL of the BootSound file.</param>
        /// <param name="console">The game console type.</param>
        /// <param name="bootSoundExtension">The file extension for BootSound.</param>
        private void DownloadAdditionalFiles(bool iniFound, string iniUrl, bool bootSoundFound, string bootSoundUrl, GameConsoles console, string bootSoundExtension)
        {
            var client = new WebClient();
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo");

            if (iniFound)
            {
                string iniFilePath = Path.Combine(repoPath, "game.ini");
                client.DownloadFile(iniUrl, iniFilePath);
                (Thing as N64Config).ini.Text = iniFilePath;
                GameConfiguration.N64Stuff.INIPath = iniFilePath;
            }

            if (bootSoundFound)
            {
                string bootSoundFilePath = Path.Combine(repoPath, $"bootSound.{bootSoundExtension}");
                client.DownloadFile(bootSoundUrl, bootSoundFilePath);
                BootSound = bootSoundFilePath;

                switch (console)
                {
                    case GameConsoles.NDS:
                    case GameConsoles.NES:
                    case GameConsoles.SNES:
                    case GameConsoles.MSX:
                        (Thing as OtherConfigs).sound.Text = bootSoundFilePath;
                        break;
                    case GameConsoles.GBA:
                        (Thing as GBA).sound.Text = bootSoundFilePath;
                        break;
                    case GameConsoles.N64:
                        (Thing as N64Config).sound.Text = bootSoundFilePath;
                        break;
                    case GameConsoles.WII:
                        if (test == GameConsoles.GCN)
                            (Thing as GCConfig).sound.Text = bootSoundFilePath;
                        else
                            (Thing as WiiConfig).sound.Text = bootSoundFilePath;
                        break;
                    case GameConsoles.TG16:
                        (Thing as TurboGrafX).sound.Text = bootSoundFilePath;
                        break;
                }
            }
        }

        public string GetURL(string console)
        {
            string lowerConsole = console.ToLowerInvariant();
            string formattedConsole = (lowerConsole == "tg16" || lowerConsole == "tgcd") ? "tgfx" : lowerConsole;

            return $"https://uwuvci-prime.github.io/UWUVCI-Resources/{formattedConsole}/{formattedConsole}.html";
        }


        WaveOutEvent waveOutEvent = new WaveOutEvent();
        AudioFileReader audioFileReader;
        public System.Timers.Timer t;
        public bool passtrough = true;
        internal bool enableWii = true;
        internal bool backupenableWii = true;
        public void PlaySound()
        {
            try
            {
                t = new System.Timers.Timer(200);
                t.Elapsed += isDone;

                audioFileReader = new AudioFileReader(BootSound);
                waveOutEvent.Init(audioFileReader);

                t.Start();
                Console.WriteLine("Playing file...");
                waveOutEvent.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        public void isDoneMW()
        {
            try
            {
                waveOutEvent?.Stop();
                waveOutEvent?.Dispose();
                audioFileReader?.Dispose();
                t?.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sound cleanup: {ex.Message}");
            }
        }

        public void isDone(object source, ElapsedEventArgs e)
        {
            try
            {
                if (waveOutEvent.PlaybackState == PlaybackState.Stopped ||
                    waveOutEvent.GetPositionTimeSpan() > TimeSpan.FromSeconds(6))
                {
                    waveOutEvent.Stop();
                    waveOutEvent.Dispose();
                    audioFileReader.Dispose();
                    t.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during playback check: {ex.Message}");
            }
        }

        public void RestartIntoBypass()
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = System.Windows.Application.ResourceAssembly.Location;
                p.StartInfo.Arguments = $"{(debug ? "--debug " : string.Empty)}--skip{(saveworkaround ? " --spacebypass" : string.Empty)}";
                p.Start();
            }
            Environment.Exit(0);
        }


        /// <param name="SystemType">The type of system (e.g., "Wii", "N64").</param>
        /// <param name="repoid">The repository ID for the image.</param>
        /// <param name="repoids">An optional list of repository IDs to check for images.</param>
        private void GetRepoImages(string SystemType, string repoid, List<string> repoids = null)
        {
            string linkbase = "https://raw.githubusercontent.com/UWUVCI-PRIME/UWUVCI-IMAGES/master/";
            string[] extensions = { "png", "jpg", "jpeg", "tga" };

            // If no specific repoids are provided, generate possible repoids based on the given repoid
            if (repoids == null || repoids?.Count == 0)
                repoids = GenerateRepoIds(SystemType, repoid);

            // Iterate through all combinations of repoids and extensions to find an existing image
            foreach (string extension in extensions)
                foreach (string id in repoids)
                {
                    string imageUrl = $"{linkbase}{id}/iconTex.{extension}";

                    if (RemoteFileExists(imageUrl))
                    {
                        HandleImageLoading(imageUrl, extension, id);
                        return;
                    }
                }

        }

        /// <summary>
        /// Generates a list of possible repository IDs based on the system type and the provided repository ID.
        /// </summary>
        /// <param name="SystemType">The type of system (e.g., "Wii", "N64").</param>
        /// <param name="repoid">The repository ID for the image.</param>
        /// <returns>A list of possible repository IDs.</returns>
        private List<string> GenerateRepoIds(string SystemType, string repoid)
        {
            string fakeId = new string(new char[] { repoid[0], repoid[2], repoid[1], repoid[3] });

            return new List<string>
            {
                SystemType + repoid,
                SystemType + repoid.Substring(0, 3) + "E",
                SystemType + repoid.Substring(0, 3) + "P",
                SystemType + repoid.Substring(0, 3) + "J",

                SystemType + fakeId,
                SystemType + fakeId.Substring(0, 3) + "E",
                SystemType + fakeId.Substring(0, 3) + "P",
                SystemType + fakeId.Substring(0, 3) + "J"
            };
        }

        /// <summary>
        /// Handles the image loading process and displays any necessary messages to the user.
        /// </summary>
        /// <param name="imageUrl">The URL of the image to load.</param>
        /// <param name="extension">The file extension of the image.</param>
        /// <param name="repoid">The repository ID associated with the image.</param>
        private void HandleImageLoading(string imageUrl, string extension, string repoid)
        {
            if (extension.Equals("tga", StringComparison.OrdinalIgnoreCase))
                ShowTgaWarning();

            var imgMessage = new IMG_Message(imageUrl, imageUrl.Replace("iconTex", "bootTvTex"), repoid);
            try
            {
                imgMessage.Owner = mw;
            }
            catch (Exception)
            {
                // Swallow exception to prevent crashing when setting the owner fails
            }

            imgMessage.ShowDialog();
        }

        /// <summary>
        /// Displays a warning message when a TGA image is detected.
        /// </summary>
        private void ShowTgaWarning()
        {
            var message = new Custom_Message("TGA Extension Warning",
                "TGA files can't natively be rendered in UWUVCI. Instead, the image may show an error. This is normal behavior.\n\n" +
                "If there are actual errors, please download the files from \"https://github.com/UWUVCI-PRIME/UWUVCI-IMAGES\", convert them to PNG, " +
                "and then manually insert them.");

            try
            {
                message.Owner = mw;
            }
            catch (Exception)
            {
                // Swallow exception to prevent crashing when setting the owner fails
            }

            message.ShowDialog();
        }
    }
}