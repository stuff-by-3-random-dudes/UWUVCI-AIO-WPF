using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class GuideWindow : Window
    {
        private DispatcherTimer timer;
        public GuideWindow()
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
            // Navigate to the next window (BaseGameWindow)
            var baseGameWindow = new BaseGameWindow();
            baseGameWindow.Show();
            Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the previous window (IntroWindow)
            var introWindow = new IntroductionWindow();
            introWindow.Show();
            Close();
        }

        private void ReadMeButton_Click(object sender, RoutedEventArgs e)
        {
            // Open ReadMe file in the default text editor
            string readMePath = Path.Combine(Directory.GetCurrentDirectory(), "ReadMe.txt");
            if (File.Exists(readMePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = readMePath,
                    UseShellExecute = true
                });
            }
            else
            {
                UWUVCI_MessageBox.Show("ReadMe.txt not found!", "Error", UWUVCI_MessageBoxType.Ok);
            }
        }

        private void ZestyGuideButton_Click(object sender, RoutedEventArgs e)
        {
            // Open Zesty's Corner video guide in the default browser
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/watch?v=8HddnYFRZDE&list=PLbQMtrmXFIxQ1hpvu9m1th41vsaqnZ2Id&index=8",
                UseShellExecute = true
            });
        }
    }
}
