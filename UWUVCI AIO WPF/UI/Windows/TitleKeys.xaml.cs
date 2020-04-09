using GameBaseClassLibrary;
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
using System.Windows.Shapes;
using UWUVCI_AIO_WPF.UI.Frames;
using UWUVCI_AIO_WPF.UI.Frames.KeyFrame;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für TitleKeys.xaml
    /// </summary>
    public partial class TitleKeys : Window
    {
        public TitleKeys()
        {
            InitializeComponent();
            tbA.Text = "To enter a TitleKey, first select the console on your left\nand then double click on a Title you want to enter the Key for.";
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
            tbA.Visibility = Visibility.Hidden;
            switch ((sender as ListView).SelectedIndex)
            {
                case 0:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - NDS TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.NDS);
                    break;
                case 1:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - GBA TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.GBA);
                    break;
                case 2:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - N64 TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.N64);
                    break;
                case 4:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - NES TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.NES);
                    break;
                case 3:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.SNES);
                    break;
                case 5:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.TG16);
                    break;
                case 6:
                    DestroyFrame();
                    tbTitleBar.Text = "UWUVCI AIO - TKeys";
                    load_frame.Content = new TKFrame(GameConsoles.MSX);
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
