using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using UWUVCI_AIO_WPF.UI.Windows;


namespace UWUVCI_AIO_WPF.UI.Frames
{
    /// <summary>
    /// Interaktionslogik für SettingsFrame.xaml
    /// </summary>
    public partial class SettingsFrame : Page, IDisposable
    {
        MainWindow parent; 
        public SettingsFrame(MainWindow mw)
        {
            InitializeComponent();
            parent = mw;
        }
        public void Dispose()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TitleKeys tk = new TitleKeys();
            tk.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.EnterKey(true);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            parent.paths(false);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.UpdateBases();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Process.Start(@"Tools\INICreator.exe");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("UWUVCI AIO - NicoAICP, Morilli, Lreiia Bot\nBeta Testers/Contributors - wowjinxy, Danis\n\n7za - Igor Pavlov\nBuildPcePkg & BuildTurboCDPcePkg - JohnnyGo\nCdecrypt -  crediar\nCNUSPACKER - NicoAICP, Morilli\nINICreator - NicoAICP\nN64Creator - Morilli\npng2tga - Easy2Convert\ninject_gba_c (psb) - Morilli\nRetroInject_C - Morilli\ntga_verify - Morilli\nWiiUDownloader - Morilli\nwiiurpxtool - 0CHB0\nGoomba - FluBBa\nDarkFilter Removal N64 - MelonSpeedruns", "Credits");
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.Update();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.UpdateTools();
        }
    }
}
