using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Modules.Nintendont;
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
           // spm.Content += "\nThis will most likely fix the Injection Process, if it's stuck before it shows Copy Base";
        }
        public void Dispose()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           /* TitleKeys tk = new TitleKeys();
            tk.ShowDialog();*/
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
            Process[] pname = Process.GetProcessesByName("INICreator");
            if (pname.Length == 0)
            {
                Process.Start(@"bin\Tools\INICreator.exe");
            }
                
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Custom_Message cm = new Custom_Message("Credits", "UWUVCI AIO - NicoAICP, Morilli, ZestyTS\nBeta Testers/Contributors - wowjinxy, Danis, Adolfobenjaminv\n\nBuildPcePkg & BuildTurboCDPcePkg - JohnnyGo\nCdecrypt -  crediar\nCNUSPACKER - NicoAICP, Morilli, ZestyTS\nINICreator - NicoAICP\nN64Converter - Morilli\npng2tga - Easy2Convert\ninject_gba_c (psb) - Morilli\nRetroInject_C - Morilli\ntga_verify - Morilli\nWiiUDownloader - Morilli, ZestyTS\nwiiurpxtool - 0CHB0\nGoomba - FluBBa\nDarkFilter Removal N64 - MelonSpeedruns, ZestyTS\nNintendont SD Card Menu - TeconMoon\nwit - Wiimm\nGetExtTypePatcher - Fix94\nnfs2iso2nfs - sabykos, piratesephiroth, Fix94 and many more\nWii-VMC - wanikoko\nIcon/TV Bootimages - Flump, ZestyTS\nNKit - Nanook\nImage Creation Base - Phacox\nWiiGameLanguage Patcher - ReturnerS\nChangeAspectRatio - andot\nvWii Title Forwarder - Fix94");
            try
            {
                cm.Owner = (FindResource("mvm") as MainViewModel).mw;
            }
            catch (Exception)
            {

            }
            cm.ShowDialog();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.Update(true);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.UpdateTools();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            mvm.ResetTKQuest();
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
         
            // Ensure only one instance of the config window at a time
            foreach (Window win in Application.Current.Windows)
            {
                if (win is NintendontConfigWindow)
                {
                    win.Activate();
                    return;
                }
            }

            // Open the new integrated WPF Nintendont Configurator
            var window = new NintendontConfigWindow
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }


        private void OpenCompatSubmit_Click(object sender, RoutedEventArgs e)
        {
            var win = new CompatEntryWindow();
            win.Owner = Window.GetWindow(this);
            win.ShowDialog();
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            JsonSettingsManager.Settings.dont = false;
            JsonSettingsManager.Settings.ndsw = false;
            JsonSettingsManager.Settings.snesw = false;
            JsonSettingsManager.Settings.gczw = false;
            JsonSettingsManager.SaveSettings();
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            using LogoCreator ic = new LogoCreator();
            try
            {
                ic.Owner = (FindResource("mvm") as MainViewModel).mw;
            }
            catch (Exception)
            {

            }
            ic.ShowDialog();
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            (FindResource("mvm") as MainViewModel).RestartIntoBypass();
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ko-fi.com/zestyts");
        }

        private void ShowTutorialScreens_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tutorial = new TutorialWizard
                {
                    Owner = Window.GetWindow(this),
                    Tag = "manual"
                };
                tutorial.ShowDialog();
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show(
                    "Error Opening Tutorial",
                    "The Tutorial Wizard could not be opened.\n\n" + ex.Message,
                    UWUVCI_MessageBoxType.Ok,
                    UWUVCI_MessageBoxIcon.Error
                );
            }
        }


        private void OpenAppSettings_Click(object sender, RoutedEventArgs e)
        {
            ApplicationSettings appSettings = new ApplicationSettings();
            appSettings.Owner = parent;
            appSettings.ShowDialog();
        }
    }
}
