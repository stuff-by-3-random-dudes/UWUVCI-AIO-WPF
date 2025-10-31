using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UWUVCI_AIO_WPF.Modules.N64Config;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class N64ConfigWindow : Window
    {
        private static bool _mergeTipShown = false;
        private string _currentPath;
        private IniDocument _doc = new IniDocument();
        private readonly Dictionary<string, Dictionary<string, FrameworkElement>> _controls = new Dictionary<string, Dictionary<string, FrameworkElement>>();

        public N64ConfigWindow()
        {
            InitializeComponent();
            AdvancedToggle.IsChecked = false;
            UpdateAdvancedState();
            BuildAutoForms();
            SetInitialTriStateNulls();
            RefreshPresetList();

        } 
        private void AdvancedToggle_Changed(object sender, RoutedEventArgs e)
        {
            UpdateAdvancedState();
        }

        private void UpdateAdvancedState()
        {
            bool enabled = AdvancedToggle.IsChecked == true;
            StatusText.Text = enabled ? "Advanced controls enabled" : "Advanced controls hidden";
            AdvancedTab.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
            PopulateHackGrids();
            if (enabled) PopulateRawGrid();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "N64 VC INI|*.ini|All files|*.*"
            };
            if (ofd.ShowDialog(this) == true)
            {
                _currentPath = ofd.FileName;
                _doc = IniSerializer.Parse(File.ReadAllText(_currentPath));
                ApplyToUi();
                StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(_currentPath)}";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentPath))
            {
                BtnSaveAs_Click(sender, e);
                return;
            }
            SaveTo(_currentPath);
        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "N64 VC INI|*.ini|All files|*.*",
                FileName = BuildSuggestedFileName()
            };
            if (sfd.ShowDialog(this) == true)
            {
                _currentPath = sfd.FileName;
                SaveTo(_currentPath);
            }
        }

        

        private string BuildSuggestedFileName()
        {
            if (!string.IsNullOrEmpty(_currentPath))
            {
                var n = Path.GetFileName(_currentPath);
                if (!string.IsNullOrWhiteSpace(n)) return n;
            }
            return "game.ini";
        }

        private void SaveTo(string path)
        {
            try
            {
                ApplyFromUi();
                // Validation: BackupType/BackupSize consistency
                try
                {
                    var rom = _doc.GetSection("RomOption");
                    var hasType = !string.IsNullOrWhiteSpace(rom?.Get("BackupType"));
                    var hasSize = !string.IsNullOrWhiteSpace(rom?.Get("BackupSize"));
                    if (hasType ^ hasSize)
                    {
                        UWUVCI_MessageBox.Show(
                            "Save warning",
                            "BackupType is set but BackupSize is empty (or vice versa). This may not behave as expected.",
                            UWUVCI_MessageBoxType.Ok,
                            UWUVCI_MessageBoxIcon.Warning,
                            this,
                            true);
                        StatusText.Text = "Warning: BackupType/BackupSize mismatch.";
                    }
                }
                catch { /* non-fatal */ }
                var text = IniSerializer.Serialize(_doc);
                File.WriteAllText(path, text);
                StatusText.Text = $"Saved: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Save failed", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RumbleMemPak_Changed(object sender, RoutedEventArgs e)
        {
            // Mutually exclusive
            if (sender == ChkRumble && ChkRumble.IsChecked == true)
                ChkMemPak.IsChecked = false;
            else if (sender == ChkMemPak && ChkMemPak.IsChecked == true)
                ChkRumble.IsChecked = false;
        }

        private void ApplyToUi()
        {
            // RomOption basics
            var rom = _doc.GetSection("RomOption");
            ChkRetraceByVsync.IsChecked = Read01(rom?.Get("RetraceByVsync"));
            ChkUseTimer.IsChecked = Read01(rom?.Get("UseTimer"));
            ChkTrueBoot.IsChecked = Read01(rom?.Get("TrueBoot"));
            ChkRumble.IsChecked = Read01(rom?.Get("Rumble"));
            ChkMemPak.IsChecked = Read01(rom?.Get("MemPak"));
            RamSizeBox.Text = rom?.Get("RamSize") ?? string.Empty;

            SetComboByText(BackupTypeBox, rom?.Get("BackupType"));
            SetComboByText(BackupSizeBox, rom?.Get("BackupSize"));

            PopulateRawGrid();
            PopulateHackGrids();
            ApplyToAutoForms();
        }

        private void ApplyFromUi()
        {
            var rom = _doc.GetOrAddSection("RomOption");
            Set01(rom, "RetraceByVsync", ChkRetraceByVsync.IsChecked);
            Set01(rom, "UseTimer", ChkUseTimer.IsChecked);
            Set01(rom, "TrueBoot", ChkTrueBoot.IsChecked);
            Set01(rom, "Rumble", ChkRumble.IsChecked);
            Set01(rom, "MemPak", ChkMemPak.IsChecked);
            if (!string.IsNullOrWhiteSpace(RamSizeBox.Text)) rom.Set("RamSize", RamSizeBox.Text.Trim()); else rom.Set("RamSize", null);

            var bt = GetComboValue(BackupTypeBox);
            if (bt != null) rom.Set("BackupType", bt);
            else rom.Set("BackupType", null);
            var bs = GetComboValue(BackupSizeBox);
            if (bs != null) rom.Set("BackupSize", bs);
            else rom.Set("BackupSize", null);

            // Advanced raw grid back into _doc
            if (AdvancedTab.Visibility == Visibility.Visible)
                ReadRawGridIntoDoc();

            // Hack grids into _doc
            ReadHackGridsIntoDoc();
            ReadFromAutoForms();

            // Remove empty sections to avoid writing blank headers
            _doc.PruneEmptySections();
        }

        private static bool? Read01(string? v)
        {
            if (v == null) return null;
            v = v.Trim();
            if (v == "1") return true;
            if (v == "0") return false;
            return null;
        }

        private static void Set01(IniSection s, string key, bool? v)
        {
            if (v == true) s.Set(key, "1");
            else if (v == false) s.Set(key, "0");
            else s.Set(key, null);
        }

        private void BuildAutoForms()
        {
            BuildSectionForm("RomOption", RomOptionAutoPanel, new HashSet<string>(new[] { "RetraceByVsync", "UseTimer", "TrueBoot", "Rumble", "MemPak", "BackupType", "BackupSize", "RamSize" }));
            BuildSectionForm("Render", RenderAutoPanel);
            BuildSectionForm("Sound", SoundAutoPanel);
            BuildSectionForm("Input", InputAutoPanel);
            BuildSectionForm("RSPG", RSPGAutoPanel);
            BuildSectionForm("Cmp", CmpAutoPanel);
            BuildSectionForm("SI", SIAutoPanel);
            BuildSectionForm("VI", VIAutoPanel);
            BuildSectionForm("TempConfig", TempConfigAutoPanel);
        }

        private void BuildSectionForm(string section, Panel host, HashSet<string>? skip = null)
        {
            _controls[section] = new Dictionary<string, FrameworkElement>();
            var grid = new Grid { Margin = new Thickness(0, 6, 0, 12) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = 0;
            if (!N64Schema.Sections.TryGetValue(section, out var fields)) return;
            string? lastGroup = null;
            foreach (var f in fields)
            {
                if (skip != null && skip.Contains(f.Key)) continue;
                if (!string.IsNullOrWhiteSpace(f.Group) && f.Group != lastGroup)
                {
                    // Add group header row
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    var head = new TextBlock { Text = f.Group, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 4) };
                    Grid.SetRow(head, row); Grid.SetColumn(head, 0); Grid.SetColumnSpan(head, 2); grid.Children.Add(head);
                    row++;
                    lastGroup = f.Group;
                }
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var lbl = new TextBlock { Text = f.Display ?? f.Key, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 2, 8, 2) };
                if (N64Tooltips.Tips.TryGetValue(f.Key, out var tip)) lbl.ToolTip = tip;
                Grid.SetRow(lbl, row); Grid.SetColumn(lbl, 0); grid.Children.Add(lbl);
                FrameworkElement input;
                switch (f.Type)
                {
                    case N64FieldType.Bool01:
                        var cb = new CheckBox { IsThreeState = true, VerticalAlignment = VerticalAlignment.Center, IsChecked = null };
                        input = cb; if (lbl.ToolTip != null) cb.ToolTip = lbl.ToolTip; break;
                    case N64FieldType.IntDec:
                    case N64FieldType.IntHex:
                    case N64FieldType.String:
                    default:
                        input = new TextBox { MinWidth = 160 };
                        if (lbl.ToolTip != null) input.ToolTip = lbl.ToolTip;
                        break;
                }
                Grid.SetRow(input, row); Grid.SetColumn(input, 1); grid.Children.Add(input);
                _controls[section][f.Key] = input;
                row++;
            }
            host.Children.Clear();
            host.Children.Add(grid);
        }

        private void ApplyToAutoForms()
        {
            foreach (var sec in _controls)
            {
                var s = _doc.GetSection(sec.Key);
                foreach (var kv in sec.Value)
                {
                    var ctrl = kv.Value;
                    var val = s?.Get(kv.Key);
                    if (ctrl is CheckBox cb)
                    {
                        cb.IsChecked = Read01(val);
                    }
                    else if (ctrl is TextBox tb)
                    {
                        tb.Text = val ?? string.Empty;
                    }
                }
            }
        }

        private void ReadFromAutoForms()
        {
            foreach (var sec in _controls)
            {
                IniSection? s = null;
                bool any = false;
                foreach (var kv in sec.Value)
                {
                    var ctrl = kv.Value;
                    if (ctrl is CheckBox cb)
                    {
                        if (cb.IsChecked != null)
                        {
                            s ??= _doc.GetOrAddSection(sec.Key);
                            Set01(s, kv.Key, cb.IsChecked);
                            any = true;
                        }
                    }
                    else if (ctrl is TextBox tb)
                    {
                        var t = (tb.Text ?? string.Empty).Trim();
                        if (t.Length > 0)
                        {
                            if (s == null) s = _doc.GetOrAddSection(sec.Key);
                            s.Set(kv.Key, t);
                            any = true;
                        }
                    }
                }
                if (!any && s != null && s.Properties.Count == 0)
                {
                    _doc.RemoveSection(sec.Key);
                }
            }
        }

        private void SetInitialTriStateNulls()
        {
            // Manual common checkboxes
            ChkRetraceByVsync.IsThreeState = true; ChkRetraceByVsync.IsChecked = null;
            ChkUseTimer.IsThreeState = true; ChkUseTimer.IsChecked = null;
            ChkTrueBoot.IsThreeState = true; ChkTrueBoot.IsChecked = null;
            ChkRumble.IsThreeState = true; ChkRumble.IsChecked = null;
            ChkMemPak.IsThreeState = true; ChkMemPak.IsChecked = null;

            // Auto-generated
            foreach (var sec in _controls.Values)
            {
                foreach (var ctrl in sec.Values)
                {
                    if (ctrl is CheckBox cb)
                    {
                        cb.IsThreeState = true;
                        cb.IsChecked = null;
                    }
                }
            }
        }

        private static void SetComboByText(ComboBox box, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) { box.SelectedIndex = 0; return; }
            var text = value.Trim();
            for (int i = 0; i < box.Items.Count; i++)
            {
                var cbi = box.Items[i] as ComboBoxItem;
                if (cbi != null && cbi.Content?.ToString()?.Contains(text) == true)
                {
                    box.SelectedIndex = i; return;
                }
            }
            box.SelectedIndex = 0;
        }

        private static string? GetComboValue(ComboBox box)
        {
            if (box.SelectedItem is ComboBoxItem cbi)
            {
                var txt = cbi.Content?.ToString();
                if (txt == "[null]") return null;
                // Extract numeric or plain token at start
                if (txt != null)
                {
                    var part = txt.Split(' ').FirstOrDefault();
                    return part;
                }
            }
            return null;
        }

        private class RawEntry { public string Section { get; set; } public string Key { get; set; } public string Value { get; set; } }

        private void PopulateRawGrid()
        {
            var items = new System.Collections.ObjectModel.ObservableCollection<RawEntry>();
            foreach (var s in _doc.Sections)
                foreach (var p in s.Properties)
                    items.Add(new RawEntry { Section = s.Name, Key = p.Key, Value = p.Value });
            RawGrid.ItemsSource = items;
            if (RawFilterBox != null)
            {
                RawFilterBox.TextChanged -= RawFilterBox_TextChanged;
                RawFilterBox.Text = string.Empty;
                RawFilterBox.TextChanged += RawFilterBox_TextChanged;
            }
        }

        private void RawFilterBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var src = RawGrid.ItemsSource as System.Collections.ObjectModel.ObservableCollection<RawEntry>;
            if (src == null) return;
            var all = new List<RawEntry>();
            foreach (var s in _doc.Sections)
                foreach (var p in s.Properties)
                    all.Add(new RawEntry { Section = s.Name, Key = p.Key, Value = p.Value });
            var term = (RawFilterBox.Text ?? string.Empty).Trim();
            if (term.Length == 0)
            {
                RawGrid.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<RawEntry>(all);
            }
            else
            {
                term = term.ToLowerInvariant();
                RawGrid.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<RawEntry>(all.Where(r =>
                    (r.Section ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (r.Key ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (r.Value ?? string.Empty).ToLowerInvariant().Contains(term)));
            }
        }

        private void ReadRawGridIntoDoc()
        {
            var items = RawGrid.ItemsSource as IEnumerable<RawEntry>;
            if (items == null) return;
            var newDoc = new IniDocument();
            foreach (var r in items)
            {
                if (r == null) continue;
                if (string.IsNullOrWhiteSpace(r.Section) || string.IsNullOrWhiteSpace(r.Key)) continue;
                newDoc.GetOrAddSection(r.Section.Trim()).Set(r.Key.Trim(), r.Value?.Trim() ?? "");
            }
            _doc = newDoc;
        }

        // Simple models for hack grids
        private class IdleRow { public int? Index { get; set; } public string Address { get; set; } public string Inst { get; set; } public int? Type { get; set; } }
        private class InsertIdleRow { public int? Index { get; set; } public string Address { get; set; } public string Inst { get; set; } public string Type { get; set; } public string Value { get; set; } }
        private class SpecialInstRow { public int? Index { get; set; } public string Address { get; set; } public string Inst { get; set; } public int? Type { get; set; } public string Value { get; set; } }
        private class BreakBlockInstRow { public int? Index { get; set; } public string Address { get; set; } public string Inst { get; set; } public int? Type { get; set; } public string JmpPC { get; set; } }
        private class FilterHackRow { public int? Index { get; set; } public string TextureAddress { get; set; } public string SumPixel { get; set; } public string Data2 { get; set; } public string Data3 { get; set; } public int? AlphaTest { get; set; } public int? MagFilter { get; set; } public string OffsetS { get; set; } public string OffsetT { get; set; } }
        private class CheatRow { public int? Index { get; set; } public int? N { get; set; } public string Addr { get; set; } public string Value { get; set; } public int? Bytes { get; set; } }
        private class RomHackRow { public int? Index { get; set; } public string Address { get; set; } public int? Type { get; set; } public string Value { get; set; } }
        private class VertexHackRow { public int? Index { get; set; } public int? VertexCount { get; set; } public string VertexAddress { get; set; } public string TextureAddress { get; set; } public string FirstVertex { get; set; } public string Value { get; set; } }

        private void PopulateHackGrids()
        {
            GridIdle.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("Idle", new[] { "Address", "Inst", "Type" }, (i, arr) => new IdleRow { Index = i, Address = arr[0], Inst = arr[1], Type = ParseInt(arr[2]) }).ToList());
            GridInsertIdleInst.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("InsertIdleInst", new[] { "Address", "Inst", "Type", "Value" }, (i, arr) => new InsertIdleRow { Index = i, Address = arr[0], Inst = arr[1], Type = arr[2], Value = arr[3] }).ToList());
            GridSpecialInst.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("SpecialInst", new[] { "Address", "Inst", "Type", "Value" }, (i, arr) => new SpecialInstRow { Index = i, Address = arr[0], Inst = arr[1], Type = ParseInt(arr[2]), Value = arr[3] }).ToList());
            GridBreakBlockInst.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("BreakBlockInst", new[] { "Address", "Inst", "Type", "JmpPC" }, (i, arr) => new BreakBlockInstRow { Index = i, Address = arr[0], Inst = arr[1], Type = ParseInt(arr[2]), JmpPC = arr[3] }).ToList());
            GridFilterHack.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("FilterHack", new[] { "TextureAddress", "SumPixel", "Data2", "Data3", "AlphaTest", "MagFilter", "OffsetS", "OffsetT" }, (i, arr) => new FilterHackRow { Index = i, TextureAddress = arr[0], SumPixel = arr[1], Data2 = arr[2], Data3 = arr[3], AlphaTest = ParseInt(arr[4]), MagFilter = ParseInt(arr[5]), OffsetS = arr[6], OffsetT = arr[7] }).ToList());
            GridCheat.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<CheatRow>(ReadCheat().ToList());

            // Advanced-only hacks
            if (AdvancedToggle.IsChecked == true)
            {
                AdvancedHacksPanel.Visibility = Visibility.Visible;
                GridRomHack.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("RomHack", new[] { "Address", "Type", "Value" }, (i, arr) => new RomHackRow { Index = i, Address = arr[0], Type = ParseInt(arr[1]), Value = arr[2] }).ToList());
                GridVertexHack.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<object>(ReadIndexedSection("VertexHack", new[] { "VertexCount", "VertexAddress", "TextureAddress", "FirstVertex", "Value" }, (i, arr) => new VertexHackRow { Index = i, VertexCount = ParseInt(arr[0]), VertexAddress = arr[1], TextureAddress = arr[2], FirstVertex = arr[3], Value = arr[4] }).ToList());
            }
            else
            {
                AdvancedHacksPanel.Visibility = Visibility.Collapsed;
            }
        }

        private static int? ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s, out var d)) return d;
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var h)) return h;
            }
            return null;
        }

        private IEnumerable<object> ReadIndexedSection(string section, string[] keys, Func<int, string[], object> factory)
        {
            var s = _doc.GetSection(section);
            var list = new List<object>();
            if (s == null) return list;
            // Find all indices by scanning for suffix digits
            var grouped = new Dictionary<int, string[]>();
            foreach (var p in s.Properties)
            {
                foreach (var k in keys)
                {
                    if (p.Key.StartsWith(k))
                    {
                        var idxStr = p.Key.Substring(k.Length);
                        if (int.TryParse(idxStr, out var idx))
                        {
                            if (!grouped.TryGetValue(idx, out var arr)) { arr = new string[keys.Length]; grouped[idx] = arr; }
                            arr[Array.IndexOf(keys, k)] = p.Value;
                        }
                    }
                }
            }
            foreach (var kv in grouped.OrderBy(k => k.Key))
            {
                list.Add(factory(kv.Key, kv.Value ?? new string[keys.Length]));
            }
            return list;
        }

        private IEnumerable<CheatRow> ReadCheat()
        {
            var s = _doc.GetSection("Cheat");
            var list = new List<CheatRow>();
            if (s == null) return list;
            var grouped = new Dictionary<int, CheatRow>();
            foreach (var p in s.Properties)
            {
                var mIndexOnly = Regex.Match(p.Key, @"^Cheat(\d+)$");
                var mAddr = Regex.Match(p.Key, @"^Cheat(\d+)_Addr$");
                var mValue = Regex.Match(p.Key, @"^Cheat(\d+)_Value$");
                var mBytes = Regex.Match(p.Key, @"^Cheat(\d+)_Bytes$");
                if (mIndexOnly.Success)
                {
                    int idx = int.Parse(mIndexOnly.Groups[1].Value);
                    if (!grouped.TryGetValue(idx, out var item)) { item = new CheatRow { Index = idx }; grouped[idx] = item; }
                    item.N = ParseInt(p.Value);
                }
                else if (mAddr.Success)
                {
                    int idx = int.Parse(mAddr.Groups[1].Value);
                    if (!grouped.TryGetValue(idx, out var item)) { item = new CheatRow { Index = idx }; grouped[idx] = item; }
                    item.Addr = p.Value;
                }
                else if (mValue.Success)
                {
                    int idx = int.Parse(mValue.Groups[1].Value);
                    if (!grouped.TryGetValue(idx, out var item)) { item = new CheatRow { Index = idx }; grouped[idx] = item; }
                    item.Value = p.Value;
                }
                else if (mBytes.Success)
                {
                    int idx = int.Parse(mBytes.Groups[1].Value);
                    if (!grouped.TryGetValue(idx, out var item)) { item = new CheatRow { Index = idx }; grouped[idx] = item; }
                    item.Bytes = ParseInt(p.Value);
                }
            }
            foreach (var it in grouped.OrderBy(k => k.Key)) list.Add(it.Value);
            return list;
        }

        private void ReadHackGridsIntoDoc()
        {
            // Idle
            WriteIndexed("Idle", new[] { "Address", "Inst", "Type" }, GridIdle.ItemsSource);
            WriteIndexed("InsertIdleInst", new[] { "Address", "Inst", "Type", "Value" }, GridInsertIdleInst.ItemsSource);
            WriteIndexed("SpecialInst", new[] { "Address", "Inst", "Type", "Value" }, GridSpecialInst.ItemsSource);
            WriteIndexed("BreakBlockInst", new[] { "Address", "Inst", "Type", "JmpPC" }, GridBreakBlockInst.ItemsSource);
            // FilterHack
            WriteIndexed("FilterHack", new[] { "TextureAddress", "SumPixel", "Data2", "Data3", "AlphaTest", "MagFilter", "OffsetS", "OffsetT" }, GridFilterHack.ItemsSource);
            // Cheat section
            var cheat = _doc.GetOrAddSection("Cheat");
            cheat.Properties.Clear();
            if (GridCheat.ItemsSource is IEnumerable<CheatRow> cheats)
            {
                foreach (var row in cheats)
                {
                    if (row?.Index == null) continue;
                    cheat.Properties.Add(new IniProperty($"Cheat{row.Index}", (row.N ?? 0).ToString()));
                    if (!string.IsNullOrWhiteSpace(row.Addr)) cheat.Properties.Add(new IniProperty($"Cheat{row.Index}_Addr", row.Addr));
                    if (!string.IsNullOrWhiteSpace(row.Value)) cheat.Properties.Add(new IniProperty($"Cheat{row.Index}_Value", row.Value));
                    if (row.Bytes != null) cheat.Properties.Add(new IniProperty($"Cheat{row.Index}_Bytes", (row.Bytes ?? 0).ToString()));
                }
            }

            // Advanced-only hacks (write only when advanced enabled and there are rows)
            if (AdvancedToggle.IsChecked == true)
            {
                WriteIndexed("RomHack", new[] { "Address", "Type", "Value" }, GridRomHack.ItemsSource);
                WriteIndexed("VertexHack", new[] { "VertexCount", "VertexAddress", "TextureAddress", "FirstVertex", "Value" }, GridVertexHack.ItemsSource);
            }
        }

        private void EditRomHackValue_Click(object sender, RoutedEventArgs e)
        {
            if (GridRomHack.SelectedItem is RomHackRow row)
            {
                var dlg = new ByteArrayEditor(row.Value);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    row.Value = dlg.Result ?? row.Value;
                    GridRomHack.Items.Refresh();
                }
            }
        }

        private void EditVertexFirst_Click(object sender, RoutedEventArgs e)
        {
            if (GridVertexHack.SelectedItem is VertexHackRow row)
            {
                var dlg = new ByteArrayEditor(row.FirstVertex);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    row.FirstVertex = dlg.Result ?? row.FirstVertex;
                    GridVertexHack.Items.Refresh();
                }
            }
        }

        private void EditVertexValue_Click(object sender, RoutedEventArgs e)
        {
            if (GridVertexHack.SelectedItem is VertexHackRow row)
            {
                var dlg = new ByteArrayEditor(row.Value);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    row.Value = dlg.Result ?? row.Value;
                    GridVertexHack.Items.Refresh();
                }
            }
        }

        private void WriteIndexed(string section, string[] keys, System.Collections.IEnumerable items)
        {
            if (items == null) { _doc.RemoveSection(section); return; }
            var list = new List<object>();
            foreach (var o in items) if (o != null) list.Add(o);
            if (list.Count == 0) { _doc.RemoveSection(section); return; }
            var sec = _doc.GetOrAddSection(section);
            sec.Properties.Clear();
            sec.Properties.Add(new IniProperty("Count", list.Count.ToString()));
            foreach (var obj in list)
            {
                if (obj == null) continue;
                // Reflect by property names matching keys or Index
                var idxProp = obj.GetType().GetProperty("Index");
                if (idxProp == null) continue;
                var idxVal = idxProp.GetValue(obj);
                if (idxVal == null) continue;
                int idx = Convert.ToInt32(idxVal);
                for (int i = 0; i < keys.Length; i++)
                {
                    var prop = obj.GetType().GetProperty(keys[i]);
                    if (prop == null) continue;
                    var value = prop.GetValue(obj)?.ToString();
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    sec.Properties.Add(new IniProperty(keys[i] + idx.ToString(), value));
                }
            }
        }

        private void RefreshPresetList()
        {
            if (PresetQuickBox != null)
            {
                var names = new System.Collections.ObjectModel.ObservableCollection<string>(
                    BuiltinPresets.All.ConvertAll(b => b.Name)
                );
                PresetQuickBox.ItemsSource = names;
                PresetQuickBox.SelectedIndex = -1;
            }
        }

        private void PresetQuickBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var name = PresetQuickBox?.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(name)) return;
                var preset = BuiltinPresets.All.FirstOrDefault(b => b.Name == name);
                if (preset == null) return;
                var merge = ChkQuickMerge?.IsChecked == true;
                var doc = BuiltinPresets.ToIni(preset);
                ApplyPreset(doc, merge);
                StatusText.Text = $"Applied preset: {name}";
                var desc = preset.Description ?? string.Empty;
                if (PresetBadge != null)
                {
                    PresetBadge.Text = merge ? $"Preset: {name} (merged) — {desc}" : $"Preset: {name} — {desc}";
                    PresetBadge.ToolTip = desc;
                }
                if (PresetQuickBox != null)
                    PresetQuickBox.ToolTip = desc;
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Preset error", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }

        private void ChkQuickMerge_Checked(object sender, RoutedEventArgs e)
        {
            if (_mergeTipShown) return;
            _mergeTipShown = true;
            UWUVCI_MessageBox.Show(
                "Merge mode",
                "When Merge is ON, applying a preset only overwrites the preset’s keys and keeps your other values. When OFF, the preset replaces the entire config.",
                UWUVCI_MessageBoxType.Ok,
                UWUVCI_MessageBoxIcon.Info,
                this,
                true);
        }

        

        private void ApplyPreset(IniDocument preset, bool merge)
        {
            if (!merge)
            {
                _doc = preset;
            }
            else
            {
                foreach (var sec in preset.Sections)
                {
                    var target = _doc.GetOrAddSection(sec.Name);
                    foreach (var p in sec.Properties)
                        target.Set(p.Key, p.Value);
                }
            }
            ApplyToUi();
            if (PresetBadge != null)
            {
                PresetBadge.Text = "Preset applied";
            }
        }

    }
}







