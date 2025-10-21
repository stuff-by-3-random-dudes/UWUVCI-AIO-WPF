using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class FeedbackWindow : Window
    {
        public FeedbackWindow()
        {
            InitializeComponent();
            TypeBox.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            string type = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Other";
            string desc = DescriptionBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(desc))
            {
                UWUVCI_MessageBox.Show("Validation Error", "Please provide a detailed description.",
                    UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Warning);
                return;
            }

            try
            {
                const string owner = "stuff-by-3-random-dudes";
                const string repo = "UWUVCI-AIO-WPF";

                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var appVersion = fvi.FileVersion ?? "unknown";

                string fingerprint = null;
                try { fingerprint = DeviceFingerprint.GetHashedFingerprint(); } catch { }

                IsEnabled = false;
                Cursor = System.Windows.Input.Cursors.Wait;

                var issueUrl = await new GitHubFeedbackService().SubmitIssueAsync(owner, repo, type, desc, appVersion);

                UWUVCI_MessageBox.Show("Success",
                    $"Your report has been submitted successfully.\n\nYou can view it here:\n{issueUrl}",
                    UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success);

                Close();
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Error", $"Failed to submit report:\n{ex.Message}",
                    UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error);
            }
            finally
            {
                IsEnabled = true;
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }
    }
}
