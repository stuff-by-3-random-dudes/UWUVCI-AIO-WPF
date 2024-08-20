using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using NAudio.Wave;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.UI.Frames;
using GameBaseClassLibrary;
using UWUVCI_AIO_WPF.UI.Frames.Path;

namespace UWUVCI_AIO_WPF
{
    public partial class MainWindow : Window
    {
        private readonly List<Key> _konamiCode = new List<Key>
        {
            Key.Up, Key.Up,
            Key.Down, Key.Down,
            Key.Left, Key.Right,
            Key.Left, Key.Right,
            Key.B, Key.A,
            Key.Enter
        };

        private bool movingrn = false;
        private bool startedmoving = false;
        private int _match;

        public static byte[] StreamToBytes(Stream stream)
        {
            long originalPosition = stream.CanSeek ? stream.Position : 0;
            if (stream.CanSeek) stream.Position = 0;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            finally
            {
                if (stream.CanSeek) stream.Position = originalPosition;
            }
        }

        internal void is32()
        {
            Wii.DataContext = this;
            Wii.IsEnabled = false;
            GC.DataContext = this;
            GC.IsEnabled = false;
        }

        static MemoryStream sound = new MemoryStream(Properties.Resources.mario);
        static MemoryStream ms = new MemoryStream(StreamToBytes(sound));
        static WaveStream ws = new Mp3FileReader(ms);
        static WaveOutEvent output = new WaveOutEvent();

        public MainWindow()
        {
            InitializeComponent();
            load_frame.Content = new StartFrame();
            (FindResource("mvm") as MainViewModel).setMW(this);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == _konamiCode[_match])
            {
                if (++_match >= _konamiCode.Count)
                {
                    _match = 0;
                    output.PlaybackStopped += Media_Ended;
                    output.Init(ws);
                    output.Play();
                }
            }
            else if (_match > 0 && e.Key != _konamiCode[_match])
            {
                _match = 0;
            }
        }

        public static void Media_Ended(object sender, EventArgs e)
        {
            DisposeResources();
        }

        private static void DisposeResources()
        {
            ms?.Dispose();
            ws?.Dispose();
            output?.Dispose();
        }

        public bool move = true;

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
        }

        private void ButtonCloseMenu_Click(object sender, MouseEventArgs e)
        {
            if (!movingrn)
            {
                // Uncomment if Storyboard is needed
                // Storyboard sb = this.FindResource("MenuClose") as Storyboard;
                // if (sb != null) { BeginStoryboard(sb); }
            }
        }

        private void ButtonOpenMenu_Click(object sender, MouseEventArgs e)
        {
            if (!movingrn)
            {
                // Uncomment if Storyboard is needed
                // Storyboard sb = this.FindResource("MenuOpen") as Storyboard;
                // if (sb != null) { BeginStoryboard(sb); }
            }
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            startedmoving = true;
            try
            {
                if (e.ChangedButton == MouseButton.Left && move)
                {
                    movingrn = true;
                    DragMove();
                }
            }
            catch (Exception)
            {
                // Exception handling logic can be added here
            }
            finally
            {
                startedmoving = false;
            }
        }

        private void DestroyFrame()
        {
            // Dispose of the content if necessary
            // (load_frame.Content as IDisposable)?.Dispose();
            load_frame.Content = null;
            load_frame.NavigationService.RemoveBackEntry();
        }

        public void ListView_Click(object sender, MouseButtonEventArgs e)
        {
            if (!startedmoving && !movingrn)
            {
                ResetMainViewModel();

                try
                {
                    MainViewModel mvm = FindResource("mvm") as MainViewModel;
                    mvm.curr = (sender as ListView).SelectedItem as ListViewItem;

                    UpdateUIForSelectedIndex(mvm, (sender as ListView).SelectedIndex);
                }
                catch
                {
                    // Exception handling logic can be added here
                }
            }
        }

        private void ResetMainViewModel()
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            if (mvm.curr != null) mvm.curr.Background = null;

            mvm.GameConfiguration = new GameConfig();
            mvm.LGameBasesString.Clear();
            mvm.CanInject = false;
            mvm.BaseDownloaded = false;
            mvm.RomSet = false;
            mvm.RomPath = null;
            mvm.Injected = false;
            mvm.CBasePath = null;
            mvm.bcf = null;
            mvm.BootSound = null;
            mvm.setThing(null);
            mvm.gc2rom = null;
            mvm.Index = -1;
            mvm.donttrim = false;
            mvm.NKITFLAG = false;
            mvm.prodcode = "";
            mvm.foldername = "";
            mvm.jppatch = false;
            mvm.GC = false;
            mvm.test = GameConsoles.WII;
            mvm.regionfrii = false;
            mvm.cd = false;
            mvm.regionfriijp = false;
            mvm.regionfriius = false;
            mvm.pixelperfect = false;
            mvm.injected2 = false;
            mvm.Brightness = 80;
            mvm.RendererScale = false;
            mvm.RemoveDeflicker = false;
            mvm.RemoveDithering = false;
            mvm.PixelArtUpscaler = 0;
            mvm.DSLayout = false;

            mvm.RemoveCreatedIMG();
            mvm.isDoneMW();
            DestroyFrame();
            mvm.saveconf = null;
            mvm.GC = false;
        }

        private void UpdateUIForSelectedIndex(MainViewModel mvm, int selectedIndex)
        {
            string title = selectedIndex switch
            {
                0 => "UWUVCI AIO - NDS VC INJECT",
                1 => "UWUVCI AIO - GBA VC INJECT",
                2 => "UWUVCI AIO - N64 VC INJECT",
                3 => "UWUVCI AIO - SNES VC INJECT",
                4 => "UWUVCI AIO - NES VC INJECT",
                5 => "UWUVCI AIO - TurboGrafX-16 VC INJECT",
                6 => "UWUVCI AIO - MSX VC INJECT",
                7 => "UWUVCI AIO - Wii VC INJECT",
                8 => "UWUVCI AIO - GC VC INJECT",
                _ => tbTitleBar.Text
            };

            tbTitleBar.Text = title;

            var console = selectedIndex switch
            {
                0 => GameConsoles.NDS,
                1 => GameConsoles.GBA,
                2 => GameConsoles.N64,
                3 => GameConsoles.SNES,
                4 => GameConsoles.NES,
                5 => GameConsoles.TG16,
                6 => GameConsoles.MSX,
                7 => GameConsoles.WII,
                8 => GameConsoles.GCN,
                _ => GameConsoles.WII
            };

            mvm.GameConfiguration = new GameConfig();
            mvm.test = console;
            load_frame.Content = new INJECTFRAME(console);
        }

        public void paths(bool remove)
        {
            load_frame.Content = null;
            if (remove)
                load_frame.Content = new SettingsFrame(this);
            else
                load_frame.Content = new Paths(this);
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            min.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }

        private void close_MouseEnter(object sender, MouseEventArgs e)
        {
            close.Background = new SolidColorBrush(Color.FromArgb(150, 255, 100, 100));
        }

        private void close_MouseLeave(object sender, MouseEventArgs e)
        {
            close.Background = new SolidColorBrush(Color.FromArgb(0, 250, 250, 250));
        }

        private void min_MouseLeave(object sender, MouseEventArgs e)
        {
            min.Background = new SolidColorBrush(Color.FromArgb(0, 250, 250, 250));
        }

        private void sett_MouseEnter(object sender, MouseEventArgs e)
        {
            settings.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        }

        private void sett_MouseLeave(object sender, MouseEventArgs e)
        {
            settings.Background = new SolidColorBrush(Color.FromArgb(0, 250, 250, 250));
        }

        public void setDebug(bool bypass)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.debug = true;
            spc.Visibility = Visibility.Visible;

            spc.Text = bypass ? "Debug & Space Bypass Mode" : "Debug Mode";
            spc.ToolTip = bypass
                ? "Disables all Space checks. May cause issues.\n\"Unhides\" used Tools (Displays whats going on in the Background while a ProgressBar appears"
                : "\"Unhides\" used Tools (Displays whats going on in the Background while a ProgressBar appears";
        }

        public void allowBypass()
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.saveworkaround = true;
            spc.Visibility = Visibility.Visible;
            spc.Text = "Space Bypass Mode";
            spc.ToolTip = "Disables all Space checks. May cause issues.";
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(30);
                if (!startedmoving)
                {
                    movingrn = false;
                }
            });
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;

            ResetMainViewModel();

            tbTitleBar.Text = "UWUVCI AIO - Settings";
            load_frame.Content = new SettingsFrame(this);
        }

        private void vwiiMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = new Process();
                var fileName = Application.ResourceAssembly.Location;
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe"))
                {
                    if (Path.GetFileName(file).ToLower().Contains("vwii"))
                    {
                        fileName = file;
                        break;
                    }
                }

                p.StartInfo.FileName = fileName;
                p.Start();

                Environment.Exit(0);
            }
            catch
            {
                // Exception handling logic can be added here
            }
        }
    }
}
