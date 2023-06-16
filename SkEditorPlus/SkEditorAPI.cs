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
        MainWindow GetMainWindow();

        event EventHandler WindowOpen;

        string GetStartupFile();

        Menu GetMenu();

        bool IsFileOpen();

        bool IsFile(HandyControl.Controls.TabItem tabItem);

        TextEditor GetTextEditor();

        HandyControl.Controls.TabControl GetTabControl();

        QuickEditsWindow GetQuickEditsWindow();

        FileManager GetFileManager();

        void ShowMessage(string text, string title = "");
        void ShowError(string text, string title = "");
        void ShowSuccess(string text, string title = "");

        void OpenUrl(string url);

        TabControl GetSideTabControl();

        string GetVersion();

        int GetApiVersion();

        Dispatcher GetDispatcher();

        bool IsAddonInstalled(string name);
    }
}