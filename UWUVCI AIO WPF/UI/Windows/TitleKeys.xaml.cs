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
           
        }
        private void Window_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = (MainViewModel)FindResource("mvm");
            mvm.EnterKey();
        }
    }
}
