using AvalonEditB;
using System;
using System.Windows.Controls;

namespace SkEditorPlus
{
    public interface SkEditorAPI
    {
        public MainWindow GetMainWindow();
        
        public event EventHandler WindowOpen;

        /// <returns>Startup file</returns>
        public string GetStartupFile();


        /// <returns>App's main menu</returns>
        public Menu GetMenu()
        {
            return GetMainWindow().GetMenu();
        }

        /// <returns>True if any file is opened</returns>
        public bool IsFileOpen()
        {
            return GetMainWindow().GetFileManager().GetTextEditor() != null;
        }

        /// <returns>Current opened text editor if exists, otherwise null</returns>
        public TextEditor GetTextEditor()
        {
            return GetMainWindow().GetFileManager().GetTextEditor();
        }
    }
}
