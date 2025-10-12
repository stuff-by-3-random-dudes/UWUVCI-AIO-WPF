using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public enum UWUVCI_MessageBoxResult { None, Ok, Cancel, Yes, No }
    public enum UWUVCI_MessageBoxType { Ok, OkCancel, YesNo, YesNoCancel }
    public enum UWUVCI_MessageBoxIcon { Info, Success, Warning, Error }

    public partial class UWUVCI_MessageBox : Window
    {
        public UWUVCI_MessageBoxResult Result { get; private set; } = UWUVCI_MessageBoxResult.None;

        public UWUVCI_MessageBox(string title, string message,
            UWUVCI_MessageBoxType type = UWUVCI_MessageBoxType.Ok,
            UWUVCI_MessageBoxIcon iconType = UWUVCI_MessageBoxIcon.Info)
        {
            InitializeComponent();

            TitleBlock.Text = title;
            MessageBlock.Text = message;

            ApplyTheme(iconType);
            ShowButtons(type);

            // Fade-in
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            BeginAnimation(OpacityProperty, fadeIn);
        }

        private void ApplyTheme(UWUVCI_MessageBoxIcon icon)
        {
            SolidColorBrush brush;
            string geometryData;

            if (icon == UWUVCI_MessageBoxIcon.Info)
            {
                brush = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // blue
                geometryData = "M12,0A12,12 0 1,0 24,12A12,12 0 1,0 12,0ZM12,4A1.5,1.5 0 1,1 10.5,5.5A1.5,1.5 0 0,1 12,4ZM13.5,19H10.5V10H13.5Z";
            }
            else if (icon == UWUVCI_MessageBoxIcon.Success)
            {
                brush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // green
                geometryData = "M12 0A12 12 0 1 0 24 12A12 12 0 1 0 12 0ZM10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8Z";
            }
            else if (icon == UWUVCI_MessageBoxIcon.Warning)
            {
                brush = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // amber
                geometryData = "M1,21H23L12,2ZM13,16H11V10H13ZM13,20H11V18H13Z";
            }
            else // Error
            {
                brush = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // red
                geometryData = "M12,0A12,12 0 1,0 24,12A12,12 0 1,0 12,0ZM13.5,13.5V6.5H10.5V13.5ZM13.5,17.5V14.5H10.5V17.5Z";
            }

            IconPath.Fill = brush;
            BorderRoot.BorderBrush = brush;
            IconPath.Data = Geometry.Parse(geometryData);
        }

        private void ShowButtons(UWUVCI_MessageBoxType type)
        {
            BtnOk.Visibility = Visibility.Collapsed;
            BtnCancel.Visibility = Visibility.Collapsed;
            BtnYes.Visibility = Visibility.Collapsed;
            BtnNo.Visibility = Visibility.Collapsed;

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

        private void CloseSmoothly()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += delegate { Close(); };
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = UWUVCI_MessageBoxResult.Ok;
            CloseSmoothly();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = UWUVCI_MessageBoxResult.Cancel;
            CloseSmoothly();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = UWUVCI_MessageBoxResult.Yes;
            CloseSmoothly();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = UWUVCI_MessageBoxResult.No;
            CloseSmoothly();
        }

        public static UWUVCI_MessageBoxResult Show(
            string title,
            string message,
            UWUVCI_MessageBoxType type = UWUVCI_MessageBoxType.Ok,
            UWUVCI_MessageBoxIcon icon = UWUVCI_MessageBoxIcon.Info,
            Window owner = null,
            bool isModal = true)
        {
            UWUVCI_MessageBox box = new UWUVCI_MessageBox(title, message, type, icon);
            if (owner != null)
                box.Owner = owner;

            bool autoClose = !isModal && (icon == UWUVCI_MessageBoxIcon.Info || icon == UWUVCI_MessageBoxIcon.Success);

            if (autoClose)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3.5);
                timer.Tick += delegate
                {
                    timer.Stop();
                    box.Dispatcher.Invoke(box.CloseSmoothly);
                };
                timer.Start();
            }

            if (isModal || !autoClose)
                box.ShowDialog();
            else
                box.Show();

            return box.Result;
        }
    }
}
