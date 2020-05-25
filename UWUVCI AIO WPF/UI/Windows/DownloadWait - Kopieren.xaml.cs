using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
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
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für DownloadWait.xaml
    /// </summary>
   
    partial class SDSetup : Window, IDisposable
    {
        bool gc = false;
        string path = "";
        ManagementEventWatcher watcher = new ManagementEventWatcher();
        ManagementEventWatcher watcher2 = new ManagementEventWatcher();
        string driveletter = "";
        public SDSetup()
        {
            gc = true;
            path = @"F:\stuffby3randomdudes\UWUVCI AIO WPF\UWUVCI AIO WPF\bin\Debug\InjectedGames\[WUP]AnimalCrossing_10";
            InitializeComponent();
            Task.Run(() => checkfornewinput());
            Task.Run(() => checkfornewoutput());
            GetDrives();
            
        }
        public void SpecifyDrive()
        {
            if (sd.SelectedValue != null)
            {
                driveletter = sd.SelectedValue.ToString().Substring(0, 3);
            }
            else
            {
                driveletter = "";
               
            }
        }
        public SDSetup(bool gamecube, string injectlocation)
        {
            InitializeComponent();
            gc = gamecube;
             path = injectlocation;
          
            
            Task.Run(() => checkfornewinput());
            Task.Run(() => checkfornewoutput());
            
            GetDrives();
        }
        private void checkfornewinput()
        {
           
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher.Query = query;
            watcher.Start();
            watcher.WaitForNextEvent();
        }
        private void checkfornewoutput()
        {
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3");
            watcher2.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher2.Query = query;
            watcher2.Start();
            watcher2.WaitForNextEvent();
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {

            this.Dispatcher.Invoke(() => { GetDrives(); });


        }

        private void GetDrives()
        {
            sd.ItemsSource = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Removable).Select(d => d.Name + " " + d.VolumeLabel + "").ToList();
        }


        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
           
        }
        private void min_MouseLeave(object sender, MouseEventArgs e)
        {
         
        }
        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
       
        public void changeOwner(MainWindow ow)
        {
            this.Owner = ow;
            try
            {
                if (this.Owner.GetType() == typeof(MainWindow))
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    this.ShowInTaskbar = false;
                }
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
            
            
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch (Exception)
            {

            }
        }
        private async void Window_Close(object sender, RoutedEventArgs e)
        {
            /*Task t = new Task(() => watcher.Stop());
            t.Start();
            t.Wait();
            t = new Task(() => watcher2.Stop());
            t.Start();
            t.Wait();*/

            watcher.Stop();
            watcher2.Stop();
            
            this.Close();
        }
        private void close_MouseLeave(object sender, MouseEventArgs e)
        {
            close.Background = new SolidColorBrush(Color.FromArgb(0, 250, 250, 250));
        }

        private void close_MouseEnter(object sender, MouseEventArgs e)
        {
            close.Background = new SolidColorBrush(Color.FromArgb(150, 255, 100, 100));
        }
        private void wind_Closed(object sender, EventArgs e)
        {
            try
            {
                if ((FindResource("mvm") as MainViewModel).mw != null)
                {
                    (FindResource("mvm") as MainViewModel).mw.Topmost = false;
                }
            }
            catch (Exception s)
            {

            }
        }

        private void setup_Click(object sender, RoutedEventArgs e)
        {
            if (gc)
            {
                SetupNintendont();
            }
            CopyInject();
            status.Content = "Done with Setup!";
        }

        private void SetupNintendont()
        {
            status.Content = "Downloading Nintendon't...";
            //https://dl.dropboxusercontent.com/cd/0/get/A4ZX24rC4DlmPAYDrj7mz9WIlmmnIu9YKh1IabTXG-l0Ftq4Ib-fyG8QYspNdv6lCOcu920AbyIdo8l2dNtoW2xydD3ycH2_B06pxATB-ROj27wDdJ0nls9gtSYmjSxMmq4/file?_download_id=8358710324855012068395852250129613778887389747613992530573453088&_notify_domain=www.dropbox.com&dl=1
            if (Directory.Exists(@"bin\tempsd"))
            {
                Directory.Delete(@"bin\tempsd", true);
            }
            Directory.CreateDirectory(@"bin\tempsd");
            var client = new WebClient();
            client.DownloadFile("https://dl.dropboxusercontent.com/cd/0/get/A4ZX24rC4DlmPAYDrj7mz9WIlmmnIu9YKh1IabTXG-l0Ftq4Ib-fyG8QYspNdv6lCOcu920AbyIdo8l2dNtoW2xydD3ycH2_B06pxATB-ROj27wDdJ0nls9gtSYmjSxMmq4/file?_download_id=8358710324855012068395852250129613778887389747613992530573453088&_notify_domain=www.dropbox.com&dl=1", @"bin\tempsd\nintendont.zip");
            using(FileStream s = new FileStream(@"bin\tempsd\nintendont.zip", FileMode.Open, FileAccess.ReadWrite))
            {
                ZipArchive z = new ZipArchive(s);
                z.ExtractToDirectory(@"bin\tempsd\nintendont");
                s.Close();
            }
            status.Content = "Setting up Nintendon't...";
            if (!File.Exists(driveletter+ "\\nincfg.bin"))
            {
                File.Copy(@"bin\tempsd\nintendont\nincfg.bin", driveletter + @"\nincfg.bin");
            }
            DirectoryCopy(@"bin\tempsd\nintendont\apps", driveletter + "\\apps", true);
            DirectoryCopy(@"bin\tempsd\nintendont\games", driveletter + "\\games", true);
            DirectoryCopy(@"bin\tempsd\nintendont\codes", driveletter + "\\codes", true);
            Directory.Delete(@"bin\tempsd", true);
        }

        private void CopyInject()
        {
            status.Content = "Copying Injected Game...";
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            if(path.Contains("[LOADIINE]") && !path.Contains("[WUP]"))
            {
                DirectoryCopy(path, driveletter + "\\wiiu\\games\\" + mvm.foldername, true);
            }
            else
            {
                DirectoryCopy(path, driveletter + "\\install\\" + mvm.foldername, true);
            }
            
            mvm.foldername = "";
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            foreach (FileInfo file in dir.EnumerateFiles())
            {
                file.CopyTo(System.IO.Path.Combine(destDirName, file.Name), true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dir.EnumerateDirectories())
                {
                    DirectoryCopy(subdir.FullName, System.IO.Path.Combine(destDirName, subdir.Name), copySubDirs);
                }
            }
        }

        private void sd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sd.SelectedIndex > -1 && sd.SelectedItem != null)
            {
                setup.IsEnabled = true;
                
            }
            else
            {
                setup.IsEnabled = false;
             
            }
            SpecifyDrive();
        }

        public void Dispose()
        {
           
        }
    }
}
