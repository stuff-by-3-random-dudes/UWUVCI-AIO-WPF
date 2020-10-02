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
using UWUVCI_AIO_WPF.Properties;
using Path = System.IO.Path;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für ImageCreator.xaml
    /// </summary>  
    
    public partial class LogoCreator : Window, IDisposable
    {
        private static readonly string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
        private static readonly string toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Tools");
        BootLogoImage bi = new BootLogoImage();
        Bitmap b;
        string console = "other";
        bool drc = false;
        private bool _disposed;

        public LogoCreator()
        {
            InitializeComponent();
            imageName.Content = "bootLogoTex";
            SetTemplate();
            t_Copy.Text = "18";
        }


        private void SetTemplate()
        {
            bi.Frame = new Bitmap(Properties.Resources.bootLogoTex);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;

            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path))
            {
                string copy = "";
                if (new FileInfo(path).Extension.Contains("tga"))
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
                        conv.StartInfo.Arguments = $"-i \"{path}\" -o \"{Path.Combine(tempPath, "image")}\"";

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
                    copy = path;
                }
                bi.Frame = new Bitmap(copy);
                b = bi.Create("", 20);
                Image.Source = BitmapToImageSource(b);
                //Finish_Click(null, null);
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
            b = bi.Create(t.Text, 20);
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

        

        
        
        void DrawImage()
        {
            if(!bi.Frame.Equals(new Bitmap(Properties.Resources.bootLogoTex)))
            {
                bi.Frame = new Bitmap(Properties.Resources.bootLogoTex);
            }
            int fontsize = 18;
            try
            {
                fontsize = Convert.ToInt32(t_Copy.Text);
                if(fontsize == 0)
                {
                    fontsize = 1;
                }
            }
            catch (Exception)
            {
                fontsize = 18;
            }
            b = bi.Create(t.Text, fontsize);
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

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PLDi_Click(object sender, RoutedEventArgs e)
        {

            DrawImage();
        }

        private void RLEn_Click(object sender, RoutedEventArgs e)
        {
           
            DrawImage();
        }

       

        private void t_TextChanged(object sender, TextChangedEventArgs e)
        {
            DrawImage();
        }

        private void t_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                
                if (Convert.ToInt32(t_Copy.Text) > 30)
                {
                    t_Copy.Text = "30";
                }
            }
            catch (Exception)
            {
                
            }
            
            DrawImage();
        }
    }
}
