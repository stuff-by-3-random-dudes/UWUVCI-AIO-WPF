using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class TurboGrafX : Page, IDisposable
    {
        MainViewModel mvm;
        bool cd = false;
        public TurboGrafX()
        {
            InitializeComponent();
            mvm = FindResource("mvm") as MainViewModel;
            mvm.setThing(this);
        }
        public void Dispose()
        {

        }

        private void Set_Rom_Path(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            if (!cd) path = mvm.GetFilePath(true, false);
            else path = mvm.turbocd();

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
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.TGATv.ImgPath = path;
                mvm.GameConfiguration.TGATv.extension = new FileInfo(path).Extension;
                tv.Text = path;
            }

        }

        private void Set_DrcTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path))
            {
                mvm.GameConfiguration.TGADrc.ImgPath = path;
                mvm.GameConfiguration.TGADrc.extension = new FileInfo(path).Extension;
                drc.Text = path;
            }

        }
        
        private void Set_IconTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) {
                mvm.GameConfiguration.TGAIco.ImgPath = path;
                mvm.GameConfiguration.TGAIco.extension = new FileInfo(path).Extension;
                ic.Text = path;
            } 
        }

        private void Set_LogoTex(object sender, RoutedEventArgs e)
        {
            mvm.ImageWarning();
            string path = mvm.GetFilePath(false, false);
            if (!CheckIfNull(path)) {
                mvm.GameConfiguration.TGALog.ImgPath = path;
                mvm.GameConfiguration.TGALog.extension = new FileInfo(path).Extension;
                log.Text = path;
            } 
        }
        public void getInfoFromConfig()
        {
            tv.Text = mvm.GameConfiguration.TGATv.ImgPath;
            ic.Text = mvm.GameConfiguration.TGAIco.ImgPath;
            drc.Text = mvm.GameConfiguration.TGADrc.ImgPath;
            log.Text = mvm.GameConfiguration.TGALog.ImgPath;
            gn.Text = mvm.GameConfiguration.GameName;

        }
        private bool CheckIfNull(string s)
        {
            if(s == null || s.Equals(string.Empty))
            {
                return true;
            }
            return false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            mvm.RomPath = null;
            mvm.RomSet = false;
            mvm.CanInject = false;
            if (cd)
            {
                cd = false;
                mvm.mw.tbTitleBar.Text = "UWUVCI AIO - TurboGrafX-16 VC INJECT";
            }

            else 
            {
                cd = true;
                mvm.mw.tbTitleBar.Text = "UWUVCI AIO - TurboGrafX-CD VC INJECT";
            }
        }
    }
}
