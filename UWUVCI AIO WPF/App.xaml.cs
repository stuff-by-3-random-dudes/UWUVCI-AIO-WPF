﻿using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using UWUVCI_AIO_WPF.UI.Windows;
using UWUVCI_AIO_WPF.Helpers;
using GameBaseClassLibrary;

namespace UWUVCI_AIO_WPF
{
    public partial class App : Application
    {
        Timer t = new Timer(5000);
        private StartupEventArgs _startupArgs;
        private static string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UWUVCI-V3");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Ensure the settings directory exists before attempting to load settings
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            // Redirect Console.WriteLine to the logger at the very beginning
            Console.SetOut(new ConsoleLoggerWriter());

            // Check if running from OneDrive
            if (IsRunningFromOneDrive())
            {
                MessageBox.Show("UWUVCI AIO cannot be run from a OneDrive folder due to compatibility issues. \n\n" +
                    "Please move it to another location (e.g., C:\\Programs or C:\\Users\\YourName\\UWUVCI_AIO) before launching.",
                    "Error: OneDrive Detected", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1); // Terminate the application
            }

            // Add global event handlers for drag-and-drop
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewDragOverEvent, new DragEventHandler(GlobalTextBox_PreviewDragOver));
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewDropEvent, new DragEventHandler(GlobalTextBox_PreviewDrop));

            if (File.Exists("tools.json"))
                File.Delete("tools.json");

            _startupArgs = e;
            JsonSettingsManager.LoadSettings();

            if (!JsonSettingsManager.Settings.IsFirstLaunch)
            {
                LaunchMainApplication(e);
            }
            else
            {
                if (MacLinuxHelper.IsRunningUnderWineOrSimilar())
                {
                    MessageBox.Show("UWUVCI cannot tell if you went through the tutorial or not. We will assume you did, but if you didn't, in the main application click the gear icon, and then click the button that says 'Show Tutorial Screens'.",
                        "UWUVCI Tutorial..?", MessageBoxButton.OK, MessageBoxImage.Question);
                    JsonSettingsManager.Settings.IsFirstLaunch = false;
                    JsonSettingsManager.SaveSettings();
                    LaunchMainApplication(e);
                }
                else
                {
                    new IntroductionWindow().ShowDialog();
                }
            }
        }

        private static void GlobalTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }
        private bool IsRunningFromOneDrive()
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            return exePath.ToLower().Contains("onedrive");
        }

        private static void GlobalTextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    TextBox txtBox = sender as TextBox;
                    if (txtBox != null)
                    {
                        // Assign to ViewModel properties dynamically
                        if (Current.MainWindow is MainWindow mainWindow &&
                            mainWindow.Content is Grid grid &&
                            grid.DataContext is MainViewModel mvm)
                        {
                            if (mvm.GameConfiguration.Console == GameConsoles.WII || mvm.GameConfiguration.Console == GameConsoles.GCN)
                                return;

                            string filePath = files[0];

                            // Temporarily make writable to allow drop
                            txtBox.IsReadOnly = false;
                            txtBox.Text = filePath;
                            txtBox.IsReadOnly = true;

                            switch (txtBox.Name)
                            {
                                case "rp":  // Special handling for ROM Path
                                    mvm.RomSet = true;
                                    mvm.RomPath = filePath;
                                    if (mvm.BaseDownloaded)
                                    {
                                        mvm.CanInject = true;
                                    }

                                    // Call the correct `getBootIMG*()` method dynamically
                                    switch (mvm.GameConfiguration.Console)
                                    {
                                        case GameConsoles.NDS:
                                            mvm.getBootIMGNDS(mvm.RomPath);
                                            break;
                                        case GameConsoles.NES:
                                            mvm.getBootIMGNES(mvm.RomPath);
                                            break;
                                        case GameConsoles.SNES:
                                            mvm.getBootIMGSNES(mvm.RomPath);
                                            break;
                                        case GameConsoles.MSX:
                                            mvm.getBootIMGMSX(mvm.RomPath);
                                            break;
                                        case GameConsoles.N64:
                                            mvm.getBootIMGN64(mvm.RomPath);
                                            break;
                                        case GameConsoles.GBA:
                                            var fileExtension = Path.GetExtension(filePath).ToLower();
                                            if (fileExtension != ".gb" && fileExtension != ".gbc")
                                                mvm.getBootIMGGBA(mvm.RomPath);
                                            break;
                                        case GameConsoles.TG16:
                                            mvm.getBootIMGTG(mvm.RomPath);
                                            break;
                                        default:
                                            Console.WriteLine("Unsupported console type: " + mvm.GameConfiguration.Console);
                                            break;
                                    }
                                    break;

                                case "ic":
                                    mvm.GameConfiguration.TGAIco.ImgPath = filePath;
                                    break;
                                case "tv":
                                    mvm.GameConfiguration.TGATv.ImgPath = filePath;
                                    break;
                                case "drc":
                                    mvm.GameConfiguration.TGADrc.ImgPath = filePath;
                                    break;
                                case "log":
                                    mvm.GameConfiguration.TGALog.ImgPath = filePath;
                                    break;
                                case "ini":
                                    mvm.GameConfiguration.N64Stuff.INIPath = filePath;
                                    break;
                                case "sound":
                                    mvm.BootSound = filePath;
                                    break;
                            }
                        }
                    }
                }
            }
        }


        public void LaunchMainApplication()
        {
            LaunchMainApplication(_startupArgs);
        }

        private void LaunchMainApplication(StartupEventArgs e)
        {
            if (Directory.Exists(@"custom") && File.Exists(@"custom\main.dol"))
            {
                if (!Directory.Exists(@"bin\Tools"))
                    Directory.CreateDirectory(@"bin\Tools");

                File.Copy(@"custom\main.dol", @"bin\Tools\nintendont.dol", true);
                File.Copy(@"custom\main.dol", @"bin\Tools\nintendont_force.dol", true);
            }

            bool check = true;
            bool bypass = false;
            if (e.Args.Length >= 1)
                foreach (var s in e.Args)
                {
                    if (s == "--skip") check = false;
                    if (s == "--spacebypass") bypass = true;
                }

            Process[] pname = Process.GetProcessesByName("UWUVCI AIO");
            if (pname.Length > 1 && check)
            {
                t.Elapsed += KillProg;
                t.Start();
                Custom_Message cm = new Custom_Message("Another Instance Running",
                    " You already got another instance of UWUVCI AIO running. \n This instance will terminate in 5 seconds. ");
                cm.ShowDialog();
                KillProg(null, null);
            }
            else
            {
                MainWindow wnd = new MainWindow();
                double height = SystemParameters.PrimaryScreenHeight;
                double width = SystemParameters.PrimaryScreenWidth;
                if (width < 1150 || height < 700)
                {
                    t.Elapsed += KillProg;
                    t.Start();
                    Custom_Message cm = new Custom_Message("Resolution not supported",
                        "Your screen resolution is not supported, please use a resolution of at least 1152x864\nIf your resolution is higher than this, then it's because of your zoom level, either way, please change your display settings.\nThis instance will terminate in 5 seconds.");
                    cm.ShowDialog();
                    KillProg(null, null);
                }

                if (bypass)
                    wnd.allowBypass();

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
