using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Services;

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
            StatusBox.SelectedIndex = 2; // Default = Works

            ConsoleBox.SelectedIndex = 0;
            string defaultConsole = GetSelectedText(ConsoleBox);
            LoadBasesForConsole(defaultConsole);
        }

        // --------------------
        // Event handlers
        // --------------------
        private void ConsoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string console = GetSelectedText(ConsoleBox);
            WiiPanel.Visibility = console == "Wii" ? Visibility.Visible : Visibility.Collapsed;
            NdsPanel.Visibility = console == "NDS" ? Visibility.Visible : Visibility.Collapsed;

            // Load the appropriate base list
            LoadBasesForConsole(console);
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

        private List<string> GetBasesForConsole(GameConsoles console)
        {
            try
            {
                string baseFilePath = Path.Combine("bin", "bases", $"bases.vcb{console.ToString().ToLower()}");

                if (!File.Exists(baseFilePath))
                    throw new FileNotFoundException($"Base file not found: {baseFilePath}");

                // Use your existing logic (assuming VCBTool.ReadBasesFromVCB is static)
                var tempBases = VCBTool.ReadBasesFromVCB(baseFilePath);

                var list = new List<string>
                {
                    "Custom"
                };

                // Append normal bases with region suffix
                list.AddRange(tempBases.Select(b => b.Name == "Custom" ? b.Name : $"{b.Name} {b.Region}"));
                return list;
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Error", $"Failed to load bases:\n{ex.Message}",
                    UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error);
                return new List<string> { "Custom" };
            }
        }

        private void LoadBasesForConsole(string consoleName)
        {
            if (string.IsNullOrWhiteSpace(consoleName))
                return;

            try
            {
                if (!Enum.TryParse(consoleName, true, out GameConsoles console))
                    return;

                var bases = GetBasesForConsole(console);

                BaseBox.Items.Clear();
                foreach (var baseName in bases)
                    BaseBox.Items.Add(baseName);

                BaseBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Error", $"Failed to load base list:\n{ex.Message}",
                    UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error);
            }
        }

        private GameCompatEntry BuildBaseEntry()
        {
            var gameName = GameNameBox.Text?.Trim();
            var gameRegion = GetSelectedText(GameRegionBox);
            var selectedBase = BaseBox.SelectedItem?.ToString()?.Trim();
            var statusText = GetSelectedText(StatusBox);

            // If user picked "Custom", get their manual entry
            var baseName = (selectedBase?.Equals("Custom", StringComparison.OrdinalIgnoreCase) ?? false)
                ? CustomBaseBox.Text?.Trim()
                : selectedBase;

            // Parse region from the base name (expected format: "BaseName REGION")
            string baseRegion = "Unknown";
            if (!string.IsNullOrEmpty(baseName) && !baseName.Equals("Custom", StringComparison.OrdinalIgnoreCase))
            {
                var parts = baseName.Split(' ');
                if (parts.Length > 1)
                {
                    baseRegion = parts[parts.Length - 1]; // Last word (e.g., "USA")
                    baseName = string.Join(" ", parts.Take(parts.Length - 1)); // All but last
                }
            }

            // --- VALIDATION ---
            if (string.IsNullOrEmpty(gameName) ||
                string.IsNullOrEmpty(gameRegion) ||
                string.IsNullOrEmpty(baseName) ||
                string.IsNullOrEmpty(statusText))
            {
                throw new InvalidOperationException("All required fields must be completed.");
            }

            if (selectedBase?.Equals("Custom", StringComparison.OrdinalIgnoreCase) == true &&
                string.IsNullOrEmpty(CustomBaseBox.Text?.Trim()))
            {
                throw new InvalidOperationException("Please enter a name for your custom base.");
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


        private void BaseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaseBox.SelectedItem == null)
                return;

            string selected = BaseBox.SelectedItem.ToString();

            if (selected.Equals("Custom", StringComparison.OrdinalIgnoreCase))
            {
                CustomBaseBox.Visibility = Visibility.Visible;
                CustomBaseBox.Focus();
            }
            else
            {
                CustomBaseBox.Visibility = Visibility.Collapsed;
            }
        }

    }
}
