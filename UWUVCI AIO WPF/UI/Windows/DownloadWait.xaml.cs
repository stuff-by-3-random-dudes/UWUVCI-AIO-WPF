using System;
using System.Windows;
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
        private TimeSpan remainingTime;
        public DownloadWait(string doing, string msg, MainViewModel mvm)
        {
            try
            {
                if (Owner?.GetType() == typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                if (Owner?.GetType() != typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            this.mvm = mvm;
            InitializeComponent();
            Key.Text = doing;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            
        }

        public DownloadWait(string doing, TimeSpan estimatedTime, MainViewModel mvm)
        {
            // Initialization same as original constructor
            try
            {
                if (Owner?.GetType() == typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            this.mvm = mvm;
            InitializeComponent();
            Key.Text = doing;

            // Handle estimated time
            if (estimatedTime != TimeSpan.MaxValue)
                remainingTime = estimatedTime;
            else
                // Can't estimate, just starting with zero
                remainingTime = TimeSpan.Zero; 

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            msgT.Text = mvm.msg;
            pb.Value = mvm.Progress;
            if(Key.Text.Contains("Downloading Base"))
            {
                // Check if remainingTime has been initialized (i.e., not zero)
                if (remainingTime != TimeSpan.Zero)
                {
                    msgT.Text += $"\nEstimated time remaining: {remainingTime.Minutes} minutes {remainingTime.Seconds} seconds";

                    if (remainingTime.TotalSeconds > 0)
                    {
                        remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                    }
                }

                if (mvm.Progress < 70)
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
            Owner = ow;
            try
            {
                if (Owner?.GetType() == typeof(MainWindow))
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    ShowInTaskbar = false;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            try
            {
                if ((FindResource("mvm") as MainViewModel).mw != null)
                {
                    (FindResource("mvm") as MainViewModel).mw.Topmost = false;
                }
            }
            catch (Exception )
            {

            }
        }
    }
}
