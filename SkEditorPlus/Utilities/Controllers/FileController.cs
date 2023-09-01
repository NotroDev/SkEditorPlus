using AvalonEditB;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using SkEditorPlus.Utilities.Builders;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Linq;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;
using TabControl = HandyControl.Controls.TabControl;

namespace SkEditorPlus.Utilities.Controllers
{
    public class FileController
    {
        public static SkEditorAPI skEditor = APIVault.GetAPIInstance();

        public static void Save()
        {
            TextEditor textEditor = skEditor.GetTextEditor();
            TabControl tabControl = skEditor.GetTabControl();
            try
            {
                if (textEditor == null) return;
                if (tabControl.SelectedItem is not TabItem ti) return;

                var toolTip = ti.ToolTip ?? "";
                if (!string.IsNullOrEmpty(toolTip.ToString()))
                {
                    textEditor.Save(toolTip.ToString());
                    ti.Header = ti.Header.ToString().TrimEnd('*');
                    TabController.OnTabChanged();
                    return;
                }
                SaveDialog();
            }
            catch (Exception e)
            {
                skEditor.ShowError("Looks like something went wrong while attempting to save your file.\nTry again, and if this keeps happening, it might be a good idea to create a backup.\n\nYou could also report it on the Discord server.", "Error");
                skEditor.ShowError($"Stack trace:\n\n{e.Message}\n{e.StackTrace}", "Error");
            }
        }

        public static void SaveDialog()
        {
            TextEditor textEditor = skEditor.GetTextEditor();
            TabControl tabControl = skEditor.GetTabControl();
            try
            {
                if (textEditor == null) return;
                if (tabControl.SelectedItem is not TabItem ti) return;
                SaveFileDialog saveFile = new()
                {
                    Filter = FileManager.filter
                };
                if (saveFile.ShowDialog() == true)
                {
                    textEditor.Save(saveFile.FileName);
                    ti.ToolTip = saveFile.FileName.ToString();
                    ti.Header = saveFile.SafeFileName;
                    AddonVault.addons.ForEach(addon =>
                    {
                        addon.OnFileCreate(saveFile.FileName);
                    });
                    TabController.OnTabChanged();
                }
            }
			catch (Exception e)
			{
				skEditor.ShowError("Looks like something went wrong while attempting to save your file.\nTry again, and if this keeps happening, it might be a good idea to create a backup.\n\nYou could also report it on the Discord server.", "Error");
				skEditor.ShowError($"Stack trace:\n\n{e.Message}\n{e.StackTrace}", "Error");
			}
		}

        public static void OpenFile()
        {
            TabControl tabControl = skEditor.GetTabControl();
            try
            {
                OpenFileDialog openFileDialog = new() { Filter = FileManager.filter, Multiselect = true };

                if (openFileDialog.ShowDialog() == true)
                {
                    if (tabControl.Items.Count > 0 && tabControl.Items[0] is TabItem tabItem &&
                            FileBuilder.regex.IsMatch(tabItem.Header?.ToString()) &&
                            skEditor.IsFile(tabItem) &&
                            tabItem.Content is TextEditor textEditor &&
                            textEditor.Document.Text.Length == 0)
                    {
                        skEditor.GetDispatcher().InvokeAsync(() =>
                        {
                            tabControl.Items.Remove(tabItem);
                            tabControl.SelectedIndex = 0;
                        });
                    }

                    foreach (var (fileName, index) in openFileDialog.FileNames.Select((v, i) => (v, i)))
                    {
                        FileBuilder.Build(openFileDialog.SafeFileNames[index], fileName);
                        skEditor.GetTextEditor().Load(fileName);
                        AddonVault.addons.ForEach(addon =>
                        {
                            addon.OnFileOpened(fileName);
                        });
                        if (tabControl.SelectedItem is TabItem ti && ti.Header.ToString().EndsWith("*", StringComparison.Ordinal))
                        {
                            ti.Header = ti.Header.ToString()[..^1];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void CloseFile()
        {
            TabControl tabControl = skEditor.GetTabControl();
            if (!skEditor.IsFileOpen()) return;
            var tabItem = tabControl.SelectedItem as TabItem;

            if (tabItem.Header.ToString().EndsWith("*"))
            {
                if (!ConfirmCloseFile()) return;
            }

            tabControl.SelectedIndex = tabControl.Items.IndexOf(tabItem) - 1 >= 0
                ? tabControl.Items.IndexOf(tabItem) - 1
                : tabControl.Items.IndexOf(tabItem) + 1;
            tabControl.Items.Remove(tabItem);
        }

        public static bool ConfirmCloseFile()
        {
            var attention = (string)Application.Current.FindResource("Attention");
            var closeConfirmation = (string)Application.Current.FindResource("CloseConfirmation");
            var yeah = (string)Application.Current.FindResource("Yeah");
            var cancel = (string)Application.Current.FindResource("Cancel");

            var result = MessageBox.Show(new MessageBoxInfo
            {
                Message = closeConfirmation,
                Caption = attention,
                ConfirmContent = yeah,
                CancelContent = cancel,
                IconBrushKey = ResourceToken.DarkWarningBrush,
                IconKey = ResourceToken.WarningGeometry,
                Button = MessageBoxButton.OKCancel
            });

            return result == MessageBoxResult.OK;
        }
    }
}
