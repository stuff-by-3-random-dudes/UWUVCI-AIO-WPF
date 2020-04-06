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
            this.console = console;
            mvm = (MainViewModel)FindResource("mvm");
            mvm.GetBases(console);
            mvm.GameConfiguration.Console = console;
        }

        public BaseContainerFrame(GameConsoles console, GameBases index)
        {
            InitializeComponent();
            this.console = console;
            mvm = (MainViewModel)FindResource("mvm");
            insertedConfig = true;
            mvm.GetBases(console);
            cbCombo.SelectedItem = index;
            ComboBox_SelectionChanged(null, null);
            insertedConfig = false;
           

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
                        fLoadFrame.Content = new CustomBaseFrame(mvm.LBases[cbCombo.SelectedIndex], console, insertedConfig);
                    }
                    else
                    {
                        fLoadFrame.Content = new NonCustomBaseFrame(mvm.LBases[cbCombo.SelectedIndex], console, insertedConfig);
                    }
                    mvm.OldIndex = cbCombo.SelectedIndex;
                }
            }
            catch (Exception)
            {

            }
            
          
        }
    }
}
