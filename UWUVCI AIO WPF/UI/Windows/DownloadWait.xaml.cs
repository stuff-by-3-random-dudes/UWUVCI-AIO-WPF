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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für DownloadWait.xaml
    /// </summary>
   
    partial class DownloadWait : Window
    {
        MainViewModel mvm;
        
        public DownloadWait(string doing, string msg, MainViewModel mvm)
        {
            this.mvm = mvm;
            InitializeComponent();
            Key.Text = doing;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        public DownloadWait(string doing, string msg, MainViewModel mvm, bool t)
        {
            this.mvm = mvm;
            InitializeComponent();
            Key.Text = doing;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            msgT.Text = mvm.msg;
            pb.Value = mvm.Progress;
            if(Key.Text.Contains("Downloading Base"))
            {
                if(mvm.Progress < 70)
                {
                    mvm.Progress += 1;
                }
            }
            if(mvm.Progress == 100)
            {
                Close();
                
            }
        }

    
    }
}
