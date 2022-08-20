using HandyControl.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Windows;

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
                if (!File.Exists(args[0]))
                    throw new FileNotFoundException("File not found... How did you do that?");
                else startupFile = args[0];
            }

            mainWindow = new MainWindow(this);
            WindowOpen?.Invoke(mainWindow, EventArgs.Empty);
            mainWindow.Show();

            /*
            string procName = Process.GetCurrentProcess().ProcessName;

            Process[] processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {

                Process.GetCurrentProcess().Close();
                return;
            }
            else
            {
                mainWindow = new MainWindow(this);
                WindowOpen?.Invoke(mainWindow, EventArgs.Empty);
                mainWindow.Show();
            }
            */
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
