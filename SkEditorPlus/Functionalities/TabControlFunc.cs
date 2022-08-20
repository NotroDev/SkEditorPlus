using SkEditorPlus.Managers;
using System;
using System.Windows.Controls;

namespace SkEditorPlus.Functionalities
{
    public class TabControlFunc : IFunctionality
    {
        FileManager fileManager;

        public void onEnable(SkEditorAPI skEditor)
        {
            fileManager = skEditor.GetMainWindow().GetFileManager();
            skEditor.GetMainWindow().tabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GC.Collect();
            fileManager.OnTabChanged();
        }
    }
}
