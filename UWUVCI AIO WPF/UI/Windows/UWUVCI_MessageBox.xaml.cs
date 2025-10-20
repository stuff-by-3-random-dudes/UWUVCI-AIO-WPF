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
            Geometry geometry;

            switch (icon)
            {
                case UWUVCI_MessageBoxIcon.Success:
                    brush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // green
                    geometry = (Geometry)FindResource("IconSuccessGeometry");
                    break;

                case UWUVCI_MessageBoxIcon.Warning:
                    brush = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // amber
                    geometry = (Geometry)FindResource("IconWarningGeometry");
                    break;

                case UWUVCI_MessageBoxIcon.Error:
                    brush = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // red
                    geometry = (Geometry)FindResource("IconErrorGeometry");
                    break;

                default:
                    brush = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // blue
                    geometry = (Geometry)FindResource("IconInfoGeometry");
                    break;
            }
            IconBack.Fill = new SolidColorBrush(Color.FromArgb(32, brush.Color.R, brush.Color.G, brush.Color.B));
            IconPath.Fill = brush;
            IconPath.Data = geometry;
            BorderRoot.BorderBrush = brush;
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
