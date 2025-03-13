using System;
using System.Windows;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class IntroductionWindow : Window
    {
        private DispatcherTimer timer;
        public IntroductionWindow()
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
            // Navigate to the next window (GuideWindow)
            var guideWindow = new GuideWindow();
            guideWindow.Show();
            Close();
            
        }
    }
}
