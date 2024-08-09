using GameBaseClassLibrary;
using System.Windows.Controls;
using System.Windows.Input;

namespace UWUVCI_AIO_WPF.UI.Frames.KeyFrame
{
    /// <summary>
    /// Interaktionslogik für TKFrame.xaml
    /// </summary>
    public partial class TKFrame : Page
    {
        MainViewModel mvm;
        public TKFrame(GameConsoles c)
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
            mvm.getTempList(c);


        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(mvm.GbTemp != null)
            {
                mvm.EnterKey(false);
            }
        }
        
    }
}
