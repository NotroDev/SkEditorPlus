using AutoUpdaterDotNET;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SkEditorPlus.Functionalities
{
    public class MenuBarFunc : IFunctionality
    {
        static readonly RoutedCommand[] fileApplicationCommands = new RoutedCommand[]
        {
            ApplicationCommands.New,
            ApplicationCommands.Open,
            ApplicationCommands.Save,
            ApplicationCommands.SaveAs,
            ApplicationCommands.Close,
            new RoutedCommand("Publish", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList()))
        };
        static readonly RoutedCommand[] editApplicationCommands = new RoutedCommand[]
        {
            new RoutedCommand("Generate", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
            new RoutedCommand("Format", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList()))
        };
        static readonly RoutedCommand[] otherApplicationCommands = new RoutedCommand[]
        {
            new RoutedCommand("Settings", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
        };
        static readonly RoutedCommand[] copilotApplicationCommand = new RoutedCommand[]
        {
            new RoutedCommand("Copilot", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
        };

        FileManager fileManager;
        SkEditorAPI skEditor;

        public void onEnable(SkEditorAPI skEditor)
        {
            this.skEditor = skEditor;
            fileManager = skEditor.GetMainWindow().GetFileManager();
            foreach (MenuItem menuItem in skEditor.GetMainWindow().File_MenuItem.Items)
            {
                menuItem.Click += File_MenuItem_Click;
            }
            foreach (MenuItem menuItem in skEditor.GetMainWindow().Edit_MenuItem.Items)
            {
                menuItem.Click += Edit_MenuItem_Click;
            }
            foreach (MenuItem menuItem in skEditor.GetMainWindow().Other_MenuItem.Items)
            {
                menuItem.Click += Other_MenuItem_Click;
            }
            foreach (RoutedCommand command in fileApplicationCommands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, File_MenuItem_Click));
            }
            foreach (RoutedCommand command in editApplicationCommands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, Edit_MenuItem_Click));
            }
            foreach (RoutedCommand command in otherApplicationCommands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, Other_MenuItem_Click));
            }

            foreach (RoutedCommand command in copilotApplicationCommand)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, fileManager.Copilot));
            }

        }

        private void File_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_NewFile":
                case "New":
                    fileManager.NewFile();
                    break;
                case "Menu_Open":
                case "Open":
                    fileManager.OpenFile();
                    break;
                case "Menu_Save":
                case "Save":
                    fileManager.Save();
                    break;
                case "Menu_SaveAs":
                case "SaveAs":
                    fileManager.SaveDialog();
                    break;
                case "Publish_SaveAs":
                case "Publish":
                    PublishWindow publishWindow = new(skEditor);
                    publishWindow.ShowDialog();
                    break;
                case "Menu_CloseFile":
                case "Close":
                    fileManager.CloseFile();
                    break;
            }
        }

        private void Edit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_Generate":
                case "Generate":
                    GenerateWindow generatorWindow = new(skEditor);
                    generatorWindow.ShowDialog();
                    break;
                case "Menu_Format":
                case "Format":
                    fileManager.FormatCode();
                    break;
            }
        }

        private void Other_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_Settings":
                case "Settings":
                    OptionsWindow optionsWindow = new(skEditor);
                    optionsWindow.ShowDialog();
                    break;
                case "Menu_Update":
                    AutoUpdater.InstalledVersion = new Version("1.0.0.0");
                    AutoUpdater.Start("https://raw.githubusercontent.com/NotroDev/SkEditorPlus/main/update.xml");
                    break;
            }
        }

        string GetName(object sender, RoutedEventArgs e)
        {
            if (e.GetType() == typeof(ExecutedRoutedEventArgs))
            {
                ExecutedRoutedEventArgs ex = (ExecutedRoutedEventArgs)e;
                RoutedCommand command = (RoutedCommand)ex.Command;
                return command.Name;
            }
            FrameworkElement element = (FrameworkElement)sender;
            return element.Name;
        }
    }
}
