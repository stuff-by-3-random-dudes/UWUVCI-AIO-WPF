using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace UWUVCI_AIO_WPF.UI.Frames
{
    /// <summary>
    /// Interaktionslogik für StartFrame.xaml
    /// </summary>
    public partial class StartFrame : Page
    {
        public StartFrame()
        {
            InitializeComponent();
            tb.Text += "\n\nIf any Issues happen during injection and you updated from the old version using the AutoUpdater, please go to settings and click Update Tools.";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenKofiLink("uwuvci");
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            OpenKofiLink("zestyts");
        }
        private void OpenKofiLink(string urlSuffix)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://ko-fi.com/" + urlSuffix,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ko-fi.com/zestyts");
        }
    }
}