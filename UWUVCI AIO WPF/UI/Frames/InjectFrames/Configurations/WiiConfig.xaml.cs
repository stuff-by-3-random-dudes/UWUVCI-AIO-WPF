using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.UI.Windows;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations
{
    /// <summary>
    /// Interaktionslogik für WiiConfig.xaml
    /// </summary>
    public partial class WiiConfig : Page, IDisposable
    {
        MainViewModel mvm;
        bool dont = true;
        public void clearImages(int i)
        {

            switch (i)
            {
                case 0:
                    icoIMG.Visibility = Visibility.Hidden;
                    mvm.GameConfiguration.TGAIco = new Classes.PNGTGA();
                    ic.Text = null;
                    break;
                case 1:
                    tvIMG.Visibility = Visibility.Hidden;
                    mvm.GameConfiguration.TGATv = new Classes.PNGTGA();
                    tv.Text = null;
                    break;
                case 2:
                    drcIMG.Visibility = Visibility.Hidden;
                    mvm.GameConfiguration.TGADrc = new Classes.PNGTGA();
                    drc.Text = null;
                    break;
                case 3:
                    logIMG.Visibility = Visibility.Hidden;
                    mvm.GameConfiguration.TGALog = new Classes.PNGTGA();
                    log.Text = null;
                    break;
            }
        }
        public void imgpath(string icon, string tv)
        {
            ic.Text = icon;
            this.tv.Text = tv;
        }
        public WiiConfig()
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
            mvm.setThing(this);
            Injection.ToolTip = "Changing the extension of a ROM may result in a faulty inject.\nWe will not give any support in such cases";
            List<string> gpEmu = new List<string>();
            gpEmu.Add("Do not use. WiiMotes only");
            gpEmu.Add("Classic Controller");
            gpEmu.Add("Horizontal WiiMote");
            gpEmu.Add("Vertical WiiMote");
            gpEmu.Add("Force Classic Controller");
            gpEmu.Add("Force No Classic Controller");
            gamepad.ItemsSource = gpEmu;
            gamepad.SelectedIndex = 0;
            mvm.test = GameBaseClassLibrary.GameConsoles.WII;
            List<string> selection = new List<string>();
            selection.Add("Video Patches");
            selection.Add("Region Patches");
            selection.Add("Extras");
            selectionDB.ItemsSource = selection;
            selectionDB.SelectedIndex = 0;
        }
        public WiiConfig(GameConfig c)
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
            getInfoFromConfig();
            mvm.GameConfiguration = c.Clone();
            mvm.setThing(this);
            Injection.ToolTip = "Changing the extension of a ROM may result in a faulty inject.\nWe will not give any support in such cases";
            List<string> gpEmu = new List<string>();
            gpEmu.Add("Do not use. WiiMotes only");
            gpEmu.Add("Classic Controller");
            gpEmu.Add("Horizontal WiiMote");
            gpEmu.Add("Vertical WiiMote");
            gpEmu.Add("Force Classic Controller");
            gpEmu.Add("Force No Classic Controller");
            gamepad.ItemsSource = gpEmu;
            gamepad.SelectedIndex = 0;
            mvm.test = GameBaseClassLibrary.GameConsoles.WII;
            List<string> selection = new List<string>();
            selection.Add("Video Patches");
            selection.Add("Region Patches");
            selection.Add("Extras");
            selectionDB.ItemsSource = selection;
            selectionDB.SelectedIndex = 0;
        }
        public void Dispose()
        {

        }

        private void Set_Rom_Path(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(true, false);
            if (!CheckIfNull(path))

            {
                int TitleIDInt = 0;
                bool isok = false;
                if (path.ToLower().Contains(".gcz") || path.ToLower().Contains(".dol") || path.ToLower().Contains(".wad"))
                {
                    isok = true;
                }
                else
                {
                    using (var reader = new BinaryReader(File.OpenRead(path)))
                    {
                        reader.BaseStream.Position = 0x00;
                        TitleIDInt = reader.ReadInt32();
                        if (TitleIDInt == 1397113431) //Performs actions if the header indicates a WBFS file
                        { isok = true; }
                        else if (TitleIDInt != 65536)
                        {
                            long GameType = 0;
                            reader.BaseStream.Position = 0x18;
                            GameType = reader.ReadInt64();
                            if (GameType == 2745048157)
                            {
                                isok = true;
                            }

                        }
                        reader.Close();
                    }
                }
                
                
                if (isok)
                {
                    motepass.IsEnabled = false;
                    motepass.IsChecked = false;
                    gamepad.IsEnabled = true;
                    mvm.NKITFLAG = false;
                    trimn.IsEnabled = false;
                    trimn.IsChecked = false;
                    vmcsmoll.IsEnabled = true;
                    pal.IsEnabled = true;
                    ntsc.IsEnabled = true;
                    mvm.donttrim = false;
                    jppatch.IsEnabled = true;
                    motepass.IsEnabled = false;
                    List<string> gpEmu = new List<string>();
                    gpEmu.Add("Do not use. WiiMotes only");
                    gpEmu.Add("Classic Controller");
                    gpEmu.Add("Horizontal WiiMote");
                    gpEmu.Add("Vertical WiiMote");
                    gpEmu.Add("Force Classic Controller");
                    gpEmu.Add("Force No Classic Controller");
                    gamepad.ItemsSource = gpEmu;
                    gamepad.ItemsSource = gpEmu;
                    mvm.RomPath = path;
                    mvm.RomSet = true;
                    if (mvm.BaseDownloaded)
                    {
                        mvm.CanInject = true;

                    }
                    if (!path.ToLower().Contains(".gcz") && !path.ToLower().Contains(".dol") && !path.ToLower().Contains(".wad"))
                    {
                        string rom = mvm.getInternalWIIGCNName(mvm.RomPath, false);
                        Regex reg = new Regex("[*'\",_&#^@:;?!<>|µ~#°²³´`éⓇ©™]");
                        gn.Text = reg.Replace(rom, string.Empty);
                        mvm.GameConfiguration.GameName = reg.Replace(rom, string.Empty);
                        if (mvm.GameConfiguration.TGAIco.ImgPath != "" || mvm.GameConfiguration.TGAIco.ImgPath != null)
                        {
                            ic.Text = mvm.GameConfiguration.TGAIco.ImgPath;
                        }
                        if (mvm.GameConfiguration.TGATv.ImgPath != "" || mvm.GameConfiguration.TGATv.ImgPath != null)
                        {
                            tv.Text = mvm.GameConfiguration.TGATv.ImgPath;
                        }
                        if (path.ToLower().Contains("iso"))
                        {

                            trimn.IsEnabled = true;
                            mvm.IsIsoNkit();
                        }
                    }
                    else if (path.ToLower().Contains(".dol"))
                    {
                        mvm.NKITFLAG = false;
                        trimn.IsEnabled = false;
                        trimn.IsChecked = false;
                        vmcsmoll.IsEnabled = false;
                        pal.IsEnabled = false;
                        ntsc.IsEnabled = false;
                        RF_n.IsEnabled = false;
                        RF_tj.IsEnabled = false;
                        RF_tn.IsEnabled = false;
                        RF_tp.IsEnabled = false;
                        jppatch.IsEnabled = false;
                        motepass.IsChecked = false;
                        motepass.IsEnabled = true;
                        mvm.donttrim = false;
                        gamepad.IsEnabled = false;
                        LR.IsEnabled = false;
                    }else if (path.ToLower().Contains(".wad"))
                    {
                        mvm.NKITFLAG = false;
                        trimn.IsEnabled = false;
                        trimn.IsChecked = false;
                        vmcsmoll.IsEnabled = false;
                        pal.IsEnabled = false;
                        ntsc.IsEnabled = false;
                        RF_n.IsEnabled = false;
                        RF_tj.IsEnabled = false;
                        RF_tn.IsEnabled = false;
                        RF_tp.IsEnabled = false;
                        jppatch.IsEnabled = false;
                        mvm.donttrim = false;
                       
                    }
                    else
                    {
                        motepass.IsChecked = false;
                        motepass.IsEnabled = false;

                        trimn.IsEnabled = true;
                    }
                       
                   
                }
                else
                {
                    Custom_Message cm = new Custom_Message("Wrong ROM", "The chosen ROM is not a supported WII Game");
                    try
                    {
                        cm.Owner = mvm.mw;
                    }
                    catch (Exception)
                    {

                    }
                    cm.ShowDialog();
                }

            }


        }

        private void InjectGame(object sender, RoutedEventArgs e)
        {
            if (File.Exists(tv.Text))
            {
                mvm.GameConfiguration.TGATv.ImgPath = tv.Text;
            }
            else if (!tv.Text.Equals("Added via Config") && !tv.Text.Equals("Downloaded from Cucholix Repo"))
            {
                mvm.GameConfiguration.TGATv.ImgPath = null;
            }
            if (File.Exists(ic.Text))
            {
                mvm.GameConfiguration.TGAIco.ImgPath = ic.Text;
            }
            else if (!ic.Text.Equals("Added via Config") && !ic.Text.Equals("Downloaded from Cucholix Repo"))
            {
                mvm.GameConfiguration.TGAIco.ImgPath = null;

            }
            if (File.Exists(log.Text))
            {
                mvm.GameConfiguration.TGALog.ImgPath = log.Text;
            }
            else if (!log.Text.Equals("Added via Config") && !log.Text.Equals("Downloaded from Cucholix Repo"))
            {
                mvm.GameConfiguration.TGALog.ImgPath = null;
            }
            if (File.Exists(drc.Text))
            {
                mvm.GameConfiguration.TGADrc.ImgPath = drc.Text;
            }
            else if (!drc.Text.Equals("Added via Config") && !drc.Text.Equals("Downloaded from Cucholix Repo"))
            {
                mvm.GameConfiguration.TGADrc.ImgPath = null;
            }
            mvm.Index = gamepad.SelectedIndex;
            if (LR.IsChecked == true)
            {
                mvm.LR = true;
            }
            else
            {
                mvm.LR = false;
            }
            mvm.GameConfiguration.GameName = gn.Text;
            mvm.Inject(false);
        }

        private void Set_TvTex(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "createdIMG", "bootTvTex.png");
            using (ImageCreator ic = new ImageCreator(mvm.GameConfiguration.Console, "bootTvTex"))
            {
                try
                {
                    ic.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                ic.ShowDialog();
            }

            if (File.Exists(path) && mvm.CheckTime(new FileInfo(path).CreationTime))
            {
                mvm.GameConfiguration.TGATv.ImgPath = path;
                mvm.GameConfiguration.TGATv.extension = new FileInfo(path).Extension;
                tv.Text = path;
                tvIMG.Visibility = Visibility.Visible;
            }
        }

        private void Set_DrcTex(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "createdIMG", "bootDrcTex.png");
            using (ImageCreator ic = new ImageCreator(mvm.GameConfiguration.Console, "bootDrcTex"))
            {
                try
                {
                    ic.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                ic.ShowDialog();
            }

            if (File.Exists(path) && mvm.CheckTime(new FileInfo(path).CreationTime))
            {
                mvm.GameConfiguration.TGADrc.ImgPath = path;
                mvm.GameConfiguration.TGADrc.extension = new FileInfo(path).Extension;
                drc.Text = path;
                drcIMG.Visibility = Visibility.Visible;
            }
        }

        private void Set_IconTex(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "createdIMG", "iconTex.png");
            using (IconCreator ic = new IconCreator("WII"))
            {
                try
                {
                    ic.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                ic.ShowDialog();
            }

            if (File.Exists(path) && mvm.CheckTime(new FileInfo(path).CreationTime))
            {
                mvm.GameConfiguration.TGAIco.ImgPath = path;
                mvm.GameConfiguration.TGAIco.extension = new FileInfo(path).Extension;
                this.ic.Text = path;
                icoIMG.Visibility = Visibility.Visible;
            }
        }

        private void Set_LogoTex(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "createdIMG", "bootLogoTex.png");
            using (LogoCreator ic = new LogoCreator())
            {
                try
                {
                    ic.Owner = (FindResource("mvm") as MainViewModel).mw;
                }
                catch (Exception)
                {

                }
                ic.ShowDialog();
            }

            if (File.Exists(path) && mvm.CheckTime(new FileInfo(path).CreationTime))
            {
                mvm.GameConfiguration.TGALog.ImgPath = path;
                mvm.GameConfiguration.TGALog.extension = new FileInfo(path).Extension;
                this.log.Text = path;
                logIMG.Visibility = Visibility.Visible;
            }

        }
        public void getInfoFromConfig()
        {
            rp.Text = "";
            mvm.RomPath = "";
            mvm.RomSet = false;
            mvm.gc2rom = "";
            tv.Text = mvm.GameConfiguration.TGATv.ImgPath;
            if (tv.Text.Length > 0)
            {
                tvIMG.Visibility = Visibility.Visible;
            }
            ic.Text = mvm.GameConfiguration.TGAIco.ImgPath;
            if (ic.Text.Length > 0)
            {
                icoIMG.Visibility = Visibility.Visible;
            }
            drc.Text = mvm.GameConfiguration.TGADrc.ImgPath;
            if (drc.Text.Length > 0)
            {
                drcIMG.Visibility = Visibility.Visible;
            }
            log.Text = mvm.GameConfiguration.TGALog.ImgPath;
            if (log.Text.Length > 0)
            {
                logIMG.Visibility = Visibility.Visible;
            }
            gn.Text = mvm.GameConfiguration.GameName;
            mvm.Index = mvm.GameConfiguration.Index;
            gamepad.SelectedIndex = mvm.GameConfiguration.Index;
            if (mvm.GameConfiguration.extension != "" && mvm.GameConfiguration.bootsound != null)
            {
                if (!Directory.Exists(@"bin\cfgBoot"))
                {
                    Directory.CreateDirectory(@"bin\cfgBoot");
                }
                if (File.Exists($@"bin\cfgBoot\bootSound.{mvm.GameConfiguration.extension}"))
                {
                    File.Delete($@"bin\cfgBoot\bootSound.{mvm.GameConfiguration.extension}");
                }
                File.WriteAllBytes($@"bin\cfgBoot\bootSound.{mvm.GameConfiguration.extension}", mvm.GameConfiguration.bootsound);
                sound.Text = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "cfgBoot", $"bootSound.{mvm.GameConfiguration.extension}");
                mvm.BootSound = sound.Text;
                sound_TextChanged(null, null);
            }
            LR.IsChecked = mvm.LR;
            if (mvm.GameConfiguration.donttrim)
            {
                trimn.IsChecked = true;
            }
            else
            {
                trimn.IsChecked = false;
            }
            jppatch.IsChecked = mvm.jppatch;
            motepass.IsChecked = mvm.passtrough;
            if (mvm.Patch)
            {
                if (mvm.toPal)
                {
                    vmcsmoll.IsChecked = false;
                    pal.IsChecked = false;
                    ntsc.IsChecked = true;
                }
                else
                {
                    vmcsmoll.IsChecked = false;
                    ntsc.IsChecked = false;
                    pal.IsChecked = true;
                }
            }
            else
            {
                vmcsmoll.IsChecked = true;
                pal.IsChecked = false;
                ntsc.IsChecked = false;
            }

            if (mvm.regionfrii)
            {
                if (mvm.regionfriijp)
                {
                    RF_n.IsChecked = false;
                    RF_tj.IsChecked = true;
                    RF_tn.IsChecked = false;
                    RF_tp.IsChecked = false;
                }
                else if (mvm.regionfriius)
                {
                    RF_n.IsChecked = false;
                    RF_tj.IsChecked = false;
                    RF_tn.IsChecked = true;
                    RF_tp.IsChecked = false;
                }
                else
                {
                    RF_n.IsChecked = false;
                    RF_tj.IsChecked = false;
                    RF_tn.IsChecked = false;
                    RF_tp.IsChecked =true;
                }
            }
            else
            {
                RF_n.IsChecked = true;
                RF_tj.IsChecked = false;
                RF_tn.IsChecked = false;
                RF_tp.IsChecked = false;
            }
        }
        private bool CheckIfNull(string s)
        {
            if (s == null || s.Equals(string.Empty))
            {
                return true;
            }
            return false;
        }

        private void gn_KeyUp(object sender, KeyEventArgs e)
        {

            /*Regex reg = new Regex("[^a-zA-Z0-9 é -]");
       string backup = string.Copy(gn.Text);
       gn.Text = reg.Replace(gn.Text, string.Empty);
       gn.CaretIndex = gn.Text.Length;
       if (gn.Text != backup)
       {
           gn.ScrollToHorizontalOffset(double.MaxValue);
       }*/




        }

        private void gn_KeyUp_1(object sender, KeyEventArgs e)
        {

        }

        private void gamepad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mvm.Index = gamepad.SelectedIndex;
            if (gamepad.SelectedIndex == 1 || gamepad.SelectedIndex == 4)
            {
                LR.IsEnabled = true;
            }
            else
            {
                LR.IsChecked = false;
                LR.IsEnabled = false;
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            mvm.toPal = true;
            mvm.Patch = true;
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            mvm.toPal = false;
            mvm.Patch = true;
        }

        private void RadioButton_Click_2(object sender, RoutedEventArgs e)
        {
            mvm.toPal = false;
            mvm.Patch = false;
        }
        public void reset()
        {
            tv.Text = "";
            drc.Text = "";
            gn.Text = "";
            ic.Text = "";
            log.Text = "";


        }
        private void icoIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ICOSHOW ics = new ICOSHOW(ic.Text);
            try
            {
                ics.Owner = mvm.mw;
            }
            catch (Exception)
            {

            }
            ics.ShowDialog();
        }

        private void tvIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TDRSHOW t = new TDRSHOW(tv.Text, false);
            try
            {
                t.Owner = mvm.mw;
            }
            catch (Exception)
            {

            }
            t.ShowDialog();
        }

        private void drcIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TDRSHOW t = new TDRSHOW(drc.Text,true);
            try
            {
                t.Owner = mvm.mw;
            }
            catch (Exception)
            {

            }
            t.ShowDialog();

        }

        private void logIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LOGSHOW t = new LOGSHOW(log.Text);
            try
            {
                t.Owner = mvm.mw;
            }
            catch (Exception)
            {

            }
            t.ShowDialog();
        }

        private void ic_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ic.Text.Length > 0)
            {
                icoIMG.Visibility = Visibility.Visible;
            }
            else
            {
                icoIMG.Visibility = Visibility.Hidden;
            }
        }

        private void drc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (drc.Text.Length > 0)
            {
                drcIMG.Visibility = Visibility.Visible;
            }
            else
            {

                drcIMG.Visibility = Visibility.Hidden;
            }
        }

        private void tv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tv.Text.Length > 0)
            {
                tvIMG.Visibility = Visibility.Visible;
            }
            else
            {
                tvIMG.Visibility = Visibility.Hidden;
            }
        }

        private void log_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (log.Text.Length > 0)
            {
                logIMG.Visibility = Visibility.Visible;
            }
            else
            {
                logIMG.Visibility = Visibility.Hidden;
            }
        }

        private void gn_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.Right))
            {
                dont = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(true, true);
            if (!CheckIfNull(path))
            {
                if (new FileInfo(path).Extension.Contains("wav"))
                {
                    if (mvm.ConfirmRiffWave(path))
                    {
                        mvm.BootSound = path;
                    }
                    else
                    {
                        Custom_Message cm = new Custom_Message("Incompatible WAV file", "Your WAV file needs to be a RIFF WAVE file which is 16 bit stereo and also 48000khz");
                        try
                        {
                            cm.Owner = mvm.mw;
                        }
                        catch (Exception)
                        {

                        }
                        cm.ShowDialog();
                    }
                }
                else
                {

                    mvm.BootSound = path;
                }
            }
            else
            {
                if (path == "")
                {
                    mvm.BootSound = null;
                    sound.Text = "";
                   
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void SoundImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mvm.PlaySound();
        }

        private void sound_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (File.Exists(mvm.BootSound))
                {
                    if (!new FileInfo(mvm.BootSound).Extension.Contains("btsnd"))
                    {
                        SoundImg.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SoundImg.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception)
            {

            }
            
            
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                TitleKeys webbrowser = new TitleKeys("wii", "Wii Inject Guide");
                try
                {
                    webbrowser.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                webbrowser.Show();
                mvm.mw.Hide();
            }
            catch (Exception)
            {
                Custom_Message cm = new Custom_Message("Not Implemented", "The Helppage for Wii is not implemented yet");
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                cm.Show();
            }
        }

        private void trimn_Click(object sender, RoutedEventArgs e)
        {
            if (!mvm.donttrim)
            {
                mvm.toPal = false;
                mvm.Patch = false;
                vmcsmoll.IsChecked = true;
                vmcsmoll.IsEnabled = false;
                pal.IsEnabled = false;
                ntsc.IsEnabled = false;
                mvm.donttrim = true;
                mvm.jppatch = false;
                int last = gamepad.SelectedIndex;
                List<string> gpEmu = new List<string>();
                gpEmu.Add("Do not use. WiiMotes only");
                gpEmu.Add("Classic Controller");
                gpEmu.Add("Horizontal WiiMote");
                gpEmu.Add("Vertical WiiMote");
                gpEmu.Add("[NEEDS TRIMMING] Force Classic Controller");
                gpEmu.Add("Force No Classic Controller");
                gamepad.ItemsSource = gpEmu;
                gamepad.SelectedIndex = last;
                jppatch.IsEnabled = false;
            }
            else
            {
                int last = gamepad.SelectedIndex;
                vmcsmoll.IsEnabled = true;
                pal.IsEnabled = true;
                ntsc.IsEnabled = true;
                mvm.donttrim = false;
                jppatch.IsEnabled = true;
                List<string> gpEmu = new List<string>();
                gpEmu.Add("Do not use. WiiMotes only");
                gpEmu.Add("Classic Controller");
                gpEmu.Add("Horizontal WiiMote");
                gpEmu.Add("Vertical WiiMote");
                gpEmu.Add("Force Classic Controller");
                gpEmu.Add("Force No Classic Controller");
                gamepad.ItemsSource = gpEmu;
                gamepad.ItemsSource = gpEmu;
                gamepad.SelectedIndex = last;
            }
            
        }

        private void jppatch_Click(object sender, RoutedEventArgs e)
        {
            if (mvm.jppatch)
            {
                mvm.jppatch = false;
            }
            else
            {
                mvm.jppatch = true;
            }
        }

        private void selectionDB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (selectionDB.SelectedIndex)
            {
                case 0:
                    VideoMode.Visibility = Visibility.Visible;
                    RegionFrii.Visibility = Visibility.Hidden;
                    Extra.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    VideoMode.Visibility = Visibility.Hidden;
                    RegionFrii.Visibility = Visibility.Visible;
                    Extra.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    VideoMode.Visibility = Visibility.Hidden;
                    RegionFrii.Visibility = Visibility.Hidden;
                    Extra.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void motepass_Checked(object sender, RoutedEventArgs e)
        {
            mvm.passtrough = false;
        }

        private void motepass_Unchecked(object sender, RoutedEventArgs e)
        {
            mvm.passtrough = true;
        }

        private void RF_tp_Click(object sender, RoutedEventArgs e)
        {
            if (RF_tp.IsChecked == true)
            {
                mvm.regionfrii = true;
                mvm.regionfriijp = false;
                mvm.regionfriius = false;
            }
            if (RF_tj.IsChecked == true)
            {
                mvm.regionfrii = true;
                mvm.regionfriijp = true;
                mvm.regionfriius = false;
            }
            if (RF_tn.IsChecked == true)
            {
                mvm.regionfrii = true;
                mvm.regionfriijp = false;
                mvm.regionfriius = true;
            }
            if (RF_n.IsChecked == true)
            {
                mvm.regionfrii = false;
                mvm.regionfriijp = false;
                mvm.regionfriius = false;
            }
        }

        private void log_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                mvm.GameConfiguration.GameName = gn.Text;
            }
            catch (Exception)
            {

            }
        }
    }
}
