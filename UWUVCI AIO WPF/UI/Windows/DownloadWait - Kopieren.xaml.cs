using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UWUVCI_AIO_WPF.Helpers;

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
                //left empty on purpose
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
                //left empty on purpose
            }

        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Dispatcher.Invoke(() => { GetDrives(); });
        }

        private void GetDrives()
        {
            sd.ItemsSource = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Removable).Select(d => d.Name + " " + d.VolumeLabel + "").ToList();
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
            WindowState = WindowState.Minimized;
        }
       
        public void changeOwner(MainWindow ow)
        {
            Owner = ow;
            try
            {
                if (Owner?.GetType() == typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    ShowInTaskbar = false;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch (Exception)
            {
                //left empty on purpose
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
            Close();
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
                //left empty on purpose
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

            return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        private void setup_Click(object sender, RoutedEventArgs e)
        {
            var mvm = FindResource("mvm") as MainViewModel;
            if (!mvm.saveworkaround)
            {
                long injctSize = GetDirectorySize(Path.Combine(path, mvm.foldername), true);
                long availableSpace = new DriveInfo(driveletter).AvailableFreeSpace;

                if (injctSize >= availableSpace)
                {
                    long neededSpace = injctSize - availableSpace + 1048576;
                    Custom_Message cm = new Custom_Message(
                        "Insufficient Space",
                        $"You do not have enough space on the selected drive.\n" +
                        $"Please make sure you have at least {FormatBytes(neededSpace)} free!"
                    );

                    try { cm.Owner = mvm.mw; } catch { }  // Ignore ownership exception
                    cm.ShowDialog();
                    return;
                }
            }

            dp.Tick += Dp_Tick;
            dp.Interval = TimeSpan.FromSeconds(1);
            dp.Start();

            setup.IsEnabled = false;

            Task.Run(async () =>
            {
                try
                {
                    if (gc) 
                        await SetupNintendont();

                    await CopyInjectAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error in setup process: {ex.Message}\n{ex.StackTrace}");
                    Dispatcher.Invoke(() => { mvm.msg = "Setup Failed!"; });
                }
            });
        }

        private void Dp_Tick(object sender, EventArgs e)
        {
            var mvm = FindResource("mvm") as MainViewModel;

            Dispatcher.Invoke(() =>
            {
                status.Content = mvm.msg;

                if (mvm.msg == "Done with Setup!" || mvm.msg == "Setup Failed!")
                {
                    dp.Stop();
                    setup.IsEnabled = true;
                    setup.Click -= setup_Click;
                    setup.Click += Window_Close;
                    setup.Content = "Close";
                }
            });
        }

        private async Task SetupNintendont()
        {
            try
            {
                var mvm = FindResource("mvm") as MainViewModel;
                Dispatcher.Invoke(() => mvm.msg = "Downloading Nintendont...");
                Logger.Log("Starting SetupNintendont...");

                string tempPath = @"bin\tempsd";

                if (Directory.Exists(tempPath)) 
                    Directory.Delete(tempPath, true);

                Directory.CreateDirectory(tempPath);

                string zipPath = Path.Combine(tempPath, "nintendont.zip");

                using (var client = new WebClient())
                {
                    Logger.Log("Downloading Nintendont...");
                    client.DownloadFile("https://dl.dropbox.com/s/3swnsatmautzlk4/Nintendont.zip?dl=1", zipPath);
                }

                Logger.Log("Extracting Nintendont...");
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                string nintendontPath = Path.Combine(driveletter, "apps", "nintendont");

                if (Directory.Exists(nintendontPath))
                    Directory.Delete(nintendontPath, true);

                Directory.CreateDirectory(nintendontPath);

                using (var client = new WebClient())
                {
                    Logger.Log("Downloading Nintendont files...");
                    client.DownloadFile("https://raw.githubusercontent.com/GaryOderNichts/Nintendont/master/loader/loader.dol", Path.Combine(nintendontPath, "boot.dol"));
                    client.DownloadFile("https://raw.githubusercontent.com/GaryOderNichts/Nintendont/master/nintendont/meta.xml", Path.Combine(nintendontPath, "meta.xml"));
                    client.DownloadFile("https://raw.githubusercontent.com/GaryOderNichts/Nintendont/master/nintendont/icon.png", Path.Combine(nintendontPath, "icon.png"));
                }

                Logger.Log("Copying Nintendont codes...");
                await CopyDirectoryAsync(Path.Combine(tempPath, "nintendont", "codes"), Path.Combine(driveletter, "codes"));

                Directory.Delete(tempPath, true);
                Logger.Log("SetupNintendont completed.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in SetupNintendont: {ex.Message}");
            }
        }

        private async Task CopyInjectAsync()
        {
            var mvm = FindResource("mvm") as MainViewModel;
            string sourcePath = Path.Combine(path, mvm.foldername);
            string destinationPath = sourcePath.Contains("[WUP]")
                ? Path.Combine(driveletter, "install", mvm.foldername)
                : Path.Combine(driveletter, "wiiu", "games", mvm.foldername);

            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Preparing stage
                    CopyProgressIndeterminate.Visibility = Visibility.Visible;
                    CopyProgress.Visibility = Visibility.Collapsed;
                    CopyProgressText.Visibility = Visibility.Visible;
                    CopyProgressText.Text = "Preparing files...";
                    mvm.msg = "Preparing copy...";
                });

                await Task.Delay(800); // small delay so the indeterminate bar actually animates

                Logger.Log($"Copying from {sourcePath} to {destinationPath}");

                Dispatcher.Invoke(() =>
                {
                    // Switch to determinate
                    CopyProgressIndeterminate.Visibility = Visibility.Collapsed;
                    CopyProgress.Visibility = Visibility.Visible;
                    CopyProgress.Value = 0;
                    CopyProgressText.Text = "0%";
                    mvm.msg = "Copying Injected Game...";
                });

                await CopyDirectoryAsync(sourcePath, destinationPath, (percent, message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        CopyProgress.Value = percent;
                        CopyProgressText.Text = $"{percent}%";
                        mvm.msg = message;
                    });
                });

                Dispatcher.Invoke(() =>
                {
                    CopyProgress.Visibility = Visibility.Collapsed;
                    CopyProgressIndeterminate.Visibility = Visibility.Collapsed;
                    CopyProgressText.Visibility = Visibility.Collapsed;
                    mvm.msg = "Done with Setup!";
                    mvm.foldername = "";
                });

                Logger.Log("CopyInject completed successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in CopyInject: {ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    mvm.msg = "Setup Failed!";
                    CopyProgress.Visibility = Visibility.Collapsed;
                    CopyProgressIndeterminate.Visibility = Visibility.Collapsed;
                    CopyProgressText.Visibility = Visibility.Collapsed;
                });
            }
        }

        private static async Task CopyDirectoryAsync(string sourceDir, string destDir, Action<int, string>? progressCallback = null, int maxParallel = 4)
        {
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            Directory.CreateDirectory(destDir);

            var allFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            int totalFiles = allFiles.Length;
            int copied = 0;

            using SemaphoreSlim concurrencyLimiter = new SemaphoreSlim(maxParallel);
            var copyTasks = allFiles.Select(async file =>
            {
                await concurrencyLimiter.WaitAsync();
                try
                {
                    string relativePath = file.Substring(sourceDir.Length + 1);
                    string destFilePath = Path.Combine(destDir, relativePath);
                    string destDirPath = Path.GetDirectoryName(destFilePath)!;

                    if (!Directory.Exists(destDirPath))
                        Directory.CreateDirectory(destDirPath);

                    await CopyFileBufferedAsync(file, destFilePath);

                    int progressNow = Interlocked.Increment(ref copied);
                    if (progressNow % 5 == 0 || progressNow == totalFiles)
                    {
                        int percent = (int)((double)progressNow / totalFiles * 100);
                        progressCallback?.Invoke(percent, $"Copying file {progressNow}/{totalFiles}...");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to copy {file}: {ex.Message}");
                }
                finally
                {
                    concurrencyLimiter.Release();
                }
            }).ToArray();

            await Task.WhenAll(copyTasks);
        }


        private static async Task CopyFileBufferedAsync(string sourceFile, string destinationFile)
        {
            const int bufferSize = 1024 * 1024; // 1MB
            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
            using var destStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);
            await sourceStream.CopyToAsync(destStream, bufferSize);
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
            throw new NotImplementedException();
        }
    }
}
