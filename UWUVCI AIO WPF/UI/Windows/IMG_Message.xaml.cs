using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations;
using Path = System.IO.Path;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für IMG_Message.xaml
    /// </summary>
    public partial class IMG_Message : Window
    {
        string ic = "";
        string tvs = "";
        string repoid = "";
        string icback = "";
        string tvback = "";
        public IMG_Message(string icon, string tv, string repoid)
        {
            try
            {
                if(this.Owner != null)
                {
                    if (this.Owner?.GetType() != typeof(MainWindow))
                    {
                        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                    
                }
                
               
            }
            catch (Exception )
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();
            
                ic = icon;
                tvs = tv;
            if (ic.Contains("tga")){
                icb.Visibility = Visibility.Hidden;
                icl.Visibility = Visibility.Hidden;
                //tgic.Visibility = Visibility.Visible;
            }
            if (tvs.Contains("tga"))
            {
                tvb.Visibility = Visibility.Hidden;
                tvl.Visibility = Visibility.Hidden;
                tgtv.Visibility = Visibility.Visible;
            }
            BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(icon, UriKind.Absolute);
                bitmap.EndInit();

                this.icon.Source = bitmap;

                BitmapImage bmp2 = new BitmapImage();
                bmp2.BeginInit();
                bmp2.UriSource = new Uri(tv, UriKind.Absolute);
                bmp2.EndInit();
                this.tv.Source = bmp2;
            this.repoid = repoid;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Canc_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            var client = new WebClient();
            if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo")))
            {
                Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo"), true);
            }
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo"));
            client.DownloadFile(ic, Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"));
            mvm.GameConfiguration.TGAIco.ImgPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}");
            mvm.GameConfiguration.TGAIco.extension = $".{ic.Split('.')[3]}";
            if (mvm.test == GameBaseClassLibrary.GameConsoles.GCN)
            {
                (mvm.Thing as GCConfig).icoIMG.Visibility = Visibility.Visible;
            }
            else if(mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.WII)
            {
                (mvm.Thing as WiiConfig).icoIMG.Visibility = Visibility.Visible;
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.N64)
            {
                (mvm.Thing as N64Config).icoIMG.Visibility = Visibility.Visible;
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.GBA)
            {
                (mvm.Thing as GBA).icoIMG.Visibility = Visibility.Visible;
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.TG16)
            {
                (mvm.Thing as TurboGrafX).icoIMG.Visibility = Visibility.Visible;
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NDS || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.SNES || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NES || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.MSX)
            {
                (mvm.Thing as OtherConfigs).icoIMG.Visibility = Visibility.Visible;
            }
            client.DownloadFile(tvs, Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));
            mvm.GameConfiguration.TGATv.ImgPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}");
            mvm.GameConfiguration.TGATv.extension = $".{tvs.Split('.')[3]}";
            if (mvm.test == GameBaseClassLibrary.GameConsoles.GCN)
            {
                (mvm.Thing as GCConfig).tvIMG.Visibility = Visibility.Visible;
                (mvm.Thing as GCConfig).imgpath(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"), Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));

            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.WII)
            {
                (mvm.Thing as WiiConfig).tvIMG.Visibility = Visibility.Visible;
                (mvm.Thing as WiiConfig).imgpath(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"), Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));

            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.N64)
            {
                (mvm.Thing as N64Config).tvIMG.Visibility = Visibility.Visible;
                (mvm.Thing as N64Config).imgpath(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"), Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));
            
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.GBA)
            {
                (mvm.Thing as GBA).tvIMG.Visibility = Visibility.Visible;
                (mvm.Thing as GBA).imgpath(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"), Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.TG16)
            {
                (mvm.Thing as TurboGrafX).tvIMG.Visibility = Visibility.Visible;
                (mvm.Thing as TurboGrafX).imgpath(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"), Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));
            }
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NDS || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.SNES || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.NES || mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.MSX)
            {
                (mvm.Thing as OtherConfigs).tvIMG.Visibility = Visibility.Visible;
                (mvm.Thing as OtherConfigs).imgpath(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"iconTex.{ic.Split('.')[3]}"), Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootTvTex.{tvs.Split('.')[3]}"));

            }
           /* if(mvm.test == GameConsoles.GCN)
            {
                checkForAdditionalFiles(GameConsoles.GCN);
            }
            else
            {
                checkForAdditionalFiles(mvm.GameConfiguration.Console);
            }*/
            
            this.Close();
        }
        private void checkForAdditionalFiles(GameConsoles console)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo"));
            }
            bool ini = false;
            bool btsnd = false;
            string inip = "";
            string btsndp = "";
            string exten = "";
            string linkbase = "https://raw.githubusercontent.com/Flumpster/UWUVCI-Images/master/";
            if (console == GameConsoles.N64)
            {
                if (RemoteFileExists(linkbase+repoid+"/game.ini"))
                {
                    ini = true;
                    inip = linkbase + repoid + "/game.ini";
                }
            }
            string[] ext = { "wav", "mp3", "btsnd" };
            foreach(var e in ext)
            {
                if (RemoteFileExists(linkbase + repoid + "/BootSound." + e))
                {
                    btsnd = true;
                    btsndp = linkbase + repoid + "/BootSound." + e;
                    exten = e;
                    break;
                }
            }
           if(ini || btsnd)
            {
                string extra = "There are more additional files found. Do you want to download those?";
                if (ini && !btsnd) { extra = "There is an additional INI file available for Dowload. Do you want to dowload it?"; }
                if (!ini && btsnd) { extra = "There is an additional BootSound file available for Dowload. Do you want to dowload it?"; }
                if (ini && btsnd) { extra = "There is an adittional INI and BootSound file available for Dowload. Do you want to download those?"; }
                MainViewModel mvm = FindResource("mvm") as MainViewModel;
                Custom_Message cm = new Custom_Message("Found additional Files",extra);
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                cm.ShowDialog();
                if (mvm.addi)
                {
                    var client = new WebClient();
                    if (ini)
                    {
                        client.DownloadFile(inip, Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", "game.ini"));
                        (mvm.Thing as N64Config).ini.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", "game.ini");
                        mvm.GameConfiguration.N64Stuff.INIPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", "game.ini");
                    }
                    if (btsnd)
                    {
                        client.DownloadFile(btsndp, Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}"));
                        mvm.BootSound = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                        switch (console)
                        {
                            case GameConsoles.NDS:
                                (mvm.Thing as OtherConfigs).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                break;
                            case GameConsoles.GBA:
                                (mvm.Thing as GBA).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                break;
                            case GameConsoles.N64:
                                (mvm.Thing as N64Config).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                break;
                            case GameConsoles.WII:
                                if(mvm.test == GameConsoles.GCN)
                                {
                                    (mvm.Thing as GCConfig).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                }
                                else
                                {
                                    (mvm.Thing as WiiConfig).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                }
                                break;
                                
                        }
                    }
                    mvm.addi = false;
                }
            }
        }
        public void checkForAdditionalFiles(GameConsoles console, string repoid)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo"));
            }
            bool ini = false;
            bool btsnd = false;
            string inip = "";
            string btsndp = "";
            string exten = "";
            string linkbase = "https://raw.githubusercontent.com/Flumpster/UWUVCI-Images/master/";
            if (console == GameConsoles.N64)
            {
                if (RemoteFileExists(linkbase + repoid + "/game.ini"))
                {
                    ini = true;
                    inip = linkbase + repoid + "/game.ini";
                }
            }
            string[] ext = { "wav", "mp3", "btsnd" };
            foreach (var e in ext)
            {
                if (RemoteFileExists(linkbase + repoid + "/BootSound." + e))
                {
                    btsnd = true;
                    btsndp = linkbase + repoid + "/BootSound." + e;
                    exten = e;
                    break;
                }
            }
            if (ini || btsnd)
            {
                string extra = "There are more additional files found. Do you want to download those?";
                if (ini && !btsnd) { extra = "There is an additional INI file available for Dowload. Do you want to dowload it?"; }
                if (!ini && btsnd) { extra = "There is an additional BootSound file available for Dowload. Do you want to dowload it?"; }
                if (ini && btsnd) { extra = "There is an adittional INI and BootSound file available for Dowload. Do you want to download those?"; }
                MainViewModel mvm = FindResource("mvm") as MainViewModel;
                Custom_Message cm = new Custom_Message("Found additional Files", extra);
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception)
                {

                }
                cm.ShowDialog();
                if (mvm.addi)
                {
                    var client = new WebClient();
                    if (ini)
                    {
                        client.DownloadFile(inip, Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", "game.ini"));
                        (mvm.Thing as N64Config).ini.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", "game.ini");
                        mvm.GameConfiguration.N64Stuff.INIPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", "game.ini");
                    }
                    if (btsnd)
                    {
                        client.DownloadFile(btsndp, Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}"));
                        mvm.BootSound = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                        switch (console)
                        {
                            case GameConsoles.NDS:
                                (mvm.Thing as OtherConfigs).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                break;
                            case GameConsoles.GBA:
                                (mvm.Thing as GBA).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                break;
                            case GameConsoles.N64:
                                (mvm.Thing as N64Config).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                break;
                            case GameConsoles.WII:
                                if (mvm.test == GameConsoles.GCN)
                                {
                                    (mvm.Thing as GCConfig).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                }
                                else
                                {
                                    (mvm.Thing as WiiConfig).sound.Text = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo", $"bootSound.{exten}");
                                }
                                break;

                        }
                    }
                    mvm.addi = false;
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
        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = true;
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = false;
        }
    }
}
