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
using UWUVCI_AIO_WPF.Classes;


namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Bases
{
    /// <summary>
    /// Interaktionslogik für BaseContainerFrame.xaml
    /// </summary>
    public partial class BaseContainerFrame : Page
    {
        private GameConsoles console;
        MainViewModel mvm;
        bool insertedConfig = false;
        public BaseContainerFrame(GameConsoles console)
        {
            InitializeComponent();
            
           

            mvm = (MainViewModel)FindResource("mvm");
            
            mvm.GameConfiguration.Console = console;
            if (console == GameConsoles.GCN) this.console = GameConsoles.WII;
            else this.console = console;
            mvm.GetBases(this.console);
            if (console == GameConsoles.NES)
            {
                cbCombo.ToolTip = "We recommend Metal Slader Glory for NES Injection!";
            }
            if(console == GameConsoles.SNES)
            {
                cbCombo.ToolTip = "We recommend Kirby's Dream Land 3 for SNES Injection!";
            }
            if(console == GameConsoles.MSX)
            {
                cbCombo.ToolTip = "You need to try which base works best.\nNemesis has the best MSX compatibility and Goemon the best MSX2. \nBoth support MSX and MSX2 ROMs";
            }
        }

       

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                mvm = FindResource("mvm") as MainViewModel;
                mvm.removeCBASE();
                if (cbCombo.SelectedIndex != -1 && cbCombo.SelectedIndex != mvm.OldIndex)
                {
                    if (cbCombo.SelectedIndex == 0)

                    {
                        id.Visibility = Visibility.Hidden;
                        idtxt.Visibility = Visibility.Hidden;
                        fLoadFrame.Content = new CustomBaseFrame(mvm.LBases[cbCombo.SelectedIndex], console, insertedConfig);
                    }
                    else
                    {
                        id.Visibility = Visibility.Visible;
                        
                        idtxt.Visibility = Visibility.Visible;
                        fLoadFrame.Content = new NonCustomBaseFrame(mvm.LBases[cbCombo.SelectedIndex], console, insertedConfig, this);
                        idtxt.Content = mvm.GameConfiguration.BaseRom.Tid;
                    }
                    mvm.OldIndex = cbCombo.SelectedIndex;
                }
            }
            catch (Exception)
            {

            }
            
          
        }

        private void id_Click(object sender, RoutedEventArgs e)
        {
            
            Clipboard.SetText(mvm.GameConfiguration.BaseRom.Tid);
        }
    }
}
