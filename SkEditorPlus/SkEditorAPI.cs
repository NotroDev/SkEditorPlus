using System;

namespace SkEditorPlus
{
    public interface SkEditorAPI
    {
        public MainWindow GetMainWindow();
        public event EventHandler WindowOpen;
        public string GetStartupFile();
    }
}
