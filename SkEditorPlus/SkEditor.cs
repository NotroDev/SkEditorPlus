using HandyControl.Controls;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Net.Http;

namespace SkEditorPlus
{
    public class SkEditor : SkEditorAPI
    {
        private readonly string[] args;
        private string startupFile = null;
        private MainWindow mainWindow;

        public event EventHandler WindowOpen;

        public SkEditor(string[] args)
        {
            this.args = args;
        }

        public void Run()
        {
            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    startupFile = args[0];
                }
                else
                {
                    string fileId = args[0].Replace("skeditor:", "");

                    string url = "https://code.skript.pl/" + fileId + "/raw";
                    string file;
                    using (var client = new HttpClient())
                    {
                        file = client.GetStringAsync(url).Result;
                    }

                    string tempFile = Path.GetTempFileName();
                    File.WriteAllText(tempFile, file);
                    startupFile = tempFile;
                }
            }

            mainWindow = new MainWindow(this);
            WindowOpen?.Invoke(mainWindow, EventArgs.Empty);
            mainWindow.Show();
        }

        public MainWindow GetMainWindow()
        {
            return mainWindow;
        }

        public string GetStartupFile()
        {
            return startupFile;
        }
    }
}
