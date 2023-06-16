using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Controllers;
using System;
using System.Windows.Controls;

namespace SkEditorPlus.Functionalities
{
    public class TabControlFunc : IFunctionality
    {
        FileManager fileManager;

        public void OnEnable(SkEditorAPI skEditor)
        {
            fileManager = skEditor.GetMainWindow().GetFileManager();
            skEditor.GetMainWindow().tabControl.SelectionChanged += OnTabChanged;
        }

        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            GC.Collect();
            TabController.OnTabChanged();
        }
    }
}
