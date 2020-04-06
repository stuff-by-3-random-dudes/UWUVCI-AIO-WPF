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
            mvm.GameConfiguration = new GameConfig();
            mvm.LGameBasesString.Clear();
            mvm.CanInject = false;
            mvm.BaseDownloaded = false;
            mvm.RomSet = false;
            mvm.RomPath = null;
            mvm.Injected = false;
            switch ((sender as ListView).SelectedIndex)
            {
                case 0:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - NDS VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsoles.NDS);
                    break;
                case 1:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - GBA VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsoles.GBA);
                    break;
                case 2:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - N64 VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsoles.N64);
                    mvm.GameConfiguration.N64Stuff = new Classes.N64Conf();
                    break;
                case 3:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - NES VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsoles.NES);
                    break;
                case 4:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - SNES VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsoles.SNES);
                    break;
                case 5:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - TurboGrafX-16 VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsoles.TG16);
                    break;
                case 6:
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
    }
}
