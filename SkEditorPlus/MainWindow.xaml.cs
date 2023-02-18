using AvalonEditB;
using Functionalities;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using SharpVectors.Dom;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
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


        public event LoadFinishedEvent LoadFinished;


        private static readonly string version = "1.4.3";

        public static string Version { get => version; }

        public MainWindow(SkEditorAPI skEditor)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

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
            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }
            if (!File.Exists(appPath + @"\SkriptHighlighting.xshd") || !File.Exists(appPath + @"\YAMLHighlighting.xshd"))
            {
                string noFilesTitle = (string)FindResource("NoFilesTitle");
                string noFilesDescription = (string)FindResource("NoFilesDescription");
                string noFilesDownloaded = (string)FindResource("NoFilesDownloaded");

                OptionsWindow optionsWindow = new(skEditor);
                HandyControl.Controls.MessageBox.Show(noFilesDescription, noFilesTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                IsEnabled = false;
                await optionsWindow.UpdateSyntaxFile().ContinueWith((t) =>
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
                    TabItem currentTabItem = tabControl.SelectedItem as TabItem;
                    currentTabItem.Header = Path.GetFileName(file);

                    File.Delete(file);
                }
            }   

            else if (startupFile != null)
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

            FormattingWindow formattingWindow = new(skEditor);

            OnFinishedLoad();
        }

        protected virtual void OnFinishedLoad()
        {
            LoadFinished?.Invoke();
        }

        public void SetUpMica(bool firstTime = true)
        {
            bool mica = false;
            if (MicaHelper.IsSupported(BackdropType.Mica) && Properties.Settings.Default.Mica) mica = true;

            var oldStyle = (Style)Application.Current.FindResource("TextEditorStyle");
            Style newStyle = new(typeof(TextEditor));
            foreach (Setter setter in oldStyle.Setters.Cast<Setter>())
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

                    Topmost = true;
                    Activate();
                    Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }));
                });
            }
            catch { }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FixCut();
        }

        private void FixCut()
        {
            if (WindowState == WindowState.Maximized)
            {
                IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

                IntPtr hNext = hWnd;
                do
                    hNext = GetWindow(hNext, GW_HWNDNEXT);
                while (!IsWindowVisible(hNext));

                SetForegroundWindow(hNext);

                Activate();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            FixCut();
        }

        private void OnProjectClick(object sender, MouseButtonEventArgs e)
        {
            //LeftTabControl.SelectedIndex ^= 1;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            string crashTitle = "Crash";
            string crashDescription = "Sorry, but the program has crashed.{n}Don't worry, though, all your files will be saved!";
            string crashDescription2 = "If you can, please report the error in the issues section on GitHub.{n}{n}Error:{n}{0}";
            string copyAndOpenWebsite = "Copy and open website";
            string ok = "OK";

            HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Message = crashDescription.Replace("{n}", Environment.NewLine),
                Caption = crashTitle,
                Button = MessageBoxButton.OK,
                ConfirmContent = ok,
                IconBrushKey = ResourceToken.DangerBrush,
                IconKey = ResourceToken.ErrorGeometry
            });

            MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Message = crashDescription2.Replace("{n}", Environment.NewLine).Replace("{0}", e.StackTrace),
                Caption = crashTitle,
                Button = MessageBoxButton.OKCancel,
                ConfirmContent = copyAndOpenWebsite,
                CancelContent = ok,
                IconBrushKey = ResourceToken.DangerBrush,
                IconKey = ResourceToken.ErrorGeometry
            });
            if (result == MessageBoxResult.OK)
            {
                Clipboard.SetText(e.StackTrace);
                skEditor.OpenUrl("https://github.com/NotroDev/SkEditorPlus/issues");
            }

            foreach (TabItem tabItem in tabControl.Items)
            {
                if (tabItem.Content is not TextEditor textEditor)
                {
                    continue;
                }

                string tempPath = Path.GetTempPath();

                string skEditorFolder = Path.Combine(tempPath, "SkEditorPlus");
                if (!Directory.Exists(skEditorFolder))
                {
                    Directory.CreateDirectory(skEditorFolder);
                }

                string fileName = tabItem.Header.ToString();
                if (fileName.EndsWith("*"))
                {
                    fileName = fileName[..^1];
                }

                string tempFile = Path.Combine(skEditorFolder, fileName);

                tabItem.ToolTip = tempFile;

                if (!string.IsNullOrEmpty(tabItem.ToolTip.ToString()))
                {
                    textEditor.Save(tabItem.ToolTip.ToString());
                    continue;
                }
            }
        }
    }
}
