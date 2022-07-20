using HandyControl.Controls;
using ICSharpCode.AvalonEdit.Search;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus
{
    public partial class MainWindow : HandyControl.Controls.Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            RegisterCommands();

            RPCManager.Initialize();
            if (App.startupFile != null)
            {
                FileManager.GetInstance().NewFile();
                FileManager.GetTextEditor().Load(App.startupFile);
                TabItem currentTabItem = tabControl.SelectedItem as TabItem;
                currentTabItem.ToolTip = App.startupFile;
                currentTabItem.Header = Path.GetFileName(App.startupFile);
            }

            else
            {
                FileManager.GetInstance().NewFile();
            }
        }

        private void RegisterCommands()
        {
            RoutedCommand optionsCommand = new();
            optionsCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(optionsCommand, MenuSettings));

            RoutedCommand publishCommand = new();
            publishCommand.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(publishCommand, MenuPublish));

            RoutedCommand genCommand = new();
            genCommand.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(genCommand, MenuGenerate));

            //RoutedCommand runServerCommand = new();
            //runServerCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift));
            //CommandBindings.Add(new CommandBinding(runServerCommand, RunServer ));
        }

        public static SolidColorBrush GetBrush(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        private void MenuNewFile(object sender, RoutedEventArgs e)
        {
            FileManager.GetInstance().NewFile();
        }

        private void MenuSave(object sender, RoutedEventArgs e)
        {
            FileManager.GetInstance().Save();
        }

        private void MenuSaveAs(object sender, RoutedEventArgs e)
        {
            FileManager.GetInstance().SaveDialog();
        }

        private void MenuOpenFile(object sender, RoutedEventArgs e)
        {
            FileManager.GetInstance().OpenFile();
        }

        private void MenuCloseFile(object sender, RoutedEventArgs e)
        {
            FileManager.GetInstance().CloseFile();
        }

        private void MenuSettings(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new();
            optionsWindow.ShowDialog();
        }

        private void MenuPublish(object sender, RoutedEventArgs e)
        {
            PublishCode();
        }

        private void MenuGenerate(object sender, RoutedEventArgs e)
        {
            GeneratorWindow generatorWindow = new();
            generatorWindow.ShowDialog();
        }

        private void TabChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GC.Collect();

            FileManager.GetInstance().OnTabChanged();
        }

        private void TabClosed(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void WindowDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    FileManager.GetInstance().NewFile();
                    FileManager.GetTextEditor().Load(files[0]);
                    TabItem currentTabItem = tabControl.SelectedItem as TabItem;
                    currentTabItem.ToolTip = files[0];
                    currentTabItem.Header = Path.GetFileName(files[0]);
                }
            }
        }

        // Shortcuts

        private void ShortCutNewFile(object sender, ExecutedRoutedEventArgs e)
        {
            FileManager.GetInstance().NewFile();
        }

        private void ShortCutOpen(object sender, ExecutedRoutedEventArgs e)
        {
            FileManager.GetInstance().OpenFile();
        }

        private void ShortCutSave(object sender, ExecutedRoutedEventArgs e)
        {
            FileManager.GetInstance().Save();
        }

        private void ShortCutSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            FileManager.GetInstance().SaveDialog();
        }

        private void ShortCutClose(object sender, ExecutedRoutedEventArgs e)
        {
            FileManager.GetInstance().CloseFile();
        }

        private void RunServer(object sender, ExecutedRoutedEventArgs e)
        {
            ServerWindow serverWindow = new();
            serverWindow.Show();
        }

        private void PublishCode()
        {
            PublishWindow publishWindow = new();
            publishWindow.ShowDialog();
        }
    }
}
