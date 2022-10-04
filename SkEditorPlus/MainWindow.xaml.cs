using Functionalities;
using HandyControl.Controls;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SkEditorPlus
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        public SkEditorAPI skEditor;
        private FileManager fileManager;
        private readonly string startupFile;

        public MainWindow(SkEditorAPI skEditor)
        {
            NamedPipeManager pipeManager = new("SkEditor+");
            pipeManager.StartServer();
            pipeManager.ReceiveString += HandleNamedPipe_OpenRequest;

            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }
            if (!File.Exists(appPath + @"\SkriptHighlighting.xshd"))
            {
                OptionsWindow optionsWindow = new(skEditor);
                optionsWindow.UpdateSyntaxFile();
            }

            startupFile = skEditor.GetStartupFile();
            this.skEditor = skEditor;
            InitializeComponent();

            Process process = Process.GetCurrentProcess();

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            fileManager = new FileManager(skEditor);
            new FunctionalitiesManager().LoadAll(skEditor);

            RPCManager.Initialize();
            if (startupFile != null)
            {
                fileManager.NewFile();
                fileManager.GetTextEditor().Load(startupFile);
                TabItem currentTabItem = tabControl.SelectedItem as TabItem;
                currentTabItem.ToolTip = startupFile;
                currentTabItem.Header = Path.GetFileName(startupFile);
                fileManager.OnTabChanged();
            }
            else
            {
                fileManager.NewFile();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            RPCManager.Uninitialize();
            Environment.Exit(0);
        }

        private void TabClosed(object sender, EventArgs e)
        {
            GC.Collect();
        }

        public FileManager GetFileManager()
        {
            return fileManager;
        }

        public void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(filesToOpen))
                {
                    TabItem lastTab = null;
                    foreach (string file in filesToOpen.Split(Environment.NewLine))
                    {
                        if (string.IsNullOrEmpty(file))
                        {
                            continue;
                        }
                        fileManager.NewFile();
                        fileManager.GetTextEditor().Load(file);
                        TabItem currentTabItem = tabControl.SelectedItem as TabItem;
                        currentTabItem.ToolTip = file;
                        currentTabItem.Header = Path.GetFileName(file);
                        lastTab = currentTabItem;
                        fileManager.OnTabChanged();
                    }


                    if (lastTab != null)
                        Dispatcher.InvokeAsync(() => tabControl.SelectedItem = lastTab);
                }


                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                this.Topmost = true;
                this.Activate();
                Dispatcher.BeginInvoke(new Action(() => { this.Topmost = false; }));
            });
        }
    }
}
