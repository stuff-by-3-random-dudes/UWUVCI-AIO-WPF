using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UWUVCI_AIO_WPF
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			double height = System.Windows.SystemParameters.PrimaryScreenHeight;
			double witdh = System.Windows.SystemParameters.PrimaryScreenWidth;
			if (witdh < 1150 || height < 700)
			{
				MessageBox.Show("Your screen resolution is not supported, please use a resolution of atleast 1152x864", "Resolution not supported", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
			MainWindow wnd = new MainWindow();
			// The OpenFile() method is just an example of what you could do with the
			// parameter. The method should be declared on your MainWindow class, where
			// you could use a range of methods to process the passed file path
			if(e.Args.Length == 1 && e.Args[0] == "--debug")
			{
				wnd.setDebug();
			}
			wnd.Show();
		}
	}
}
