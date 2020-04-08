using GameBaseClassLibrary;
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
namespace UWUVCI_AIO_WPF.UI.Frames
{
    /// <summary>
    /// Interaktionslogik für NDSFrame.xaml
    /// </summary>
    public partial class INJECTFRAME : Page, IDisposable
    {
        MainViewModel mvm;
        GameConsoles console;
        public INJECTFRAME(GameConsoles console)
        {
            mvm = FindResource("mvm") as MainViewModel;
            InitializeComponent();
            this.console = console;
            if(console == GameConsoles.N64)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.N64Config();
            } else if (console == GameConsoles.TG16)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.TurboGrafX();
            }
            else
            {
                fLoadConfig.Content = new InjectFrames.Configurations.OtherConfigs();
            }

            fBaseFrame.Content = new InjectFrames.Bases.BaseContainerFrame(console);
        }
        public void Dispose()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //import config
            mvm.selectConfig(console);

            

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Export config
            mvm.ExportFile();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            mvm.Pack(true);
            mvm.resetCBASE();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            mvm.Pack(false);
            mvm.resetCBASE();
        }
    }
}
