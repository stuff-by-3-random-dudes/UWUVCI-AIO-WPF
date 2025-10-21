using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace UWUVCI_AIO_WPF.UI.Frames
{
    /// <summary>
    /// Interaktionslogik für StartFrame.xaml
    /// </summary>
    public partial class StartFrame : Page
    {
        public StartFrame()
        {
            InitializeComponent();
            tb.Text += "\n\n Want to go through the tutorial again? Click on the gear icon, and click 'Show Tutorial Screens'.";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("https://ko-fi.com/nicoaicp");
            Button_Click2(sender, e);
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ko-fi.com/zestyts");
        }
    }
}