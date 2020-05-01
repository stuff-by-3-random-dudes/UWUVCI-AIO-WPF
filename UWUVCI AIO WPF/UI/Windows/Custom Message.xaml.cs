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
    public partial class Custom_Message : Window
    {
        string path;
        bool reset = false;
        bool add = false;
        public Custom_Message(string title, string message)
        {
            
            InitializeComponent();
            try
            {
                if(this.Owner != null)
                {
                    if (this.Owner.GetType() != typeof(MainWindow))
                    {
                        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                    
                }
            }
            if(title.Equals("Image Warning"))
            {
                dont.Visibility = Visibility.Visible;
            }

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
                Properties.Settings.Default.dont = true;
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
                    Properties.Settings.Default.dont = true;
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
                Properties.Settings.Default.dont = true;
                Properties.Settings.Default.Save();
            }
            this.Close();
        }
    }
}
