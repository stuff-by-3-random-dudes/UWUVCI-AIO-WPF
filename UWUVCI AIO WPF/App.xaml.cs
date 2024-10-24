using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using UWUVCI_AIO_WPF.UI.Windows;
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Classes;

namespace UWUVCI_AIO_WPF
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
		
		Timer t = new Timer(5000);
        private StartupEventArgs _startupArgs;
        private void Application_Startup(object sender, StartupEventArgs e)
		{
            // Store the StartupEventArgs
            _startupArgs = e; 
            
            // Load settings from the JSON file
            JsonSettingsManager.LoadSettings();

            if (!JsonSettingsManager.Settings.IsFirstLaunch)
            {
                // Proceed with the regular application startup
                LaunchMainApplication(e);
            }
            else
            {
                if (MacLinuxHelper.IsRunningUnderWineOrSimilar())
                {
                    MessageBox.Show("UWUVCI cannot tell if you went through the tutorial or not. We will assume you did, but if you didn't, in the main application click the gear icon, and then click the button that says 'Show Tutorial Screens'.", "UWUVCI Tutorial..?", MessageBoxButton.OK, MessageBoxImage.Question);
                    JsonSettingsManager.Settings.IsFirstLaunch = false;
                    JsonSettingsManager.SaveSettings();
                    LaunchMainApplication(e);
                }
                else
                {
                    // Show the introductory window sequence
                    new IntroductionWindow().ShowDialog();
                }
            }
        }
        public void LaunchMainApplication()
        {
            LaunchMainApplication(_startupArgs);
        }
        private void LaunchMainApplication(StartupEventArgs e)
		{
            if (Directory.Exists(@"custom"))
                if (File.Exists(@"custom\main.dol"))
                {
                    if (File.Exists(@"bin\Tools\nintendont.dol"))
                    {
                        File.Delete(@"bin\Tools\nintendont.dol");
                        File.Copy(@"custom\main.dol", @"bin\Tools\nintendont.dol");
                    }
                    else
                    {
                        if (!Directory.Exists(@"bin"))
                            Directory.CreateDirectory(@"bin");

                        else if (!Directory.Exists(@"bin\Tools"))
                            Directory.CreateDirectory(@"bin\Tools");

                        File.Copy(@"custom\main.dol", @"bin\Tools\nintendont.dol");
                    }

                    if (File.Exists(@"bin\Tools\nintendont_force.dol"))
                    {
                        File.Delete(@"bin\Tools\nintendont_force.dol");
                        File.Copy(@"custom\main.dol", @"bin\Tools\nintendont_force.dol");
                    }
                    else
                        File.Copy(@"custom\main.dol", @"bin\Tools\nintendont_force.dol");
                }

            bool check = true;
            bool bypass = false;
            if (e.Args.Length >= 1)
                foreach (var s in e.Args)
                {
                    if (s == "--skip")
                        check = false;

                    if (s == "--spacebypass")
                        bypass = true;
                }

            Process[] pname = Process.GetProcessesByName("UWUVCI AIO");
            if (pname.Length > 1 && check)
            {
                t.Elapsed += KillProg;
                t.Start();
                Custom_Message cm = new Custom_Message("Another Instance Running", " You already got another instance of UWUVCI AIO running. \n This instance will terminate in 5 seconds. ");

                cm.ShowDialog();
                KillProg(null, null);
            }
            else
            {
                MainWindow wnd = new MainWindow();
                double height = SystemParameters.PrimaryScreenHeight;
                double witdh = SystemParameters.PrimaryScreenWidth;
                if (witdh < 1150 || height < 700)
                {
                    t.Elapsed += KillProg;
                    t.Start();
                    Custom_Message cm = new Custom_Message("Resolution not supported", "Your screen resolution is not supported, please use a resolution of atleast 1152x864\nThis instance will terminate in 5 seconds.");
                    cm.ShowDialog();
                    KillProg(null, null);
                }

                if (!Environment.Is64BitOperatingSystem)
                {
                    Custom_Message cm = new Custom_Message("Warning", "Some features may cause issues on a 32Bit System. Upgrading to 64Bit would be recommended.\nDue to an Issue with packing on 32Bit Systems, you need Java installed for packing. \nReport any issues in the UWUVCI Discord, or ping @NicoAICP or @ZestyTS in #wiiu-assistance in the Nintendo Homebrew discord. ");
                    cm.ShowDialog();
                }


                if (bypass)
                    wnd.allowBypass();

                // The OpenFile() method is just an example of what you could do with the
                // parameter. The method should be declared on your MainWindow class, where
                // you could use a range of methods to process the passed file path
                if (e.Args.Length >= 1 && e.Args[0] == "--debug")
                    wnd.setDebug(bypass);
                wnd.Show();
            }
        }

		private void KillProg(object sender, ElapsedEventArgs e)
		{
			t.Stop();
			Environment.Exit(1);
		}
	}
}
