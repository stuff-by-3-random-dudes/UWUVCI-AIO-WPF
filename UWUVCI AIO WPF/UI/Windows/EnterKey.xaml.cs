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
using System.Windows.Shapes;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    /// <summary>
    /// Interaktionslogik für EnterKey.xaml
    /// </summary>
    public partial class EnterKey : Window
    {
        bool ckey = false;
        Custom_Message cm;
        public EnterKey(bool ckey)
        {
            try
            {
                if (this.Owner?.GetType() != typeof(MainWindow))
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            InitializeComponent();
            this.ckey = ckey;
           
            if (ckey)
            {
                Key.Text = "CommonKey";
                region.Text = "WIIU";
                otp.Visibility = Visibility.Visible;
            }
            else
            {
                if ((FindResource("mvm") as MainViewModel).GbTemp.Tid != "")
                {
                  
                }
            }

        }

        public EnterKey(int i)
        {
            if (this.Owner?.GetType() != typeof(MainWindow))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            InitializeComponent();
            region.Visibility = Visibility.Hidden;
            
            Key.Text = "Enter the TitleKey for 000500101000400";
            if (i == 1)
            {
                Key.Text += "0";
            }
            else
            {
                Key.Text += "1";
            }
            or.Visibility = Visibility.Hidden;
            otp.Visibility = Visibility.Hidden;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(tbKey.Text.Length > 32 || tbKey.Text.Length < 32)
            {
                if(tbKey.Text.Length > 32)
                {
                    cm = new Custom_Message("Wrong Key", "The entered Key is too long");
                    cm.Owner = this;
                    cm.ShowDialog();
                }
                else
                {
                    cm = new Custom_Message("Wrong Key", "The entered Key is too short");
                    cm.Owner = this;
                    cm.ShowDialog();
                }
                    
            }
            else
            {
                if (ckey)
                {
                    MainViewModel mvm = (MainViewModel)FindResource("mvm");
                    if (mvm.checkcKey(tbKey.Text))
                    {
                        this.Visibility = Visibility.Hidden;
                        cm = new Custom_Message("Correct Key", "The entered CommonKey is correct!");
                        cm.Owner = this.Owner;
                        (this.Owner as MainWindow).move = false;
                        cm.ShowDialog();
                        (this.Owner as MainWindow).move = true;
                        this.Close();
                        mvm.ArePathsSet();
                    }
                    else
                    {
                       cm =  new Custom_Message("Incorrect Key", "The entered CommonKey is incorrect!");
                        cm.Owner = this;
                        cm.ShowDialog();
                    }
                }

                else
                {
                    MainViewModel mvm = (MainViewModel)FindResource("mvm");
                    if (mvm.checkKey(tbKey.Text))
                    {
                        this.Visibility = Visibility.Hidden;
                        cm =  new Custom_Message("Correct Key", "The entered TitleKey is correct!");
                        cm.Owner = this.Owner;
                        (this.Owner as MainWindow).move = false;
                        cm.ShowDialog();
                        (this.Owner as MainWindow).move = true;
                        this.Close();
                    }
                    else
                    {
                      cm =  new Custom_Message("Incorrect Key", "The entered TitleKey is incorrect!");
                        cm.Owner = this;
                        cm.ShowDialog();
                    }
                }
            }
           
           
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            tbKey.Focus();
            tbKey.Select(0,0);
        }
        private void wind_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = true;
        }

        private void wind_Closed(object sender, EventArgs e)
        {
            (FindResource("mvm") as MainViewModel).mw.Topmost = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            tbKey.Text = (FindResource("mvm") as MainViewModel).ReadCkeyFromOtp();
        }
    }
}
