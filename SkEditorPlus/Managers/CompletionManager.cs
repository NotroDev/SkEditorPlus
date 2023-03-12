using AvalonEditB;
using SkEditorPlus.Data;
using SkEditorPlus.Windows;
using SkEditorPlus.Windows.Generators;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SkEditorPlus.Managers
{
    public class CompletionManager
    {
        public static TextEditor textEditor;

        private readonly SkEditorAPI skEditor;

        private static ListBoxItem[] completionList = System.Array.Empty<ListBoxItem>();

        private CompletionWindow completionWindow;

        private Popup popup;

        public static CompletionManager instance;

        public CompletionManager(SkEditorAPI skEditor)
        {
            instance = this;
            this.skEditor = skEditor;
        }

        public void LoadCompletionManager(TextEditor textEditor)
        {
            CompletionManager.textEditor = textEditor;
            textEditor.TextArea.TextEntered += OnTextEntered;

            textEditor.PreviewKeyDown += OnKeyDown;
        }


        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            var caretOffset = textEditor.CaretOffset;
            var line = textEditor.Document.GetLineByOffset(caretOffset);
            var text = textEditor.Document.GetText(line.Offset, caretOffset - line.Offset);
            if (textEditor.SelectedText.Equals(text)) text = "";
            else if (text.Length > 0) text = text.Remove(text.Length - 1);

            if (e.Key == Key.Back || e.Key == Key.Left)
            {
                HidePopup();
                ShowCompletionWindow(text);
            }

            else if (e.Key == Key.Right)
            {
                if (text.Length > 0)
                {
                    try
                    {
                        if (caretOffset - line.Offset + 1 > textEditor.Document.TextLength) return;
                        var caretText = textEditor.Document.GetText(line.Offset, caretOffset - line.Offset + 1);

                        HidePopup();
                        ShowCompletionWindow(caretText);
                    }
                    catch
                    {
                        
                    }
                }
            }

            else if ((e.Key == Key.Down || e.Key == Key.Up) && completionWindow != null)
            {
                HandleArrowKey(e.Key, completionWindow.completionList);
                if (popup.IsOpen)
                {
                    e.Handled = true;
                }
            }

            else if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (completionWindow == null) return;
                if (!popup.IsOpen) return;

                int selectedIndex = completionWindow.completionList.SelectedIndex;
                if (selectedIndex < 0) return;

                ListBoxItem item = (ListBoxItem)completionWindow.completionList.SelectedItem;

                var split = text.Split(' ');
                var lastWord = split[^1].TrimStart();
                var index = text.LastIndexOf(lastWord);
                var newText = text.Remove(index, lastWord.Length);

                switch (item.Content)
                {
                    case "commandgen":
                        textEditor.Document.Replace(line.Offset, caretOffset - line.Offset, "");
                        CommandGenerator commandGenerator = new(skEditor);
                        commandGenerator.ShowDialog();
                        break;

                    case "guigen":
                        textEditor.Document.Replace(line.Offset, caretOffset - line.Offset, "");
                        GuiGenerator guiGenerator = new(skEditor);
                        guiGenerator.ShowDialog();
                        break;

                    default:
                        CompletionDataElement element = null;
                        foreach (CompletionDataElement dataElement in CompletionData.completionList)
                        {
                            if (dataElement.Name.Equals(item.Content))
                            {
                                element = dataElement;
                                break;
                            }
                        }


                        newText += element.Word;

                        string[] lines = newText.Split("\n");
                        string firstLine = lines[0];

                        int tabs = GetTabCount(firstLine);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (i == 0) continue;
                            lines[i] = new string('\t', tabs) + lines[i];
                        }

                        newText = string.Join("\n", lines);


                        if (element.Word.Contains("{c}"))
                        {
                            var caretIndex = newText.IndexOf("{c}");
                            newText = newText.Replace("{c}", "");

                            textEditor.Document.Replace(line.Offset, caretOffset - line.Offset, newText);

                            caretIndex = Math.Min(caretIndex, newText.Length);
                            textEditor.CaretOffset = line.Offset + caretIndex;
                        }
                        else
                        {
                            textEditor.Document.Replace(line.Offset, caretOffset - line.Offset, newText);
                        }

                        break;
                }

                completionWindow.completionList.SelectedIndex = -1;
                completionWindow.completionList.ItemsSource = null;
                completionWindow = null;
                popup.IsOpen = false;
                popup = null;
                e.Handled = true;
            }
        }

        private void HandleArrowKey(Key key, ListBox listBox)
        {
            int selectedIndex = listBox.SelectedIndex;
            listBox.SelectedIndex = key switch
            {
                Key.Down when selectedIndex < listBox.Items.Count - 1 => selectedIndex + 1,
                Key.Up when selectedIndex > 0 => selectedIndex - 1,
                _ => selectedIndex
            };
            listBox.ScrollIntoView(listBox.SelectedItem);
        }

        private static int GetTabCount(string line)
        {
            return line.TakeWhile(c => c == '\t').Count();
        }


        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            string text = GetText();
            string lastWord = GetLastWord(text);

            if (IsInQuote(text, lastWord))
            {
                return;
            }

            HidePopup();
            ShowCompletionWindow(lastWord);
        }

        private string GetText()
        {
            var caretOffset = textEditor.CaretOffset;
            var line = textEditor.Document.GetLineByOffset(caretOffset);
            return textEditor.Document.GetText(line.Offset, caretOffset - line.Offset);
        }

        private string GetLastWord(string text)
        {
            return text.Split(' ').LastOrDefault() ?? text;
        }

        private bool IsInQuote(string text, string lastWord)
        {
            string textBeforeLastWord = text[..^lastWord.Length];
            return textBeforeLastWord.Count(c => c == '"') % 2 == 1;
        }

        private void HidePopup()
        {
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }

        private CompletionWindow CreateCompletionWindow(ListBoxItem[] completionList)
        {
            var completionWindow = new CompletionWindow
            {
                completionList =
                {
                    ItemsSource = completionList
                }
            };
            completionWindow.completionList.SelectedIndex = 0;
            return completionWindow;
        }

        private void ShowCompletionWindow(string lastWord)
        {
            lastWord = lastWord.TrimStart();

            var completionList = CompletionData.GetCompletionData(lastWord, skEditor.GetTextEditor().Text);

            if (lastWord.Length <= 0 || completionList.Length <= 0)
            {
                return;
            }

            var completionWindow = CreateCompletionWindow(completionList);

            var caret = textEditor.TextArea.Caret.CalculateCaretRectangle();
            var pointOnScreen = textEditor.TextArea.TextView.PointToScreen(caret.Location - textEditor.TextArea.TextView.ScrollOffset);

            popup = new Popup
            {
                PlacementTarget = textEditor,
                Placement = PlacementMode.Absolute,
                HorizontalOffset = pointOnScreen.X + 5,
                VerticalOffset = pointOnScreen.Y + 5,
                AllowsTransparency = true,
                StaysOpen = false,
                Child = completionWindow,
                IsOpen = true
            };

        }
    }
}