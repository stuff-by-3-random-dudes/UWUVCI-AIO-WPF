using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für DownloadWait.xaml
    /// </summary>
   
    partial class DownloadWait : Window
    {
        MainViewModel mvm;
        DispatcherTimer timer = new DispatcherTimer();
        public DownloadWait(string doing, string msg, MainViewModel mvm)
        {
            try
            {
                if (this.Owner.GetType() == typeof(MainWindow))
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            this.mvm = mvm;
            InitializeComponent();
            Key.Text = doing;
           
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        public DownloadWait(string doing, string msg, MainViewModel mvm, bool t)
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
            this.mvm = mvm;
            InitializeComponent();
            Key.Text = doing;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            
        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
           
        }
        private void min_MouseLeave(object sender, MouseEventArgs e)
        {
         
        }
        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            msgT.Text = mvm.msg;
            pb.Value = mvm.Progress;
            if(Key.Text.Contains("Downloading Base"))
            {
                if(mvm.Progress < 70)
                {
                    mvm.Progress += 1;
                }
            }
            if(mvm.Progress == 100)
            {
                timer.Stop();
                Close();
                
            }
        }
        public void changeOwner(MainWindow ow)
        {
            this.Owner = ow;
            try
            {
                if (this.Owner.GetType() == typeof(MainWindow))
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    this.ShowInTaskbar = false;
                }
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
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
}
