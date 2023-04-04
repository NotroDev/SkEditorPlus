using AvalonEditB;
using SkEditorPlus.Managers;
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

        FormattingWindow GetQuickEditsWindow();

        FileManager GetFileManager();

        void ShowMessage(string title, string text = "");
        void ShowError(string title, string text = "");

        void OpenUrl(string url);

        TabControl GetSideTabControl();

        string GetVersion();

        int GetApiVersion();

        Dispatcher GetDispatcher();

        bool IsAddonInstalled(string name);
    }
}