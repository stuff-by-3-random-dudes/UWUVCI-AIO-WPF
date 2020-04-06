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
        public EnterKey(bool ckey)
        {
            InitializeComponent();
            this.ckey = ckey;
            if (ckey)
            {
                region.Visibility = Visibility.Hidden;
                Key.Text = "CommonKey";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ckey)
            {
                MainViewModel mvm = (MainViewModel)FindResource("mvm");
                if (mvm.checkcKey(tbKey.Text))
                {
                    this.Close();
                    MessageBox.Show("The entered CommonKey is correct!", "Correct Key", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("The entered CommonKey is incorrect!", "Incorrect Key", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MainViewModel mvm = (MainViewModel)FindResource("mvm");
                if (mvm.checkKey(tbKey.Text))
                {
                    MessageBox.Show("The entered TitleKey is correct!", "Correct Key", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("The entered TitleKey is incorrect!", "Incorrect Key", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
           
        }
    }
}
