using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using UWUVCI_AIO_WPF.UI.Frames;
using UWUVCI_AIO_WPF.UI.Frames.KeyFrame;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für TitleKeys.xaml
    /// </summary>
    public partial class TitleKeys : Window
    {
        public TitleKeys(string url)
        {
            InitializeComponent();
            wb.Source = new Uri(url, UriKind.Absolute);
            /*dynamic activeX = this.wb.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, this.wb, new object[] { });

            activeX.Silent = true;*/
            clsWebbrowser_Errors.SuppressscriptErrors(wb, true);

        }
        public TitleKeys(string url, string title)
        {
            InitializeComponent();
            wb.Source = new Uri(url, UriKind.Absolute);
            /*dynamic activeX = this.wb.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, this.wb, new object[] { });

            activeX.Silent = true;*/
            clsWebbrowser_Errors.SuppressscriptErrors(wb, true);
            tbTitleBar.Text = title;
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch (Exception)
            {

            }
        }


        private void Window_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
    public static class clsWebbrowser_Errors

    {

        //*set wpf webbrowser Control to silent

        //*code source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/4f686de1-8884-4a8d-8ec5-ae4eff8ce6db



        public static void SuppressscriptErrors(this WebBrowser webBrowser, bool hide)

        {

            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fiComWebBrowser == null)

                return;

            object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);

            if (objComWebBrowser == null)

                return;



            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });

        }

    }
}
