using AvalonEditB;
using Functionalities;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using Microsoft.Win32;
using SharpVectors.Dom;
using SkEditorPlus.Managers;
using SkEditorPlus.Utilities;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = HandyControl.Controls.MessageBox;
using TabItem = HandyControl.Controls.TabItem;
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

        public static string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";

        public MainWindow(SkEditorAPI skEditor)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            ExceptionHandler.skEditor = skEditor;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler.OnUnhandledException);

            RegistryKey skEditorProtocol = Registry.ClassesRoot.OpenSubKey("skeditor");
            if (skEditorProtocol == null)
            {
                var isAdmin = IsRunningAsAdministrator();
                if (!isAdmin)
                {

                    MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
                    {
                        Message = "Looks like it's your first time using SkEditor+ or you've recently updated the app.\nJust for this first time, you'll need to run the application as an administrator to be able to add registry keys.",
                        Caption = "SkEditor+",
                        Button = MessageBoxButton.YesNo,
                        YesContent = "Run as admin",
                        NoContent = "Close app",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });

                    if (result == MessageBoxResult.Yes)
                    {
                        var processInfo = new ProcessStartInfo(Environment.ProcessPath)
                        {
                            UseShellExecute = true,
                            Verb = "runas"
                        };

                        try
                        {
                            Process.Start(processInfo);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(new MessageBoxInfo
                            {
                                Message = "Oops, looks like you might have denied running the app as an administrator.\nI'm sorry, but it's necessary. The app will close now.",
                                Caption = "SkEditor+",
                                Button = MessageBoxButton.OK,
                                IconBrushKey = ResourceToken.DarkDangerBrush,
                                IconKey = ResourceToken.ErrorGeometry
                            });
                        }
                        Application.Current.Shutdown();
                        return;
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    skEditorProtocol = Registry.ClassesRoot.CreateSubKey("skeditor");
                    skEditorProtocol.SetValue("", "URL:skeditor Protocol");
                    skEditorProtocol.SetValue("URL Protocol", "");

                    RegistryKey shell = skEditorProtocol.CreateSubKey("shell");
                    RegistryKey open = shell.CreateSubKey("open");
                    RegistryKey command = open.CreateSubKey("command");

                    command.SetValue("", $"\"{Environment.ProcessPath}\" \"%1\"");


                    MessageBox.Show(new MessageBoxInfo
                    {
                        Message = "Thanks for cooperating! ;) The registry keys have been successfully added.",
                        Caption = "SkEditor+",
                        Button = MessageBoxButton.OK,
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                }
            }

            try
            {
                NamedPipeManager pipeManager = new("SkEditor+");
                pipeManager.StartServer();
                pipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }

            SettingsManager.LoadSettings();

            startupFile = skEditor.GetStartupFile();
            this.skEditor = skEditor;
            Process process = Process.GetCurrentProcess();

            InitializeComponent();
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            Directory.CreateDirectory(appPath);
            if (!File.Exists(appPath + @"\Syntax Highlighting\Default.xshd") || !File.Exists(appPath + @"\YAMLHighlighting.xshd") || !File.Exists(appPath + @"\items.json") || !Directory.Exists(appPath + @"\Items"))
            {
                string noFilesTitle = "No files";
                string noFilesDescription = "Looks like it's your first time using SkEditor+ or you've recently updated the app, because there are missing files. Click to download.";
                string noFilesDownloaded = "Successfully downloaded!";

                MessageBox.Show(noFilesDescription, noFilesTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                IsEnabled = false;

                if (!File.Exists(appPath + @"\items.json") || !Directory.Exists(appPath + @"\Items"))
                {
                    await UpdateManager.UpdateItemFiles().ContinueWith((t) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            IsEnabled = true;
                        });
                    });
                }

                if (!File.Exists(appPath + @"\Syntax Highlighting\Default.xshd") || !File.Exists(appPath + @"\YAMLHighlighting.xshd"))
                {
                    await UpdateManager.UpdateSyntaxFiles().ContinueWith((t) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            IsEnabled = true;
                        });
                    });
                }
                MessageBox.Show(noFilesDownloaded, noFilesTitle, MessageBoxButton.OK, MessageBoxImage.Information);
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
            Directory.CreateDirectory(skEditorFolder);

            if (Directory.GetFiles(skEditorFolder).Length > 0)
            {
                foreach (string file in Directory.GetFiles(skEditorFolder))
                {
                    Dispatcher.Invoke(() =>
                    {
                        fileManager.NewFile();
                        fileManager.GetTextEditor().Load(file);
                        TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                        currentTabItem.Header = Path.GetFileName(file);

                        File.Delete(file);
                    });
                }
            }

            else if (startupFile != null)
            {
                Dispatcher.Invoke(() =>
                {
                    fileManager.NewFile();
                    fileManager.GetTextEditor().Load(startupFile);
                    TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                    currentTabItem.ToolTip = startupFile;
                    currentTabItem.Header = Path.GetFileName(startupFile);
                    fileManager.OnTabChanged();
                });
            }
            else
            {
                Dispatcher.Invoke(() => fileManager.NewFile());
            }

            QuickEditsWindow quickEditsWindow = new(skEditor);

            AddonManager.addons.ForEach(addon =>
            {
                addon.OnLoadFinished();
            });

            if (Properties.Settings.Default.CheckForUpdates)
            {
                UpdateManager.CheckUpdate(false);
            }

            RemoveThisWeirdBorder(tabControl);
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

        private void OnSkEditorLogoTooltip(object sender, ToolTipEventArgs e)
        {
            SkEditorLogo.ToolTip = IsRunningAsAdministrator() ? "SkEditor+ (Administrator)" : "SkEditor+";
        }


        public static bool IsRunningAsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }


        public static SolidColorBrush GetSolidColorBrush(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }

        public static System.Drawing.Brush GetBrush(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            return new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SettingsManager.SaveSettings();

            AddonManager.addons.ForEach(addon =>
                {
                    addon.OnExiting();
                });

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

        private void OnItemClosing(object sender, EventArgs e)
        {
            // For now I don't think there is way to cancel this event. I'll add it in the future.

            //MessageBoxResult result = HandyControl.Controls.MessageBox.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            //if (result != MessageBoxResult.Yes)
            //{
            //    
            //}
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            RPCManager.Uninitialize();

            Environment.Exit(0);
        }

        private void TabClosed(object sender, EventArgs e)
        {
            GC.Collect();
            AddonManager.addons.ForEach(addon =>
            {
                addon.OnTabClose();
            });
        }

        public FileManager GetFileManager()
        {
            return fileManager;
        }

        public async void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            try
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    if (!string.IsNullOrEmpty(filesToOpen))
                    {
                        TabItem lastTab = null;

                        if (filesToOpen.StartsWith("skeditor://"))
                        {
                            string id = filesToOpen[11..^1].Trim().TrimEnd('/');
                            string path = await CodeFromWeb.GetCodeFromSkript(id);
                            if (path != null)
                            {
                                fileManager.NewFile();
                                fileManager.GetTextEditor().Load(path);
                                TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                                currentTabItem.Header = id + ".sk";
                                lastTab = currentTabItem;
                                fileManager.OnTabChanged();
                                fileManager.ChangeGeometry(currentTabItem);
                            }
                        }
                        else
                        {
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
                        }

                        if (lastTab != null)
                            await Dispatcher.InvokeAsync(() => tabControl.SelectedItem = lastTab);
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

        // I really don't know how to fix this in other way xD
        // So I'm using this weird method.
        // If you have any idea how to fix this, please tell me XD
        private void RemoveThisWeirdBorder(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Border border)
                {
                    border.BorderThickness = new Thickness(0, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                }

                RemoveThisWeirdBorder(child);
            }
        }
    }
}
