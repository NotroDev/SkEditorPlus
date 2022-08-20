using HandyControl.Controls;
using HandyControl.Data;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;
        private FileManager fileManager;
        private readonly string startupFile;

        public MainWindow(SkEditorAPI skEditor)
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }
            if (!File.Exists(appPath + @"\SkriptHighlighting.xshd"))
            {
                OptionsWindow.UpdateSyntaxFile();
            }

            startupFile = skEditor.GetStartupFile();
            this.skEditor = skEditor;
            InitializeComponent();

            Process process = Process.GetCurrentProcess();
            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            fileManager = new FileManager(skEditor);
            new FunctionalitiesManager().LoadAll(skEditor);

            RPCManager.Initialize();
            if (startupFile != null)
            {
                fileManager.NewFile();
                fileManager.GetTextEditor().Load(startupFile);
                TabItem currentTabItem = tabControl.SelectedItem as TabItem;
                currentTabItem.ToolTip = startupFile;
                currentTabItem.Header = Path.GetFileName(startupFile);
            }
            else
            {
                fileManager.NewFile();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            RPCManager.Uninitialize();
        }

        private void TabClosed(object sender, EventArgs e)
        {
            GC.Collect();
        }

        public FileManager GetFileManager()
        {
            return fileManager;
        }
    }
}
