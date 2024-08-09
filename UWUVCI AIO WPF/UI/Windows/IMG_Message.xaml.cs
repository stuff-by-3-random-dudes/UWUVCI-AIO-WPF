using GameBaseClassLibrary;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class IMG_Message : Window
    {
        private string iconPath = "";
        private string tvPath = "";
        private string repositoryId = "";
        private static List<GCHandle> handles = new List<GCHandle>();

        public IMG_Message(string icon, string tv, string repoId)
        {
            InitializeComponent();

            iconPath = icon;
            tvPath = tv;
            repositoryId = repoId;

            SetWindowStartupLocation();
            SetupImageControls(icon, tv);
            LoadImages();
        }

        private void SetWindowStartupLocation()
        {
            try
            {
                if (Owner != null && Owner.GetType() != typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void SetupImageControls(string icon, string tv)
        {
            if (icon.Contains("tga"))
            {
                icb.Visibility = Visibility.Hidden;
                icl.Visibility = Visibility.Hidden;
                //tgic.Visibility = Visibility.Visible;
            }
            if (tv.Contains("tga"))
            {
                tvb.Visibility = Visibility.Hidden;
                tvl.Visibility = Visibility.Hidden;
                tgtv.Visibility = Visibility.Visible;
            }
        }

        private void LoadImages()
        {
            icon.Source = LoadImageFromUrl(iconPath);
            tv.Source = LoadImageFromUrl(tvPath);
        }

        private BitmapImage LoadImageFromUrl(string imageUrl)
        {
            var bitmap = new BitmapImage();

            try
            {
                // Create the web request (not wrapped in using, as it doesn't implement IDisposable)
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                // Wrap only the response in a using statement
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                using (Stream stream = webResponse.GetResponseStream())
                {
                    if (!imageUrl.EndsWith(".tga"))
                    {
                        var image = System.Drawing.Image.FromStream(stream);
                        bitmap = ConvertToBitmapImage(image);
                    }
                    else
                    {
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                        bitmap.EndInit();
                    }
                }
            }
            catch
            {
                // Fallback to loading image directly if an error occurs
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.EndInit();
            }

            return bitmap;
        }


        private BitmapImage ConvertToBitmapImage(System.Drawing.Image image)
        {
            var bitmap = new BitmapImage();

            using (var memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                bitmap.BeginInit();
                bitmap.StreamSource = memory;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Canc_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            DownloadFiles(mvm, iconPath, tvPath);
            Close();
        }

        private void DownloadFiles(MainViewModel mvm, string iconUrl, string tvUrl)
        {
            var client = new WebClient();
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "repo");

            if (Directory.Exists(repoPath))
            {
                Directory.Delete(repoPath, true);
            }
            Directory.CreateDirectory(repoPath);

            string iconExtension = Path.GetExtension(iconUrl);
            string tvExtension = Path.GetExtension(tvUrl);

            string iconDestination = Path.Combine(repoPath, $"iconTex{iconExtension}");
            string tvDestination = Path.Combine(repoPath, $"bootTvTex{tvExtension}");

            client.DownloadFile(iconUrl, iconDestination);
            client.DownloadFile(tvUrl, tvDestination);

            SetGameConfigurationPaths(mvm, iconDestination, tvDestination);
        }

        private void SetGameConfigurationPaths(MainViewModel mvm, string iconPath, string tvPath)
        {
            mvm.GameConfiguration.TGAIco.ImgPath = iconPath;
            mvm.GameConfiguration.TGAIco.extension = Path.GetExtension(iconPath);
            mvm.GameConfiguration.TGATv.ImgPath = tvPath;
            mvm.GameConfiguration.TGATv.extension = Path.GetExtension(tvPath);

            UpdateConsoleVisibility(mvm);
        }

        private void UpdateConsoleVisibility(MainViewModel mvm)
        {
            var console = mvm.GameConfiguration.Console;

            if (mvm.test == GameConsoles.GCN)
            {
                (mvm.Thing as GCConfig).icoIMG.Visibility = Visibility.Visible;
                (mvm.Thing as GCConfig).tvIMG.Visibility = Visibility.Visible;
            }
            else if (console == GameConsoles.WII)
            {
                (mvm.Thing as WiiConfig).icoIMG.Visibility = Visibility.Visible;
                (mvm.Thing as WiiConfig).tvIMG.Visibility = Visibility.Visible;
            }
            else if (console == GameConsoles.N64)
            {
                (mvm.Thing as N64Config).icoIMG.Visibility = Visibility.Visible;
                (mvm.Thing as N64Config).tvIMG.Visibility = Visibility.Visible;
            }
            else if (console == GameConsoles.GBA)
            {
                (mvm.Thing as GBA).icoIMG.Visibility = Visibility.Visible;
                (mvm.Thing as GBA).tvIMG.Visibility = Visibility.Visible;
            }
            else if (console == GameConsoles.TG16)
            {
                (mvm.Thing as TurboGrafX).icoIMG.Visibility = Visibility.Visible;
                (mvm.Thing as TurboGrafX).tvIMG.Visibility = Visibility.Visible;
            }
            else if (console == GameConsoles.NDS ||
                     console == GameConsoles.SNES ||
                     console == GameConsoles.NES ||
                     console == GameConsoles.MSX)
            {
                (mvm.Thing as OtherConfigs).icoIMG.Visibility = Visibility.Visible;
                (mvm.Thing as OtherConfigs).tvIMG.Visibility = Visibility.Visible;
            }
        }

        private bool RemoteFileExists(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
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
