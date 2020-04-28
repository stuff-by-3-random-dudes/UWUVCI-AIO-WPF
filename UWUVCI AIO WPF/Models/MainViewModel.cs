using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.UI;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Bases;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations;
using UWUVCI_AIO_WPF.UI.Windows;
using AutoUpdaterDotNET;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;
using MaterialDesignThemes.Wpf;

namespace UWUVCI_AIO_WPF
{
    public class MainViewModel : BaseModel
    {
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

        public string RomPath
        {
            get { return romPath; }
            set { romPath = value;
                OnPropertyChanged();
            }
        }

        

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
            set { baseStore = value;
                OnPropertyChanged();
            }
        }

        private string injectStore;

        public string InjectStore
        {
            get { return injectStore; }
            set { injectStore = value;
                OnPropertyChanged();
            }
        }

        private bool injected = false;

        public bool Injected
        {
            get { return injected; }
            set { injected = value;
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
            set { lBases = value;
                OnPropertyChanged();
            }
        }
        private int progress = 0;

        public int Progress
        {
            get { return progress ; }
            set { progress = value;
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
        private List<GameBases> lWii = new List<GameBases>();

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


        private bool canInject = false;

        public bool CanInject
        {
            get { return canInject; }
            set { canInject = value;
                OnPropertyChanged();
            }
        }

        private string cBasePath;

        public string CBasePath
        {
            get { return cBasePath; }
            set { cBasePath = value;
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
        private string Msg;
       
        private string Gc2rom = "";

        public string gc2rom
        {
            get { return Gc2rom; }
            set { Gc2rom = value;
                OnPropertyChanged();
            }
        }


        public string msg
        {
            get { return Msg; }
            set { Msg = value;
                OnPropertyChanged();
            }
        }
        private string bootsound;

        public string BootSound
        {
            get { return bootsound; }
            set { bootsound = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Controls.ListViewItem curr = null;

        private bool ckeys;

        public bool Ckeys
        {
            get { return ckeys; }
            set { ckeys = value;
                OnPropertyChanged();
            }
        }

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

                AutoUpdater.Start("https://raw.githubusercontent.com/Hotbrawl20/testing/master/update.xml");
                if (Properties.Settings.Default.UpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeRequired = false;
                    Properties.Settings.Default.Save();
                }
                if (button && Convert.ToInt32(version.Split('.')[3]) >= GetNewVersion())
                {
                    Custom_Message cm = new Custom_Message("No Updates available", "You are currently using the newest version of UWUVCI AIO");
                    try
                    {
                        cm.Owner = mw;
                    }
                    catch (Exception) { }
                    cm.ShowDialog();

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
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    return Convert.ToInt32(responseFromServer);
                }

            }
            catch (Exception)
            {
                return 100000;
            }
        }

        public bool ConfirmRiffWave(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                reader.BaseStream.Position = 0x00;
                long WAVHeader1 = reader.ReadInt32();
                reader.BaseStream.Position = 0x08;
                long WAVHeader2 = reader.ReadInt32();
                if (WAVHeader1 == 1179011410 & WAVHeader2 == 1163280727)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
            
          
            //if (Directory.Exists(@"Tools")) Directory.Delete(@"Tools", true);
            if (Directory.Exists(@"bases")) Directory.Delete(@"bases", true);
            if (Directory.Exists(@"temp")) Directory.Delete(@"temp", true);

            if (Directory.Exists(@"keys"))
            {
               if(Directory.Exists(@"bin\keys")) Directory.Delete(@"bin\keys", true);
                Injection.DirectoryCopy("keys", "bin/keys", true);
                Directory.Delete("keys", true);
            }
            if (!Directory.Exists("InjectedGames")) Directory.CreateDirectory("InjectedGames");
            if (!Directory.Exists("SourceFiles")) Directory.CreateDirectory("SourceFiles");
            if (!Directory.Exists("bin\\BaseGames")) Directory.CreateDirectory("bin\\BaseGames");
            if(Properties.Settings.Default.OutPath == "" || Properties.Settings.Default.OutPath == null)
            {
                Settings.Default.OutPath = Path.Combine(Directory.GetCurrentDirectory(), "InjectedGames");
            }
            if(Settings.Default.BasePath == "" || Properties.Settings.Default.BasePath == null)
            {
                Settings.Default.BasePath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "BaseGames");
            }
            Settings.Default.Save();
            ArePathsSet();

            Update(false);

            toolCheck();
            BaseCheck();

            GameConfiguration = new GameConfig();
            if (!ValidatePathsStillExist() && Settings.Default.SetBaseOnce && Settings.Default.SetOutOnce)
            {
                new Custom_Message("Issue", "One of your added Paths seems to not exist anymore.\nThe Tool is now using it's default Paths\nPlease check the paths in the Path menu!").ShowDialog();
            }
            UpdatePathSet();

            GetAllBases();
        }
        public string turbocd()
        {
           
            
            string ret = string.Empty;
            Custom_Message cm = new Custom_Message("Information", "Please put a TurboGrafX CD ROM into a folder and select said folder.\n\nThe Folder should atleast contain:\nEXACTLY ONE *.hcd file\nOne or more *.ogg files\nOne or More *.bin files\n\nNot doing so will result in a faulty Inject. You have been warned!");
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
                            cm = new Custom_Message("Issue", "The folder is Empty. Please choose another folder");
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
                                cm = new Custom_Message("Issue", "This folder mustn't contain any subfolders.");
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
                                {
                                    ret = dialog.FileName;
                                }
                                else
                                {
                                    cm = new Custom_Message("Issue", "This Folder does not contain needed minimum of Files");
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
            if (cb != null) cb.Reset();
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

        public void ExportFile()
        {
            string drcp = null;
            string tvcp = null;
            string iccp = null;
            string lgcp = null;
            string incp = null;
           if(GameConfiguration.TGADrc.ImgPath != null || GameConfiguration.TGADrc.ImgPath == "") drcp = String.Copy(GameConfiguration.TGADrc.ImgPath);
            if (GameConfiguration.TGATv.ImgPath != null || GameConfiguration.TGATv.ImgPath == "") tvcp = String.Copy(GameConfiguration.TGATv.ImgPath);
            if (GameConfiguration.TGAIco.ImgPath != null || GameConfiguration.TGAIco.ImgPath == "") iccp = String.Copy(GameConfiguration.TGAIco.ImgPath);
            if (GameConfiguration.TGALog.ImgPath != null || GameConfiguration.TGALog.ImgPath == "") lgcp = String.Copy(GameConfiguration.TGALog.ImgPath);

            if (GameConfiguration.N64Stuff.INIPath != null || GameConfiguration.N64Stuff.INIPath == "") incp = String.Copy(GameConfiguration.N64Stuff.INIPath);
            ReadImagesIntoConfig();
            if (GameConfiguration.Console == GameConsoles.N64)
            {
                ReadIniIntoConfig();
            }
            GameConfig backup = GameConfiguration;
            if (test == GameConsoles.GCN) backup.Console = GameConsoles.GCN;
            if(GameConfiguration.TGADrc.ImgBin != null && GameConfiguration.TGADrc.ImgBin.Length > 0) backup.TGADrc.ImgPath = "Added via Config";
            if (GameConfiguration.TGATv.ImgBin != null && GameConfiguration.TGATv.ImgBin.Length > 0) backup.TGATv.ImgPath = "Added via Config";
            if (GameConfiguration.TGALog.ImgBin != null && GameConfiguration.TGALog.ImgBin.Length > 0) backup.TGALog.ImgPath = "Added via Config";
            if (GameConfiguration.TGAIco.ImgBin != null && GameConfiguration.TGAIco.ImgBin.Length > 0) backup.TGAIco.ImgPath = "Added via Config";
            if (GameConfiguration.N64Stuff.INIBin != null && GameConfiguration.N64Stuff.INIBin.Length > 0) backup.N64Stuff.INIPath = "Added via Config";
            if (GameConfiguration.GameName == "" || GameConfiguration.GameName == null) backup.GameName = "NoName";
            CheckAndFixConfigFolder();
            string outputPath = $@"configs\[{backup.Console.ToString()}]{backup.GameName}.uwuvci";
            int i = 1;
            while (File.Exists(outputPath))
            {
                outputPath = $@"configs\[{backup.Console.ToString()}]{backup.GameName}_{i}.uwuvci";
                i++;
            }
            Stream createConfigStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            GZipStream compressedStream = new GZipStream(createConfigStream, CompressionMode.Compress);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(compressedStream, backup);
            compressedStream.Close();
            createConfigStream.Close();
            Custom_Message cm = new Custom_Message("Export success", "The Config was successfully exported.\nClick the Open Folder Button to open the Location where the Config is stored.", Path.Combine(Directory.GetCurrentDirectory(), outputPath));
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
            if (fn.Extension.Contains("uwuvci"))
            {
                FileStream inputConfigStream = new FileStream(configPath, FileMode.Open, FileAccess.Read);
                GZipStream decompressedConfigStream = new GZipStream(inputConfigStream, CompressionMode.Decompress);
                IFormatter formatter = new BinaryFormatter();
                GameConfiguration = (GameConfig)formatter.Deserialize(decompressedConfigStream);
            }
            if(GameConfiguration.Console == GameConsoles.N64)
            {
                (thing as N64Config).getInfoFromConfig();
            }
            else if(gameConfiguration.Console == GameConsoles.TG16)
            {
                (thing as TurboGrafX).getInfoFromConfig();
            }else if(gameConfiguration.Console == GameConsoles.WII && test != GameConsoles.GCN)
            {
                (thing as WiiConfig).getInfoFromConfig();
            }else if (test == GameConsoles.GCN)
            {
                (thing as GCConfig).getInfoFromConfig();
            }else if(gameConfiguration.Console == GameConsoles.GBA)
            {
                (thing as GBA).getInfoFromConfig();
            }
            else
            {
                try
                {
                    (thing as OtherConfigs).getInfoFromConfig();
                }
               catch(Exception e)
                {
                    (thing as GCConfig).getInfoFromConfig();
                }
            }
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
            if(FilePath != null)
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
        private static void CheckAndFixConfigFolder()
        {
            if (!Directory.Exists(@"configs"))
            {
                Directory.CreateDirectory(@"configs");
            }
        }
        public void Pack(bool loadiine)
        {
            ValidatePathsStillExist();
            if (loadiine)
            {
                Injection.Loadiine(GameConfiguration.GameName);
                //
            }
            else
            {
                if(gameConfiguration.GameName != null)
                {
                    Regex reg = new Regex("[^a-zA-Z0-9 é -]");
                    gameConfiguration.GameName = reg.Replace(gameConfiguration.GameName, "");
                }
                
                Task.Run(() => { Injection.Packing(GameConfiguration.GameName, this); });

                DownloadWait dw = new DownloadWait("Packing Inject - Please Wait", "", this);
                try
                {
                    dw.Owner = mw;
                }
                catch (Exception) { }
                dw.ShowDialog();
             
                Progress = 0;
                string extra = "";
                if (GameConfiguration.Console == GameConsoles.WII) extra = "\nSome games cannot reboot into the WiiU Menu. Shut down via the GamePad.\nIf Stuck in a BlackScreen, you need to unplug your WiiU.";
                if (GC) extra = "\nMake sure to have Nintendon't + config on your SD.\nYou can add them under Settings -> \"Start Nintendont Config Tool\".";
                gc2rom = "";
                Custom_Message cm = new Custom_Message("Injection Complete", $"It's recommended to install onto USB to avoid brick risks.{extra}\nConfig will stay filled, choose a Console again to clear it!\nTo Open the Location of the Inject press Open Folder.", Settings.Default.OutPath);
                try
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
            if(Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo"))) Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo"), true);
        }

        DownloadWait Injectwait;
        public void runInjectThread(bool force)
        {
            
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick2;
            timer.Start();
            var thread = new Thread(() =>
            {
                Injectwait =new DownloadWait("Injecting Game - Please Wait", "", this);

                try
                {
                    Injectwait.Owner = mw;
                }
                catch (Exception) { }
                Injectwait.Topmost = true;
                Injectwait.ShowDialog();

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            

        }


        public void Inject(bool force)
        {
            ValidatePathsStillExist();
            /* var task = new Task(() => runInjectThread(true));
              task.Start();*/
            Task.Run(() =>
            {
                if (Injection.Inject(GameConfiguration, RomPath, this, force)) Injected = true;
                else Injected = false;
            });
            DownloadWait dw = new DownloadWait("Injecting Game - Please Wait", "", this);
            try
            {
                dw.Owner = mw;
            }catch(Exception e) { }
            dw.ShowDialog();
            Progress = 0;
            if (Injected)
            {
                Custom_Message cm = new Custom_Message("Finished Injection Part", "Injection Finished, please choose how you want to export the Inject next.");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
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
                            dw.Owner = mw;
                        }
                        catch (Exception) { }
                        dw.ShowDialog();

                        BaseCheck();
                    }
                    else
                    {
                        Custom_Message dw = new Custom_Message("No Internet connection", "You have files missing, which need to be downloaded but you dont have an Internet Connection.\nThe Program will now terminate");
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
                        dw.Owner = mw;
                    }
                    catch (Exception) { }
                    dw.ShowDialog();
                    Progress = 0;
                    BaseCheck();
                }
                else
                {
                    Custom_Message dw = new Custom_Message("No Internet connection", "You have files missing, which need to be downloaded but you dont have an Internet Connection.\nThe Program will now terminate");
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
                    dw.Owner = mw;
                }
                catch (Exception)
                {

                }
                dw.ShowDialog();
                Custom_Message cm = new Custom_Message("Finished Update", "Finished Updating Tools! Restarting UWUVCI AIO");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                Environment.Exit(0);
            }
            
        }
        public void ResetTKQuest()
        {
            Custom_Message cm = new Custom_Message("Resetting TitleKeys", "This Option will reset all entered TitleKeys meaning you will need to reenter them again!\nDo you still wish to continue?");
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
                Custom_Message cm = new Custom_Message("Reset Successful", "The TitleKeys are now reset.\nThe Program will now restart.");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();
            mw.Close();
            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
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
                    dw.Owner = mw;
                }
                catch (Exception)
                {

                }
                dw.ShowDialog();

                Custom_Message cm = new Custom_Message("Finished Updating", "Finished Updating Bases! Restarting UWUVCI AIO");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                Environment.Exit(0);
            }
            
            
        }
        public bool checkSysKey(string key)
        {
            if(key.GetHashCode() == -589797700)
            {
                Properties.Settings.Default.SysKey = key;
                Properties.Settings.Default.Save();
                return true;
            }
            return false;
        }
        public bool SysKey1set()
        {
            return checkSysKey1(Properties.Settings.Default.SysKey1);
        }
        public bool checkSysKey1(string key)
        {
            if (key.GetHashCode() == -1230232583)
            {
                Properties.Settings.Default.SysKey1 = key;
                Properties.Settings.Default.Save();
                return true;
            }
            return false;
        }
        public bool SysKeyset()
        {
            return checkSysKey(Properties.Settings.Default.SysKey);
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
                dialog.Filter = "UWUVCI Config (*.uwuvci) | *.uwuvci";
                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    ret = dialog.FileName;
                    if (GetConsoleOfConfig(ret, console))
                    {
                        ImportConfig(ret);
                        Custom_Message cm = new Custom_Message("Import Complete", "Importing of Config completed.\nPlease reselect a Base!");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }
                    else
                    {
                        Custom_Message cm = new Custom_Message("Import Failed", $"The config you are trying to import is not made for {console.ToString()} Injections. \nPlease choose a config made for these kind of Injections or choose a different kind of Injection");
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
                        cm = new Custom_Message("Information", "You can only inject NDS ROMs that are not DSi Enhanced (example for not working: Pokémon Black & White)\n\nIf attempting to inject a DSi Enhanced ROM, we will not give you any support with fixing said injection");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();

                        break;
                    case GameConsoles.SNES:
                        cm = new Custom_Message("Information", "You can only inject SNES ROMs that are not using any Co-Processors (example for not working: Star Fox)\n\nIf attempting to inject a ROM in need of a Co-Processor, we will not give you any support with fixing said injection");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();

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
                                if(test == GameConsoles.GCN)
                                {
                                    dialog.Filter = "GCN ROM (*.iso; *.gcm) | *.iso; *.gcm";
                                }
                                else
                                {
                                    dialog.Filter = "Wii ROM (*.nkit.iso; *.iso; *.wbfs) | *.nkit.iso; *.iso; *.wbfs";
                                }
                                
                                break;
                            case GameConsoles.GCN:
                                dialog.Filter = "GCN ROM (*.nkit.iso; *.iso; *.gcm) | *.nkit.iso; *.iso; *.gcm";
                                break;
                        }
                    }
                   
                   
                }
                else if(!INI)
                {
                    
                    dialog.Filter = "BootImages (*.png; *.jpg; *.bmp; *.tga) | *.png;*.jpg;*.bmp;*.tga";
                }
                else if(INI)
                {
                    dialog.Filter = "N64 VC Configuration (*.ini) | *.ini";
                }
                if (Directory.Exists("SourceFiles"))
                {
                    dialog.InitialDirectory = "SourceFiles";
                }
               
                DialogResult res = dialog.ShowDialog();
                if(res == DialogResult.OK)
                {
                    ret = dialog.FileName;
                }
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
            {
                ret.Add(path + "nds");
            }
            if (!File.Exists(path + "nes"))
            {
                ret.Add(path + "nes");
            }
            if (!File.Exists(path + "n64"))
            {
                ret.Add(path + "n64");
            }
            if (!File.Exists(path + "snes"))
            {
                ret.Add(path + "snes");
            }
            if (!File.Exists(path + "gba"))
            {
                ret.Add(path + "gba");
            }
            if (!File.Exists(path + "tg16"))
            {
                ret.Add(path + "tg16");
            }
            if (!File.Exists(path + "msx"))
            {
                ret.Add(path + "msx");
            }
            if (!File.Exists(path + "wii"))
            {
                ret.Add(path + "wii");
            }
            return ret;
        }
        public static void DownloadBase(string name, MainViewModel mvm)
        {
            string olddir = Directory.GetCurrentDirectory();
            try
            {
                string basePath = $@"bin\bases\";
                Directory.SetCurrentDirectory(basePath);
                using (var client = new WebClient())

                {
                    var fixname = name.Split('\\');
                    client.DownloadFile(getDownloadLink(name, false), fixname[fixname.Length -1]);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Custom_Message cm = new Custom_Message("Error 005: \"Unable to Download VCB Base\"", "There was an Error downloading the VCB Base File.\nThe Programm will now terminate.");
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
               

                string basePath = $@"bin\Tools\";
                Directory.SetCurrentDirectory(basePath);
                using (var client = new WebClient())
                {
                    client.DownloadFile(getDownloadLink(name, true), name);
                    
                }
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Custom_Message cm = new Custom_Message("Error 006: \"Unable to Download Tool\"", "There was an Error downloading the Tool.\nThe Programm will now terminate.");
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
                WebRequest request;
                //get download link from uwuvciapi
                if (tool)
                {
                     request = WebRequest.Create("https://uwuvciapi.azurewebsites.net/GetToolLink?tool=" + toolname);
                }
                else
                {
                    request = WebRequest.Create("https://uwuvciapi.azurewebsites.net/GetVcbLink?vcb=" + toolname);
                }
                
                var response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    return responseFromServer;
                }
               
            }
            catch (Exception)
            {
                return null;
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
                        {
                            DownloadTool(m.Name,this);
                            
                        }
                       
                    
                   
                        InjcttoolCheck();
                    
                }
            }
            else
            {
                string path = $@"{Directory.GetCurrentDirectory()}bin\\Tools";
                
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
                    DownloadTool(m.Name,this);
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
                
                if(missingTools.Count > 0)
                {
                    if (CheckForInternetConnection())
                    {
                        Task.Run(() => ThreadDownload(missingTools));
                        new DownloadWait("Downloading Tools - Please Wait", "", this).ShowDialog();
                        Thread.Sleep(200);
                        //Download Tools
                        Progress = 0;
                        toolCheck();
                    }
                    else
                    {
                        Custom_Message dw = new Custom_Message("No Internet connection", "You have files missing, which need to be downloaded but you dont have an Internet Connection.\nThe Program will now terminate");
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
                
                    Directory.CreateDirectory("bin/Tools");
                    toolCheck();
               
                
            }
        }

        public void UpdatePathSet()
        {
            PathsSet = Settings.Default.PathsSet;
            if(BaseStore != Settings.Default.BasePath)
            {
                BaseStore = Settings.Default.BasePath;
            }
            if (InjectStore != Settings.Default.BasePath)
            {
                InjectStore = Settings.Default.OutPath;
            }
        }

        public bool ValidatePathsStillExist()
        {
            bool ret = false;
            bool basep = false;
            try
            {
                if (Directory.Exists(Settings.Default.BasePath))
                {
                    basep = true;
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bin","BaseGames"))) Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "bin", "BaseGames"));
                    Settings.Default.BasePath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "BaseGames");
                    Settings.Default.PathsSet = true;
                    Settings.Default.Save();
                }
                if (Directory.Exists(Settings.Default.OutPath))
                {
                    if (basep)
                    {
                        ret = true;
                    }
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "InjectedGames"))) Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "InjectedGames"));
                    Settings.Default.OutPath = Path.Combine(Directory.GetCurrentDirectory(), "InjectedGames");
                    Settings.Default.PathsSet = true;
                    Settings.Default.Save();
                }
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        public void GetBases(GameConsoles Console)
        {
            List<GameBases> lTemp = new List<GameBases>();
            string vcbpath = $@"bin/bases/bases.vcb{Console.ToString().ToLower()}";
            lTemp = VCBTool.ReadBasesFromVCB(vcbpath);
            LBases.Clear();
            GameBases custom = new GameBases();
            custom.Name = "Custom";
            custom.Region = Regions.EU;
            LBases.Add(custom);
            foreach(GameBases gb in lTemp)
            {
                LBases.Add(gb);
            }
            lGameBasesString.Clear();
            foreach(GameBases gb in LBases)
            {
                LGameBasesString.Add($"{gb.Name} {gb.Region}");
            }
        }
        
        public GameBases getBasefromName(string Name)
        {
            string NameWORegion = Name.Remove(Name.Length - 3, 3);
            string Region = Name.Remove(0, Name.Length - 2);
            foreach(GameBases b in LNDS)
            {
                if(b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LN64)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LNES)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LSNES)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LGBA)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LTG16)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LMSX)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            foreach (GameBases b in LWII)
            {
                if (b.Name == NameWORegion && b.Region.ToString() == Region)
                {
                    return b;
                }
            }
            return null;
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
        private void CreateSettingIfNotExist(List<GameBases> l, GameConsoles console)
        {
            string file = $@"bin\keys\{console.ToString().ToLower()}.vck";
            if (!File.Exists(file))
            {
                List<TKeys> temp = new List<TKeys>();
                foreach(GameBases gb in l)
                {
                    TKeys tempkey = new TKeys();
                    tempkey.Base = gb;
                    temp.Add(tempkey);
                }
                KeyFile.ExportFile(temp, console);
            }
                       
        }
        private void UpdateKeyFile(List<GameBases> l, GameConsoles console)
        {
            string file = $@"bin\keys\{console.ToString().ToLower()}.vck";
            if (File.Exists(file))
            {
                List<TKeys> keys = KeyFile.ReadBasesFromKeyFile($@"bin\keys\{console.ToString().ToLower()}.vck");
                List <TKeys> newTK = new List<TKeys>();
                foreach(GameBases gb in l)
                {
                    bool inOld = false;
                    foreach(TKeys tk in keys)
                    {
                        if(gb.Name == tk.Base.Name && gb.Region == tk.Base.Region)
                        {
                            newTK.Add(tk);
                            inOld = true;
                        }
                        if (inOld) break;
                    }
                    if (!inOld)
                    {
                        TKeys tkn = new TKeys();
                        tkn.Base = gb;
                        tkn.Tkey = null;
                        newTK.Add(tkn);
                    }
                }
                File.Delete($@"bin\keys\{console.ToString().ToLower()}.vck");
                KeyFile.ExportFile(newTK, console);
            }
        }
        public void getTempList(GameConsoles console)
        {
            switch (console)
            {
                case GameConsoles.NDS:
                    Ltemp = LNDS;
                    break;
                case GameConsoles.N64:
                    Ltemp = LN64;
                    break;
                case GameConsoles.GBA:
                    Ltemp = LGBA;
                    break;
                case GameConsoles.NES:
                    Ltemp = LNES;
                    break;
                case GameConsoles.SNES:
                    Ltemp = LSNES;
                    break;
                case GameConsoles.TG16:
                    Ltemp = LTG16;
                    break;
                case GameConsoles.MSX:
                    Ltemp = LMSX;
                    break;
                case GameConsoles.WII:
                    Ltemp = LWII;
                    break;
            }
        }

        public void EnterKey(bool ck)
        {
            EnterKey ek = new EnterKey(ck);
            try
            {
                ek.Owner = mw;
            }catch(Exception e) { }
            ek.ShowDialog();
        }
        public bool checkcKey(string key)
        {
            if (1274359530 == key.ToLower().GetHashCode())
            {
                Settings.Default.Ckey = key.ToLower();
                ckeys = true;
                Settings.Default.Save();
                
                return true;
            }
            ckeys = false;
            return false;
        }
        public bool isCkeySet()
        {
            if (Settings.Default.Ckey.ToLower().GetHashCode() == 1274359530)
            {
                ckeys = true;
                return true;
            }
            else
            {
                ckeys = false;
                return false;
            }
        }
        public bool checkKey(string key)
        {
            if(GbTemp.KeyHash == key.ToLower().GetHashCode())
            {
                UpdateKeyInFile(key, $@"bin\keys\{GetConsoleOfBase(gbTemp).ToString().ToLower()}.vck", GbTemp, GetConsoleOfBase(gbTemp));
                return true;
            }
            return false;
        }
        public void UpdateKeyInFile(string key, string file, GameBases Base, GameConsoles console)
        {
            if (File.Exists(file))
            {
                var temp = KeyFile.ReadBasesFromKeyFile(file);
                foreach(TKeys  t in temp)
                {
                    if(t.Base.Name == Base.Name && t.Base.Region == Base.Region)
                    {
                        t.Tkey = key;
                    }
                }
                File.Delete(file);
                KeyFile.ExportFile(temp, console);
            }
        }
        public bool isKeySet(GameBases bases)
        {
            var temp = KeyFile.ReadBasesFromKeyFile($@"bin\keys\{GetConsoleOfBase(bases).ToString().ToLower()}.vck");
            foreach(TKeys t in temp)
            {
                if(t.Base.Name == bases.Name && t.Base.Region == bases.Region)
                {
                    if(t.Tkey != null)
                    {
                        return true;
                    }
                }
            }
            return false;

        }
        public void ImageWarning()
        {
            Custom_Message cm = new Custom_Message("Image Warning", "Images need to either be in a Bit Depth of 32bit or 24bit. \nIf using Tools like paint.net do not choose the Auto function.");
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
            bool ret = false;
            Custom_Message cm = new Custom_Message("NUS Custom Base", "You seem to have added a NUS format Custom Base.\nDo you want it to be converted to be used with the Injector?");
            try
            {
                cm.Owner = mw;
            }
            catch (Exception) { }
            cm.ShowDialog();

            if (choosefolder)
            {
                ret = true;
                choosefolder = false;
            }
            return ret;
        }
        public TKeys getTkey(GameBases bases)
        {
            var temp = KeyFile.ReadBasesFromKeyFile($@"bin\keys\{GetConsoleOfBase(bases).ToString().ToLower()}.vck");
            foreach (TKeys t in temp)
            {
                if (t.Base.Name == bases.Name && t.Base.Region == bases.Region)
                {
                    if (t.Tkey != null)
                    {
                        return t;
                    }
                }
            }
            return null;

        }
        public void ThreadDOwn()
        {

           
        }
        public void Download()
        {
            ValidatePathsStillExist();
            if (CheckForInternetConnection())
            {
                Task.Run(() => { Injection.Download(this); });

                DownloadWait dw = new DownloadWait("Downloading Base - Please Wait", "", this);
                try
                {
                    dw.Owner = mw;
                }
                catch (Exception) { }
                dw.ShowDialog();
                Progress = 0;
            }
           
            
        }
        public GameConsoles GetConsoleOfBase(GameBases gb)
        {
            GameConsoles ret = new GameConsoles();
            bool cont = false;
            foreach(GameBases b in lNDS)
            {
                if(b.Name == gb.Name && b.Region == gb.Region)
                {
                    ret = GameConsoles.NDS;
                    cont = true;
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lN64)
                {
                    if (b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.N64;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lNES)
                {
                    if (b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.NES;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lSNES)
                { if(b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.SNES;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lGBA)
                {
                    if (b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.GBA;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lTG16)
                {
                    if (b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.TG16;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lMSX)
                {
                    if (b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.MSX;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lWii)
                {
                    if (b.Name == gb.Name && b.Region == gb.Region)
                    {
                        ret = GameConsoles.WII;
                        cont = true;
                    }
                }
            }
            return ret;
        }
        public List<bool> getInfoOfBase(GameBases gb)
        {
            List<bool> info = new List<bool>();
            if (Directory.Exists($@"{Settings.Default.BasePath}\{gb.Name.Replace(":","")} [{gb.Region}]"))
            {
                info.Add(true);
            }
            else
            {
                info.Add(false);
            }
            if (isKeySet(gb))
            {
                info.Add(true);
            }
            else
            {
                info.Add(false);
            }
            if (isCkeySet())
            {
                info.Add(true);
            }
            else
            {
                info.Add(false);
            }
            return info;
        }

       
        public void SetInjectPath()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
               CommonFileDialogResult result = dialog.ShowDialog();
                if(result == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        if (DirectoryIsEmpty(dialog.FileName))
                        {
                            Settings.Default.OutPath = dialog.FileName;
                            Settings.Default.SetOutOnce = true;
                            Settings.Default.Save();
                            UpdatePathSet();
                        }
                        else
                        {
                            Custom_Message cm = new Custom_Message("Information", "Folder contains Files or Subfolders, do you really want to use this folder as the Inject Folder?");
                            try
                            {
                                cm.Owner = mw;
                            }
                            catch (Exception) { }
                            cm.ShowDialog();
                            if (choosefolder)
                            {
                                choosefolder = false;
                                Settings.Default.OutPath = dialog.FileName;
                                Settings.Default.SetOutOnce = true;
                                Settings.Default.Save();
                                UpdatePathSet();

                            }
                            else
                            {
                                SetInjectPath();
                            }
                        }
                    }catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Custom_Message cm = new Custom_Message("Error", "An Error occured, please try again!");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }
                  
                }
            }
            ArePathsSet();
        }
        public void SetBasePath()
        {
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
                            Settings.Default.BasePath = dialog.FileName;
                            Settings.Default.SetBaseOnce = true;
                            Settings.Default.Save();
                            UpdatePathSet();
                        }
                        else
                        {
                            Custom_Message cm = new Custom_Message("Information", "Folder contains Files or Subfolders, do you really want to use this folder as the Bases Folder?");
                            try
                            {
                                cm.Owner = mw;
                            }
                            catch (Exception) { }
                            cm.ShowDialog();
                            if (choosefolder)
                            {
                                choosefolder = false;
                                Settings.Default.BasePath = dialog.FileName;
                                Settings.Default.SetBaseOnce = true;
                                Settings.Default.Save();
                                UpdatePathSet();

                            }
                            else
                            {
                                SetInjectPath();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Custom_Message cm = new Custom_Message("Error", "An Error occured, please try again!");
                        try
                        {
                            cm.Owner = mw;
                        }
                        catch (Exception) { }
                        cm.ShowDialog();
                    }

                }
            }
            ArePathsSet();
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
            int fileCount = Directory.GetFiles(path).Length;
            if (fileCount > 0)
            {
                return false;
            }

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (!DirectoryIsEmpty(dir))
                {
                    return false;
                }
            }

            return true;
        }
        public void getBootIMGGBA(string rom)
        {
            string linkbase = "https://raw.githubusercontent.com/Flumpster/UWUVCI-Images/master/";
            string repoid = "";
            string SystemType = "gba/";
            IMG_Message img = null;
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
            string[] ext = { "png", "jpg", "tga", "bmp" };
            if (CheckForInternetConnectionWOWarning())
            {
                foreach(var e in ext)
                {
                    if (RemoteFileExists(linkbase + SystemType + repoid + $"/iconTex.{e}") == true)
                    {
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog();
                        break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "E" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "E";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog();
                        break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "P" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "P";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}g", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog();
                        break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "J" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "J";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog();
                        break;
                    }
                }
                
               



            }

        }
        public void getBootIMGNDS(string rom)
        {
            string linkbase = "https://raw.githubusercontent.com/Flumpster/UWUVCI-Images/master/";
            string repoid = "";
            string SystemType = "nds/";
            IMG_Message img = null;
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
            string[] ext = { "png", "jpg", "tga", "bmp" };
            if (CheckForInternetConnectionWOWarning())
            {
                foreach (var e in ext)
                {
                    if (RemoteFileExists(linkbase + SystemType + repoid + $"/iconTex.{e}") == true)
                    {
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog(); break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "E" + $"/iconTex.png") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "E";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog(); break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "P" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "P";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + "/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog(); break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "J" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "J";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        img.ShowDialog(); break;
                    }
                }


            }

        }
        public void getBootIMGN64(string rom)
        {
            string linkbase = "https://raw.githubusercontent.com/Flumpster/UWUVCI-Images/master/";
            string repoid = "";
            string SystemType = "n64/";
            IMG_Message img = null;
            using (var fs = new FileStream(rom,
                                 FileMode.Open,
                                 FileAccess.Read))
            {
                byte[] procode = new byte[6];
                fs.Seek(0x3A, SeekOrigin.Begin);
                fs.Read(procode, 0, 6);
                repoid = ByteArrayToString(procode);
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                repoid = rgx.Replace(repoid, "");
                Console.WriteLine("prodcode before scramble: "+repoid);
               
                fs.Close();
                Console.WriteLine("prodcode after scramble: "+repoid);
            }
            bool found = false;
            string[] ext = { "png", "jpg", "tga", "bmp" };
            if (CheckForInternetConnectionWOWarning())
            {
                foreach (var e in ext)
                {


                    if (RemoteFileExists(linkbase + SystemType + repoid + $"/iconTex.{e}") == true)
                    {
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        found = true;
                        img.ShowDialog(); break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "E" + $"/iconTex.png") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "E";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        found = true;
                        img.ShowDialog(); break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "P" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "P";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        found = true;
                        img.ShowDialog(); break;
                    }
                    else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "J" + $"/iconTex.{e}") == true)
                    {
                        repoid = repoid.Substring(0, 3) + "J";
                        img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                        try
                        {
                            img.Owner = mw;
                        }
                        catch (Exception) { }
                        found = true;
                        img.ShowDialog(); break;
                    }
                    else
                    {
                        repoid = new string(new char[] { repoid[0], repoid[2], repoid[1], repoid[3] });
                        if (RemoteFileExists(linkbase + SystemType + repoid + $"/iconTex.{e}") == true)
                        {
                            img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                            try
                            {
                                img.Owner = mw;
                            }
                            catch (Exception) { }
                            img.ShowDialog(); break;
                        }
                        else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "E" + $"/iconTex.png") == true)
                        {
                            repoid = repoid.Substring(0, 3) + "E";
                            img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                            try
                            {
                                img.Owner = mw;
                            }
                            catch (Exception) { }
                            found = true;
                            img.ShowDialog(); break;
                        }
                        else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "P" + $"/iconTex.png") == true)
                        {
                            repoid = repoid.Substring(0, 3) + "P";
                            img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.png", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                            try
                            {
                                img.Owner = mw;
                            }
                            catch (Exception) { }
                            img.ShowDialog(); break;
                        }
                        else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "J" + "/iconTex.png") == true)
                        {
                            repoid = repoid.Substring(0, 3) + "J";
                            img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                            try
                            {
                                img.Owner = mw;
                            }
                            catch (Exception) { }
                            found = true;
                            img.ShowDialog(); break;
                        }
                    }
                }
                
                
            }

        }
        private string ByteArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }
        public string getInternalWIIGCNName(string OpenGame, bool gc)
        {
            //string linkbase = "https://raw.githubusercontent.com/Flumpster/wiivc-bis/master/";
            string linkbase = "https://raw.githubusercontent.com/Flumpster/UWUVCI-Images/master/";
            string ret = "";
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(OpenGame)))
                {
                    string TempString = "";
                    string SystemType = "wii/";
                    if (gc)
                    {
                        SystemType = "gcn/";
                    }
                    IMG_Message img;
                    reader.BaseStream.Position = 0x00;
                    char TempChar;
                    //WBFS Check
                    string[] ext = { "tga", "jpg", "png", "bmp" };
                    if (new FileInfo(OpenGame).Extension.Contains("wbfs")) //Performs actions if the header indicates a WBFS file
                    {

                        reader.BaseStream.Position = 0x200;

                        reader.BaseStream.Position = 0x218;


                        reader.BaseStream.Position = 0x220;
                        while ((int)(TempChar = reader.ReadChar()) != 0) ret = ret + TempChar;
                        reader.BaseStream.Position = 0x200;
                        while ((int)(TempChar = reader.ReadChar()) != 0) TempString = TempString + TempChar;
                        string repoid = TempString;
                        
                        if (CheckForInternetConnectionWOWarning())
                        {
                            foreach(var e in ext)
                            {
                                if (RemoteFileExists(linkbase + SystemType + repoid + $"/iconTex.{e}") == true)
                                {
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                                else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "E" + repoid.Substring(4, 2) + $"/iconTex.{e}") == true)
                                {
                                    repoid = repoid.Substring(0, 3) + "E" + repoid.Substring(4, 2);
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                                else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "P" + repoid.Substring(4, 2) + $"/iconTex.{e}") == true)
                                {
                                    repoid = repoid.Substring(0, 3) + "P" + repoid.Substring(4, 2);
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                                else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "J" + repoid.Substring(4, 2) + $"/iconTex.{e}") == true)
                                {
                                    repoid = repoid.Substring(0, 3) + "J" + repoid.Substring(4, 2);
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                            }
                            
                        }
                    }
                    else
                    {


                        string repoid = "";
                        reader.BaseStream.Position = 0x18;

                        reader.BaseStream.Position = 0x20;
                        while ((int)(TempChar = reader.ReadChar()) != 0) ret = ret + TempChar;
                        reader.BaseStream.Position = 0x00;
                            while ((int)(TempChar = reader.ReadChar()) != 0) TempString = TempString + TempChar;
                            repoid = TempString;
                        
                        
                        if (CheckForInternetConnectionWOWarning())
                        {
                            foreach (var e in ext)
                            {
                                if (RemoteFileExists(linkbase + SystemType + repoid + $"/iconTex.{e}") == true)
                                {
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                                else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "E" + repoid.Substring(4, 2) + $"/iconTex.{e}") == true)
                                {
                                    repoid = repoid.Substring(0, 3) + "E" + repoid.Substring(4, 2);
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                                else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "P" + repoid.Substring(4, 2) + $"/iconTex.{e}") == true)
                                {
                                    repoid = repoid.Substring(0, 3) + "P" + repoid.Substring(4, 2);
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                                else if (RemoteFileExists(linkbase + SystemType + repoid.Substring(0, 3) + "J" + repoid.Substring(4, 2) + $"/iconTex.{e}") == true)
                                {
                                    repoid = repoid.Substring(0, 3) + "J" + repoid.Substring(4, 2);
                                    img = new IMG_Message(linkbase + SystemType + repoid + $"/iconTex.{e}", linkbase + SystemType + repoid + $"/bootTvTex.{e}", SystemType + repoid);
                                    try
                                    {
                                        img.Owner = mw;
                                    }
                                    catch (Exception) { }
                                    img.ShowDialog(); break;
                                }
                            }
                        }

                    }
                }
            }catch(Exception e)
            {
                Custom_Message cm = new Custom_Message("Unknown ROM", "It seems that you inserted an unknown ROM as a Wii or GameCube game.\nIt is not recommended continuing with said ROM!");
                try
                {
                    cm.Owner = mw;
                }
                catch (Exception) { }
                cm.ShowDialog();
                ret = "";
            }
           
            
            return ret;
        }
        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                Custom_Message cm = new Custom_Message("No Internet Connection", "To Download Tools, Bases or required Files you need to be connected to the Internet");
                try
                {
                    cm.Owner = mw;
                    
                }


                catch (Exception) { }
                cm.ShowDialog();
                return false;
            }
        }
        public bool CheckForInternetConnectionWOWarning()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                
                
                return false;
            }
        }
        public string GetURL(string console)
        {
            WebRequest request;
            //get download link from uwuvciapi
            
                request = WebRequest.Create("https://uwuvciapi.azurewebsites.net/GetURL?cns=" + console.ToLower());
            
           

            var response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                // Display the content.  
                return responseFromServer;
            }
        }
    }
}
