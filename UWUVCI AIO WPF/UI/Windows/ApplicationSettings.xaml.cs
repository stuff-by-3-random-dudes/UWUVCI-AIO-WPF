using System.Windows;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class ApplicationSettings : Window
    {
        private readonly SettingsViewModel _vm;

        public ApplicationSettings()
        {
            InitializeComponent();
            _vm = new SettingsViewModel();
            DataContext = _vm;

            // initialize both views
            CkeyBox.Password = _vm.Ckey ?? "";
            CkeyPlain.Text = _vm.Ckey ?? "";
            AncastBox.Password = _vm.Ancast ?? "";
            AncastPlain.Text = _vm.Ancast ?? "";

            // initialize performance slider
            try
            {
                int deg = _vm.FileCopyParallelism;
                if (CopyParallelSlider != null) CopyParallelSlider.Value = deg;
                if (CopyParallelValue != null) CopyParallelValue.Text = deg.ToString();
            }
            catch { }
        }

        // keep VM in sync (hidden mode)
        private void CkeyBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Ckey = CkeyBox.Password;
            if (CkeyPlain.Visibility == Visibility.Visible)
                CkeyPlain.Text = _vm.Ckey ?? "";
        }

        private void AncastBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Ancast = AncastBox.Password;
            if (AncastPlain.Visibility == Visibility.Visible)
                AncastPlain.Text = _vm.Ancast ?? "";
        }

        // keep VM in sync (visible mode)
        private void CkeyPlain_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _vm.Ckey = CkeyPlain.Text;
            if (CkeyBox.Visibility == Visibility.Visible)
                CkeyBox.Password = _vm.Ckey ?? "";
        }

        private void AncastPlain_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _vm.Ancast = AncastPlain.Text;
            if (AncastBox.Visibility == Visibility.Visible)
                AncastBox.Password = _vm.Ancast ?? "";
        }

        // toggles
        private bool _ckeyVisible;
        private void ToggleCkeyVisibility(object sender, RoutedEventArgs e)
        {
            _ckeyVisible = !_ckeyVisible;
            if (_ckeyVisible)
            {
                // show plaintext
                CkeyPlain.Text = CkeyBox.Password;
                CkeyBox.Visibility = Visibility.Collapsed;
                CkeyPlain.Visibility = Visibility.Visible;
                BtnShowCkey.Content = "Hide";
                CkeyPlain.Focus();
                CkeyPlain.CaretIndex = CkeyPlain.Text.Length;
            }
            else
            {
                // show password
                CkeyBox.Password = CkeyPlain.Text;
                CkeyPlain.Visibility = Visibility.Collapsed;
                CkeyBox.Visibility = Visibility.Visible;
                BtnShowCkey.Content = "Show";
                CkeyBox.Focus();
            }
        }

        private bool _ancastVisible;
        private void ToggleAncastVisibility(object sender, RoutedEventArgs e)
        {
            _ancastVisible = !_ancastVisible;
            if (_ancastVisible)
            {
                AncastPlain.Text = AncastBox.Password;
                AncastBox.Visibility = Visibility.Collapsed;
                AncastPlain.Visibility = Visibility.Visible;
                BtnShowAncast.Content = "Hide";
                AncastPlain.Focus();
                AncastPlain.CaretIndex = AncastPlain.Text.Length;
            }
            else
            {
                AncastBox.Password = AncastPlain.Text;
                AncastPlain.Visibility = Visibility.Collapsed;
                AncastBox.Visibility = Visibility.Visible;
                BtnShowAncast.Content = "Show";
                AncastBox.Focus();
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };
            helpWindow.ShowDialog();
        }

        private void OpenPatchNotes_Click(object sender, RoutedEventArgs e)
        {
            var patchNotes = new HelpWindow("patchnotes")
            {
                Owner = this
            };
            patchNotes.ShowDialog();
        }

        private void CopyParallelSlider_OnValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (_vm == null || JsonSettingsManager.Settings == null)
                    return;

                int deg = (int)CopyParallelSlider.Value;
                _vm.FileCopyParallelism = deg;
                if (CopyParallelValue != null) CopyParallelValue.Text = _vm.FileCopyParallelism.ToString();
                JsonSettingsManager.Settings.FileCopyParallelism = _vm.FileCopyParallelism;
                JsonSettingsManager.SaveSettings();
                ToolRunner.Log($"FileCopyParallelism updated via UI to {deg}");
            }
            catch { }
        }

        private void ClearBaseCache_Click(object sender, RoutedEventArgs e)
        {
            bool ok = false;
            try { ok = BaseExtractor.ClearCache(); } catch { ok = false; }
            UWUVCI_MessageBox.Show(
                ok ? "Cache Cleared" : "Error",
                ok ? "BASE cache cleared successfully." : "Failed to clear BASE cache.",
                UWUVCI_MessageBoxType.Ok,
                ok ? UWUVCI_MessageBoxIcon.Info : UWUVCI_MessageBoxIcon.Error
            );
        }

        private void ResetAllCaches_Click(object sender, RoutedEventArgs e)
        {
            bool ok = false;
            try { ok = BaseExtractor.ClearCache(); } catch { ok = false; }
            UWUVCI_MessageBox.Show(
                ok ? "Caches Reset" : "Error",
                ok ? "All caches reset successfully." : "Failed to reset all caches.",
                UWUVCI_MessageBoxType.Ok,
                ok ? UWUVCI_MessageBoxIcon.Info : UWUVCI_MessageBoxIcon.Error
            );
        }

    }
}
