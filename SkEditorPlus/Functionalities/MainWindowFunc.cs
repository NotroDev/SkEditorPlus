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
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    mainWindow.GetFileManager().NewFile();
                    mainWindow.GetFileManager().GetTextEditor().Load(files[0]);
                    TabItem currentTabItem = mainWindow.tabControl.SelectedItem as TabItem;
                    currentTabItem.ToolTip = files[0];
                    currentTabItem.Header = Path.GetFileName(files[0]);
                }
            }
        }
    }
}
