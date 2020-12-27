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
            Process.Start("https://www.reddit.com/r/WiiUHacks/comments/jchcls/poc_retroarch_autoboot_starts_rom_automatically/");
        }
    }
}
