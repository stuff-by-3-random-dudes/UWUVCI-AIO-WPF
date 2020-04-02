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
    /// Interaktionslogik für NonCustomBaseFrame.xaml
    /// </summary>
    public partial class NonCustomBaseFrame : Page
    {
        MainViewModel mvm;
        GameBases Base;
        public NonCustomBaseFrame(GameBases Base, GameConsoles console, bool existing)
        {
            InitializeComponent();
            mvm = (MainViewModel)FindResource("mvm");
            if (!existing)
            {
                createConfig(Base, console);
            }
            this.Base = Base;
            checkStuff(mvm.getInfoOfBase(Base));
        }
       
        public NonCustomBaseFrame()
        {
            InitializeComponent();
            mvm = (MainViewModel)FindResource("mvm");
            
        }
        private void createConfig(GameBases Base, GameConsoles console)
        {
            mvm.GameConfiguration = new GameConfig();
            mvm.GameConfiguration.BaseRom = Base;
            mvm.GameConfiguration.Console = console;
           
        }
        private void checkStuff(List<bool> info)
        {
            if (info[0])
            {
                tbDWNL.Text = "Base Downloaded";
                tbDWNL.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            if (info[1])
            {
                tbTK.Text = "TitleKey Entered";
                tbTK.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            if (info[2])
            {
                tbCK.Text = "CommonKey Entered";
                tbCK.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }

            if(info[1] && info[2])
            {
                btnDwnlnd.IsEnabled = true;
                if (info[0])
                {
                    mvm.BaseDownloaded = true;
                }
                else
                {
                    mvm.BaseDownloaded = false;
                }
            }
            
        }

        private void btnDwnlnd_Click(object sender, RoutedEventArgs e)
        {
            mvm.Download();
            checkStuff(mvm.getInfoOfBase(Base));
        }
    }
}
