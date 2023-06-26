using AvalonEditB;
using Functionalities;
using HandyControl.Themes;
using HandyControl.Tools;
using SharpVectors.Converters;
using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Builders;
using SkEditorPlus.Utilities.Controllers;
using SkEditorPlus.Utilities.Handlers;
using SkEditorPlus.Utilities.Managers;
using SkEditorPlus.Utilities.Services;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
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

        private List<string> splashes = new();
        private int lastSplash = new();

        public Menu GetMenu()
        {
            return MenuBar;
        }

        public static string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";

        public MainWindow(SkEditorAPI skEditor)
        {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(CrashExceptionHandler.OnUnhandledException);

            RegistryUtility.PerformRegistryOperations();

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

            StateChanged += BackgroundFixer.OnStateChanged;

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
                    await UpdateService.UpdateItemFiles().ContinueWith((t) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            IsEnabled = true;
                        });
                    });
                }

                if (!File.Exists(appPath + @"\Syntax Highlighting\Default.xshd") || !File.Exists(appPath + @"\YAMLHighlighting.xshd"))
                {
                    await UpdateService.UpdateSyntaxFiles().ContinueWith((t) =>
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
            string[] languages = new string[] { "zh-cn", "en", "fa", "fr", "ko-kr", "ru", "tr", "pt-br", "pl", "es", "ca-es", "ja" };
            if (!languages.Contains(countryName.ToLower()))
            {
                countryName = "en";
            }
            else
            {
                ConfigHelper.Instance.SetLang(countryName);
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
            BackgroundFixer.FixBackground(this);

            fileManager = new FileManager();
            ProjectManager projectManager = new();

            projectTabItemLabel.MouseLeftButtonDown += new(projectManager.OnProjectClick);
            structureTabItemLabel.MouseLeftButtonDown += new(fileManager.OnStructureClick);

            BottomBarSpacesToTabsButton.Click += (s, e) => QuickEditsWindow.SpacesToTabs();

            if (Properties.Settings.Default.ProjectsExperiment)
            {
                leftTabControl.Visibility = Visibility.Visible;
            }
            if (Properties.Settings.Default.BottomBarExperiment)
            {
                BottomBar.Visibility = Visibility.Visible;
            }
            if (Properties.Settings.Default.AnalyzerExperiment)
            {
                ToggleAnalyzer(true);
            }

            new FunctionalityLoader().LoadAll(skEditor);

            DiscordRPCManager.Instance.InitializeClient();


            string tempPath = Path.GetTempPath();

            string skEditorFolder = Path.Combine(tempPath, "SkEditorPlus");
            Directory.CreateDirectory(skEditorFolder);

            if (Directory.GetFiles(skEditorFolder).Length > 0)
            {
                foreach (string file in Directory.GetFiles(skEditorFolder))
                {
                    Dispatcher.Invoke(() =>
                    {
                        FileManager.NewFile();

                        string[] lines = File.ReadAllLines(file);

                        if (lines.First().StartsWith("# This file was saved to the temp folder by SkEditor+."))
                        {
                            string originalPath = lines[3][17..];
                            bool saved = lines[4][9..].Equals("True");

                            File.WriteAllLines(file, lines.Skip(6));

                            fileManager.GetTextEditor().Load(file);

                            TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                            currentTabItem.Header = Path.GetFileName(file);
                            if (!saved)
                                currentTabItem.Header += "*";
                            if (!originalPath.Equals("null"))
                                currentTabItem.ToolTip = originalPath;


                            File.Delete(file);

                            TabController.OnTabChanged();

                            if (fileManager.GetTextEditor().Document.Text.EndsWith("\r"))
                            {
                                fileManager.GetTextEditor().Document.Text = fileManager.GetTextEditor().Document.Text.Remove(fileManager.GetTextEditor().Document.Text.Length - 1);
                            }
                        }
                        else
                        {
                            fileManager.GetTextEditor().Load(file);
                            TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                            currentTabItem.Header = Path.GetFileName(file);

                            File.Delete(file);
                        }
                    });
                }
            }

            else if (startupFile != null)
            {
                Dispatcher.Invoke(() =>
                {
                    FileManager.NewFile();
                    fileManager.GetTextEditor().Load(startupFile);
                    TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                    currentTabItem.ToolTip = startupFile;
                    currentTabItem.Header = Path.GetFileName(startupFile);
                    TabController.OnTabChanged();
                });
            }
            else
            {
                Dispatcher.Invoke(() => FileManager.NewFile());
            }

            QuickEditsWindow quickEditsWindow = new(skEditor);

            AddonVault.addons.ForEach(addon =>
            {
                addon.OnLoadFinished();
            });

            if (Properties.Settings.Default.CheckForUpdates)
            {
                UpdateService.CheckUpdate(false);
            }

            RemoveThisWeirdBorder(tabControl);

            var overflowScrollViewer = tabControl.Template.FindName("PART_OverflowScrollviewer", tabControl) as ScrollViewer;
            overflowScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            overflowScrollViewer.Style = (Style)Application.Current.FindResource("ScrollViewerNativeBaseStyle");
            overflowScrollViewer.IsDeferredScrollingEnabled = false;

            await Task.Run(() =>
            {
                if (File.Exists("Data/splashes.txt"))
                {
                    Task <string[]> task = File.ReadAllLinesAsync("Data/splashes.txt");
                    task.Wait();
                    splashes = task.Result.ToList();
                }
            });

            await CheckForAlert();
        }

        public static async Task CheckForAlert()
        {
            if (!Properties.Settings.Default.AlertsExperiment) return;

            string url = "https://notro.tech/resources/alert.txt";

            HttpClient client = new();
            string result = await client.GetStringAsync(url);
            if (!string.IsNullOrWhiteSpace(result))
            {
                string[] lines = result.Split('\n');

                if (lines.Length < 2) return;

                int alertId = int.Parse(lines[0]);
                if (Properties.Settings.Default.LastAlertId == alertId) return;
                Properties.Settings.Default.LastAlertId = alertId;
                Properties.Settings.Default.Save();

                string[] alertLines = lines.Skip(2).ToArray();
                string alertText = string.Join('\n', alertLines);

                MessageBox.Info(alertText, "Alert");
            }
        }

        public void SetUpMica(bool firstTime = true)
        {
            try
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
            catch
            {

            }
        }

        public void ToggleAnalyzer(bool enable)
        {
            Menu menu = MenuBar;
            MenuItem otherMenu = menu.Items[2] as MenuItem;
            if (enable)
            {
                MenuItem analyzerMenuItem = new()
                {
                    Header = "Analyze",
                    Icon = new TextBlock
                    {
                        Text = "\xe721",
                        FontFamily = new FontFamily("Segoe Fluent Icons")
                    }
                };

                MenuItem cleanAnalysisMenuItem = new()
                {
                    Header = "Clear analysis",
                    Icon = new TextBlock
                    {
                        Text = "\xe74d",
                        FontFamily = new FontFamily("Segoe Fluent Icons")
                    }
                };

                analyzerMenuItem.Click += async (sender, e) =>
                {
                    await CodeParsingUtility.ParseCode();
                };

                cleanAnalysisMenuItem.Click += (sender, e) =>
                {
                    CodeParsingUtility.RemoveLineBackgroundTransformers();
                    CodeParsingUtility.RemovePreviousHoverEvents();
                };

                otherMenu.Items.Insert(otherMenu.Items.Count - 1, analyzerMenuItem);
                otherMenu.Items.Insert(otherMenu.Items.Count - 1, cleanAnalysisMenuItem);
            }
            else
            {
                otherMenu.Items.RemoveAt(otherMenu.Items.Count - 2);
                otherMenu.Items.RemoveAt(otherMenu.Items.Count - 2);
            }
        }

        private void OnSkEditorLogoTooltip(object sender, ToolTipEventArgs e)
        {
            string dateFormat = DateTime.Now.ToString("dd.MM");

            string splash = GetSpecialSplash(dateFormat);
            if (!string.IsNullOrEmpty(splash))
            {
                SkEditorLogo.ToolTip = splash;
                return;
            }

            if (splashes.Count > 0)
            {
                int index = GetRandomSplashIndex();
                SkEditorLogo.ToolTip = splashes[index];
                lastSplash = index;
            }
            else
            {
                SkEditorLogo.ToolTip = "SkEditor+";
            }
        }

        private static string GetSpecialSplash(string dateFormat)
        {
            return dateFormat switch
            {
                "01.01" => "Happy new year!",
                "14.02" => "Happy Valentine's day!",
                "01.04" => "Happy April Fools' day!",
                "25.12" => "Merry Christmas!",
                "25.09" => "Happy birthday to me!",
                _ => null,
            };
        }

        private int GetRandomSplashIndex()
        {
            int index;
            int attemptCount = 0;
            do
            {
                index = new Random().Next(0, splashes.Count);
                attemptCount++;
            } while (index == lastSplash && attemptCount < 5);
            return index;
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

            AddonVault.addons.ForEach(addon =>
                {
                    addon.OnExiting();
                });

            if (tabControl.Items.Count > 0)
            {
                CrashExceptionHandler.SaveFilesToTemp();
                return;
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
            DiscordRPCManager.Uninitialize();

            Environment.Exit(0);
        }

        private void TabClosed(object sender, EventArgs e)
        {
            GC.Collect();
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnTabClose();
            });
            APIVault.GetAPIInstance().GetMainWindow().BottomBarLenght.Text = (Application.Current.FindResource("BottomBarLenght") as string).Replace("{0}", "0");
            APIVault.GetAPIInstance().GetMainWindow().BottomBarLines.Text = (Application.Current.FindResource("BottomBarLines") as string).Replace("{0}", "0");
            APIVault.GetAPIInstance().GetMainWindow().BottomBarPos.Text = (Application.Current.FindResource("BottomBarPosition") as string).Replace("{0}", "0");
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
                            string path = await WebBrowserService.GetCodeFromSkript(id);
                            if (path != null)
                            {
                                FileManager.NewFile();
                                fileManager.GetTextEditor().Load(path);
                                TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                                currentTabItem.Header = id + ".sk";
                                lastTab = currentTabItem;
                                TabController.OnTabChanged();
                                GeometryUtility.ChangeGeometry(currentTabItem);
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
                                FileManager.NewFile();
                                fileManager.GetTextEditor().Load(file);
                                TabItem currentTabItem = (TabItem)tabControl.SelectedItem;
                                currentTabItem.ToolTip = file;
                                currentTabItem.Header = Path.GetFileName(file);
                                lastTab = currentTabItem;
                                TabController.OnTabChanged();
                            }
                        }

                        if (lastTab != null)
                            await Dispatcher.InvokeAsync(() => tabControl.SelectedItem = lastTab);
                    }

                    if (WindowState == WindowState.Minimized)
                        WindowState = WindowState.Normal;

                    Topmost = true;
                    Activate();
                    await Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }));
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
            StructureBuilder.CreateStructure();
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

        private void OnLeftTabControlTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (leftTabControl.SelectedIndex == -1 || leftTabControl.SelectedIndex == 0)
            {
                leftTabControl.BorderThickness = new Thickness(1, 1, 0, 1);
            }
            else
            {
                leftTabControl.BorderThickness = new Thickness(1, 1, 1, 0);
            }
        }

        private void OnItemClosing(object sender, HandyControl.Controls.ClosingEventArgs e)
        {
            TabItem tabItem = e.TabItem;
            if (!tabItem.Header.ToString().EndsWith('*'))
            {
                return;
            }
            if (!FileController.ConfirmCloseFile())
            {
                e.Cancel = true;
            }
        }
    }
}
