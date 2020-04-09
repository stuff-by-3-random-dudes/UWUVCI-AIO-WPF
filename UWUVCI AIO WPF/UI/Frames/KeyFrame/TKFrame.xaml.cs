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
