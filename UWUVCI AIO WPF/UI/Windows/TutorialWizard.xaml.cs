using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class TutorialWizard : Window
    {
        private int currentStep = 0;
        private readonly string[] steps =
        {
            "Introduction",
            "Video & FAQ",
            "Base Game Info",
            "Mac / Linux Users",
            "Community Guidelines"
        };

        public TutorialWizard()
        {
            InitializeComponent();
            StepList.ItemsSource = steps;
            UpdateStep();
        }

        private void UpdateStep()
        {
            // Reset visibility for optional buttons
            ZestyButton.Visibility = Visibility.Collapsed;
            DiscordButton.Visibility = Visibility.Collapsed;
            DonateButton.Visibility = Visibility.Collapsed;
            GameButton.Visibility = Visibility.Collapsed;
            ReadMeButton.Visibility = Visibility.Collapsed;
            PatchNotesButton.Visibility = Visibility.Collapsed;
            AgreementPanel.Visibility = Visibility.Collapsed;

            TitleBlock.Text = steps[currentStep];
            ProgressBar.Value = (currentStep + 1) * (100 / steps.Length);

            switch (currentStep)
            {
                case 0:
                    BodyBlock.Text = "Welcome to the UWUVCI V3 tutorial! This short guide walks you through key setup and community info before you begin injecting.";
                    DetailsBlock.Text = "After this tutorial, you won't see it again unless a major update is installed.";
                    HelperNote.Text = "Click Next to continue.";
                    break;

                case 1:
                    BodyBlock.Text = "Official video guides and FAQs are hosted on Zesty's Corner YouTube channel and the included ReadMe.";
                    DetailsBlock.Text = "The ReadMe includes compatibility info, troubleshooting tips, and detailed explanations for each console type.";
                    HelperNote.Text = "Please review both the ReadMe and the official videos before using UWUVCI.";
                    ZestyButton.Visibility = Visibility.Visible;
                    DiscordButton.Visibility = Visibility.Visible;
                    ReadMeButton.Visibility = Visibility.Visible;
                    break;

                case 2:
                    BodyBlock.Text = "Every inject requires a base game from the eShop. The base determines compatibility.";
                    DetailsBlock.Text = "Check the compatibility list on the official UWUVCI site or from the '?' button in the app.";
                    HelperNote.Text = "If no entry exists for a base, it has not been documented yet.";
                    PatchNotesButton.Visibility = Visibility.Visible;
                    break;

                case 3:
                    BodyBlock.Text = "UWUVCI V3 is a Windows-native WPF application, but support exists for macOS and Linux via helper tools.";
                    DetailsBlock.Text = "The helper app enables GameCube and Wii functionality on non-Windows platforms.";
                    HelperNote.Text = "If the helper doesn't launch automatically, instructions will appear explaining what to do.";
                    break;

                case 4:
                    BodyBlock.Text = "UWUVCI collects a lightweight fingerprint (MAC address, computer name, username) for community submission tracking.";
                    DetailsBlock.Text = "By proceeding, you agree that you’ve read this tutorial, understand and accept the fingerprint policy and ReadMe.";
                    HelperNote.Text = "Your fingerprint is used only for moderation and abuse prevention in community uploads.";
                    AgreementPanel.Visibility = Visibility.Visible;
                    ZestyButton.Visibility = Visibility.Visible;
                    DiscordButton.Visibility = Visibility.Visible;
                    DonateButton.Visibility = Visibility.Visible;
                    GameButton.Visibility = Visibility.Visible;
                    PatchNotesButton.Visibility = Visibility.Visible;
                    break;
            }

            BackButton.IsEnabled = currentStep > 0;
            if (currentStep == steps.Length - 1)
                NextButton.Content = "Finish";
            else
                NextButton.Content = "Next";
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep < steps.Length - 1)
            {
                currentStep++;
                UpdateStep();
            }
            else
            {
                if (AgreementPanel.Visibility == Visibility.Visible && AgreementCheck.IsChecked != true)
                {
                    MessageBox.Show("Please confirm you have read and understood the tutorial and FAQ before continuing.", "Confirmation Required");
                    return;
                }

                // Determine if this was launched manually or automatically
                if (Tag as string != "manual")
                {
                    JsonSettingsManager.Settings.IsFirstLaunch = false;
                    JsonSettingsManager.SaveSettings();
                    ((App)Application.Current).LaunchMainApplication();
                }

                Close();
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > 0)
            {
                currentStep--;
                UpdateStep();
            }
        }

        private void PatchNotesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var patchWindow = new HelpWindow("patchnotes")
                {
                    Owner = Application.Current.MainWindow
                };
                patchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show(
                    "Error",
                    "Failed to load the Patch Notes. Attempting to open the local copy.\n\n" + ex.Message,
                    UWUVCI_MessageBoxType.Ok,
                    UWUVCI_MessageBoxIcon.Error
                );

                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "PatchNotes.txt");
                if (File.Exists(localPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = localPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    UWUVCI_MessageBox.Show(
                        "Error",
                        "Local PatchNotes.txt not found either!",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Warning
                    );
                }
            }
        }


        private void ZestyButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/@ZestysCorner",
                UseShellExecute = true
            });
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/mPZpqJJVmZ",
                UseShellExecute = true
            });
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/zestyts",
                UseShellExecute = true
            });
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.youtube.com/watch?v=RcfVhFi3wJc",
                UseShellExecute = true
            });
        }

        private void ReadMeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var helpWindow = new HelpWindow()
                {
                    Owner = Application.Current.MainWindow
                };
                helpWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show(
                    "Error",
                    "Failed to load the online ReadMe. Attempting to open the local copy.\n\n" + ex.Message,
                    UWUVCI_MessageBoxType.Ok,
                    UWUVCI_MessageBoxIcon.Error
                );

                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "ReadMe.txt");
                if (File.Exists(localPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = localPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    UWUVCI_MessageBox.Show(
                        "Error",
                        "Local ReadMe.txt not found either!",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Warning
                    );
                }
            }
        }
    }
}
