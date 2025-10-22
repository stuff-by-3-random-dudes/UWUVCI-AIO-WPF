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
                    BodyBlock.Text = "Welcome to the UWUVCI V3 tutorial! This short guide walks you through key setup and community info before you begin injecting. After this tutorial, you won't see it again unless a major update is installed.";
                    DetailsBlock.Text = "This tool helps you create Virtual Console Injects (VCI) for consoles: NDS, GBA, N64, SNES, NES, TG16, MSX, Wii, and GCN.";
                    HelperNote.Text = "Notes: GCN does not use VC, instead it uses Nintendont. Wii allows for creating Homebrew and WAD forwarders.";
                    break;

                case 1:
                    BodyBlock.Text = "The only official videos for UWUVCI V3 can be found on Zesty's Corner YouTube channel, this includes setup, expectations, and supplment information. Click the button below to check it out.";
                    DetailsBlock.Text = "There is also a ReadMe with useful information like the FAQ, which includes the video guide URL. The ReadMe file is located in the same directory as the executable, and is the same ReadMe file that was recommended in the installer to open.";
                    HelperNote.Text = "The ReadMe can also be found by clicking the gear icon at the top right once you get into the app.";
                    ZestyButton.Visibility = Visibility.Visible;
                    ReadMeButton.Visibility = Visibility.Visible;
                    break;

                case 2:
                    BodyBlock.Text = "A Base is an eshop game required for the inject. Outside of GCN and Wii games, the base matters.";
                    DetailsBlock.Text = "Check the compatibility list on the official UWUVCI site, the FAQ, the discord, or from the '?' button in the app.";
                    HelperNote.Text = "If no entry exists for a base, it has not been documented yet.";
                    DiscordButton.Visibility = Visibility.Visible;
                    ReadMeButton.Visibility = Visibility.Visible;
                    break;

                case 3:
                    BodyBlock.Text = "UWUVCI V3 is a Windows-native WPF application, but support has been added for Unix (mac/Linux) so it should be near feature parity.";
                    DetailsBlock.Text = "V3.200 is the update that brough it altogether.";
                    HelperNote.Text = "If you got this update for free, it'll be cool if you could donate or check out a game I made.";
                    DonateButton.Visibility = Visibility.Visible;
                    GameButton.Visibility = Visibility.Visible;
                    PatchNotesButton.Visibility = Visibility.Visible;
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
                var agreementNotChecked = AgreementPanel.Visibility == Visibility.Visible &&
                    (AgreementCheck.IsChecked == false|| 
                    AgreementCheck2.IsChecked == false || 
                    AgreementCheck3.IsChecked == false);

                if (agreementNotChecked)
                {
                    UWUVCI_MessageBox.Show("Confirmation Required","Please check all checkboxes to show agreement and understanding of the tutorial.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Info, this);
                    return;
                }

                // Determine if this was launched manually or automatically
                if (Tag as string != "manual")
                {
                    JsonSettingsManager.Settings.HasAcknowledgedTutorial = true;
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
