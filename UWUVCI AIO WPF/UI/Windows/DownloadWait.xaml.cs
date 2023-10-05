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

        //These variables are for handling a better progress bar
        private TimeSpan remainingTime;
        private int motion = 1;
        private double accumulatedProgress = 0.0;
        private double progressIncrementPerSecond = 0.0;
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
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
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

            if (Key.Text.Contains("Downloading Base"))
            {
                if (mvm.Progress >= 96)
                {
                    msgT.Text += $"Verifying download...";
                    
                    if (motion == 6)
                        motion = 1;

                    for (var i = 0; i < motion; i++)
                        msgT.Text += ".";

                    motion++;
                }
                // Check if remainingTime has been initialized (i.e., not zero)
                else if (remainingTime != TimeSpan.Zero)
                {
                    if (remainingTime.TotalSeconds > 0)
                    {
                        msgT.Text += $"Estimated time remaining: {remainingTime.Minutes} minutes {remainingTime.Seconds} seconds";

                        if (mvm.Progress < 95)
                        {
                            // Calculate the progress increment if not already calculated
                            if (progressIncrementPerSecond == 0.0)
                                progressIncrementPerSecond = (95 - mvm.Progress) / remainingTime.TotalSeconds;

                            accumulatedProgress += progressIncrementPerSecond;

                            while (accumulatedProgress >= 1)
                            {
                                mvm.Progress++;
                                accumulatedProgress--;
                            }

                            remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                        }
                    }
                    else
                    {
                        msgT.Text += $"Completing download, eta not available";

                        if (motion == 6)
                            motion = 1;

                        for (var i = 0; i < motion; i++)
                            msgT.Text += ".";

                        motion++;
                    }
                }
                else
                {
                    if (mvm.Progress < 95)
                        mvm.Progress += 1;
                }
            }
            if (mvm.Progress == 100)
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
