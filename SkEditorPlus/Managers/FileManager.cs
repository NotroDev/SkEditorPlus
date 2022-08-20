using HandyControl.Controls;
using AvalonEditB;
using AvalonEditB.Highlighting;
using AvalonEditB.Search;
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
using System.Net.Http;
using System.Net.Http.Headers;
using MessageBox = HandyControl.Controls.MessageBox;
using Newtonsoft.Json.Linq;
using AvalonEditB.Document;
using HandyControl.Data;
using System.Windows.Documents;

namespace SkEditorPlus.Managers
{
    public class FileManager
    {
        readonly TabControl tabControl;
        static readonly Regex regex = new("Nowy plik [0-9]+");

        System.Windows.Controls.ToolTip toolTip = new();

        readonly string SkriptGeometry = "M5.8418 5.92578C5.8418 6.28906 5.76758 6.60742 5.61914 6.88086C5.4707 7.1543 5.26367 7.38281 4.99805 7.56641C4.73242 7.74609 4.41211 7.88086 4.03711 7.9707C3.66602 8.06055 3.25586 8.10547 2.80664 8.10547C2.60352 8.10547 2.40039 8.09766 2.19727 8.08203C1.99805 8.06641 1.80469 8.04688 1.61719 8.02344C1.43359 8 1.25977 7.97266 1.0957 7.94141C0.931641 7.91016 0.783203 7.87695 0.650391 7.8418V6.83398C0.943359 6.94336 1.27148 7.0293 1.63477 7.0918C2.00195 7.1543 2.41797 7.18555 2.88281 7.18555C3.21875 7.18555 3.50391 7.16016 3.73828 7.10938C3.97656 7.05469 4.16992 6.97656 4.31836 6.875C4.4707 6.76953 4.58008 6.64258 4.64648 6.49414C4.7168 6.3457 4.75195 6.17578 4.75195 5.98438C4.75195 5.77734 4.69336 5.60156 4.57617 5.45703C4.46289 5.30859 4.3125 5.17773 4.125 5.06445C3.9375 4.94727 3.72266 4.8418 3.48047 4.74805C3.24219 4.65039 2.99805 4.55078 2.74805 4.44922C2.49805 4.34766 2.25195 4.23828 2.00977 4.12109C1.77148 4 1.55859 3.85938 1.37109 3.69922C1.18359 3.53516 1.03125 3.34375 0.914062 3.125C0.800781 2.90625 0.744141 2.64648 0.744141 2.3457C0.744141 2.08398 0.798828 1.82617 0.908203 1.57227C1.01758 1.31836 1.1875 1.09375 1.41797 0.898438C1.64844 0.699219 1.94336 0.539063 2.30273 0.417969C2.66602 0.296875 3.09766 0.236328 3.59766 0.236328C3.72656 0.236328 3.86523 0.242188 4.01367 0.253906C4.16602 0.265625 4.31836 0.283203 4.4707 0.306641C4.62695 0.326172 4.7793 0.349609 4.92773 0.376953C5.08008 0.404297 5.2207 0.433594 5.34961 0.464844V1.40234C5.04883 1.31641 4.74805 1.25195 4.44727 1.20898C4.14648 1.16211 3.85547 1.13867 3.57422 1.13867C2.97656 1.13867 2.53711 1.23828 2.25586 1.4375C1.97461 1.63672 1.83398 1.9043 1.83398 2.24023C1.83398 2.44727 1.89062 2.625 2.00391 2.77344C2.12109 2.92188 2.27344 3.05469 2.46094 3.17188C2.64844 3.28906 2.86133 3.39648 3.09961 3.49414C3.3418 3.58789 3.58789 3.68555 3.83789 3.78711C4.08789 3.88867 4.33203 4 4.57031 4.12109C4.8125 4.24219 5.02734 4.38672 5.21484 4.55469C5.40234 4.71875 5.55273 4.91211 5.66602 5.13477C5.7832 5.35742 5.8418 5.62109 5.8418 5.92578ZM12.7324 8H11.4199L8.55469 4.24414V8H7.51172V0.341797H8.55469V3.89844L11.3613 0.341797H12.5977L9.57422 3.98047L12.7324 8Z";
        readonly string YmlGeometry = "M6.59766 0.341797L3.82617 5.26367V8H2.77148V5.24023L0 0.341797H1.25977L2.7832 3.14258L3.3457 4.26758L3.86133 3.24805L5.40234 0.341797H6.59766ZM12.9082 8H11.8887L11.7363 3.22461L11.6719 1.39062L11.3145 2.45703L10.1836 5.50391H9.46289L8.38477 2.57422L8.02734 1.39062L8.00391 3.30664L7.86914 8H6.88477L7.25977 0.341797H8.49609L9.52734 3.22461L9.86133 4.19727L10.1836 3.22461L11.2676 0.341797H12.5391L12.9082 8ZM18.9668 8H14.5605V0.341797H15.6211V7.10938H18.9668V8Z";
        readonly string OtherGeometry = "M4.40234 3.32227C4.40234 3.95508 4.22656 4.43555 3.875 4.76367C3.52344 5.08789 3.01367 5.27148 2.3457 5.31445L2.29883 6.75586H1.42578L1.34961 4.51172H2.03516C2.27344 4.51172 2.47266 4.48633 2.63281 4.43555C2.79297 4.38477 2.92188 4.3125 3.01953 4.21875C3.11719 4.125 3.1875 4.01172 3.23047 3.87891C3.27344 3.74219 3.29492 3.58984 3.29492 3.42188C3.29492 3.13672 3.23633 2.88281 3.11914 2.66016C3.00195 2.4375 2.83789 2.25 2.62695 2.09766C2.41602 1.94531 2.16406 1.83008 1.87109 1.75195C1.58203 1.67383 1.26562 1.63477 0.921875 1.63477H0.775391V0.720703H0.933594C1.32812 0.720703 1.6875 0.757812 2.01172 0.832031C2.33984 0.90625 2.63086 1.00781 2.88477 1.13672C3.14258 1.26172 3.36523 1.41016 3.55273 1.58203C3.74414 1.75391 3.90234 1.93555 4.02734 2.12695C4.15625 2.31836 4.25 2.51758 4.30859 2.72461C4.37109 2.92773 4.40234 3.12695 4.40234 3.32227ZM1.85352 7.55859C1.96289 7.55859 2.06445 7.58008 2.1582 7.62305C2.25195 7.66211 2.33203 7.7168 2.39844 7.78711C2.46875 7.85742 2.52344 7.94141 2.5625 8.03906C2.60156 8.13281 2.62109 8.23242 2.62109 8.33789C2.62109 8.44336 2.60156 8.54297 2.5625 8.63672C2.52344 8.73047 2.46875 8.8125 2.39844 8.88281C2.33203 8.94922 2.25195 9.00195 2.1582 9.04102C2.06445 9.08398 1.96289 9.10547 1.85352 9.10547C1.74805 9.10547 1.64844 9.08398 1.55469 9.04102C1.46094 9.00195 1.37891 8.94922 1.30859 8.88281C1.23828 8.8125 1.18359 8.73047 1.14453 8.63672C1.10547 8.54297 1.08594 8.44336 1.08594 8.33789C1.08594 8.23242 1.10547 8.13281 1.14453 8.03906C1.18359 7.94141 1.23828 7.85742 1.30859 7.78711C1.37891 7.7168 1.46094 7.66211 1.55469 7.62305C1.64844 7.58008 1.74805 7.55859 1.85352 7.55859Z";
        
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
            int tabIndex = 1;
            foreach(TabItem ti in tabControl.Items)
            {
                if (regex.IsMatch(ti.Header.ToString()))
                {
                    tabIndex++;
                }
            }
            return tabIndex;
        }

        public void NewFile()
        {
            TabItem tabItem = new()
            {
                Header = "Nowy plik " + UntitledCount(),
                ToolTip = "",
                IsSelected = true,
            };

            IconElement.SetGeometry(tabItem,
                Geometry.Parse(OtherGeometry));
            IconElement.SetHeight(tabItem, 16);
            IconElement.SetWidth(tabItem, 16);

            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            TextEditor codeEditor = new()
            {
                Style = Application.Current.FindResource("TextEditorStyle") as Style,
            };

            SearchPanel searchPanel = SearchPanel.Install(codeEditor.TextArea);
            searchPanel.ShowReplace = true;
            searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");

            codeEditor.PreviewMouseWheel += EditorMouseWheel;
            codeEditor.MouseHover += TextEditorMouseHover;
            codeEditor.MouseHoverStopped += TextEditorMouseHoverStopped;

            if (Properties.Settings.Default.Font != null)
            {
                codeEditor.FontFamily = new FontFamily(Properties.Settings.Default.Font);
            }

            codeEditor.TextChanged += OnTextChanged;

            codeEditor.TextArea.TextView.LinkTextForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a94c4");
            codeEditor.TextArea.TextView.LinkTextUnderline = true;

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
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Skrypt (*.sk)|*.sk|Wszystkie pliki (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TextEditor codeEditor = new()
                {
                    Style = Application.Current.FindResource("TextEditorStyle") as Style
                };
                codeEditor.TextChanged += OnTextChanged;
                codeEditor.PreviewMouseWheel += EditorMouseWheel;

                if (Properties.Settings.Default.Font != null)
                {
                    codeEditor.FontFamily = new FontFamily(Properties.Settings.Default.Font);
                }

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
                ChangeGeometry();
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            TabItem tabItem = (TabItem)tabControl.SelectedItem;

            if (!tabItem.Header.ToString().EndsWith("*"))
            {
                tabItem.Header += "*";
            }

        }

        public void Copilot(object sender, RoutedEventArgs e)
        {
            // Do MakeCopilot() 25 times
            for (int i = 0; i < 100; i++)
            {
                MakeCopilot();
            }
        }

        private void MakeCopilot()
        {
            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync($"http://51.83.182.188:3567/code/?code={GetTextEditor().Text}&key=KAY_1&temperature=1").Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    dynamic data = JObject.Parse(result);

                    string code = data.code;

                    code = code.Replace("<maxq1ampersand>", "&");
                    code = code.Replace("<maxq1hash>", "&");
                    code = code.Replace("<maxq1procent>", "%");
                    code = code.Replace("<maxq1plus>", "+");
                    code = code.Replace("<maxq1equal>", "=");
                    code = code.Replace("<maxq1quest>", "?");

                    GetTextEditor().Text = code;

                    TextEditor editor = GetTextEditor();

                    int offset = editor.CaretOffset;
                    DocumentLine line = editor.Document.GetLineByOffset(offset);
                    editor.CaretOffset = editor.Document.TextLength;
                }
            }
            catch (Exception)
            {

            }
        }

        public void OnTabChanged()
        {
            TabItem ti = tabControl.SelectedItem as TabItem;

            if (ti == null) return;

            ChangeGeometry();

            var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var skriptHighlightingFile = Path.Combine(appFolderPath, "SkEditor Plus", "SkriptHighlighting.xshd");

            if (ti.ToolTip != null)
            {
                string extension = Path.GetExtension(ti.ToolTip.ToString());

                if (true)//extension.Equals(".sk"))
                {
                    try
                    {
                        using StreamReader s = new StreamReader(skriptHighlightingFile);
                        using XmlTextReader reader = new XmlTextReader(s);
                        GetTextEditor().SyntaxHighlighting =
                            AvalonEditB.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                    catch(Exception e)
                    {
                        HandyControl.Controls.MessageBox.Show("Nie udało się wczytać pliku podświetlanaia składni.\n" + e.Message);
                    }

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

            if (tabItem.Header.ToString().EndsWith("*"))
            {
                MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
                {
                    Message = "Plik nie został zapisany. Czy na pewno chcesz go zamknąć?",
                    Caption = "Uwaga!",
                    ConfirmContent = "Ta",
                    CancelContent = "Anuluj",
                    Button = MessageBoxButton.OKCancel

                });

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

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

        private void ChangeGeometry()
        {
            TabItem tabItem = tabControl.SelectedItem as TabItem;

            string extension = Path.GetExtension(tabItem.ToolTip.ToString());

            string geometry = "";
            int size = 0;

            if (extension.Equals(".sk"))
            {
                geometry = SkriptGeometry;
                size = 16;
            }
            else if (extension.Equals(".yml") || extension.Equals("yaml"))
            {
                geometry = YmlGeometry;
                size = 24;
            }
            else
            {
                geometry = OtherGeometry;
                size = 14;
            }
            
            IconElement.SetGeometry(tabItem,
                Geometry.Parse(geometry));
            IconElement.SetHeight(tabItem, size);
            IconElement.SetWidth(tabItem, size);
        }

        public void FormatCode()
        {
            string code = GetTextEditor().Text;
            
            Regex regex = new("{(\\w)+(\\.(\\w)+)+}");
            
            foreach (Match variableMatch in regex.Matches(code))
            {
                string variable = variableMatch.Value.Replace(".", "::");
                code = code.Replace(variableMatch.Value, variable);
                GetTextEditor().Text = code;
            }
        }

        void TextEditorMouseHover(object sender, MouseEventArgs e)
        {
            if (GetWordAtMousePosition(e) != null)
            {
                toolTip.PlacementTarget = sender as UIElement;
                toolTip.Content = GetWordAtMousePosition(e);
                toolTip.IsOpen = true;
                e.Handled = true;
            }
        }

        void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
        }

        private string GetWordAtMousePosition(MouseEventArgs e)
        {
            var mousePosition = GetTextEditor().GetPositionFromPoint(e.GetPosition(GetTextEditor()));

            if (mousePosition == null)
                return string.Empty;

            var line = mousePosition.Value.Line;
            var column = mousePosition.Value.Column;
            var offset = GetTextEditor().Document.GetOffset(line, column);

            if (offset >= GetTextEditor().Document.TextLength)
                offset--;

            int offsetStart = TextUtilities.GetNextCaretPosition(GetTextEditor().Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
            int offsetEnd = TextUtilities.GetNextCaretPosition(GetTextEditor().Document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

            if (offsetEnd == -1 || offsetStart == -1)
                return string.Empty;

            var currentChar = GetTextEditor().Document.GetText(offset, 1);

            if (string.IsNullOrWhiteSpace(currentChar))
                return string.Empty;

            return GetTextEditor().Document.GetText(offsetStart, offsetEnd - offsetStart);
        }
    }
}
