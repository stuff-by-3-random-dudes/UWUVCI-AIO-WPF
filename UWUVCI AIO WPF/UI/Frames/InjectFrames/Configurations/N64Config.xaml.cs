using System;
using System.Collections.Generic;
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

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations
{
    /// <summary>
    /// Interaktionslogik für N64Config.xaml
    /// </summary>
    public partial class N64Config : Page
    {
        MainViewModel mvm;
        public N64Config()
        {
            mvm = (MainViewModel)FindResource("mvm");
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
