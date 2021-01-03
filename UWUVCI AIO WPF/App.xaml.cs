using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.UI.Windows;

namespace UWUVCI_AIO_WPF
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
		
		Timer t = new Timer(5000);
		private void Application_Startup(object sender, StartupEventArgs e)
		 {

			if (Directory.Exists(@"custom"))
			{
				if (File.Exists(@"custom\main.dol"))
				{
					if (File.Exists(@"bin\Tools\nintendont.dol"))
					{
						File.Delete(@"bin\Tools\nintendont.dol");
						File.Copy(@"custom\main.dol", @"bin\Tools\nintendont.dol");
					}
					else
					{
						if(!Directory.Exists(@"bin"))
						{
							Directory.CreateDirectory(@"bin");
						}
						else if(!Directory.Exists(@"bin\Tools"))
						{
							Directory.CreateDirectory(@"bin\Tools");
						}
						File.Copy(@"custom\main.dol", @"bin\Tools\nintendont.dol");
					}

					if (File.Exists(@"bin\Tools\nintendont_force.dol"))
					{
						File.Delete(@"bin\Tools\nintendont_force.dol");
						File.Copy(@"custom\main.dol", @"bin\Tools\nintendont_force.dol");
					}
					else
					{
						File.Copy(@"custom\main.dol", @"bin\Tools\nintendont_force.dol");
					}
				}
			}
			bool check = true;
			bool bypass = false;
			if (e.Args.Length >= 1)
			{
				foreach(var s in e.Args)
				{
					if(s == "--skip")
					{
						check = false;
					}
					if(s == "--spacebypass")
					{
						
						bypass = true;
					}
				}
			}
			
			Process[] pname = Process.GetProcessesByName("UWUVCI AIO");
			if (pname.Length > 1 && check)
			{
				t.Elapsed += KillProg;
				t.Start();
				Custom_Message cm = new Custom_Message("Another Instance Running", " You already got another instance of UWUVCI AIO running. \n This instance will terminate in 5 seconds. ");

				cm.ShowDialog();
				KillProg(null, null);
			}
			else
			{
				MainWindow wnd = new MainWindow();
				double height = System.Windows.SystemParameters.PrimaryScreenHeight;
				double witdh = System.Windows.SystemParameters.PrimaryScreenWidth;
				if (witdh < 1150 || height < 700)
				{
					t.Elapsed += KillProg;
					t.Start();
					Custom_Message cm = new Custom_Message("Resolution not supported", "Your screen resolution is not supported, please use a resolution of atleast 1152x864\nThis instance will terminate in 5 seconds.");
					cm.ShowDialog();
					KillProg(null, null);
				}
				if (Environment.Is64BitOperatingSystem)
				{
					
				}
				else
				{
					
					//wnd.is32();
					Custom_Message cm = new Custom_Message("Warning", "Some features may cause issues on a 32Bit System. Upgrading to 64Bit would be recommended.\nDue to an Issue with packing on 32Bit Systems, you need Java installed for packing. \nReport any issues in the UWUVCI Discord, or ping @NicoAICP in #wiiu-assistance in the Nintendo Homebrew discord. ");
					cm.ShowDialog();
				}
				
				if (bypass) {
					wnd.allowBypass();

				}
				// The OpenFile() method is just an example of what you could do with the
				// parameter. The method should be declared on your MainWindow class, where
				// you could use a range of methods to process the passed file path
				if (e.Args.Length >= 1 && e.Args[0] == "--debug")
				{
					wnd.setDebug(bypass);
				}
				wnd.Show();
			}
			
		}

		private void KillProg(object sender, ElapsedEventArgs e)
		{
			t.Stop();
			Environment.Exit(1);
		}
	}
}
