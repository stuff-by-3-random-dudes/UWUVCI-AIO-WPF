using System.Windows;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public enum UWUVCI_MessageBoxResult
    {
        None, Ok, Cancel, Yes, No
    }

    public enum UWUVCI_MessageBoxType
    {
        Ok, OkCancel, YesNo, YesNoCancel
    }

    public partial class UWUVCI_MessageBox : Window
    {
        public UWUVCI_MessageBoxResult Result { get; private set; } = UWUVCI_MessageBoxResult.None;

        public UWUVCI_MessageBox(string title, string message, UWUVCI_MessageBoxType type = UWUVCI_MessageBoxType.Ok)
        {
            InitializeComponent();

            TitleBlock.Text = title;
            MessageBlock.Text = message;

            switch (type)
            {
                case UWUVCI_MessageBoxType.Ok:
                    BtnOk.Visibility = Visibility.Visible;
                    break;
                case UWUVCI_MessageBoxType.OkCancel:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
                case UWUVCI_MessageBoxType.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    break;
                case UWUVCI_MessageBoxType.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) { Result = UWUVCI_MessageBoxResult.Ok; Close(); }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) { Result = UWUVCI_MessageBoxResult.Cancel; Close(); }
        private void BtnYes_Click(object sender, RoutedEventArgs e) { Result = UWUVCI_MessageBoxResult.Yes; Close(); }
        private void BtnNo_Click(object sender, RoutedEventArgs e) { Result = UWUVCI_MessageBoxResult.No; Close(); }

        public static UWUVCI_MessageBoxResult Show(string title, string message,
            UWUVCI_MessageBoxType type = UWUVCI_MessageBoxType.Ok, Window owner = null)
        {
            var box = new UWUVCI_MessageBox(title, message, type);
            if (owner != null)
                box.Owner = owner;

            box.ShowDialog();
            return box.Result;
        }
    }
}
