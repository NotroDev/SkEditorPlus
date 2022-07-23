using System;
using System.IO;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus
{
    public partial class App : Application
    {
        public static string startupFile;

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
