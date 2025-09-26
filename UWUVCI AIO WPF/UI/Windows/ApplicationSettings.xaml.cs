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

            // initialize password boxes from vm
            CkeyBox.Password = _vm.Ckey ?? "";
            AncastBox.Password = _vm.Ancast ?? "";
        }

        private void CkeyBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Ckey = CkeyBox.Password;
        }

        private void AncastBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Ancast = AncastBox.Password;
        }

        // Simple “show/hide” toggles (replace with more secure UI if you want)
        private bool _ckeyShown;
        private void ToggleCkeyVisibility(object sender, RoutedEventArgs e)
        {
            _ckeyShown = !_ckeyShown;
            MessageBox.Show(_ckeyShown ? (_vm.Ckey ?? "") : "Hidden");
        }

        private bool _ancastShown;
        private void ToggleAncastVisibility(object sender, RoutedEventArgs e)
        {
            _ancastShown = !_ancastShown;
            MessageBox.Show(_ancastShown ? (_vm.Ancast ?? "") : "Hidden");
        }
    }
}
