using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UWUVCI_AIO_WPF.UI.Frames;

using GameBaseClassLibrary;
using UWUVCI_AIO_WPF.UI.Frames.Path;
using System.IO;
using NAudio.Wave;
using System.Diagnostics;

namespace UWUVCI_AIO_WPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
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
        public static byte[] StreamToBytes(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
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
        private int _match;
        static MemoryStream ms = new MemoryStream(StreamToBytes(sound));

        static WaveStream ws = new Mp3FileReader(ms);

        static WaveOutEvent output = new WaveOutEvent();
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == _konamiCode[_match])
            {
                if (++_match >= _konamiCode.Count)
                {
                    _match = 0;

                    output.PlaybackStopped += new EventHandler<StoppedEventArgs>(Media_Ended);
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
            if (output.PlaybackState == PlaybackState.Stopped)
            {
                if (ms != null)
                {
                    ms.Close();
                    ms.Flush();
                }
                if (ws != null)
                {
                    ws.Close();
                }
                if (output != null)
                {
                    output.Dispose();
                }
            }
        }

        public bool move = true;
        public MainWindow()
        {
            InitializeComponent();
            load_frame.Content = new StartFrame();
            (FindResource("mvm") as MainViewModel).setMW(this);
        }
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
              //  Storyboard sb = this.FindResource("MenuClose") as Storyboard;
              //  if (sb != null) { BeginStoryboard(sb); }
            }
          
        }

        private void ButtonOpenMenu_Click(object sender, MouseEventArgs e)
        {
            if (!movingrn)
            {
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
                //left empty on purpose
            }
            startedmoving = false;
        }
        private void DestroyFrame()
        {
            //(load_frame.Content as IDisposable).Dispose();
            load_frame.Content = null;
            load_frame.NavigationService.RemoveBackEntry();
        }
        public void ListView_Click(object sender, MouseButtonEventArgs e)
        {
            if(!startedmoving && !movingrn)
            {
                try
                {
                    MainViewModel mvm = FindResource("mvm") as MainViewModel;

                    /*if((sender as ListView).SelectedIndex == 9)
                    {
                        mvm.saveconf = mvm.GameConfiguration.Clone();
                    }*/


                    if (mvm.curr != null)
                    {
                        mvm.curr.Background = null;
                    }
                    mvm.curr = (sender as ListView).SelectedItem as ListViewItem;
                    if (mvm.curr != null)
                    {
                        mvm.curr.Background = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));
                    }

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
                    mvm.test = GameConsoles.WII;
                    mvm.regionfrii = false;
                    mvm.cd = false;
                    mvm.regionfriijp = false;
                    mvm.regionfriius = false;
                    mvm.pixelperfect = false;
                    mvm.injected2 = false;

                    mvm.RemoveCreatedIMG();
                    mvm.isDoneMW();

                    DestroyFrame();
                    mvm.saveconf = null;
                    mvm.GC = false;
                    switch ((sender as ListView).SelectedIndex)
                    {
                        case 0:
                            tbTitleBar.Text = "UWUVCI AIO - NDS VC INJECT";
                            /*  if(mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.NDS)
                              {
                                  load_frame.Content = new INJECTFRAME(GameConsoles.NDS, mvm.saveconf);
                              }
                              else
                              {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.NDS);

                            //}
                            break;
                        case 1:
                            tbTitleBar.Text = "UWUVCI AIO - GBA VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.GBA)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.GBA, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.GBA);

                            //}
                            break;
                        case 2:
                            tbTitleBar.Text = "UWUVCI AIO - N64 VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.N64)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.N64, mvm.saveconf);
                            }
                            else
                            {*/
                            mvm.GameConfiguration.N64Stuff = new Classes.N64Conf();
                            load_frame.Content = new INJECTFRAME(GameConsoles.N64);

                            //}
                            break;
                        case 4:
                            tbTitleBar.Text = "UWUVCI AIO - NES VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.NES)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.NES, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.NES);

                            //}
                            break;
                        case 3:
                            tbTitleBar.Text = "UWUVCI AIO - SNES VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.SNES)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.SNES, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.SNES);
                            //}
                            break;
                        case 5:
                            tbTitleBar.Text = "UWUVCI AIO - TurboGrafX-16 VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.TG16 )
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.TG16, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.TG16);

                            // }
                            break;
                        case 6:
                            tbTitleBar.Text = "UWUVCI AIO - MSX VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.MSX)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.MSX, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.MSX);

                            //}
                            break;
                        case 7:
                            tbTitleBar.Text = "UWUVCI AIO - Wii VC INJECT";
                            /*if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.WII)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.WII, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.WII);

                            //}
                            break;
                        case 8:
                            mvm.GC = true;
                            tbTitleBar.Text = "UWUVCI AIO - GC VC INJECT";
                            /*if (mvm.saveconf != null && (mvm.saveconf.Console == GameConsoles.WII || mvm.saveconf.Console == GameConsoles.GCN) && mvm.GC == true)
                            {
                                load_frame.Content = new INJECTFRAME(GameConsoles.GCN, mvm.saveconf);
                            }
                            else
                            {*/
                            load_frame.Content = new INJECTFRAME(GameConsoles.GCN);

                            //}
                            break;
                        /*case 9:
                            DestroyFrame();
                            tbTitleBar.Text = "UWUVCI AIO - Retroarch VC Inject";
                            load_frame.Content = new SettingsFrame(this);

                            break;*/
                        case 9:
                            tbTitleBar.Text = "UWUVCI AIO - ???????? ?? ??????";
                            load_frame.Content = new Teaser();

                            break;
                    }
                }
                catch
                {
                    // left empty on purpose
                }
            }
        }

        public void paths(bool remove)
        {

            load_frame.Content = null;
            if (remove)
            {
                load_frame.Content = new SettingsFrame(this);
            }
            else
            {
                load_frame.Content = new Paths(this);
            }
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
            close.Background = new SolidColorBrush(Color.FromArgb(150,255, 100, 100));
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
           
            if (bypass)
            {
                spc.Text = "Debug & Space Bypass Mode";
                spc.ToolTip = "Disables all Space checks. May cause issues.\n\"Unhides\" used Tools (Displays whats going on in the Background while a ProgressBar appears";
            }
            else
            {
                spc.Text = "Debug Mode";
                spc.ToolTip = "\"Unhides\" used Tools (Displays whats going on in the Background while a ProgressBar appears";
            }
        }
        public void allowBypass()
        {
            (FindResource("mvm") as MainViewModel).saveworkaround = true;
            spc.Visibility = Visibility.Visible;
            spc.Text = "Space Bypass Mode";
            spc.ToolTip = "Disables all Space checks. May cause issues.";
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            new Task(() =>
            {
                System.Threading.Thread.Sleep(30);
                if (!startedmoving)
                {
                    movingrn = false;
                }
            }).Start();
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
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

            mvm.RemoveCreatedIMG();
            mvm.isDoneMW();
            DestroyFrame();
            tbTitleBar.Text = "UWUVCI AIO - Settings";
            load_frame.Content = new SettingsFrame(this);
        }

        private void vwiiMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("UWUVCI VWII.exe");
                Environment.Exit(0);
            }
            catch
            {
                //left empty on purpose
            }
        }
    }
}
