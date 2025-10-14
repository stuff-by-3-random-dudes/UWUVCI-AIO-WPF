using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Services;
using UWUVCI_AIO_WPF.UI.Windows;

namespace UWUVCI_AIO_WPF.Modules.Nintendont
{
    public partial class NintendontConfigWindow : Window
    {
        private readonly NintendontConfigService _service = new NintendontConfigService();
        private NintendontConfig _cfg = NintendontConfig.CreateDefault();

        private string _selectedDriveRoot; // e.g. "E:\"
        private bool _suppressUiToModel;

        public NintendontConfigWindow()
        {
            InitializeComponent();
            InitUi();
        }

        private void InitUi()
        {
            // Fill static lists
            ReloadDrives();

            PresetBox.ItemsSource = NintendontPresets.AllPresets.Select(p => p.Name).ToList();
            if (PresetBox.Items.Count > 0) PresetBox.SelectedIndex = 0;

            VideoForceBox.ItemsSource = new[] { "Auto", "Force", "Force (Deflicker)", "None" };
            VideoTypeBox.ItemsSource = new[] { "Auto", "NTSC", "MPAL", "PAL50", "PAL60" };
            LanguageBox.ItemsSource = new[] { "Automatic", "English", "German", "French", "Spanish", "Italian", "Dutch" };
            MemcardBlocksBox.ItemsSource = new[] { "59", "123", "251", "507 (Unstable)", "1019 (Unstable)", "2043 (Unstable)" };
            GamepadSlotBox.ItemsSource = new[] { "1", "2", "3", "4" };
            MaxPadsBox.ItemsSource = new[] { "1", "2", "3", "4" };

            // Wire up clicks (kept out of XAML to avoid compile issues earlier)
            BtnCloseFooter.Click += (s, e) => Close();
            BtnReloadDrives.Click += (s, e) => ReloadDrives();
            BtnOpen.Click += async (s, e) => await OpenExistingAsync();
            BtnDownload.Click += async (s, e) => await DownloadNintendontAsync();
            BtnDetect.Click += async (s, e) => await DetectExistingAsync();
            BtnSaveToSd.Click += async (s, e) => await SaveToSdAsync();
            BtnSaveAs.Click += async (s, e) => await SaveAsAsync();

            DriveBox.SelectionChanged += (s, e) => OnDriveChanged();
            PresetBox.SelectionChanged += (s, e) => ApplyPreset();
            VideoForceBox.SelectionChanged += (s, e) => OnVideoForceChanged();
            VideoTypeBox.SelectionChanged += (s, e) => OnVideoTypeChanged();
            ChkAutoWidth.Checked += (s, e) => UpdateWidthUi();
            ChkAutoWidth.Unchecked += (s, e) => UpdateWidthUi();
            VideoWidthSlider.ValueChanged += (s, e) => UpdateWidthUi();

            ApplyModelToUi(_cfg);
            UpdateMemcardUi();
        }

        // ---------- UI <-> Model ----------
        private void ApplyModelToUi(NintendontConfig c)
        {
            _suppressUiToModel = true;

            // Gameplay
            ChkCheats.IsChecked = c.Cheats;
            ChkAutoBoot.IsChecked = c.AutoBoot;
            ChkSkipIpl.IsChecked = c.SkipIpl;
            ChkTriArcade.IsChecked = c.ArcadeMode;
            ChkBba.IsChecked = c.BbaEmulation;
            ChkCheatPath.IsChecked = c.CheatPath;

            // Memory
            ChkMemcardEmu.IsChecked = c.MemcardEmulation;
            MemcardBlocksBox.SelectedIndex = Math.Max(0, c.MemcardBlocksIndex);
            ChkMemcardMulti.IsChecked = c.MemcardMulti;
            ChkMemcardEmu.Checked += (s, e) => UpdateMemcardUi();
            ChkMemcardEmu.Unchecked += (s, e) => UpdateMemcardUi();


            // Controller
            ChkCcRumble.IsChecked = c.CcRumble;
            ChkNativeSi.IsChecked = c.NativeSi;

            // Video
            VideoForceBox.SelectedIndex = (int)c.VideoForceMode;
            VideoTypeBox.SelectedIndex = (int)c.VideoTypeMode;
            ChkAutoWidth.IsChecked = c.AutoVideoWidth;
            VideoWidthSlider.IsEnabled = !c.AutoVideoWidth;
            VideoWidthSlider.Value = c.AutoVideoWidth ? 640 : Math.Max(640, Math.Min(720, c.VideoWidth == 0 ? 640 : c.VideoWidth));
            VideoWidthLabel.Text = c.AutoVideoWidth ? "Auto" : (c.VideoWidth == 0 ? "640" : c.VideoWidth.ToString());
            ChkForceWide.IsChecked = c.ForceWidescreen;
            ChkForceProg.IsChecked = c.ForceProgressive;
            ChkPatchPal50.IsChecked = c.PatchPAL50;

            // System
            LanguageBox.SelectedIndex = Math.Max(0, c.LanguageIndex);
            GamepadSlotBox.SelectedIndex = Math.Max(0, c.WiiUGamepadSlot);
            MaxPadsBox.SelectedIndex = Math.Max(0, c.MaxPads - 1);

            // Debug/Perf
            ChkDebugger.IsChecked = c.Debugger;
            ChkDebugWait.IsChecked = c.DebuggerWait;
            ChkOsReport.IsChecked = c.OsReport;
            ChkLog.IsChecked = c.EnableLog;
            ChkDriveLed.IsChecked = c.DriveLed;
            ChkUnlockRead.IsChecked = c.UnlockReadSpeed;

            CheatPathBox.Text = "/codes";
            GamePathBox.Text = "/games";

            StatusText.Text = string.Empty;
            DownloadStatusText.Text = string.Empty;

            _suppressUiToModel = false;
        }

        private void UpdateMemcardUi()
        {
            bool enabled = ChkMemcardEmu.IsChecked == true;
            MemcardBlocksBox.IsEnabled = enabled;
            ChkMemcardMulti.IsEnabled = enabled;

            if (!enabled)
            {
                MemcardBlocksBox.SelectedIndex = -1;
                ChkMemcardMulti.IsChecked = false;
            }

            ApplyUiToModel();
        }


        private void ApplyUiToModel()
        {
            if (_suppressUiToModel) return;

            _cfg.Cheats = ChkCheats.IsChecked == true;
            _cfg.AutoBoot = ChkAutoBoot.IsChecked == true;
            _cfg.SkipIpl = ChkSkipIpl.IsChecked == true;
            _cfg.ArcadeMode = ChkTriArcade.IsChecked == true;
            _cfg.BbaEmulation = ChkBba.IsChecked == true;
            _cfg.CheatPath = ChkCheatPath.IsChecked == true;

            _cfg.MemcardEmulation = ChkMemcardEmu.IsChecked == true;
            _cfg.MemcardBlocksIndex = Math.Max(0, MemcardBlocksBox.SelectedIndex);
            _cfg.MemcardMulti = ChkMemcardMulti.IsChecked == true;

            _cfg.CcRumble = ChkCcRumble.IsChecked == true;
            _cfg.NativeSi = ChkNativeSi.IsChecked == true;

            _cfg.VideoForceMode = (NintendontVideoForceMode)Math.Max(0, VideoForceBox.SelectedIndex);
            _cfg.VideoTypeMode = (NintendontVideoTypeMode)Math.Max(0, VideoTypeBox.SelectedIndex);
            _cfg.AutoVideoWidth = ChkAutoWidth.IsChecked == true;
            _cfg.VideoWidth = _cfg.AutoVideoWidth ? 0 : (int)Math.Round(VideoWidthSlider.Value);
            _cfg.ForceWidescreen = ChkForceWide.IsChecked == true;
            _cfg.ForceProgressive = ChkForceProg.IsChecked == true;
            _cfg.PatchPAL50 = ChkPatchPal50.IsChecked == true;

            _cfg.LanguageIndex = Math.Max(0, LanguageBox.SelectedIndex);
            _cfg.WiiUGamepadSlot = Math.Max(0, GamepadSlotBox.SelectedIndex);
            _cfg.MaxPads = Math.Max(1, MaxPadsBox.SelectedIndex + 1);

            _cfg.Debugger = ChkDebugger.IsChecked == true;
            _cfg.DebuggerWait = ChkDebugWait.IsChecked == true;
            _cfg.OsReport = ChkOsReport.IsChecked == true;
            _cfg.EnableLog = ChkLog.IsChecked == true;
            _cfg.DriveLed = ChkDriveLed.IsChecked == true;
            _cfg.UnlockReadSpeed = ChkUnlockRead.IsChecked == true;
        }

        // ---------- Event helpers ----------
        private void OnDriveChanged()
        {
            var sel = DriveBox.SelectedItem as string;
            _selectedDriveRoot = null;
            if (!string.IsNullOrEmpty(sel))
            {
                var first = sel.Split(' ').FirstOrDefault();
                if (!string.IsNullOrEmpty(first) && first.EndsWith("\\"))
                    _selectedDriveRoot = first;
            }
        }

        private void ApplyPreset()
        {
            var name = PresetBox.SelectedItem as string;
            var preset = NintendontPresets.AllPresets.FirstOrDefault(p => p.Name == name);
            if (preset == null) return;

            _cfg = preset.ApplyTo(NintendontConfig.CreateDefault());
            ApplyModelToUi(_cfg);
        }

        private void OnVideoForceChanged()
        {
            // If Force/ForceDF, allow type; else set type=Auto
            if (VideoForceBox.SelectedIndex != 1 && VideoForceBox.SelectedIndex != 2)
                VideoTypeBox.SelectedIndex = 0;
            ApplyUiToModel();
        }

        private void OnVideoTypeChanged()
        {
            // Only valid when force/forceDF
            if (VideoForceBox.SelectedIndex != 1 && VideoForceBox.SelectedIndex != 2)
                VideoTypeBox.SelectedIndex = 0;
            ApplyUiToModel();
        }

        private void UpdateWidthUi()
        {
            if (ChkAutoWidth.IsChecked == true)
            {
                VideoWidthSlider.IsEnabled = false;
                VideoWidthLabel.Text = "Auto";
            }
            else
            {
                VideoWidthSlider.IsEnabled = true;
                VideoWidthLabel.Text = ((int)Math.Round(VideoWidthSlider.Value)).ToString();
            }
            ApplyUiToModel();
        }

        private void ReloadDrives()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Removable)
                .Select(d => d.Name + " (" + d.VolumeLabel + ")")
                .ToList();

            DriveBox.ItemsSource = drives;
            if (drives.Count > 0) DriveBox.SelectedIndex = 0;
            OnDriveChanged();
        }

        // ---------- Commands ----------
        private async Task OpenExistingAsync()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Open nincfg.bin",
                Filter = "Nintendont config (nincfg.bin)|nincfg.bin|All files|*.*",
                CheckFileExists = true
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var (cfg, extraBytesSkipped) = await _service.LoadConfigAsync(ofd.FileName);
                    _cfg = cfg;

                    ApplyModelToUi(_cfg);
                    BannerNewFormat.Visibility = extraBytesSkipped ? Visibility.Visible : Visibility.Collapsed;

                    UWUVCI_MessageBox.Show("Loaded", "Existing nincfg.bin loaded.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success, this, true);
                }
                catch (Exception ex)
                {
                    UWUVCI_MessageBox.Show("Load Failed", "Could not read nincfg.bin:\n" + ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
                }
            }
        }

        private async Task DetectExistingAsync()
        {
            if (string.IsNullOrEmpty(_selectedDriveRoot))
            {
                UWUVCI_MessageBox.Show("Drive not selected", "Please choose your SD drive first.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Info, this, true);
                return;
            }

            var path = Path.Combine(_selectedDriveRoot, "nincfg.bin");
            if (!File.Exists(path))
            {
                UWUVCI_MessageBox.Show("Not Found", "No nincfg.bin found at:\n" + path, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Warning, this, true);
                return;
            }

            try
            {
                var (cfg, extraBytesSkipped) = await _service.LoadConfigAsync(path);
                _cfg = cfg;

                ApplyModelToUi(_cfg);
                BannerNewFormat.Visibility = extraBytesSkipped ? Visibility.Visible : Visibility.Collapsed;

                UWUVCI_MessageBox.Show("Loaded", "Existing nincfg.bin loaded.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success, this, true);
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Load Failed", "Could not read nincfg.bin:\n" + ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }

        private async Task SaveToSdAsync()
        {
            ApplyUiToModel();

            if (string.IsNullOrEmpty(_selectedDriveRoot))
            {
                UWUVCI_MessageBox.Show("Drive not selected", "Please choose your SD drive first.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Info, this, true);
                return;
            }

            var path = Path.Combine(_selectedDriveRoot, "nincfg.bin");
            try
            {
                await _service.SaveConfigAsync(path, _cfg);
                StatusText.Text = "Saved to " + path;
                UWUVCI_MessageBox.Show("Saved", "nincfg.bin saved to SD.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success, this, false);
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show("Save Failed", "Could not write nincfg.bin:\n" + ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }

        private async Task SaveAsAsync()
        {
            ApplyUiToModel();

            var sfd = new SaveFileDialog
            {
                Title = "Save nincfg.bin",
                Filter = "Nintendont config (nincfg.bin)|nincfg.bin|Binary|*.bin",
                FileName = "nincfg.bin",
                AddExtension = true
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    await _service.SaveConfigAsync(sfd.FileName, _cfg);
                    StatusText.Text = "Saved to " + sfd.FileName;
                    UWUVCI_MessageBox.Show("Saved", "nincfg.bin saved.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success, this, false);
                }
                catch (Exception ex)
                {
                    UWUVCI_MessageBox.Show("Save Failed", "Could not write nincfg.bin:\n" + ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
                }
            }
        }

        private async Task DownloadNintendontAsync()
        {
            if (string.IsNullOrEmpty(_selectedDriveRoot))
            {
                UWUVCI_MessageBox.Show("Drive not selected", "Please choose your SD drive first.", UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Info, this, true);
                return;
            }

            DownloadStatusText.Text = "Downloading…";
            try
            {
                var target = Path.Combine(_selectedDriveRoot, @"apps\nintendont");
                await _service.DownloadNintendontAsync(target);
                DownloadStatusText.Text = "Download complete.";
                UWUVCI_MessageBox.Show("Done", "Nintendont downloaded to:\n" + target, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Success, this, false);
            }
            catch (Exception ex)
            {
                DownloadStatusText.Text = "Download failed.";
                UWUVCI_MessageBox.Show("Download Failed", ex.Message, UWUVCI_MessageBoxType.Ok, UWUVCI_MessageBoxIcon.Error, this, true);
            }
        }
    }
}
