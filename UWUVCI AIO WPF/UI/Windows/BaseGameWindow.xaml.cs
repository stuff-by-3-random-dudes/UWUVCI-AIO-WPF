using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class BaseGameWindow : Window
    {
        private DispatcherTimer timer;
        public BaseGameWindow()
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
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the next window (MacLinuxWindow)
            var macLinuxWindow = new MacLinuxWindow();
            macLinuxWindow.Show();
            Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the previous window (GuideWindow)
            var guideWindow = new GuideWindow();
            guideWindow.Show();
            Close();
        }

        private void CompatibilityListButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the compatibility list in the default browser
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://uwuvci.net/",
                UseShellExecute = true
            });
        }
    }
}
