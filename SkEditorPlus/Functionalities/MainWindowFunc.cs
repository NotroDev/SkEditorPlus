using HandyControl.Controls;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SkEditorPlus.Functionalities
{
    public class MainWindowFunc : IFunctionality
    {
        MainWindow mainWindow;

        public void onEnable(SkEditorAPI skEditor)
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
