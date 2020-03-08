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
using UWUVCI_AIO_WPF.Classes.ENUM;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Bases
{
    /// <summary>
    /// Interaktionslogik für BaseContainerFrame.xaml
    /// </summary>
    public partial class BaseContainerFrame : Page
    {
        private GameConsole console;
        MainViewModel mvm;
        bool insertedConfig = false;
        public BaseContainerFrame(GameConsole console)
        {
            InitializeComponent();
            this.console = console;
            mvm = (MainViewModel)FindResource("mvm");
            //READ INTO BASE LIST
        }

        public BaseContainerFrame(GameConsole console, GameBases index)
        {
            InitializeComponent();
            this.console = console;
            mvm = (MainViewModel)FindResource("mvm");
            insertedConfig = true;
            //READ INTO BASE LIST
            cbCombo.SelectedItem = index;
            ComboBox_SelectionChanged(null, null);
            insertedConfig = false;
           

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbCombo.SelectedIndex != -1)
            { 
                if (cbCombo.SelectedIndex == 0)
                {
                    fLoadFrame.Content = new CustomBaseFrame((GameBases)cbCombo.SelectedItem, console, insertedConfig);
                }
                else
                {
                    fLoadFrame.Content = new NonCustomBaseFrame((GameBases)cbCombo.SelectedItem, console, insertedConfig);
                }
            }
          
        }
    }
}
