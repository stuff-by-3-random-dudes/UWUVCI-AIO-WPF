using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                if(this.Owner != null)
                {
                    if (this.Owner.GetType() != typeof(MainWindow))
                    {
                        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                    else
                    {
                        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    }
                }
               
               
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
            if (title.ToLower().Contains("instance"))
            {
               
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
                if (this.Owner.GetType() != typeof(MainWindow))
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
            this.path = Path;
            Folder.Visibility = Visibility.Visible;
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(dont.IsChecked == true)
            {
                if (Message.Content.ToString().ToLower().Contains("gcz"))
                {
                    Properties.Settings.Default.gczw = true;
                }
                else if (Message.Content.ToString().ToLower().Contains("dsi"))
                {
                    Properties.Settings.Default.ndsw = true;
                }
                else if (Message.Content.ToString().ToLower().Contains("co-processor"))
                {
                    Properties.Settings.Default.snesw = true;
                }
                else if (Message.Content.ToString().ToLower().Contains("images"))
                {
                    Properties.Settings.Default.dont = true;
                }

                Properties.Settings.Default.Save();
            }
            this.Close();
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(path)) path = new FileInfo(path).DirectoryName;
                Process.Start(path);
                this.Close();
            }
            catch (Exception)
            {
                Custom_Message cm = new Custom_Message("An Error occured", "An error occured opening the folder. Please make sure the Output Path exists.");
                try
                {
                    cm.Owner = (FindResource("mvm") as MainViewModel).mw;
                }catch(Exception)
                {

                }
                cm.ShowDialog();
                if (dont.IsChecked == true)
                {
                    if (Message.Content.ToString().ToLower().Contains("gcz"))
                    {
                        Properties.Settings.Default.gczw = true;
                    }
                    else if (Message.Content.ToString().ToLower().Contains("dsi"))
                    {
                        Properties.Settings.Default.ndsw = true;
                    }
                    else if (Message.Content.ToString().ToLower().Contains("co-processor"))
                    {
                        Properties.Settings.Default.snesw = true;
                    }
                    else if (Message.Content.ToString().ToLower().Contains("images"))
                    {
                        Properties.Settings.Default.dont = true;
                    }
                    Properties.Settings.Default.Save();
                }
                this.Close();
            }
           
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (reset)
            {
                this.Close();
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
            if (dont.IsChecked == true)
            {
                if (Message.Content.ToString().ToLower().Contains("gcz"))
                {
                    Properties.Settings.Default.gczw = true;
                }
                else if (Message.Content.ToString().ToLower().Contains("dsi"))
                {
                    Properties.Settings.Default.ndsw = true;
                }
                else if (Message.Content.ToString().ToLower().Contains("co-processor"))
                {
                    Properties.Settings.Default.snesw = true;
                }
                else if (Message.Content.ToString().ToLower().Contains("images"))
                {
                    Properties.Settings.Default.dont = true;
                }
                Properties.Settings.Default.Save();
            }
            this.Close();
        }

        private void nc_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (Message.Content.ToString().ToLower().Contains("nintendont"))
            {
               SDSetup sd = new SDSetup(true, path);
                try
                {
                    sd.Owner = (FindResource("mvm") as MainViewModel).mw;
                    sd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                catch (Exception)
                {

                }
               sd.ShowDialog();
            }
            else
            {
                SDSetup sd = new SDSetup(false, path);
                try
                {
                    sd.Owner = (FindResource("mvm") as MainViewModel).mw;
                    sd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                catch (Exception)
                {

                }
                sd.ShowDialog();
            }
        }

        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            try
            {
                if((FindResource("mvm") as MainViewModel).mw != null)
                (FindResource("mvm") as MainViewModel).mw.Topmost = true;
            }
            catch (Exception ) 
            { 
            }

            
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            try
            {
                if ((FindResource("mvm") as MainViewModel).mw != null)
                    (FindResource("mvm") as MainViewModel).mw.Topmost = false;
            }
            catch (Exception)
            {
            }

        }
    }
}
