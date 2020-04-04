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
            mvm.GameConfiguration.N64Stuff = new N64Conf();
            InitializeComponent();
            
        }

        private void Set_Rom_Path(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(true, false);
            if (!CheckIfNull(path))
            {
                mvm.RomSet = true;
                mvm.RomPath = path;
                if (mvm.BaseDownloaded)
                {
                    mvm.CanInject = true;
                    
                }
            }
        }

        private void InjectGame(object sender, RoutedEventArgs e)
        {
            mvm.Inject();
            mvm.Injected = true;
        }

        private void Set_TvTex(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGATv.ImgPath = path;


        }

        private void Set_DrcTex(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGADrc.ImgPath = path;
        }

        private void Set_IconTex(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGAIco.ImgPath = path;
        }

        private void Set_LogoTex(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGALog.ImgPath = path;
        }

        private bool CheckIfNull(string s)
        {
            if (s == null || s.Equals(string.Empty))
            {
                return true;
            }
            return false;
        }

        private void Set_IniPath(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(false, true);
            if (!CheckIfNull(path)) mvm.GameConfiguration.N64Stuff.INIPath = path;
        }
    }
}
