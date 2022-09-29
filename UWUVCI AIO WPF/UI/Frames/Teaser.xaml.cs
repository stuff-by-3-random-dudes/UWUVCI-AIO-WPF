using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UWUVCI_AIO_WPF.UI.Windows;

namespace UWUVCI_AIO_WPF.UI.Frames
{
    /// <summary>
    /// Interaktionslogik für Teaser.xaml
    /// </summary>
    public partial class Teaser : Page
    {
        public Teaser()
        {
            InitializeComponent();
        }

        private void tb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cm = new Custom_Message("Soon™", "Coming Soon™ to a UWUVCI Prime Near You!");
            cm.ShowDialog();
        }
    }
}
