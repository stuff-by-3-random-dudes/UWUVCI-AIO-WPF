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
using UWUVCI_AIO_WPF.Properties;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations
{
    /// <summary>
    /// Interaktionslogik für OtherConfigs.xaml
    /// </summary>
    public partial class OtherConfigs : Page, IDisposable
    {
        MainViewModel mvm;
        public OtherConfigs()
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
        }
        public void Dispose()
        {

        }

        private void Set_Rom_Path(object sender, RoutedEventArgs e)
        {
            string path = mvm.GetFilePath(true, false);
            if (!CheckIfNull(path)) {
                mvm.RomPath = path;
                mvm.RomSet = true;
                if (mvm.BaseDownloaded)
                {
                    mvm.CanInject = true;
                    
                }
                    }
            
        }

        private void InjectGame(object sender, RoutedEventArgs e)
        {
            mvm.Inject();
        }

        private void Set_TvTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGATv.ImgPath = path;


        }

        private void Set_DrcTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGADrc.ImgPath = path;
        }

        private void Set_IconTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGAIco.ImgPath = path;
        }

        private void Set_LogoTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) mvm.GameConfiguration.TGALog.ImgPath = path;
        }

        private bool CheckIfNull(string s)
        {
            if(s == null || s.Equals(string.Empty))
            {
                return true;
            }
            return false;
        }

    }
}
