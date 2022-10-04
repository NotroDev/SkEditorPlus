using Functionalities;
using System;
using System.Text;
using System.Threading;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus
{
    public partial class App : Application
    {
        public static string startupFile;
        public Mutex Mutex;

        public App()
        {
            SingleInstanceCheck();
        }

        public void SingleInstanceCheck()
        {
            bool isOnlyInstance = false;
            Mutex = new Mutex(true, @"SkEditor+", out isOnlyInstance);
            if (!isOnlyInstance)
            {
                string filesToOpen = " ";
                var args = Environment.GetCommandLineArgs();
                if (args != null && args.Length > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i < args.Length; i++)
                    {
                        sb.AppendLine(args[i]);
                    }
                    filesToOpen = sb.ToString();
                }

                var manager = new NamedPipeManager("SkEditor+");
                manager.Write(filesToOpen);

                Environment.Exit(0);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            try
            {
                new SkEditor(e.Args).Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong.\n\nError message:\n" + ex.Message,
                        "SkEditor+", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
