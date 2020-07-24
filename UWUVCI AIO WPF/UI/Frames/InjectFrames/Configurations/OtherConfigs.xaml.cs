using GameBaseClassLibrary;
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
using System.Windows.Forms.VisualStyles;
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
    /// Interaktionslogik für OtherConfigs.xaml
    /// </summary>
    public partial class OtherConfigs : Page, IDisposable
    {
        MainViewModel mvm;
        public OtherConfigs()
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
            mvm.setThing(this);
            Injection.ToolTip = "Changing the extension of a ROM may result in a faulty inject.\nWe will not give any support in such cases";
            sound.ToolTip += "\nWill be cut to 6 seconds of Length";
            if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NES || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.SNES)
            {
                snesnes.Visibility = Visibility.Visible;
            }
        }
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
        public OtherConfigs(GameConfig c)
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
            mvm.GameConfiguration = c.Clone(); getInfoFromConfig();
            mvm.setThing(this);
            Injection.ToolTip = "Changing the extension of a ROM may result in a faulty inject.\nWe will not give any support in such cases";
            sound.ToolTip += "\nWill be cut to 6 seconds of Length";
            if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NES || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.SNES)
            {
                snesnes.Visibility = Visibility.Visible;
            }
        }
        public void Dispose()
        {

        }
        public void imgpath(string icon, string tv)
        {
            ic.Text = icon;
            this.tv.Text = tv;
        }
        private void Set_Rom_Path(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(true, false);
            if (!CheckIfNull(path)) {
                mvm.RomPath = path;
                mvm.RomSet = true;
                if (mvm.BaseDownloaded)
                {
                    mvm.CanInject = true;
                    
                }
                if(mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NDS)
                {
                    mvm.getBootIMGNDS(mvm.RomPath);
                }else if(mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NES)
                {
                    mvm.getBootIMGNES(mvm.RomPath);
                }
                else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.SNES)
                {
                    mvm.getBootIMGSNES(mvm.RomPath);
                }
                else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.MSX)
                {
                    mvm.getBootIMGMSX(mvm.RomPath);
                }
            }
            
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
            using (IconCreator ic = new IconCreator())
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
            if(mvm.GameConfiguration.Console == GameConsoles.NES || mvm.GameConfiguration.Console == GameConsoles.SNES)
            {
                pixp.IsChecked = mvm.pixelperfect;
            }
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
        }
        private bool CheckIfNull(string s)
        {
            if(s == null || s.Equals(string.Empty))
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
            TDRSHOW t = new TDRSHOW(drc.Text, true);
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
        {/*
            try
            {
                TitleKeys webbrowser = new TitleKeys("n64", "UWUVCI AIO - N64 Help");
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
                Custom_Message cm = new Custom_Message("Not Implemented", "The Helppage for N64 is not implemented yet");
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                cm.Show();
            }*/
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = "";
                if(mvm.GameConfiguration.Console.ToString().ToLower() == "nds")
                {
                    title = $"Nintendo DS Inject Guide";
                }
                else
                {
                    title = $"{mvm.GameConfiguration.Console.ToString()} Inject Guide";
                }
                TitleKeys webbrowser = new TitleKeys(mvm.GameConfiguration.Console.ToString().ToLower(),title);
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
                Custom_Message cm = new Custom_Message("Not Implemented", $"The Helppage for {mvm.GameConfiguration.Console.ToString()} is not implemented yet");
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if(pixp.IsChecked == true)
            {
                mvm.pixelperfect = true;
            }
            else
            {
                mvm.pixelperfect = false;
            }
        }

        private void gn_TextChanged(object sender, TextChangedEventArgs e)
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
