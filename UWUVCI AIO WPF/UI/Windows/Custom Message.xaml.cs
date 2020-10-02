using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für Custom_Message.xaml
    /// </summary>
    /// 

    public partial class Custom_Message : Window
    {
        string path;
        bool reset = false;
        bool add = false;
        public Custom_Message(string title, string message)
        {
            
            InitializeComponent();
            nc.Visibility = Visibility.Hidden;
            try
            {
                if(Owner != null)
                {
                    if (Owner?.GetType() != typeof(MainWindow))
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                    else
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    }
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            dont.Visibility = Visibility.Hidden;
            Title.Text = title;
            Message.Content = message;
            Folder.Visibility = Visibility.Hidden;
           
            if (title.Contains("Resetting") || message.Contains("NUS format") || message.Contains("Folder contains Files or Subfolders, do you really want to use this") || message.Contains("If using Custom Bases") || title.Contains("Found additional Files"))
            {
                Reset.Visibility = Visibility.Visible;
                if (title.Contains("Resetting"))
                {
                    reset = true;
                }
                if(title.Contains("Found additional Files"))
                {
                    add = true;
                    Reset.Content = "Yes";
                    btnClose.Content = "No";
                }
            }
            if(title.Equals("Image Warning") || message.ToLower().Contains("dsi") ||message.ToLower().Contains("gcz") || message.ToLower().Contains("co-processor"))
            {
                dont.Visibility = Visibility.Visible;
            }
        }

        public void CloseProgram(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }
        public Custom_Message(string title, string message, string Path)
        {           
            try
            {
                if (Owner?.GetType() != typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();
            if (!message.Contains("Nintendont"))
            {
                nc.Visibility = Visibility.Hidden;
            }
            if (message.Contains("If you want the inject to be put on your SD now"))
            {
                if(message.Contains("Copy to SD"))
                {
                    nc.Content = "Copy to SD";
                }
                nc.Visibility = Visibility.Visible;
            }
            dont.Visibility = Visibility.Hidden;
            Title.Text = title;
            Message.Content = message;
            path = Path;
            Folder.Visibility = Visibility.Visible;
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dontChecked();
            Close();
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(path)) path = new FileInfo(path).DirectoryName;
                Process.Start(path);
                Close();
            }
            catch (Exception)
            {
                Custom_Message cm = new Custom_Message("An Error occured", "An error occured opening the folder. Please make sure the Output Path exists.");
                try
                {
                    cm.Owner = (FindResource("mvm") as MainViewModel).mw;
                }catch(Exception)
                {
                    //left empty on purpose
                }
                cm.ShowDialog();
                dontChecked();
                Close();
            }
           
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (reset)
            {
                Close();
                ((MainViewModel)FindResource("mvm")).ResetTitleKeys();
            }
            else if (add)
            {
                ((MainViewModel)FindResource("mvm")).addi = true;
            }
            else
            {
                ((MainViewModel)FindResource("mvm")).choosefolder = true;
            }
            dontChecked();
            Close();
        }

        private void nc_Click(object sender, RoutedEventArgs e)
        {
            Close();
            var containNintendont = Message.Content.ToString().ToLower().Contains("nintendont");

            SDSetup sd = new SDSetup(containNintendont ? true : false, path);
            try
            {
                sd.Owner = (FindResource("mvm") as MainViewModel).mw;
                sd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            catch (Exception)
            {
                //left empty on purpose
            }
            sd.ShowDialog();
        }

        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isWindClosing(true);
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            isWindClosing(false);
        }

        private void isWindClosing(bool topMost)
        {
            try
            {
                if (!Title.Text.Contains("Warning"))
                {
                    if ((FindResource("mvm") as MainViewModel).mw != null)
                        (FindResource("mvm") as MainViewModel).mw.Topmost = topMost;
                }
            }
            catch (Exception)
            {
                //left empty on purpose;
            }
        }

        private void dontChecked()
        {
            if (dont.IsChecked == true)
            {
                var messageLower = Message.Content.ToString().ToLower();
                if (messageLower.Contains("gcz"))
                {
                    Properties.Settings.Default.gczw = true;
                }
                else if (messageLower.Contains("dsi"))
                {
                    Properties.Settings.Default.ndsw = true;
                }
                else if (messageLower.Contains("co-processor"))
                {
                    Properties.Settings.Default.snesw = true;
                }
                else if (messageLower.Contains("images"))
                {
                    Properties.Settings.Default.dont = true;
                }

                Properties.Settings.Default.Save();
            }
        }
    }
}
