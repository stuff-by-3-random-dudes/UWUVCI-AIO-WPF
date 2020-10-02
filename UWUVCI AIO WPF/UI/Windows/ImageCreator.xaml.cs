using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
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
using Path = System.IO.Path;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für ImageCreator.xaml
    /// </summary>  

    public partial class ImageCreator : Window, IDisposable
    {
        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
        BootImage bi = new BootImage();
        Bitmap b;
        string console = "other";
        bool drc = false;
        private string backupcons;
        private bool _disposed;

        public ImageCreator(string name)
        {
            InitializeComponent();
            imageName.Content = name;
            
           
            if (name.ToLower().Contains("drc"))
            {
                drc = true;
                imageName_Copy.Content = "DRC IMAGE";
            }
        }
        public ImageCreator(GameConsoles console, string name) : this(name)
        {
            InitializeComponent();
            SetTemplate(console);
        }
        public ImageCreator(bool other, GameConsoles consoles, string name) : this(name)
        {
            InitializeComponent();
            Bitmap bit;
            if (consoles == GameConsoles.TG16)
            {
                bit = new Bitmap(other ? Properties.Resources.TGCD : Properties.Resources.TG16);
            }
            else
            {
                console = "GBC";
                bit = new Bitmap(other ? Properties.Resources.GBC : Properties.Resources.newgameboy);
            }
            bi.Frame = bit;
        }

        private void SetTemplate(GameConsoles console)
        {
            Bitmap bit;
            switch (console)
            {
                case GameConsoles.NDS:
                    bit = new Bitmap(Properties.Resources.NDS);

                    this.console = "NDS";
                    break;
                case GameConsoles.N64:
                    bit = new Bitmap(Properties.Resources.N64);

                    break;
                case GameConsoles.NES:
                    bit = new Bitmap(Properties.Resources.NES);

                    break;
                case GameConsoles.GBA:
                    bit = new Bitmap(Properties.Resources.GBA);
                    this.console = "GBA";
                    break;
                case GameConsoles.WII:
                    bit = new Bitmap(Properties.Resources.WII);
                    this.console = "WII";
                    GameName1.Visibility = Visibility.Hidden;
                    GameName2.Visibility = Visibility.Hidden;
                    ReleaseYearLabel.Visibility = Visibility.Hidden;
                    RLDi.Visibility = Visibility.Hidden;
                    RLEn.Visibility = Visibility.Hidden;
                    ReleaseYear.Visibility = Visibility.Hidden;

                    PLDi.Visibility = Visibility.Hidden;
                    PLEn.Visibility = Visibility.Hidden;
                    Players.Visibility = Visibility.Hidden;

                    PlayerLabel.Visibility = Visibility.Hidden;
                    snesonly.Visibility = Visibility.Visible;
                    pal.Content = "Wii";
                    sntsc.Content = "WiiWare";
                    sfc.Content = "Homebrew";
                    altwii.Visibility = Visibility.Visible;
                    backupcons = "WII";
                    break;
                case GameConsoles.GCN:
                    bit = new Bitmap(Properties.Resources.GCN);
                    break;
                case GameConsoles.MSX:
                    bit = new Bitmap(Properties.Resources.MSX);
                    break;
                case GameConsoles.SNES:
                    bit = new Bitmap(Properties.Resources.SNES_PAL);
                    snesonly.Visibility = Visibility.Visible;
                    List<string> styles = new List<string>();
                    styles.Add("SNES - PAL");
                    styles.Add("SNES - NTSC");
                    styles.Add("Super Famicom");
                    combo.ItemsSource = styles;
                    combo.SelectedIndex = 0;
                    break;
                default:
                    bit = null;
                    break;
            }
            bi.Frame = bit;
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
                bi.TitleScreen = new Bitmap(copy);
                b = bi.Create(console);
                Image.Source = BitmapToImageSource(b);
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

        void EnableOrDisbale(bool en)
        {
            GameName1.IsEnabled = en;
            GameName2.IsEnabled = en;
            ReleaseYearLabel.IsEnabled = en;
            RLDi.IsEnabled = en;
            RLEn.IsEnabled = en;
            ReleaseYear.IsEnabled = en;
            if (en && RLDi.IsChecked == true)
            {
                ReleaseYear.IsEnabled = false;
            }
            PLDi.IsEnabled = en;
            PLEn.IsEnabled = en;
            Players.IsEnabled = en;
            if (en && PLDi.IsChecked == true)
            {
                Players.IsEnabled = false;
            }
            PlayerLabel.IsEnabled = en;
            if (snesonly.IsVisible == true)
            {
                snesonly.IsEnabled = en;
            }
        }

        private void enOv_Click(object sender, RoutedEventArgs e)
        {
            if ((ovl.IsVisible == true && enOv.IsChecked == true))
            {
                EnableOrDisbale(true);
                b = bi.Create(console);
                Image.Source = BitmapToImageSource(b);
            }
            else
            {
                EnableOrDisbale(false);
                if (bi.TitleScreen != null)
                {
                    b = ResizeImage(bi.TitleScreen, 1280, 720); Image.Source = BitmapToImageSource(b);
                }
                else
                {
                    b = new Bitmap(1280, 720);
                    using (Graphics gfx = Graphics.FromImage(b))
                    using (SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(0, 0, 0)))
                    {
                        gfx.FillRectangle(brush, 0, 0, 1280, 720);
                    }
                    Image.Source = BitmapToImageSource(b);
                }
            }
        }
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
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
            bi.NameLine1 = GameName1.Text;
            bi.NameLine2 = GameName2.Text;

            bi.Longname = !string.IsNullOrWhiteSpace(GameName2.Text);

            bi.Players = (PLEn.IsChecked == true && !string.IsNullOrWhiteSpace(Players.Text)) ? Convert.ToInt32(Players.Text) : 0;

            bi.Released = (RLEn.IsChecked == true && !string.IsNullOrWhiteSpace(ReleaseYear.Text)) ? Convert.ToInt32(ReleaseYear.Text) : 0;

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
            if (console != "WII" && backupcons != "WII")
            {
                bi.Frame = pal.IsChecked == true ? Properties.Resources.SNES_PAL : Properties.Resources.SNES_USA;
            }
            else
            {
                console = "WII";
                switchs(Visibility.Hidden);

                bi.Frame = pal.IsChecked == true ? Properties.Resources.WII : Properties.Resources.WIIWARE;
            }

            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (console != "WII" && backupcons != "WII")
            {
                backupcons = console;
                bi.Frame = Properties.Resources.SFAM;
            }
            else
            {
                backupcons = "WII";
                if (altwii.IsChecked == false)
                {
                    console = "WII";
                    switchs(Visibility.Hidden);
                    bi.Frame = Properties.Resources.homebrew3;
                }
                else
                {
                    switchs(Visibility.Visible);
                    console = "other";
                    bi.Frame = Properties.Resources.wii3New;
                }

            }

            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void PLDi_Click(object sender, RoutedEventArgs e)
        {
            Players.IsEnabled = PLEn.IsChecked == true;

            DrawImage();
        }

        private void RLEn_Click(object sender, RoutedEventArgs e)
        {
            ReleaseYearLabel.IsEnabled = RLEn.IsChecked == true;

            DrawImage();
        }

        private void switchs(Visibility v)
        {
            GameName1.Visibility = v;
            GameName2.Visibility = v;

            ReleaseYear.Visibility = v;

            Players.Visibility = v;

            if (v == Visibility.Hidden)
            {
                bi.NameLine1 = "";
                bi.NameLine2 = "";
                bi.Released = 0;
                bi.Players = 0;
            }
            else
            {
                bi.NameLine1 = GameName1.Text;
                bi.NameLine2 = GameName2.Text;
                if (!string.IsNullOrEmpty(ReleaseYear.Text))
                {
                    bi.Released = Convert.ToInt32(ReleaseYear.Text);
                }
                if (!string.IsNullOrEmpty(Players.Text))
                {
                    bi.Players = Convert.ToInt32(Players.Text);
                }

            }

        }
    }
}
