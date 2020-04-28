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
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.UI.Windows;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations
{
    /// <summary>
    /// Interaktionslogik für N64Config.xaml
    /// </summary>
    public partial class N64Config : Page
    {
        MainViewModel mvm;
        public N64Config()
        {
            mvm = (MainViewModel)FindResource("mvm");
            mvm.GameConfiguration.N64Stuff = new N64Conf();
            InitializeComponent();
            mvm.setThing(this);
            Injection.ToolTip = "Changing the extension of a ROM may result in a faulty inject.\nWe will not give any support in such cases";

        }
        public N64Config(GameConfig c)
        {
            mvm = (MainViewModel)FindResource("mvm");
            mvm.GameConfiguration = c.Clone(); getInfoFromConfig();
            InitializeComponent();
            mvm.setThing(this);
            Injection.ToolTip = "Changing the extension of a ROM may result in a faulty inject.\nWe will not give any support in such cases";

        }
        public void imgpath(string icon, string tv)
        {
            ic.Text = icon;
            this.tv.Text = tv;
        }

        private void Set_Rom_Path(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(true, false);
            if (!CheckIfNull(path))
            {
                mvm.RomSet = true;
                mvm.RomPath = path;
                if (mvm.BaseDownloaded)
                {
                    mvm.CanInject = true;

                }
                mvm.getBootIMGN64(mvm.RomPath);
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
            if (File.Exists(ini.Text))
            {
                mvm.GameConfiguration.N64Stuff.INIPath = ini.Text;
            }
            else if (!ini.Text.Equals("Added via Config") && !ini.Text.Equals("Downloaded from Cucholix Repo"))
            {
                mvm.GameConfiguration.TGADrc.ImgPath = null;
            }
            mvm.Inject(false);

        }
        public void getInfoFromConfig()
        {
            if (rp != null) rp.Text = "";
            mvm.RomPath = "";
            mvm.RomSet = false;
            mvm.gc2rom = "";
            if (tv != null)
            {
                tv.Text = mvm.GameConfiguration.TGATv.ImgPath;
                if (tv.Text.Length > 0)
                {
                    tvIMG.Visibility = Visibility.Visible;
                }
            }
            if (ic != null)
            {
                ic.Text = mvm.GameConfiguration.TGAIco.ImgPath;
                if (ic.Text.Length > 0)
                {
                    icoIMG.Visibility = Visibility.Visible;
                }
            }
            if (drc != null)
            {
                drc.Text = mvm.GameConfiguration.TGADrc.ImgPath;
                if (drc.Text.Length > 0)
                {
                    drcIMG.Visibility = Visibility.Visible;
                }
            }
            if (log != null)
            {
                log.Text = mvm.GameConfiguration.TGALog.ImgPath;
                if (log.Text.Length > 0)
                {
                    logIMG.Visibility = Visibility.Visible;
                }
            }
            if (gn != null) gn.Text = mvm.GameConfiguration.GameName;
            if (ini != null) ini.Text = mvm.GameConfiguration.N64Stuff.INIPath;

        }
        private void Set_TvTex(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.dont)
            {
                mvm.ImageWarning();
            }
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.TGATv.ImgPath = path;
                mvm.GameConfiguration.TGATv.extension = new FileInfo(path).Extension;
                tv.Text = path;
                tvIMG.Visibility = Visibility.Visible;
            }

        }

        private void Set_DrcTex(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.dont)
            {
                mvm.ImageWarning();
            }
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.TGADrc.ImgPath = path;
                mvm.GameConfiguration.TGADrc.extension = new FileInfo(path).Extension;
                drc.Text = path;
                drcIMG.Visibility = Visibility.Visible;
            }

        }

        private void Set_IconTex(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.dont)
            {
                mvm.ImageWarning();
            }
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.TGAIco.ImgPath = path;
                mvm.GameConfiguration.TGAIco.extension = new FileInfo(path).Extension;
                ic.Text = path;
                icoIMG.Visibility = Visibility.Visible;
            }
        }

        private void Set_LogoTex(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.dont)
            {
                mvm.ImageWarning();
            }
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.TGALog.ImgPath = path;
                mvm.GameConfiguration.TGALog.extension = new FileInfo(path).Extension;
                log.Text = path;
                logIMG.Visibility = Visibility.Visible;
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

        private void Set_IniPath(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(false, true);
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.N64Stuff.INIPath = path;
                ini.Text = path;
            }
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

        private void rbRDF_Click(object sender, RoutedEventArgs e)
        {
            mvm.GameConfiguration.N64Stuff.DarkFilter = true;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            mvm.GameConfiguration.N64Stuff.DarkFilter = false;
        }
        public void reset()
        {
            ini.Text = "";
            tv.Text = "";
            drc.Text = "";
            gn.Text = "";
            ic.Text = "";
            log.Text = "";
        }
        private void icoIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new ICOSHOW(ic.Text).ShowDialog();
        }

        private void tvIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new TDRSHOW(tv.Text, false).ShowDialog();
        }

        private void drcIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new TDRSHOW(drc.Text, true).ShowDialog();
        }

        private void logIMG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new LOGSHOW(log.Text).ShowDialog();
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
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string url = mvm.GetURL("n64");
                if (url == null || url == "") throw new Exception();
                TitleKeys webbrowser = new TitleKeys(url, "UWUVCI AIO - N64 Help");
                try
                {
                    webbrowser.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                webbrowser.ShowDialog();
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
            }
        }
    }
}
