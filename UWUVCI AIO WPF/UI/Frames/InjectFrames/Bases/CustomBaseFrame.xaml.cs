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
using GameBaseClassLibrary;
using System.Windows.Forms;
using System.IO;
using MessageBox = System.Windows.Forms.MessageBox;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Bases
{
    /// <summary>
    /// Interaktionslogik für CustomBaseFrame.xaml
    /// </summary>
    public partial class CustomBaseFrame : Page
    {
        GameConsoles console;
        GameBases bases;
        bool existing;
        MainViewModel mvm;
        public CustomBaseFrame(GameBases Base, GameConsoles console, bool existing)
        {
            InitializeComponent();
            tbCode.Text = "Code Folder not found";
            tbCode.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            tbContent.Text = "Content Folder not found";
            tbContent.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            tbMeta.Text = "Meta Folder not found";
            tbMeta.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            mvm = (MainViewModel)FindResource("mvm");
            bases = Base;
            this.existing = existing;
            this.console = console;
            mvm.SetCBASE(this);
        }
        private void CreateConfig(GameBases Base, GameConsoles console, bool existing)
        {
            if (!existing)
            {
                mvm.GameConfiguration = new GameConfig();
                mvm.GameConfiguration.Console = console;
            }
            
            mvm.GameConfiguration.BaseRom = Base;

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbCode.Text = "Code Folder not found";
            tbCode.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            tbContent.Text = "Content Folder not found";
            tbContent.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            tbMeta.Text = "Meta Folder not found";
            tbMeta.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            mvm.BaseDownloaded = false;
            //warning if using custom bases programm may crash
            DialogResult res = System.Windows.Forms.MessageBox.Show("If using Custom Bases there will be a chance that the programm crashes if adding a wrong base (example: a normal wiiu game instead of a nds vc game).\nIf you add a wrong base, we will not assist you fixing it, other than telling you to use another base.\nIf you agree to this please select Yes", "Custom base Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if(res == DialogResult.Yes)
            {                //get folder
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            if (mvm.DirectoryIsEmpty(dialog.SelectedPath))
                            {
                                System.Windows.Forms.MessageBox.Show("The folder is Empty. Please choose another folder");
                            }
                            else
                            { 
                               if(Directory.GetDirectories(dialog.SelectedPath).Length > 3)
                                {
                                    MessageBox.Show("This folder has too many subfolders. Please choose another folder");
                                }
                                else
                                {
                                    if(Directory.GetDirectories(dialog.SelectedPath).Length > 0)
                                    {
                                        //Code Content Meta
                                        if (Directory.Exists(System.IO.Path.Combine(dialog.SelectedPath, "content")) && Directory.Exists(System.IO.Path.Combine(dialog.SelectedPath, "code")) && Directory.Exists(System.IO.Path.Combine(dialog.SelectedPath, "meta")))
                                        {
                                            //create new Game Config
                                            mvm.GameConfiguration = new GameConfig();
                                            mvm.GameConfiguration.Console = console;
                                            mvm.GameConfiguration.CBasePath = dialog.SelectedPath;
                                            GameBases gb = new GameBases();
                                            gb.Name = "Custom";
                                            gb.Region = Regions.EU;
                                            gb.Path = mvm.GameConfiguration.CBasePath;
                                            mvm.GameConfiguration.BaseRom = gb;
                                            tbCode.Text = "Code Folder exists";
                                            tbCode.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                                            tbContent.Text = "Content Folder exists";
                                            tbContent.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                                            tbMeta.Text = "Meta Folder exists";
                                            tbMeta.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                                            mvm.BaseDownloaded = true;
                                        }
                                        else
                                        {
                                            MessageBox.Show("This Folder is not in the \"loadiine\" format");
                                        }
                                    }
                                    else
                                    {
                                        //WUP
                                        if (Directory.GetFiles(dialog.SelectedPath, "*.app").Length > 0 && Directory.GetFiles(dialog.SelectedPath, "*.h3").Length > 0 && File.Exists(System.IO.Path.Combine(dialog.SelectedPath, "title.tmd")) && File.Exists(System.IO.Path.Combine(dialog.SelectedPath, "title.tik")))
                                        {
                                            if (mvm.CBaseConvertInfo())
                                            {
                                                //Convert to LOADIINE => save under bases/custom or custom_x path => create new config
                                                string path = Injection.ExtractBase(dialog.SelectedPath, console);
                                                mvm.GameConfiguration = new GameConfig();
                                                mvm.GameConfiguration.Console = console;
                                                mvm.GameConfiguration.CBasePath = path;
                                                GameBases gb = new GameBases();
                                                gb.Name = "Custom";
                                                gb.Region = Regions.EU;
                                                gb.Path = mvm.GameConfiguration.CBasePath;
                                                mvm.GameConfiguration.BaseRom = gb;
                                                tbCode.Text = "Code Folder exists";
                                                tbCode.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                                                tbContent.Text = "Content Folder exists";
                                                tbContent.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                                                tbMeta.Text = "Meta Folder exists";
                                                tbMeta.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                                                mvm.BaseDownloaded = true;
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("This Folder does not contain needed NUS files");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception )
                        {
                           
                        }
                    }

                }

               

            }

        }
        public void Reset()
        {
            tbCode.Text = "Code Folder not found";
            tbCode.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            tbContent.Text = "Content Folder not found";
            tbContent.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            tbMeta.Text = "Meta Folder not found";
            tbMeta.Foreground = new SolidColorBrush(Color.FromRgb(205, 50, 50));
            mvm = (MainViewModel)FindResource("mvm");
        }
    }
}
