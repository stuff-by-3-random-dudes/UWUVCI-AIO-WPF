using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UWUVCI_AIO_WPF.Classes;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für ImageCreator.xaml
    /// </summary>  
    
    public partial class ImageCreator : Window, IDisposable
    {
        private static readonly string tempPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
        private static readonly string toolsPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
        BootImage bi = new BootImage();
        Bitmap b;
        string console = "other";
        bool drc = false;

        public ImageCreator(string name)
        {
            InitializeComponent();
            imageName.Content = name;
            if (name.ToLower().Contains("drc"))
            {
                drc = true;
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
            if(consoles == GameConsoles.TG16)
            {
                if (other)
                {
                    bit = new Bitmap(Properties.Resources.TGCD);
                }
                else
                {
                    bit = new Bitmap(Properties.Resources.TG16);
                }
            }
            else
            {
                this.console = "GBC";
                if (other)
                {
                    bit = new Bitmap(Properties.Resources.GBC);
                }
                else
                {
                    bit = new Bitmap(Properties.Resources.newgameboy);

                }
            }
            bi.Frame = bit;
        }

        private void SetTemplate(GameConsoles console)
        {
            Bitmap bit;
            switch (console){
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
            this.Close();
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            string file = "";
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
           file = mvm.GetFilePath(false, false);
            if(!string.IsNullOrEmpty(file))
            {
      
                string copy = "";
                if (new FileInfo(file).Extension.Contains("tga"))
                {
                    using (Process conv = new Process())
                    {

                        conv.StartInfo.UseShellExecute = false;
                        conv.StartInfo.CreateNoWindow = true;
                        if (Directory.Exists(System.IO.Path.Combine(tempPath, "image")))
                        {
                            Directory.Delete(System.IO.Path.Combine(tempPath, "image"), true);
                        }
                        Directory.CreateDirectory(System.IO.Path.Combine(tempPath, "image"));
                        conv.StartInfo.FileName = System.IO.Path.Combine(toolsPath, "tga2png.exe");
                        conv.StartInfo.Arguments = $"-i \"{file}\" -o \"{System.IO.Path.Combine(tempPath, "image")}\"";

                        conv.Start();
                        conv.WaitForExit();

                        foreach (string sFile in Directory.GetFiles(System.IO.Path.Combine(tempPath, "image"), "*.png"))
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
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"bin\createdIMG"))
            {
                Directory.CreateDirectory(@"bin\createdIMG");
            }
            if(File.Exists(System.IO.Path.Combine(@"bin\createdIMG", imageName.Content + ".png")))
            {
                File.Delete(System.IO.Path.Combine(@"bin\createdIMG", imageName.Content + ".png"));
            }
            if (drc)
            {
                b = ResizeImage(b, 854, 480);

            }
            
                b.Save(System.IO.Path.Combine(@"bin\createdIMG", imageName.Content + ".png"));
            
           
            this.Close();
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

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
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
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
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
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
            if(en && RLDi.IsChecked == true)
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
            if(snesonly.IsVisible == true)
            {
                snesonly.IsEnabled = en;
            }
        }

        private void enOv_Click(object sender, RoutedEventArgs e)
        {
            if((ovl.IsVisible == true && enOv.IsChecked == true))
            {
                EnableOrDisbale(true);
                b = bi.Create(console);
                Image.Source = BitmapToImageSource(b);
            }
            else
            {
                EnableOrDisbale(false);
                if(bi.TitleScreen != null)
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
            if (!string.IsNullOrWhiteSpace(GameName2.Text))
            {
                bi.Longname = true;
            }
            else
            {
                bi.Longname = false;
            }
    
            if(PLEn.IsChecked == true && !String.IsNullOrWhiteSpace(Players.Text))
            {
                bi.Players = Convert.ToInt32(Players.Text);
            }
            else
            {
                bi.Players = 0;
            }
            if (RLEn.IsChecked == true && !String.IsNullOrWhiteSpace(ReleaseYear.Text))
            {
                bi.Released = Convert.ToInt32(ReleaseYear.Text);
            }
            else
            {
                bi.Released = 0;
            }

            b = bi.Create(console);
            Image.Source = BitmapToImageSource(b);
        }

        private void Players_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }

        private void Players_TextChanged(object sender, TextChangedEventArgs e)
        {
            DrawImage();
        }

        public void Dispose()
        {
         
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combo.SelectedIndex == 0)
            {
                bi.Frame = Properties.Resources.SNES_PAL;

            }
            else if(combo.SelectedIndex == 1)
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
            if(pal.IsChecked == true)
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
            if (PLEn.IsChecked != true)
            {
                Players.IsEnabled = false;
            }
            else
            {
                Players.IsEnabled = true;
            }
            DrawImage();
        }

        private void RLEn_Click(object sender, RoutedEventArgs e)
        {
            if (RLEn.IsChecked != true)
            {
                ReleaseYear.IsEnabled = false;
            }
            else
            {
                ReleaseYear.IsEnabled = true;
            }
            DrawImage();
        }
    }
}
