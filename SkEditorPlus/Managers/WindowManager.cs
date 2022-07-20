using System.Collections.Generic;
using System.Windows;

namespace SkEditorPlus.Managers
{
    public static class WindowManager
    {
        public static MainWindow getMainWindow()
        {
            List<Window> windowList = new List<Window>();
            foreach (Window window in App.Current.Windows)
                windowList.Add(window);
            return (MainWindow)windowList.Find(window => window.GetType() == typeof(MainWindow));
        }
    }
}