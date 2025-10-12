using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UWUVCI_AIO_WPF.Services;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class CompatEntryWindow : Window
    {
        public CompatEntryWindow()
        {
            InitializeComponent();
            InitializeDefaults();
        }

        // --------------------
        // Initialization
        // --------------------
        private void InitializeDefaults()
        {
            GameRegionBox.SelectedIndex = 0;
            BaseRegionBox.SelectedIndex = 0;
            StatusBox.SelectedIndex = 2; // Default = Works
        }

        // --------------------
        // Event handlers
        // --------------------
        private void ConsoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string console = GetSelectedText(ConsoleBox);
            WiiPanel.Visibility = console == "Wii" ? Visibility.Visible : Visibility.Collapsed;
            NdsPanel.Visibility = console == "NDS" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RenderSizeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var appVersion = fvi.FileVersion ?? "unknown";

                var console = GetSelectedText(ConsoleBox);
                if (string.IsNullOrEmpty(console))
                {
                    UWUVCI_MessageBox.Show(
                        "Validation Error",
                        "Please select a console.",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Warning
                    );

                    return;
                }

                var entry = BuildBaseEntry();

                // Append version note to Notes
                entry.Notes = $"{entry.Notes} (UWUVCI Version: {appVersion})".Trim();

                int? gamepad = null;
                string renderSize = null;

                if (console == "Wii")
                {
                    gamepad = ParseIntFromCombo(GamePadBox, "GamePad option");
                }
                else if (console == "NDS")
                {
                    var input = RenderSizeBox.Text?.Trim();
                    renderSize = string.IsNullOrEmpty(input) ? "" : $"{input}x";
                }

                // Repo target (you can move these to settings later)
                const string owner = "UWUVCI-Prime";
                const string repo = "UWUVCI-Compatibility";

                IsEnabled = false;
                Cursor = Cursors.Wait;

                var prUrl = await GitHubCompatService.SubmitEntryAsync(owner, repo, console, entry, gamepad, renderSize, appVersion);

                UWUVCI_MessageBox.Show(
                    "Success",
                    $"Pull Request created successfully:\n{prUrl}",
                    UWUVCI_MessageBoxType.Ok,
                    UWUVCI_MessageBoxIcon.Success,
                    null,
                    isModal: false
                );

                Close();
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show(
                    "Error",
                    "Failed to create PR:\n" + ex.Message,
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

        // --------------------
        // Helpers
        // --------------------
        private string GetSelectedText(ComboBox box)
            => (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        private int ParseIntFromCombo(ComboBox box, string fieldName)
        {
            var text = GetSelectedText(box);
            if (!int.TryParse(text.Substring(0, 1), out var val))
                throw new InvalidOperationException($"Please select a valid {fieldName}.");
            return val;
        }

        private GameCompatEntry BuildBaseEntry()
        {
            var gameName = GameNameBox.Text?.Trim();
            var gameRegion = GetSelectedText(GameRegionBox);
            var baseName = BaseNameBox.Text?.Trim();
            var baseRegion = GetSelectedText(BaseRegionBox);
            var statusText = GetSelectedText(StatusBox);

            if (string.IsNullOrEmpty(gameName) || string.IsNullOrEmpty(gameRegion) ||
                string.IsNullOrEmpty(baseName) || string.IsNullOrEmpty(baseRegion) ||
                string.IsNullOrEmpty(statusText))
            {
                throw new InvalidOperationException("All required fields must be completed.");
            }

            if (!int.TryParse(statusText.Substring(0, 1), out var status))
                throw new InvalidOperationException("Invalid status value.");

            return new GameCompatEntry
            {
                GameName = gameName,
                GameRegion = gameRegion,
                BaseName = baseName,
                BaseRegion = baseRegion,
                Status = status,
                Notes = NotesBox.Text?.Trim()
            };
        }
    }
}
