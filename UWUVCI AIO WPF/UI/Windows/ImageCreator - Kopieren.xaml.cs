using GameBaseClassLibrary;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UWUVCI_AIO_WPF.Classes;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für ImageCreator.xaml
    /// </summary>  

    public partial class IconCreator : Window, IDisposable
    {
        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
        MenuIconImage bi = new MenuIconImage();
        Bitmap b;
        string console = "other";
        string othercons = "";
        bool drc = false;
        private bool _disposed;

        public IconCreator()
        {
            setUpIconCreator();
            SetTemplate();
        }
        public IconCreator(string name)
        {
            setUpIconCreator();
            console = name;
            SetTemplate();
        }
        public IconCreator(string name, string extra)
        {
            setUpIconCreator();
            console = name;
            othercons = extra;
            SetTemplate();
        }

        private void setUpIconCreator()
        {
            InitializeComponent();
            imageName.Content = "iconTex";
        }
        private void SetTemplate()
        {
            if (othercons == "GB")
            {
                setUpBiFrame();
                wii.Content = "GB";
            }
            if (othercons == "GBC")
            {
                setUpBiFrame();
                wii.Content = "GBC";
            }
            if (console == "WII" && (FindResource("mvm") as MainViewModel).test != GameConsoles.GCN)
            {
                bi.Frame = new Bitmap(Properties.Resources.Wii2);
                ww.Visibility = Visibility.Visible;
                ws.Visibility = Visibility.Visible;
                hb.Visibility = Visibility.Visible;
                wii.Visibility = Visibility.Visible;
            }
            else if ((FindResource("mvm") as MainViewModel).test == GameConsoles.GCN)
            {
                setUpBiFrame();
                wii.Content = "GCN";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.NDS)
            {
                setUpBiFrame();
                wii.Content = "NDS";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.N64)
            {
                setUpBiFrame();
                wii.Content = "N64";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.GBA && othercons == "")
            {
                setUpBiFrame();
                wii.Content = "GBA";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.MSX)
            {
                setUpBiFrame();
                wii.Content = "MSX";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.SNES)
            {
                setUpBiFrame();
                wii.Content = "SNES";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.NES)
            {
                setUpBiFrame();
                wii.Content = "NES";
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.TG16)
            {
                setUpBiFrame();
                wii.Content = "TGX";
            }
            else
            {
                bi.Frame = new Bitmap(Properties.Resources.Icon);
            }

        }

        private void setUpBiFrame()
        {
            bi.Frame = new Bitmap(Properties.Resources.Icon);
            wii.IsChecked = true;
            ww.Content = "Alt 1";
            hb.Content = "Alt 2";
            ww.Visibility = Visibility.Visible;
            ws.Visibility = Visibility.Visible;
            hb.Visibility = Visibility.Visible;
            wii.Visibility = Visibility.Visible;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            string file = mvm.GetFilePath(false, false);
            if (!string.IsNullOrEmpty(file))
            {
                string copy = "";
                if (new FileInfo(file).Extension.Contains("tga"))
                {
                    using (Process conv = new Process())
                    {
                        conv.StartInfo.UseShellExecute = false;
                        conv.StartInfo.CreateNoWindow = true;
                        if (Directory.Exists(Path.Combine(tempPath, "image")))
                        {
                            Directory.Delete(Path.Combine(tempPath, "image"), true);
                        }
                        Directory.CreateDirectory(Path.Combine(tempPath, "image"));
                        conv.StartInfo.FileName = Path.Combine(toolsPath, "tga2png.exe");
                        conv.StartInfo.Arguments = $"-i \"{file}\" -o \"{Path.Combine(tempPath, "image")}\"";

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
                    copy = file;
                }
                try
                {
                    bi.TitleScreen = new Bitmap(copy);
                    b = bi.Create(console);
                    Image.Source = BitmapToImageSource(b);
                }
                catch
                {    
                    Custom_Message cm = new Custom_Message("Image Issue", "The image you're trying to use will not work, please try a different image.");
                    try
                    {
                        cm.Owner = mvm.mw;
                    }
                    catch (Exception)
                    {
                        //left empty on purpose
                    }
                    cm.ShowDialog();
                }
            }
            enOv_Click(null, null);
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"bin\createdIMG"))
            {
                Directory.CreateDirectory(@"bin\createdIMG");
            }
            if (File.Exists(Path.Combine(@"bin\createdIMG", imageName.Content + ".png")))
            {
                File.Delete(Path.Combine(@"bin\createdIMG", imageName.Content + ".png"));
            }
            if (drc)
            {
                b = ResizeImage(b, 854, 480);
            }

            b.Save(Path.Combine(@"bin\createdIMG", imageName.Content + ".png"));

            Close();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void wind_Loaded(object sender, RoutedEventArgs e)
        {
            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }



        private void enOv_Click(object sender, RoutedEventArgs e)
        {
            if (enOv.IsChecked == true)
            {
                b = bi.Create(console);
                Image.Source = BitmapToImageSource(b);
                ww.IsEnabled = true;
                wii.IsEnabled = true;
                hb.IsEnabled = true;
                ws.IsEnabled = true;
            }
            else
            {
                ww.IsEnabled = false;
                wii.IsEnabled = false;
                hb.IsEnabled = false;
                ws.IsEnabled = false;

                if (bi.TitleScreen != null)
                {
                    b = ResizeImage(bi.TitleScreen, 128, 128); Image.Source = BitmapToImageSource(b);
                }
                else
                {
                    b = new Bitmap(128, 128);
                    using (Graphics gfx = Graphics.FromImage(b))
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 0)))
                    {
                        gfx.FillRectangle(brush, 0, 0, 128, 128);
                    }
                    Image.Source = BitmapToImageSource(b);
                }
            }
        }
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        void DrawImage()
        {
            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void Players_TextChanged(object sender, TextChangedEventArgs e)
        {
            DrawImage();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                bi?.Dispose();
                b?.Dispose();
            }

            _disposed = true;
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combo.SelectedIndex == 0)
            {
                bi.Frame = Properties.Resources.SNES_PAL;
            }
            else if (combo.SelectedIndex == 1)
            {
                bi.Frame = Properties.Resources.SNES_USA;
            }
            else
            {
                bi.Frame = Properties.Resources.SFAM;
            }
            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void pal_Click(object sender, RoutedEventArgs e)
        {
            if (pal.IsChecked == true)
            {
                bi.Frame = Properties.Resources.SNES_PAL;
            }
            else
            {
                bi.Frame = Properties.Resources.SNES_USA;
            }
            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            bi.Frame = Properties.Resources.SFAM;
            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void PLDi_Click(object sender, RoutedEventArgs e)
        {
            DrawImage();
        }

        private void RLEn_Click(object sender, RoutedEventArgs e)
        {
            DrawImage();
        }

        private void ww_Click(object sender, RoutedEventArgs e)
        {
            if (othercons == "GB")
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GB_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GB_alt2);
                    console = "WII";
                }
            }
            else if (othercons == "GBC")
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GBC_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GBC_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.NDS)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.NDS_Alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.NDS_Alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.SNES)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.SNES_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.SNES_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.NES)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.NES_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.NES_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.N64)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.N64_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.N64_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.MSX)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.MSX_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.MSX_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.TG16)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.TGFX_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.TGFX_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).GameConfiguration.Console == GameConsoles.GBA)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GBA_alt1);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GBA_alt2);
                    console = "WII";
                }
            }
            else if ((FindResource("mvm") as MainViewModel).test != GameConsoles.GCN)
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.WiiIcon);
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Wii2);
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.HBICON);
                }
            }
            else
            {
                if (ww.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GCN_ICON2);
                    console = "WII";
                }
                else if (wii.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.Icon);
                    console = "other";
                }
                else if (hb.IsChecked == true)
                {
                    bi.Frame = new Bitmap(Properties.Resources.GCN_ICON3);
                    console = "WII";
                }
            }

            DrawImage();
        }
    }
}
