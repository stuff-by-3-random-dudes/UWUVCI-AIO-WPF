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
            mvm.GameConfiguration.Console = console;
            
            InitializeComponent();
            this.console = console;
            if(console == GameConsoles.N64)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.N64Config();
            } else if (console == GameConsoles.TG16)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.TurboGrafX();
            }
            else if (console == GameConsoles.GBA)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.GBA();
            }
            else if (console == GameConsoles.WII)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.WiiConfig();
              
            }
            else if (console == GameConsoles.GCN)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.GCConfig();
               
            }
            else
            {
                fLoadConfig.Content = new InjectFrames.Configurations.OtherConfigs();
            }
            mvm.injected2 = false;
            Console.WriteLine("GameConfig : " + mvm.GameConfiguration.Console.ToString());
            fBaseFrame.Content = new InjectFrames.Bases.BaseContainerFrame(console);
        }
        public INJECTFRAME(GameConsoles console, GameConfig c)
        {
            mvm = FindResource("mvm") as MainViewModel;
            mvm.GameConfiguration.Console = console;
           
            InitializeComponent();
            this.console = console;
            if (console == GameConsoles.N64)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.N64Config(c);
            }
            else if (console == GameConsoles.TG16)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.TurboGrafX(c);
            }
            else if (console == GameConsoles.GBA)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.GBA(c);
            }
            else if (console == GameConsoles.WII)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.WiiConfig(c);
            }
            else if (console == GameConsoles.GCN)
            {
                fLoadConfig.Content = new InjectFrames.Configurations.GCConfig(c);
            }
            else
            {
                fLoadConfig.Content = new InjectFrames.Configurations.OtherConfigs(c);
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
