using SkEditorPlus.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
            if (args.Length > 1)
            {
                if (!File.Exists(args[1])) 
                    throw new FileNotFoundException("File not found... How did you do that?");
                else startupFile = args[1];
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
