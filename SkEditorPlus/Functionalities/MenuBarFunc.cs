using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Controllers;
using SkEditorPlus.Utilities.Managers;
using SkEditorPlus.Utilities.Services;
using SkEditorPlus.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SkEditorPlus.Functionalities
{
    public class MenuBarFunc : IFunctionality
    {
        private static RoutedCommand CreateCommand(string commandName, Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            var inputGestures = new InputGestureCollection(new InputGesture[] { new KeyGesture(key, modifiers) });

            return new RoutedCommand(commandName, typeof(MenuBarFunc), inputGestures);
        }

        static readonly RoutedCommand[] fileApplicationCommands = new RoutedCommand[]
        {
            ApplicationCommands.New,
            ApplicationCommands.Open,
            ApplicationCommands.Save,
            ApplicationCommands.SaveAs,
            ApplicationCommands.Close,
            CreateCommand("Publish", Key.P, ModifierKeys.Control | ModifierKeys.Shift),
            CreateCommand("Export", Key.E, ModifierKeys.Control | ModifierKeys.Shift)
        };

        static readonly RoutedCommand[] editApplicationCommands = new RoutedCommand[]
        {
            CreateCommand("Generate", Key.G, ModifierKeys.Control | ModifierKeys.Shift),
            CreateCommand("Format", Key.F, ModifierKeys.Control | ModifierKeys.Shift),
            CreateCommand("Backpack", Key.B, ModifierKeys.Control | ModifierKeys.Shift),
        };

        static readonly RoutedCommand[] otherApplicationCommands = new RoutedCommand[]
        {
            CreateCommand("Settings", Key.O, ModifierKeys.Control | ModifierKeys.Shift),
            CreateCommand("Marketplace", Key.M, ModifierKeys.Control | ModifierKeys.Shift),
        };


        FileManager fileManager;
        SkEditorAPI skEditor;

        public void OnEnable(SkEditorAPI skEditor)
        {
            this.skEditor = skEditor;
            fileManager = skEditor.GetMainWindow().GetFileManager();
            AttachClickHandlers(skEditor.GetMainWindow().File_MenuItem.Items, File_MenuItem_Click);
            AttachClickHandlers(skEditor.GetMainWindow().Edit_MenuItem.Items, Edit_MenuItem_Click);
            AttachClickHandlers(skEditor.GetMainWindow().Other_MenuItem.Items, Other_MenuItem_Click);
            AttachCommandBindings(fileApplicationCommands, File_MenuItem_Click);
            AttachCommandBindings(editApplicationCommands, Edit_MenuItem_Click);
            AttachCommandBindings(otherApplicationCommands, Other_MenuItem_Click);
        }

        private static void AttachClickHandlers(ItemCollection items, RoutedEventHandler handler)
        {
            foreach (MenuItem menuItem in items)
            {
                menuItem.Click += handler;
            }
        }

        private void AttachCommandBindings(RoutedCommand[] commands, ExecutedRoutedEventHandler handler)
        {
            foreach (RoutedCommand command in commands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, handler));
            }
        }

        private void File_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_NewFile":
                case "New":
                    FileManager.NewFile();
                    break;

                case "Menu_Open":
                case "Open":
                    FileController.OpenFile();
                    break;

                case "Menu_OpenFolder":
                    fileManager.OpenFolder();
                    break;

                case "Menu_Save":
                case "Save":
                    FileController.Save();
                    break;

                case "Menu_SaveAs":
                case "SaveAs":
                    FileController.SaveDialog();
                    break;

                case "Menu_Publish":
                case "Publish":
                    if (skEditor.GetMainWindow().GetFileManager().GetTextEditor() == null) return;
                    PublishWindow publishWindow = new(skEditor);
                    publishWindow.ShowDialog();
                    break;

                case "Menu_ExportOptions":
                    ExportOptionsWindow exportOptionsWindow = new(skEditor);
                    exportOptionsWindow.ShowDialog();
                    break;

                case "Menu_CloseFile":
                case "Close":
                    FileController.CloseFile();
                    break;
            }

        }

        private void Edit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_Generate":
                case "Generate":
                    if (skEditor.GetMainWindow().GetFileManager().GetTextEditor() == null) return;
                    GenerateWindow generatorWindow = new(skEditor);
                    generatorWindow.ShowDialog();
                    break;
                case "Menu_Format":
                case "Format":
                    fileManager.FormatCode();
                    break;
                case "Menu_Backpack":
                case "Backpack":
                    if (skEditor.GetMainWindow().GetFileManager().GetTextEditor() == null) return;
                    BackpackWindow backpackWindow = new(skEditor);
                    backpackWindow.Show();
                    break;
            }
        }

        private async void Other_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_Settings":
                case "Settings":
                    NewOptionsWindow optionsWindow = new();
                    optionsWindow.ShowDialog();
                    break;

                case "Menu_ChangeSyntax":
                    SyntaxManager.ChangeSyntax("none");
                    break;

                case "Menu_Parser":
                    FileManager.OpenParser();
                    break;
                case "Menu_Docs":
                    FileManager.OpenDocs();
                    break;
                case "Menu_CheckUpdate":
                    UpdateService.CheckUpdate();
                    break;
                case "Menu_Marketplace":
                case "Marketplace":

                    MarketplaceWindow marketPlaceWindow = new(skEditor);

                    marketPlaceWindow.Show();
                    break;
            }

        }

        static string GetName(object sender, RoutedEventArgs e)
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
