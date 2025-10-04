using System;
using System.Windows;
using UWUVCI_AIO_WPF.Helpers;
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
            StatusBox.SelectedIndex = 2; // default = Works
        }

        // --------------------
        // Event handlers
        // --------------------
        private void ConsoleBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string console = GetSelectedText(ConsoleBox);
            WiiPanel.Visibility = console == "Wii" ? Visibility.Visible : Visibility.Collapsed;
            NdsPanel.Visibility = console == "NDS" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var console = GetSelectedText(ConsoleBox);
                var entry = BuildBaseEntry();

                int? gamepad = null;
                string renderSize = null;

                if (console == "Wii")
                {
                    gamepad = ParseIntFromCombo(GamePadBox, "GamePad option");
                }
                else if (console == "NDS")
                {
                    renderSize = RenderSizeBox.Text?.Trim();
                }

                // repo target (make configurable if you like)
                const string owner = "UWUVCI-PRIME";
                const string repo = "UWUVCI-Compatibility";

                IsEnabled = false;
                Cursor = System.Windows.Input.Cursors.Wait;

                var prUrl = await GitHubCompatService.SubmitEntryAsync(owner, repo, console, entry, gamepad, renderSize);
                MessageBox.Show($"Pull Request created:\n{prUrl}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create PR:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        // --------------------
        // Helpers
        // --------------------
        private string GetSelectedText(System.Windows.Controls.ComboBox box)
            => (box.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "";

        private int ParseIntFromCombo(System.Windows.Controls.ComboBox box, string fieldName)
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
                string.IsNullOrEmpty(baseName) || string.IsNullOrEmpty(baseRegion) || string.IsNullOrEmpty(statusText))
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
                Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? "None" : NotesBox.Text.Trim()
            };
        }
    }
}
