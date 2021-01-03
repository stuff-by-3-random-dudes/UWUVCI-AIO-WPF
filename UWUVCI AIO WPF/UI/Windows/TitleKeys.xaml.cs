using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für TitleKeys.xaml
    /// </summary>
    public partial class TitleKeys : Window
    {
        string url = "";
        public TitleKeys(string url)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
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
            string fullurl = "";
            try
            {
                fullurl = mvm.GetURL(url);
                if (fullurl == "" || fullurl == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                Custom_Message cm = new Custom_Message("Not Implemented", $"The Helppage for {url.ToUpper()} is not implemented yet");
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception)
                {
                    // left empty on purpose
                }
                cm.ShowDialog();
                Close();
                mvm.mw.Show();
            }
            
            wb.Source = new Uri(fullurl, UriKind.Absolute);
            wb.Refresh(true);

            clsWebbrowser_Errors.SuppressscriptErrors(wb, true);
            InitializeComponent();

            this.url = url;
        }
        public TitleKeys(string url, string title)
        {
            InitializeComponent();
            wb.Visibility = Visibility.Hidden;
            Thread t = new Thread(() => DoStuff(url));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            tbTitleBar.Text = title.Replace(" Inject "," ");
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Owner.PointToScreen(new Point(Left, Top));
                    DragMove();
                   // this.Owner.PointFromScreen(new Point(this.Left, this.Top));
                    //(FindResource("mvm") as MainViewModel).mw.PointFromScreen(new Point(this.Left, this.Top));
                }
                   
                //PointFromScreen(new Point(this.Left, this.Top));
               
            }
            catch (Exception)
            {
                // left empty on purpose
            }
        }

        public void DoStuff(string url)
        {
            MainViewModel mvm = FindResource("mvm") as MainViewModel;
            string fullurl = "";
            try
            {
                fullurl = mvm.GetURL(url);
                if (string.IsNullOrEmpty(fullurl))
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Custom_Message cm = new Custom_Message("Not Implemented", $"The Helppage for {url.ToUpper()} is not implemented yet");
                try
                {
                    cm.Owner = mvm.mw;
                }
                catch (Exception)
                {
                    // left empty on purpose
                }
                cm.Show();
                Close();
                mvm.mw.Show();
            }

            Dispatcher.Invoke(() =>
            {
                wb.Source = new Uri(fullurl, UriKind.Absolute);
                clsWebbrowser_Errors.SuppressscriptErrors(wb, true);
            });
            
            /*dynamic activeX = this.wb.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, this.wb, new object[] { });

            activeX.Silent = true;*/
           
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Owner.Left =Left;
            Owner.Top = Top;
            Owner.Show();
            Close();
        }

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void wb_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            load.Visibility = Visibility.Hidden;
            wb.Visibility = Visibility.Visible;
        }


      
        private void wb_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().Contains("flumpster"))
            {
                e.Cancel = true;

                var startInfo = new ProcessStartInfo
                {
                    FileName = e.Uri.ToString()
                };

                Process.Start(startInfo);
            }
            
            // cancel navigation to the clicked link in the webBrowser control
        }

        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = true;
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = false;
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
