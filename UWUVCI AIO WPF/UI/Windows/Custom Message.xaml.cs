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
        public Custom_Message(string title, string message)
        {
            InitializeComponent();
            


            Title.Text = title;
            Message.Content = message;
            Folder.Visibility = Visibility.Hidden;
            if (title.Contains("Resetting") || message.Contains("NUS format") || message.Contains("Folder contains Files or Subfolders, do you really want to use this") || message.Contains("If using Custom Bases"))
            {
                Reset.Visibility = Visibility.Visible;
                if (title.Contains("Resetting"))
                {
                    reset = true;
                }
            }

        }
        public Custom_Message(string title, string message, string Path)
        {
            InitializeComponent();
            Title.Text = title;
            Message.Content = message;
            this.path = Path;
            Folder.Visibility = Visibility.Visible;
           
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new FileInfo(path).DirectoryName);
            this.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (reset)
            {
                ((MainViewModel)FindResource("mvm")).ResetTitleKeys();
            }
            else
            {
                ((MainViewModel)FindResource("mvm")).choosefolder = true;
            }
            
            this.Close();
        }
    }
}
