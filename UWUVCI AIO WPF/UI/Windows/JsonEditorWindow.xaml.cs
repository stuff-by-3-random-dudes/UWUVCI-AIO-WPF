using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class JsonEditorWindow : Window
    {
        private string _path;
        private Models.JsonAppSettings _model;
        private readonly System.Collections.Generic.Dictionary<string, FrameworkElement> _inputs = new System.Collections.Generic.Dictionary<string, FrameworkElement>(StringComparer.OrdinalIgnoreCase);
        private bool _rawUnlocked = false;

        public JsonEditorWindow(string path)
        {
            InitializeComponent();
            _path = path;
            PathText.Text = path;
            LoadFile();
        }

        private void LoadFile()
        {
            try
            {
                var text = File.ReadAllText(_path, Encoding.UTF8);
                Editor.Text = text.TrimStart();
                try
                {
                    _model = JsonConvert.DeserializeObject<Models.JsonAppSettings>(text) ?? new Models.JsonAppSettings();
                }
                catch { _model = new Models.JsonAppSettings(); }
                BuildFormFromModel();
                Status.Text = "Loaded";
            }
            catch (Exception ex)
            {
                Status.Text = "Load failed";
                try { Logger.Log("JsonEditor LoadFile error: " + ex.ToString()); } catch { }
                UWUVCI_MessageBox.Show("Load failed", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }

        private void BuildFormFromModel()
        {
            _inputs.Clear();
            FormGrid.RowDefinitions.Clear();
            FormGrid.Children.Clear();
            // Fixed set and order
            AddRow("BasePath", _model.BasePath ?? "");
            AddRow("OutPath", _model.OutPath ?? "");
            AddRow("CKey", _model.Ckey ?? "");
            AddRow("Ancast", _model.Ancast ?? "");
            AddRowBool("UpgradeRequired", _model.UpgradeRequired);
            AddRowBool("ForceTutorialOnNextLaunch", _model.ForceTutorialOnNextLaunch);
            AddRow("UnixWaitDelayMs", _model.UnixWaitDelayMs.ToString() ?? "");
        }

        private void AddRow(string label, string value)
        {
            int row = FormGrid.RowDefinitions.Count;
            FormGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var tbLabel = new TextBlock { Text = label, Margin = new Thickness(0, 4, 8, 4), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(tbLabel, row); Grid.SetColumn(tbLabel, 0); FormGrid.Children.Add(tbLabel);
            var tb = new TextBox { Margin = new Thickness(0, 4, 0, 4) };
            tb.Text = value ?? string.Empty;
            tb.ToolTip = GetTooltip(label);
            Grid.SetRow(tb, row); Grid.SetColumn(tb, 1); FormGrid.Children.Add(tb);
            // Map CKey to property Ckey; others are identical
            string key = label == "CKey" ? "Ckey" : label;
            _inputs[key] = tb;
        }

        private void AddRowBool(string label, bool value)
        {
            int row = FormGrid.RowDefinitions.Count;
            FormGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var tbLabel = new TextBlock { Text = label, Margin = new Thickness(0, 4, 8, 4), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(tbLabel, row); Grid.SetColumn(tbLabel, 0); FormGrid.Children.Add(tbLabel);
            var cb = new CheckBox { Margin = new Thickness(0, 4, 0, 4), VerticalAlignment = VerticalAlignment.Center };
            cb.IsChecked = value;
            cb.ToolTip = GetTooltip(label);
            Grid.SetRow(cb, row); Grid.SetColumn(cb, 1); FormGrid.Children.Add(cb);
            _inputs[label] = cb;
        }

        private string GetTooltip(string key)
        {
            switch (key)
            {
                case "BasePath": return "Folder containing the Wii U base files used for injections.";
                case "OutPath": return "Where finished injects are written.";
                case "CKey": return "Wii U Common Key (ckey). Required to build some content. Keep this private.";
                case "Ancast": return "Ancast key for vWii OC features (advanced). Optional; leave blank if unsure.";
                case "UpgradeRequired": return "Internal flag for first-run migrations. Normally false.";
                case "ForceTutorialOnNextLaunch": return "Show the tutorial wizard the next time the app launches.";
                default: return null;
            }
        }

        private void ApplyFormToModel()
        {
            _model ??= new Models.JsonAppSettings();
            // Only update selected keys
            if (_inputs.TryGetValue("BasePath", out var bp)) _model.BasePath = (bp as TextBox)?.Text ?? "";
            if (_inputs.TryGetValue("OutPath", out var op)) _model.OutPath = (op as TextBox)?.Text ?? "";
            if (_inputs.TryGetValue("Ckey", out var ck)) _model.Ckey = (ck as TextBox)?.Text ?? "";
            if (_inputs.TryGetValue("Ancast", out var an)) _model.Ancast = (an as TextBox)?.Text ?? "";
            if (_inputs.TryGetValue("UpgradeRequired", out var ur)) _model.UpgradeRequired = (ur as CheckBox)?.IsChecked == true;
            if (_inputs.TryGetValue("ForceTutorialOnNextLaunch", out var ft)) _model.ForceTutorialOnNextLaunch = (ft as CheckBox)?.IsChecked == true;
            if (_inputs.TryGetValue("UnixWaitDelayMs", out var uw)) 
            {
                if (int.TryParse((uw as TextBox)?.Text, out int delay))
                {
                    _model.UnixWaitDelayMs = delay;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string toWrite;
                if (Tabs.SelectedIndex == 0)
                {
                    ApplyFormToModel();
                    toWrite = JsonConvert.SerializeObject(_model, Formatting.Indented);
                    Editor.Text = toWrite;
                }
                else
                {
                    toWrite = Editor.Text;
                }
                File.WriteAllText(_path, toWrite, Encoding.UTF8);
                Status.Text = "Saved";

                // Keep the in-memory settings in sync so later saves don't overwrite edits
                if (string.Equals(_path, JsonSettingsManager.SettingsFile, StringComparison.OrdinalIgnoreCase))
                {
                    JsonSettingsManager.LoadSettings();
                }
            }
            catch (Exception ex)
            {
                try { Logger.Log("JsonEditor Save_Click error: " + ex.ToString()); } catch { }
                UWUVCI_MessageBox.Show("Save failed", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON|*.json|All files|*.*", FileName = Path.GetFileName(_path) };
            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    string toWrite;
                    if (Tabs.SelectedIndex == 0)
                    {
                        ApplyFormToModel();
                        toWrite = JsonConvert.SerializeObject(_model, Formatting.Indented);
                        Editor.Text = toWrite;
                    }
                    else toWrite = Editor.Text;

                    File.WriteAllText(dlg.FileName, toWrite, Encoding.UTF8);
                    _path = dlg.FileName;
                    PathText.Text = _path;
                    Status.Text = "Saved";

                    // If the user saved over the live settings file, refresh the manager instance
                    if (string.Equals(_path, JsonSettingsManager.SettingsFile, StringComparison.OrdinalIgnoreCase))
                    {
                        JsonSettingsManager.LoadSettings();
                    }
                }
                catch (Exception ex)
                {
                    try { Logger.Log("JsonEditor SaveAs_Click error: " + ex.ToString()); } catch { }
                    UWUVCI_MessageBox.Show("Save failed", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
                }
            }
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = Tabs.SelectedIndex == 0 ? JsonConvert.SerializeObject(GetModelFromForm(), Formatting.None) : Editor.Text;
                JsonConvert.DeserializeObject(text);
                UWUVCI_MessageBox.Show("Valid JSON", "The JSON is valid.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success, this, true);
            }
            catch (Exception ex)
            {
                try { Logger.Log("JsonEditor Validate_Click error: " + ex.ToString()); } catch { }
                UWUVCI_MessageBox.Show("Invalid JSON", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Warning, this, true);
            }
        }

        private void Format_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = Tabs.SelectedIndex == 0 ? JsonConvert.SerializeObject(GetModelFromForm(), Formatting.Indented) : Editor.Text;
                var obj = JsonConvert.DeserializeObject(text);
                Editor.Text = JsonConvert.SerializeObject(obj, Formatting.Indented);
                Status.Text = "Formatted";
            }
            catch (Exception ex)
            {
                try { Logger.Log("JsonEditor Format_Click error: " + ex.ToString()); } catch { }
                UWUVCI_MessageBox.Show("Format failed", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Warning, this, true);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private Models.JsonAppSettings GetModelFromForm()
        {
            ApplyFormToModel();
            return _model;
        }

        private void UnlockRaw_Click(object sender, RoutedEventArgs e)
        {
            _rawUnlocked = true;
            RawLockPanel.Visibility = Visibility.Collapsed;
            Editor.Visibility = Visibility.Visible;
        }

    }
}
