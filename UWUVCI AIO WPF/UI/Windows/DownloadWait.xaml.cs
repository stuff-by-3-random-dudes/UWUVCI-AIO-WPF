using System;
using System.Threading;
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
        private int stallCounter = 0;
        private double lastProgress = -1;
        private readonly CancellationTokenSource _cts;

        public DownloadWait(string doing, string msg, MainViewModel mvm, bool showCancel = false)
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

            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        public DownloadWait(string doing, MainViewModel mvm, CancellationTokenSource cts, bool showCancel = false)
        {
            InitializeComponent();
            this.mvm = mvm;
            _cts = cts;
            Key.Text = doing;

            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            msgT.Text = "Cancelling…";
            CancelButton.IsEnabled = false;
            pb.IsIndeterminate = true;

            timer.Stop();
            Close();
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            timer.Stop();
            try
            {
                if ((FindResource("mvm") as MainViewModel).mw != null)
                    (FindResource("mvm") as MainViewModel).mw.Topmost = false;
            }
            catch (Exception)
            {

            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            msgT.Text = mvm.msg;

            if (Key.Text.Contains("Downloading Base"))
            {
                // Map actual downloader progress (0–100) into the 20–80% range
                double shownProgress = 20 + (mvm.Progress * 0.6);

                if (shownProgress > 80)
                    shownProgress = 80;
                if (shownProgress < 20)
                    shownProgress = 20;

                pb.Value = shownProgress;

                if (mvm.Progress >= 96)
                {
                    msgT.Text += " Verifying download";
                    if (motion == 6) motion = 1;
                    for (var i = 0; i < motion; i++)
                        msgT.Text += ".";
                    motion++;
                }
                else if (remainingTime != TimeSpan.Zero)
                {
                    if (remainingTime.TotalSeconds > 0)
                    {
                        msgT.Text += $" Estimated time remaining: {remainingTime.Minutes}m {remainingTime.Seconds}s";

                        if (mvm.Progress < 95)
                        {
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
                        msgT.Text += " Completing download, ETA not available";
                        if (motion == 6) motion = 1;
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
            else
            {
                // Non-base downloads just bind raw progress
                pb.Value = mvm.Progress;
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
    }
}
