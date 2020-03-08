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
using UWUVCI_AIO_WPF.Classes.ENUM;

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
            DestroyFrame();
            switch ((sender as ListView).SelectedIndex)
            {
                case 0:
                    tbTitleBar.Text = "UWUVCI AIO - NDS VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsole.NDS);
                    break;
                case 1:
                    tbTitleBar.Text = "UWUVCI AIO - GBA VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsole.GBA);
                    break;
                case 2:
                    tbTitleBar.Text = "UWUVCI AIO - N64 VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsole.N64);
                    break;
                case 3:
                    tbTitleBar.Text = "UWUVCI AIO - NES VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsole.NES);
                    break;
                case 4:
                    tbTitleBar.Text = "UWUVCI AIO - SNES VC INJECT";
                    load_frame.Content = new INJECTFRAME(GameConsole.SNES);
                    break;
                case 5:
                    tbTitleBar.Text = "UWUVCI AIO - SETTINGS";
                    load_frame.Content = new SettingsFrame();
                    break;
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
