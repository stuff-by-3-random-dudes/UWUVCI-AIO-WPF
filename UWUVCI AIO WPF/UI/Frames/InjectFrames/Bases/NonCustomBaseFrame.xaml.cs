using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        BaseContainerFrame bc;
        bool ex;
        GameConsoles consoles;
        public NonCustomBaseFrame(GameBases Base, GameConsoles console, bool existing, BaseContainerFrame bcf)
        {
            InitializeComponent();
            mvm = (MainViewModel)FindResource("mvm");
            bc = bcf;
            this.Base = Base;
            ex = existing;
            consoles = console;
            createConfig(Base, console);
            checkStuff(mvm.getInfoOfBase(Base));
        }
       
        public NonCustomBaseFrame()
        {
            InitializeComponent();
            mvm = (MainViewModel)FindResource("mvm");
            
        }
        private void createConfig(GameBases Base, GameConsoles console)
        {
            
            mvm.GameConfiguration.BaseRom = Base;
            mvm.GameConfiguration.Console = console;
           
        }
        private void checkStuff(List<bool> info)
        {
            mvm.CanInject = false;
            mvm.Injected = false;
            if (info[0])
            {
                tbDWNL.Text = "Base Downloaded";
                tbDWNL.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                btnDwnlnd.Content = "Redownload";
            }
            if (info[1])
            {
                TK.Visibility = Visibility.Hidden;
                tbTK.Text = "TitleKey Entered";
                tbTK.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            else
            {
                TK.Visibility = Visibility.Visible;
            }
            if (info[2])
            {
                CK.Visibility = Visibility.Hidden;
                tbCK.Text = "CommonKey Entered";
                tbCK.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
            }
            else
            {
                CK.Visibility = Visibility.Visible;
            }

            if(info[1] && info[2])
            {
                btnDwnlnd.IsEnabled = true;
                if (info[0])
                {
                    mvm.BaseDownloaded = true;
                    if (mvm.RomSet) mvm.CanInject = true;
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
            Thread.Sleep(500);
                checkStuff(mvm.getInfoOfBase(Base));
         
           
            
        }

        private void btnDwnlnd_Copy_Click(object sender, RoutedEventArgs e)
        {
            mvm.GbTemp = Base;
            mvm.EnterKey(false);
            checkStuff(mvm.getInfoOfBase(Base));
        }

        private void btnDwnlnd_Copy1_Click(object sender, RoutedEventArgs e)
        {
            mvm.EnterKey(true);
            checkStuff(mvm.getInfoOfBase(Base));
        }
    }
}
