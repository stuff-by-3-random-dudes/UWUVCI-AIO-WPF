using System;
using System.Windows;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class MacLinuxWindow : Window
    {
        private DispatcherTimer timer;
        public MacLinuxWindow()
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
            // Navigate to the next window (CloseWindow)
            var closeWindow = new CloseWindow();
            closeWindow.Show();
            Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the previous window (BaseGameWindow)
            var baseGameWindow = new BaseGameWindow();
            baseGameWindow.Show();
            Close();
        }
    }
}
