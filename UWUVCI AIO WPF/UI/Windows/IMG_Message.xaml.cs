using GameBaseClassLibrary;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;
using System.Runtime.InteropServices;
using PixelFormat = System.Windows.Media.PixelFormat;
using Pfim;
using System.Windows.Media;
using System.Collections.Generic;
using System.Drawing;
using Image = System.Windows.Controls.Image;

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
        private static List<GCHandle> handles = new List<GCHandle>();
        public IMG_Message(string icon, string tv, string repoid)
        {
            try
            {
                if (Owner != null)
                {
                    if (Owner?.GetType() != typeof(MainWindow))
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }

                }


            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();

            ic = icon;
            tvs = tv;
            if (ic.Contains("tga"))
            {
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


            this.icon.Source = GetRepoImages(icon);
            this.tv.Source = GetRepoImages(tv);
            this.repoid = repoid;
        }

        private BitmapImage GetRepoImages(string imageURL)
        {
            BitmapImage bitmap = new BitmapImage();
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(imageURL, UriKind.Absolute));
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                var webReponse = webRequest.GetResponse();
                var stream = webReponse.GetResponseStream();

                if (!imageURL.EndsWith(".tga"))
                {
                    var image = System.Drawing.Image.FromStream(stream);

                    using (var graphics = Graphics.FromImage(image))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    }

                    using (var memory = new MemoryStream())
                    {
                        image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;

                        bitmap.BeginInit();
                        bitmap.StreamSource = memory;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                }
                else
                {
                    //This code doesn't work
                    /* 
                    var image = Pfim.Pfim.FromStream(stream);
                    foreach (var im in WpfImage(image))
                    {
                        var encoder = new PngBitmapEncoder();
                        using (var memory = new MemoryStream())
                        {
                            BitmapSource bitmapSource = (BitmapSource)im.Source;
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                            encoder.Save(memory);
                            memory.Position = 0;

                            bitmap.BeginInit();
                            bitmap.StreamSource = memory;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }
                    }
                    */

                    //using old method for .tga
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageURL, UriKind.Absolute);
                    bitmap.EndInit();
                }
            }
            catch
            {
                //something broke, so yolo we go with the old method!
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageURL, UriKind.Absolute);
                bitmap.EndInit();
            }
            return bitmap;
        }

        private static PixelFormat PixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra32;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
            }
        }

        private static IEnumerable<Image> WpfImage(IImage image)
        {
            var pinnedArray = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            var addr = pinnedArray.AddrOfPinnedObject();
            var bsource = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0,
                PixelFormat(image), null, addr, image.DataLen, image.Stride);

            handles.Add(pinnedArray);
            yield return new Image
            {
                Source = bsource,
                Width = image.Width,
                Height = image.Height,
                MaxHeight = image.Height,
                MaxWidth = image.Width,
                Margin = new Thickness(4),
            };

            foreach (var mip in image.MipMaps)
            {
                var mipAddr = addr + mip.DataOffset;
                var mipSource = BitmapSource.Create(mip.Width, mip.Height, 96.0, 96.0,
                    PixelFormat(image), null, mipAddr, mip.DataLen, mip.Stride);
                yield return new Image
                {
                    Source = mipSource,
                    Width = mip.Width,
                    Height = mip.Height,
                    MaxHeight = mip.Height,
                    MaxWidth = mip.Width,
                    Margin = new Thickness(4)
                };
            }
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
            else if (mvm.GameConfiguration.Console == GameBaseClassLibrary.GameConsoles.WII)
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

            Close();
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
            string linkbase = "https://raw.githubusercontent.com/UWUVCI-PRIME/UWUVCI-IMAGES/master/";
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
            string linkbase = "https://raw.githubusercontent.com/UWUVCI-PRIME/UWUVCI-IMAGES/master/";
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
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
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