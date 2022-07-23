using HandyControl.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace SkEditorPlus.Managers
{
    public class FileManager
    {
        TabControl tabControl;
        static Regex regex = new("Nowy plik [0-9]+");

        public FileManager(SkEditorAPI skEditor)
        {
            tabControl = skEditor.GetMainWindow().tabControl;
        }

        public TextEditor GetTextEditor()
        {
            TextEditor textEditor = null;
            TabItem tp = tabControl.SelectedItem as TabItem;
            if (tp != null)
                textEditor = tp.Content as TextEditor;
            return textEditor;
        }

        int UntitledCount()
        {
            int itemIndex = 1;
            List<TabItem> tabItems = new List<TabItem>();
            List<int> tabItemsIndexes = new List<int>() { 1 };
            foreach (TabItem tabItem in tabControl.Items.OfType<TabItem>()
                .Where(item => regex.Matches(item.Header.ToString())
                .Count >= 1))
            {
                tabItems.Add(tabItem);
                tabItemsIndexes.Add(int.Parse(tabItem.Header.ToString().Replace("Nowy plik ", "")) + 1);
            }
            for (int i = 1; i <= tabItemsIndexes.Max(); i++)
            {
                if (tabItems.Find(item => item.Header.ToString().EndsWith(i.ToString())) == null)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        public void NewFile()
        {
            TabItem tabItem = new()
            {
                Header = "Nowy plik " + UntitledCount(),
                ToolTip = "",
                IsSelected = true
            };
            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            TextEditor codeEditor = new()
            {
                Style = Application.Current.FindResource("TextEditorStyle") as Style,
            };

            SearchPanel searchPanel = SearchPanel.Install(codeEditor);
            searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");

            codeEditor.PreviewMouseWheel += EditorMouseWheel;

            if (Properties.Settings.Default.Font != null)
            {
                codeEditor.FontFamily = new FontFamily(Properties.Settings.Default.Font);
            }

            codeEditor.TextChanged += OnTextChanged;

            tabItem.Content = codeEditor;

            tabControl.Items.Add(tabItem);
        }



        public void Save()
        {
            TabItem ti = tabControl.SelectedItem as TabItem;

            if (ti.ToolTip != null)
            {
                if (ti.ToolTip.ToString() != null && ti.ToolTip.ToString() != "")
                {
                    GetTextEditor().Save(ti.ToolTip.ToString());
                    if (ti.Header.ToString().EndsWith("*"))
                    {
                        ti.Header = ti.Header.ToString().Substring(0, ti.Header.ToString().Length - 1);
                    }
                    OnTabChanged();
                    return;
                }
                SaveDialog();
            }
        }

        public void SaveDialog()
        {
            SaveFileDialog saveFile = new()
            {
                Filter = "Skrypt (*.sk)|*.sk|Wszystkie pliki (*.*)|*.*"
            };
            if (saveFile.ShowDialog() == true)
            {
                GetTextEditor().Save(saveFile.FileName);
                TabItem ti = tabControl.SelectedItem as TabItem;
                ti.ToolTip = saveFile.FileName.ToString();
                ti.Header = saveFile.SafeFileName;
                OnTabChanged();
            }
        }

        public void OpenFile()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Skrypt (*.sk)|*.sk|Wszystkie pliki (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                TextEditor codeEditor = new()
                {
                    Style = Application.Current.FindResource("TextEditorStyle") as Style
                };
                codeEditor.TextChanged += OnTextChanged;
                codeEditor.PreviewMouseWheel += EditorMouseWheel;

                SearchPanel searchPanel = SearchPanel.Install(codeEditor);
                searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");

                TabItem tabItem = new()
                {
                    Header = openFileDialog.SafeFileName,
                    ToolTip = openFileDialog.FileName,
                    IsSelected = true,
                    Content = codeEditor
                };
                System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

                tabControl.Items.Add(tabItem);

                GetTextEditor().Load(openFileDialog.FileName);
                tabItem.Header = tabItem.Header.ToString()[..^1];

            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            //Save();
            TabItem tabItem = (TabItem)tabControl.SelectedItem;
            if (!tabItem.Header.ToString().EndsWith("*"))
            {
                tabItem.Header += "*";
            }
        }

        public void OnTabChanged()
        {
            TabItem ti = tabControl.SelectedItem as TabItem;

            if (ti == null) return;

            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var skriptHighlightingFile = Path.Combine(appFolderPath, "SkEditor Plus", "SkriptHighlighting.xshd");

            if (ti.ToolTip != null)
            {
                string extension = Path.GetExtension(ti.ToolTip.ToString());

                if (true)//extension.Equals(".sk"))
                {
                    /*
                    using (StreamReader s = new StreamReader(skriptHighlightingFile))
                    {
                        using (XmlTextReader reader = new XmlTextReader(s))
                        {
                            GetTextEditor().SyntaxHighlighting =
                                ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }
                    }
                    */
                }
            }
            if (ti.Header != null)
            {
                RPCManager.SetFile(ti.Header.ToString());
            }
            else
            {
                RPCManager.SetFile("brak");
            }
        }

        public void CloseFile()
        {
            TabItem tabItem = tabControl.SelectedItem as TabItem;
            if (tabControl.Items.IndexOf(tabItem) - 1 >= 0)
            {
                tabControl.SelectedIndex = tabControl.Items.IndexOf(tabItem) - 1;
            }
            else
            {
                tabControl.SelectedIndex = tabControl.Items.IndexOf(tabItem) + 1;
            }
            tabControl.Items.Remove(tabItem);
        }

        private void EditorMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double fontSize = GetTextEditor().FontSize + e.Delta / 25.0;

                if (fontSize < 6)
                    GetTextEditor().FontSize = 6;
                else
                {
                    if (fontSize > 200)
                        GetTextEditor().FontSize = 200;
                    else
                        GetTextEditor().FontSize = fontSize;
                }

                e.Handled = true;
            }
        }
    }
}
