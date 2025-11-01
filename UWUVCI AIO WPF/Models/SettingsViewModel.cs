using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms; // FolderBrowserDialog
using System.Windows.Input;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.UI.Windows;
using static UWUVCI_AIO_WPF.MainViewModel;

namespace UWUVCI_AIO_WPF.Models
{
    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        // Bindable props backed by a copy of settings (so Cancel discards)
        private string _basePath;
        private string _outPath;
        private string _ckey;
        private string _ancast;
        private bool _setBaseOnce;
        private bool _setOutOnce;

        public string BasePath { get => _basePath; set { _basePath = value; OnPropertyChanged(); } }
        public string OutPath { get => _outPath; set { _outPath = value; OnPropertyChanged(); } }
        public string Ckey { get => _ckey; set { _ckey = value; OnPropertyChanged(); } }
        public string Ancast { get => _ancast; set { _ancast = value; OnPropertyChanged(); } }
        public bool SetBaseOnce { get => _setBaseOnce; set { _setBaseOnce = value; OnPropertyChanged(); } }
        public bool SetOutOnce { get => _setOutOnce; set { _setOutOnce = value; OnPropertyChanged(); } }

        public ICommand BrowseBasePathCommand { get; }
        public ICommand BrowseOutPathCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand OpenSettingsFileCommand { get; }
        public ICommand OpenBaseFolderCommand { get; }
        public ICommand OpenOutFolderCommand { get; }
        public ICommand OpenLogFolderCommand { get; }
        public ICommand SaveCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel()
        {
            // load from current settings
            var s = JsonSettingsManager.Settings ?? new JsonAppSettings();
            _basePath = s.BasePath;
            _outPath = s.OutPath;
            _ckey = s.Ckey;
            _ancast = s.Ancast;
            _setBaseOnce = s.SetBaseOnce;
            _setOutOnce = s.SetOutOnce;

            BrowseBasePathCommand = new RelayCommand(_ => PickFolder(p => BasePath = p, initial: BasePath));
            BrowseOutPathCommand = new RelayCommand(_ => PickFolder(p => OutPath = p, initial: OutPath));
            ResetToDefaultsCommand = new RelayCommand(_ => ResetDefaults());
            OpenSettingsFileCommand = new RelayCommand(_ => OpenJsonFileSafely(JsonSettingsManager.SettingsFile));
            OpenBaseFolderCommand = new RelayCommand(_ => OpenPath(BasePath));
            OpenOutFolderCommand = new RelayCommand(_ => OpenPath(OutPath));
            OpenLogFolderCommand = new RelayCommand(_ => OpenPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI-V3", "Logs")));
            SaveCommand = new RelayCommand(_ => Save());
        }

        private void PickFolder(Action<string> set, string initial)
        {
            try
            {
                using var dlg = new FolderBrowserDialog
                {
                    SelectedPath = Directory.Exists(initial) ? initial : AppPaths.DefaultOutPath,
                    ShowNewFolderButton = true,
                    Description = "Select folder"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                    set(dlg.SelectedPath);
            }
            catch { /* ignore for Wine edge cases */ }
        }

        private void OpenPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (Directory.Exists(path))
                    Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                else if (File.Exists(path))
                    Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch { /* ignore */ }
        }

        private void OpenJsonFileSafely(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            try
            {
                if (!ToolRunner.UnderWine())
                {
                    // 🪟 Windows — just use default app
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    return;
                }

                // 🍷 Under Wine — attempt open fallbacks
                bool opened = false;

                // 1️⃣ Try macOS "open" or Linux "xdg-open"
                bool isMacOS = File.Exists("/usr/bin/open") && Directory.Exists("/Applications");
                string escaped = path.Replace("'", "'\\''");

                try
                {
                    string opener = isMacOS ? "open" : "xdg-open";
                    Process.Start("bash", $"-c \"{opener} '{escaped}'\"");
                    opened = true;
                }
                catch
                {
                    // Continue to next fallback
                }

                // 2️⃣ Try Notepad++
                if (!opened)
                {
                    try
                    {
                        Process.Start("wine", $"notepad++.exe \"{path.Replace("\\", "/")}\"");
                        opened = true;
                    }
                    catch { }
                }

                // 3️⃣ Try Wine Notepad
                if (!opened)
                {
                    try
                    {
                        Process.Start("wine", $"notepad \"{path.Replace("\\", "/")}\"");
                        opened = true;
                    }
                    catch { }
                }

                // 4️⃣ Final fallback — message to user
                if (!opened)
                {
                    UWUVCI_MessageBox.Show(
                        "Open Failed",
                        $"Could not open the settings file automatically.\n\nFile location:\n{path}",
                        UWUVCI_MessageBoxType.Ok,
                        UWUVCI_MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                UWUVCI_MessageBox.Show(
                    "Error Opening File",
                    $"An error occurred while trying to open the settings file:\n\n{ex.Message}\n\nLocation:\n{path}",
                    UWUVCI_MessageBoxType.Ok,
                    UWUVCI_MessageBoxIcon.Error
                );
            }
        }

        private void ResetDefaults()
        {
            BasePath = AppPaths.DefaultBasePath;
            OutPath = AppPaths.DefaultOutPath;
        }

        private void Save()
        {
            // Normalize & ensure folders
            if (string.IsNullOrWhiteSpace(BasePath)) BasePath = AppPaths.DefaultBasePath;
            if (string.IsNullOrWhiteSpace(OutPath)) OutPath = AppPaths.DefaultOutPath;

            Directory.CreateDirectory(BasePath);
            Directory.CreateDirectory(OutPath);

            // Commit to settings and persist
            var s = JsonSettingsManager.Settings ?? new JsonAppSettings();
            s.BasePath = BasePath;
            s.OutPath = OutPath;
            s.Ckey = Ckey?.Trim() ?? "";
            s.Ancast = Ancast?.Trim() ?? "";
            s.SetBaseOnce = SetBaseOnce;
            s.SetOutOnce = SetOutOnce;
            s.PathsSet = true;

            JsonSettingsManager.SaveSettings();

            // close dialog
            CloseOwnerWindow();
        }

        private void CloseOwnerWindow()
        {
            // hacky but fine for a small dialog VM
            foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                if (w.DataContext == this) { w.DialogResult = true; w.Close(); break; }
        }

        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    // Tiny command helper
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object> _exec;
        private readonly Func<object, bool> _can;
        public RelayCommand(Action<object> exec, Func<object, bool> can = null) { _exec = exec; _can = can; }
        public bool CanExecute(object parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _exec(parameter);
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
