using System.Windows;
using UWUVCI_AIO_WPF.Models;

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

    }
}
