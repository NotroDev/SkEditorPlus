using HandyControl.Controls;
using System.IO;
using System.Windows;

namespace SkEditorPlus.Functionalities
{
    public class MainWindowFunc : IFunctionality
    {
        MainWindow mainWindow;

        public void OnEnable(SkEditorAPI skEditor)
        {
            mainWindow = skEditor.GetMainWindow();
            mainWindow.Drop += MainWindow_Drop;
        }

        void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    var fm = mainWindow.GetFileManager();
                    Utilities.FileManager.NewFile();
                    fm.GetTextEditor().Load(file);
                    var currentTabItem = mainWindow.tabControl.SelectedItem as TabItem;
                    currentTabItem.ToolTip = file;
                    currentTabItem.Header = Path.GetFileName(file);
                }
            }
        }
    }
}
