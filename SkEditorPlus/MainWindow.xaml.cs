using AvalonEditB;
using Functionalities;
using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Window = HandyControl.Controls.Window;

namespace SkEditorPlus
{
    public partial class MainWindow : Window
    {
        public SkEditorAPI skEditor;
        private FileManager fileManager;
        private readonly string startupFile;

        private static readonly string version = "1.3.0";

        public static string Version { get => version; }

        public MainWindow(SkEditorAPI skEditor)
        {
            try
            {
                NamedPipeManager pipeManager = new("SkEditor+");
                pipeManager.StartServer();
                pipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
            }
            catch { }

            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }
            if (!File.Exists(appPath + @"\SkriptHighlighting.xshd") || !File.Exists(appPath + @"\YAMLHighlighting.xshd"))
            {
                OptionsWindow optionsWindow = new(skEditor);
                optionsWindow.UpdateSyntaxFile();
            }

            startupFile = skEditor.GetStartupFile();
            this.skEditor = skEditor;
            InitializeComponent();
            Process process = Process.GetCurrentProcess();

            string countryName = RegionInfo.CurrentRegion.Name;
            if (countryName.Equals("PL"))
            {
                ConfigHelper.Instance.SetLang("pl");
            }
            else
            {
                ConfigHelper.Instance.SetLang("en");
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
    }
}
