using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UWUVCI_AIO_WPF.Services;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class FeedbackWindow : Window
    {
        public FeedbackWindow()
        {
            InitializeComponent();

            // Wire up dropdown change
            TypeBox.SelectionChanged += TypeBox_SelectionChanged;

            // Default checkbox states
            IncludeLogFileBox.IsChecked = true;
            IncludeSystemInfoBox.IsChecked = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string description = DescriptionBox.Text?.Trim();
                string type = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

                if (string.IsNullOrEmpty(description))
                {
                    UWUVCI_MessageBox.Show(
                        "Validation Error",
                        "Please enter a description before submitting.",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Warning
                    );
                    return;
                }

                // Gather app version
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var appVersion = fvi.FileVersion ?? "unknown";

                // Check what to include
                bool includeSys = IncludeSystemInfoBox.IsChecked == true;
                bool includeLog = IncludeLogFileBox.IsChecked == true;

                // Log directory (not file)
                string logDir = includeLog
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI-V3", "Logs")
                    : null;

                // Disable window during submission
                IsEnabled = false;
                Cursor = Cursors.Wait;

                const string owner = "stuff-by-3-random-dudes";
                const string repo = "UWUVCI-AIO-WPF";

                var service = new GitHubFeedbackService();
                string issueUrl = await service.SubmitIssueAsync(
                    owner,
                    repo,
                    type,
                    description,
                    appVersion,
                    includeSys,
                    logDir
                );

                if (string.IsNullOrEmpty(issueUrl))
                {
                    UWUVCI_MessageBox.Show(
                        "Access Restricted",
                        "Your device is not allowed to submit feedback or reports.\n\nIf you believe this is an error, please contact support.",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Error,
                        this
                    );
                }
                else
                {
                    UWUVCI_MessageBox.Show(
                        "Success",
                        $"Feedback submitted successfully!\n\n{issueUrl}",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Success,
                        this
                    );
                }

                Close();
            }
            catch (Exception ex)
            {
                try { Logger.Log("Feedback submission error: " + ex.ToString()); } catch { }
                UWUVCI_MessageBox.Show(
                    "Error",
                    "Failed to submit feedback:\n" + ex.Message,
                    UWUVCI_MessageBoxType.Ok,
                    UWUVCI_MessageBoxIcon.Error
                );
            }
            finally
            {
                IsEnabled = true;
                Cursor = Cursors.Arrow;
            }
        }

        // ----------------------------
        // Auto-toggle behavior
        // ----------------------------
        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (selected == "Bug Report")
            {
                IncludeSystemInfoBox.IsChecked = true;
                IncludeLogFileBox.IsChecked = true;
            }
            else
            {
                IncludeSystemInfoBox.IsChecked = false;
                IncludeLogFileBox.IsChecked = false;
            }
        }
    }
}
