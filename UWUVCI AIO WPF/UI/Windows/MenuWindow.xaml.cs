using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UWUVCI_AIO_WPF.UI.Frames;

using GameBaseClassLibrary;
using UWUVCI_AIO_WPF.UI.Frames.Path;

namespace UWUVCI_AIO_WPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        private void DestroyFrame()
        {
            //(load_frame.Content as IDisposable).Dispose();
            load_frame.Content = null;
            load_frame.NavigationService.RemoveBackEntry();
        }
        private void ListView_Click(object sender, MouseButtonEventArgs e)
        {
            
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            if(mvm.GameConfiguration != new GameConfig() && mvm.GameConfiguration != null && mvm.saveconf == null)
            {
                mvm.saveconf = mvm.GameConfiguration.Clone();
            }
            if(mvm.curr != null)
            {
                mvm.curr.Background = null;
            }
            mvm.curr = (sender as ListView).SelectedItem as ListViewItem;
            mvm.curr.Background = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));
            mvm.GameConfiguration = new GameConfig();
            mvm.LGameBasesString.Clear();
            mvm.CanInject = false;
            mvm.BaseDownloaded = false;
            mvm.RomSet = false;
            mvm.RomPath = null;
            mvm.Injected = false;
            mvm.CBasePath = null;
            
            
            mvm.setThing(null);
            switch ((sender as ListView).SelectedIndex)
            {
                case 0:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - NDS VC INJECT";
                    if(mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.NDS)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.NDS, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.NDS);
                        mvm.saveconf = null;
                    }
                    
                    break;
                case 1:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - GBA VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.GBA)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.GBA, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.GBA);
                        mvm.saveconf = null;
                    }
                    
                    break;
                case 2:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - N64 VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.N64)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.N64, mvm.saveconf);
                    }
                    else
                    {
                        mvm.GameConfiguration.N64Stuff = new Classes.N64Conf();
                        load_frame.Content = new INJECTFRAME(GameConsoles.N64);
                        mvm.saveconf = null;
                    }
                    
                    break;
                case 4:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - NES VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.NES)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.NES, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.NES);
                        mvm.saveconf = null;
                    }
                    break;
                case 3:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - SNES VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.SNES)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.SNES, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.SNES);
                        mvm.saveconf = null;
                    }
                    break;
                case 5:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - TurboGrafX-16 VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.TG16 )
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.TG16, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.TG16);
                        mvm.saveconf = null;
                    }
                    break;
                case 6:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - MSX VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.MSX)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.MSX, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.MSX);
                        mvm.saveconf = null;
                    }
                    break;
                case 7:
                    DestroyFrame();
                    mvm.GC = false;
                    tbTitleBar.Text = "UWUVCI AIO - Wii VC INJECT";
                    if (mvm.saveconf != null && mvm.saveconf.Console == GameConsoles.WII)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.WII, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.WII);
                        mvm.saveconf = null;
                    }
                    break;
                case 8:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - GC VC INJECT";
                    if (mvm.saveconf != null && (mvm.saveconf.Console == GameConsoles.WII || mvm.saveconf.Console == GameConsoles.GCN) && mvm.GC == true)
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.GCN, mvm.saveconf);
                    }
                    else
                    {
                        load_frame.Content = new INJECTFRAME(GameConsoles.GCN);
                        mvm.saveconf = null;
                    }
                    
                    break;
                case 9:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - SETTINGS";
                    load_frame.Content = new SettingsFrame(this);
                    break;
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
            this.Close();
        }

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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
        public void setDebug()
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.debug = true;
        }
    }
}
