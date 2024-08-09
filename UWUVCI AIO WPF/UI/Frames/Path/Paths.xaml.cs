using System;
using System.Windows;
using System.Windows.Controls;

namespace UWUVCI_AIO_WPF.UI.Frames.Path
{
    /// <summary>
    /// Interaktionslogik für Paths.xaml
    /// </summary>
    public partial class Paths : Page, IDisposable
    {
        MainWindow parent;
        MainViewModel mvm;
        public Paths(MainWindow mw)
        {
            InitializeComponent();
            parent = mw;
            mvm = FindResource("mvm") as MainViewModel;
        }
        public void Dispose()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mvm.SetBasePath();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mvm.SetInjectPath();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            parent.paths(true);
        }
    }
}
