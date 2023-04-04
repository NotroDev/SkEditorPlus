using AvalonEditB;
using Functionalities;
using HandyControl.Themes;
using HandyControl.Tools;
using SkEditorPlus.Managers;
using SkEditorPlus.Utilities;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Window = HandyControl.Controls.Window;

namespace SkEditorPlus
{
    public delegate void LoadFinishedEvent();
    
    public partial class MainWindow : Window
    {

        public SkEditorAPI skEditor;
        private FileManager fileManager;
        private readonly string startupFile;
        public Menu GetMenu()
        {
            return MenuBar;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);
        const uint GW_HWNDNEXT = 2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        public static string Version { get; } = "1.5.2";

        public MainWindow(SkEditorAPI skEditor)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            ExceptionHandler.skEditor = skEditor;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler.OnUnhandledException);

            try
            {
                NamedPipeManager pipeManager = new("SkEditor+");
                pipeManager.StartServer();
                pipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
            }
            catch { }

            startupFile = skEditor.GetStartupFile();
            this.skEditor = skEditor;
            Process process = Process.GetCurrentProcess();
            InitializeComponent();
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            Directory.CreateDirectory(appPath);
            if (!File.Exists(appPath + @"\SkriptHighlighting.xshd") || !File.Exists(appPath + @"\YAMLHighlighting.xshd"))
            {
                string noFilesTitle = (string)FindResource("NoFilesTitle");
                string noFilesDescription = (string)FindResource("NoFilesDescription");
                string noFilesDownloaded = (string)FindResource("NoFilesDownloaded");

                HandyControl.Controls.MessageBox.Show(noFilesDescription, noFilesTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                IsEnabled = false;
                await UpdateManager.UpdateSyntaxFile().ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsEnabled = true;
                        HandyControl.Controls.MessageBox.Show(noFilesDownloaded, noFilesTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                });

            }

            string countryName = RegionInfo.CurrentRegion.Name;
            if (countryName.Equals("PL"))
            {
                ConfigHelper.Instance.SetLang("pl");
            }
            else
            {
                ConfigHelper.Instance.SetLang("en");
            }


            string lang = Properties.Settings.Default.Language;
            string appDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location);

            string langFile = appDirectory + @"\Languages\" + lang + ".xaml";
            if (File.Exists(langFile))
            {
                ResourceDictionary dict = new()
                {
                    Source = new Uri(langFile, UriKind.RelativeOrAbsolute)
                };
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            else
            {
                Properties.Settings.Default.Language = "English";
                Properties.Settings.Default.Save();
            }
            SetUpMica();
            BackgroundFixManager.FixBackground(this);

            fileManager = new FileManager(skEditor);
            ProjectManager projectManager = new(skEditor);

            projectTabItemLabel.MouseLeftButtonDown += new(projectManager.OnProjectClick);
            structureTabItemLabel.MouseLeftButtonDown += new(fileManager.OnStructureClick);


            UpdateManager.skEditor = skEditor;
            new FunctionalitiesManager().LoadAll(skEditor);

            RPCManager.Initialize();

            string tempPath = Path.GetTempPath();

            string skEditorFolder = Path.Combine(tempPath, "SkEditorPlus");
            if (!Directory.Exists(skEditorFolder))
            {
                Directory.CreateDirectory(skEditorFolder);
            }

            if (Directory.GetFiles(skEditorFolder).Length > 0)
            {
                foreach (string file in Directory.GetFiles(skEditorFolder))
                {
                    fileManager.NewFile();
                    fileManager.GetTextEditor().Load(file);
                    TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                    currentTabItem.Header = Path.GetFileName(file);

                    File.Delete(file);
                }
            }   

            else if (startupFile != null)
            {
                fileManager.NewFile();
                fileManager.GetTextEditor().Load(startupFile);
                TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                currentTabItem.ToolTip = startupFile;
                currentTabItem.Header = Path.GetFileName(startupFile);
                fileManager.OnTabChanged();
            }
            else
            {
                fileManager.NewFile();
            }

            FormattingWindow formattingWindow = new(skEditor);

            AddonManager.addons.ForEach(addon =>
            {
                addon.OnLoadFinished();
            });

            if (Properties.Settings.Default.CheckForUpdates)
            {
                UpdateManager.CheckUpdate(false);
            }
        }

        public void SetUpMica(bool firstTime = true)
        {
            bool mica = MicaHelper.IsSupported(BackdropType.Mica) && Properties.Settings.Default.Mica;

            var oldStyle = (Style)Application.Current.FindResource("TextEditorStyle");
            var newStyle = new Style(typeof(TextEditor));
            foreach (var setter in oldStyle.Setters.Cast<Setter>())
            {
                newStyle.Setters.Add(setter);
            }

            byte transparency = (byte)Properties.Settings.Default.EditorTransparency;

            
            
            newStyle.Setters.Add(new Setter(BackgroundProperty, mica ? new SolidColorBrush(Color.FromArgb(transparency, 30, 30, 30)) : new SolidColorBrush(Color.FromRgb(0x1e, 0x1e, 0x1e))));

            Application.Current.Resources["TextEditorStyle"] = newStyle;

            if (mica && firstTime)
            {
                tabControl.Style = (Style)Application.Current.FindResource("TabControlStyle");
                tabControl.Background = Brushes.Transparent;

                ThemeResources.Current.Add("SecondaryRegionBrush", Brushes.Transparent);
                ThemeResources.Current.Add("SecondaryTextBrush", Brushes.Transparent);
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (tabControl.Items.Count > 0)
            {
                if (tabControl.Items.Cast<TabItem>().Any(tab => tab.Header.ToString().EndsWith("*")))
                {
                    string title = (string)Application.Current.FindResource("UnsavedFiles");
                    string message = (string)Application.Current.FindResource("UnsavedFilesMessage");
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                    }
                }
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
            try
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
                            TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
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

                    Topmost = true;
                    Activate();
                    Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }));
                });
            }
            catch { }
        }

        private void OnProjectIconClick(object sender, MouseButtonEventArgs e)
        {
            fileManager.OpenFolder();
        }

        private void OnStructureRefresh(object sender, RoutedEventArgs e)
        {
            fileManager.CreateStructure();
        }
    }
}
