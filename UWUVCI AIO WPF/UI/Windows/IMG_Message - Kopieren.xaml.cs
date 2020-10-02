using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations;
using Path = System.IO.Path;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für IMG_Message.xaml
    /// </summary>
    public partial class TDRSHOW : Window, IDisposable
    {
        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
        string copy = "";
        string pat = "";
        bool drc = false;
        BitmapImage bitmap = new BitmapImage();
        public TDRSHOW(string path, bool drc)
        {
            this.drc = drc;
            try
            {
                if (this.Owner?.GetType() != typeof(MainWindow))
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            pat = String.Copy(path);
            InitializeComponent();
            if (Directory.Exists(Path.Combine(tempPath, "image"))) Directory.Delete(Path.Combine(tempPath, "image"),true);
                Directory.CreateDirectory(Path.Combine(tempPath, "image"));
            if (pat == "Added via Config")
            {
                string ext = "";
                byte[] imageb = new byte[] { };
                if (drc)
                {
                    ext = (FindResource("mvm") as MainViewModel).GameConfiguration.TGADrc.extension;
                    imageb = (FindResource("mvm") as MainViewModel).GameConfiguration.TGADrc.ImgBin;
                    File.WriteAllBytes(Path.Combine(tempPath, "image", "drc." + ext), imageb);
                    pat = Path.Combine(tempPath, "image", "drc." + ext);
                }
                else
                {
                    ext = (FindResource("mvm") as MainViewModel).GameConfiguration.TGATv.extension;
                    imageb = (FindResource("mvm") as MainViewModel).GameConfiguration.TGATv.ImgBin;
                    File.WriteAllBytes(Path.Combine(tempPath, "image", "tv." + ext), imageb);
                    pat = Path.Combine(tempPath, "image", "tv." + ext);
                }
                
            }
            if (new FileInfo(pat).Extension.Contains("tga"))
            {
                using (Process conv = new Process())
                {

                    conv.StartInfo.UseShellExecute = false;
                    conv.StartInfo.CreateNoWindow = true;


                    conv.StartInfo.FileName = Path.Combine(toolsPath, "tga2png.exe");
                    conv.StartInfo.Arguments = $"-i \"{pat}\" -o \"{Path.Combine(tempPath, "image")}\"";

                    conv.Start();
                    conv.WaitForExit();

                    foreach (string sFile in Directory.GetFiles(Path.Combine(tempPath, "image"), "*.png"))
                    {
                        copy = sFile;
                    }
                }
            }
            else
            {
                copy = pat;
            }




            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(copy, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            img.Source = image;

            if (path == "Added via Config")
            {
                File.Delete(pat);
            }
        }

        public void Dispose()
        {
           
        }

        private void Canc_Click(object sender, RoutedEventArgs e)
        {
            bitmap.UriSource = null;
            this.Close();

        }

        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = true;
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int a = 1;
            if (drc) a = 2;
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            switch (mvm.GameConfiguration.Console)
            {
                case GameBaseClassLibrary.GameConsoles.NDS:
                case GameBaseClassLibrary.GameConsoles.NES:
                case GameBaseClassLibrary.GameConsoles.SNES:
                case GameBaseClassLibrary.GameConsoles.MSX:
                    (mvm.Thing as OtherConfigs).clearImages(a);
                    break;
                case GameBaseClassLibrary.GameConsoles.GBA:
                    (mvm.Thing as GBA).clearImages(a);
                    break;
                case GameBaseClassLibrary.GameConsoles.WII:
                    if (mvm.test == GameBaseClassLibrary.GameConsoles.GCN)
                    {
                        (mvm.Thing as GCConfig).clearImages(a);
                    }
                    else
                    {
                        (mvm.Thing as WiiConfig).clearImages(a);
                    }
                    break;
                case GameBaseClassLibrary.GameConsoles.N64:
                    (mvm.Thing as N64Config).clearImages(a);
                    break;
                case GameBaseClassLibrary.GameConsoles.TG16:
                    (mvm.Thing as TurboGrafX).clearImages(a);
                    break;
            }
            this.Close();
        }
    }
}
