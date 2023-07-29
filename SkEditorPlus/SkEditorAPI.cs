using AvalonEditB;
using SkEditorPlus.Utilities;
using SkEditorPlus.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SkEditorPlus
{
    public interface SkEditorAPI
    {
        public MainWindow GetMainWindow();

		public event EventHandler WindowOpen;

		public string GetStartupFile();

        public Menu GetMenu();

        public bool IsFileOpen();

        public bool IsFile(HandyControl.Controls.TabItem tabItem);

        public TextEditor GetTextEditor();

        public HandyControl.Controls.TabControl GetTabControl();

        public QuickEditsWindow GetQuickEditsWindow();

        public FileManager GetFileManager();

        public void ShowMessage(string text, string title = "");
        public void ShowError(string text, string title = "");
        public void ShowSuccess(string text, string title = "");

        public void OpenUrl(string url);

        public TabControl GetSideTabControl();

        public string GetVersion();

        public int GetApiVersion();

        public Dispatcher GetDispatcher();

        public bool IsAddonInstalled(string name);
    }
}