using HandyControl.Controls;
using ICSharpCode.AvalonEdit.Search;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;
        private FileManager fileManager;
        private readonly string startupFile;

        public MainWindow(SkEditorAPI skEditor)
        {
            startupFile = skEditor.GetStartupFile();
            this.skEditor = skEditor;
            InitializeComponent();
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
