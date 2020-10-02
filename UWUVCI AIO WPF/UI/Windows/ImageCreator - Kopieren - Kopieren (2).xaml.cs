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
using Path = System.IO.Path;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für ImageCreator.xaml
    /// </summary>  
    
    public partial class asd : Window, IDisposable
    {
        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
        MenuIconImage bi = new MenuIconImage();
        Bitmap b;
        string console = "other";
        bool drc = false;

        public asd()
        {
            InitializeComponent();
            imageName.Content = "iconTex";
            SetTemplate();
        }
        public asd(string name) 
        {
            InitializeComponent();
            console = name;
            imageName.Content = "iconTex";
            SetTemplate();
        }

        private void SetTemplate()
        {
            if(console == "WII")
            {
                bi.Frame = new Bitmap(Properties.Resources.WiiIcon);
            }
            else
            {
                bi.Frame = new Bitmap(Properties.Resources.Icon);
            }
         
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
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"bin\createdIMG"))
            {
                Directory.CreateDirectory(@"bin\createdIMG");
            }
            if(File.Exists(Path.Combine(@"bin\createdIMG", imageName.Content + ".png")))
            {
                File.Delete(Path.Combine(@"bin\createdIMG", imageName.Content + ".png"));
            }
            if (drc)
            {
                b = ResizeImage(b, 854, 480);

            }
            
                b.Save(Path.Combine(@"bin\createdIMG", imageName.Content + ".png"));
            
           
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

        

        private void enOv_Click(object sender, RoutedEventArgs e)
        {
            if(enOv.IsChecked == true)
            {
               
                b = bi.Create(console);
                Image.Source = BitmapToImageSource(b);
            }
            else
            {
                
                if(bi.TitleScreen != null)
                {
                    b = ResizeImage(bi.TitleScreen, 128, 128); Image.Source = BitmapToImageSource(b);
                }
                else
                {
                    b = new Bitmap(1280, 720);
                    using (Graphics gfx = Graphics.FromImage(b))
                    using (SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(0, 0, 0)))
                    {
                        gfx.FillRectangle(brush, 0, 0, 128, 128);
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

            DrawImage();
        }

        private void RLEn_Click(object sender, RoutedEventArgs e)
        {
           
            DrawImage();
        }
    }
}
