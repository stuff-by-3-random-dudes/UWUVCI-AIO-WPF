using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class CloseWindow : Window
    {
        private DispatcherTimer timer;

        public CloseWindow()
        {
            InitializeComponent();

            // Disable the Next button initially
            NextButton.IsEnabled = false;

            // Create a dispatcher timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // Set the timer for 3 seconds
            timer.Tick += Timer_Tick; // Subscribe to the Tick event
            timer.Start(); // Start the timer
        }

        // Event triggered when the timer ticks (after 3 seconds)
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Enable the Next button
            NextButton.IsEnabled = true;

            // Stop the timer after it has run once
            timer.Stop();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //Clicked button to see tutorial
            if (JsonSettingsManager.Settings.IsFirstLaunch)
            {
                // Close the tutorial by marking the first launch as false
                JsonSettingsManager.Settings.IsFirstLaunch = false;

                // Save settings
                JsonSettingsManager.SaveSettings();

                // Call the LaunchMainApplication method directly from the current application instance
                ((App)Application.Current).LaunchMainApplication();
            }
            
            Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the previous window
            var baseGameWindow = new BaseGameWindow();
            baseGameWindow.Show();
            Close();
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the Discord invite link in the default browser
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/mPZpqJJVmZ",
                UseShellExecute = true
            });
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            // Open donation page in the default browser
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/zestyts",
                UseShellExecute = true
            });
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            // Open YT game page in the default browser
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/watch?v=RcfVhFi3wJc",
                UseShellExecute = true
            });
        }
    }
}
