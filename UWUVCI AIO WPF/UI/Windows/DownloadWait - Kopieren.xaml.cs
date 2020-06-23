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
            if (!gamecube)
            {
                setup.Content = "Copy to SD";
                tbTitleBar.Text = "Copy to SD";
            }
        
            Task.Run(() => checkfornewinput());
            Task.Run(() => checkfornewoutput());
            
            GetDrives();
        }
        private void checkfornewinput()
        {
           
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher.Query = query;
            watcher.Options.Timeout = new TimeSpan(0, 0, 5);
            watcher.Start();
            try
            {
                watcher.WaitForNextEvent();
            }
            catch (Exception)
            {

            }
            
        }
        private void checkfornewoutput()
        {
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3");
            watcher2.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher2.Query = query;
            watcher2.Options.Timeout = new TimeSpan(0, 0, 5);
            watcher2.Start();
            try
            {
                watcher2.WaitForNextEvent();
            }
            catch (Exception)
            {

            }

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
        public static long GetDirectorySize(string p)
        {
            string[] a = Directory.GetFiles(p, "*.*");
            long b = 0;
            foreach (string name in a)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            return b;
        }
        public static long GetDirectorySize(string b, bool t)
        {
           

            long result = 0;
            Stack<string> stack = new Stack<string>();
            stack.Push(b);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                try
                {
                    result += GetDirectorySize(dir);
                    foreach (string dn in Directory.GetDirectories(dir))
                    {
                        stack.Push(dn);
                    }
                }
                catch { }
            }
            return result;
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
        private void Window_Close(object sender, RoutedEventArgs e)
        {
            /*Task t = new Task(() => watcher.Stop());
            t.Start();
            t.Wait();
            t = new Task(() => watcher2.Stop());
            t.Start();
            t.Wait();*/

            watcher.Stop();
            watcher2.Stop();
            watcher.Dispose();
            watcher2.Dispose();
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
            catch (Exception )
            {

            }
        }
        DispatcherTimer dp = new DispatcherTimer();
        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        private void setup_Click(object sender, RoutedEventArgs e)
        {
            long injctSize = GetDirectorySize(System.IO.Path.Combine(path, (FindResource("mvm") as MainViewModel).foldername), true);
            if(injctSize >= new DriveInfo(driveletter).AvailableFreeSpace)
            {
                long div = injctSize - new DriveInfo(driveletter).AvailableFreeSpace + 1048576;
                Custom_Message cm = new Custom_Message("Insufficient Space", $" You do not have enough space on the selected drive. \n Please make sure you have atleast {FormatBytes(div)} free on the drive! ");
                try
                {
                    cm.Owner = (FindResource("mvm") as MainViewModel).mw;
                }
                catch (Exception)
                {

                }
                cm.ShowDialog();
            }
            else
            {
                dp.Tick += Dp_Tick;
                dp.Interval = TimeSpan.FromSeconds(1);
                dp.Start();
                Task.Run(() =>
                {

                    if (gc)
                    {
                        SetupNintendont();
                    }
                    CopyInject();
                });
                setup.IsEnabled = false;
            }
            

         
            
           
            
        }

        private void Dp_Tick(object sender, EventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            status.Content = mvm.msg;
            if(mvm.msg == "Done with Setup!")
            {
                mvm.msg = "";
                dp.Stop();
                setup.IsEnabled = true;
                setup.Click -= setup_Click;
                setup.Click += Window_Close;
                setup.Content = "Close";
            }
        }

        private void SetupNintendont()
        {
           
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.msg = "";
            mvm.msg = "Downloading Nintendont...";
            if (Directory.Exists(@"bin\tempsd"))
            {
                Directory.Delete(@"bin\tempsd", true);
            }
            Directory.CreateDirectory(@"bin\tempsd");
            var client = new WebClient();
            client.DownloadFile("https://dl.dropbox.com/s/3swnsatmautzlk4/Nintendont.zip?dl=1", @"bin\tempsd\nintendont.zip");
            using(FileStream s = new FileStream(@"bin\tempsd\nintendont.zip", FileMode.Open, FileAccess.ReadWrite))
            {
                ZipArchive z = new ZipArchive(s);
                z.ExtractToDirectory(@"bin\tempsd\nintendont");
                s.Close();
            }
           mvm.msg = "Setting up Nintendon't...";
            if (!File.Exists(driveletter+ "\\nincfg.bin"))
            {
                File.Copy(@"bin\tempsd\nintendont\nincfg.bin", driveletter + @"\nincfg.bin");
            }
            DirectoryCopy(@"bin\tempsd\nintendont\apps", driveletter + "\\apps", true);
            DirectoryCopy(@"bin\tempsd\nintendont\codes", driveletter + "\\codes", true);
            Directory.Delete(@"bin\tempsd", true);
        }

        private void CopyInject()
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.msg = "Copying Injected Game...";
            if(!System.IO.Path.Combine(path, mvm.foldername).Contains("[WUP]"))
            {
                DirectoryCopy(System.IO.Path.Combine(path, mvm.foldername), driveletter + "\\wiiu\\games\\" + mvm.foldername, true);
            }
            else
            {
                DirectoryCopy(System.IO.Path.Combine(path,mvm.foldername), driveletter + "\\install\\" + mvm.foldername, true);
            }
            
            mvm.foldername = "";
            mvm.msg = "Done with Setup!";
            
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
