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
using UWUVCI_AIO_WPF.Classes.ENUM;
using UWUVCI_AIO_WPF.Classes;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Bases
{
    /// <summary>
    /// Interaktionslogik für CustomBaseFrame.xaml
    /// </summary>
    public partial class CustomBaseFrame : Page
    {
        GameConsole console;
        GameBases bases;
        bool existing;
        MainViewModel mvm;
        public CustomBaseFrame(GameBases Base, GameConsole console, bool existing)
        {
            InitializeComponent();
            mvm = (MainViewModel)FindResource("mvm");
            bases = Base;
            this.existing = existing;
            this.console = console;
        }
        private void CreateConfig(GameBases Base, GameConsole console, bool existing)
        {
            if (!existing)
            {
                mvm.GameConfiguration = new GameConfig();
                mvm.GameConfiguration.Console = console;
            }
            
            mvm.GameConfiguration.BaseRom = Base;

        }
       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //get folder
            //check if everything fine
            //if yes, enable config and create new game config
        }
    }
}
