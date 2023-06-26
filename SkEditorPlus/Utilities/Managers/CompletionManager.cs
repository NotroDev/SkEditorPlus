﻿using AvalonEditB;
using SkEditorPlus.Data;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Windows;
using SkEditorPlus.Windows.Generators;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SkEditorPlus.Utilities.Managers
{
    public class CompletionManager
    {
        public static TextEditor textEditor;

        private readonly SkEditorAPI skEditor = APIVault.GetAPIInstance();

        private CancellationTokenSource cancellationTokenSource;

        private CompletionWindow completionWindow;

        private static CompletionBindings completionBindings;

        private Popup popup;

        public static CompletionManager instance;

        public CompletionManager()
        {
            instance = this;

            for (int i = 0; i < 50; i++)
            {
                CompletionData.completionSet.Add(new CompletionDataElement("command" + i, "command /{c}:\n\t"));
            }
        }

        public void LoadCompletionManager(TextEditor textEditor)
        {
            CompletionManager.textEditor = textEditor;
            textEditor.TextArea.TextEntered += OnTextEntered;

            textEditor.PreviewKeyDown += OnKeyDown;
        }


        public async void OnKeyDown(object sender, KeyEventArgs e)
        {
            var caretOffset = textEditor.CaretOffset;
            var line = textEditor.Document.GetLineByOffset(caretOffset);
            var text = textEditor.Document.GetText(line.Offset, caretOffset - line.Offset);
            if (textEditor.SelectedText.Equals(text)) text = "";
            else if (text.Length > 0) text = text.Remove(text.Length - 1);

            if (e.Key == Key.Back || e.Key == Key.Left)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new();
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.25), cancellationTokenSource.Token);
                    ShowCompletionWindow(text);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
            else if (e.Key == Key.Right)
            {
                if (caretOffset + 1 <= textEditor.Document.TextLength)
                {
                    cancellationTokenSource?.Cancel();

                    if (text.Length > 0)
                    {
                        cancellationTokenSource = new();

                        try
                        {
                            var caretText = textEditor.Document.GetText(line.Offset, caretOffset - line.Offset + 1);
                            await Task.Delay(TimeSpan.FromSeconds(0.25), cancellationTokenSource.Token);
                            ShowCompletionWindow(caretText);
                        }
                        catch { }
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

                string item = completionWindow.completionList.SelectedItem as string;

                var split = text.Split(' ');
                var lastWord = split[^1].TrimStart();
                var index = text.LastIndexOf(lastWord);
                var newText = text.Remove(index, lastWord.Length);

                switch (item)
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
                        foreach (CompletionDataElement dataElement in CompletionData.completionSet)
                        {
                            if (dataElement.Name.Equals(item))
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

                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnCompletionAccept(newText);
                });
            }

            else if (e.Key == Key.Escape)
            {
                HidePopup();
            }
        }

        private void HandleArrowKey(Key key, ListBox listBox)
        {
            int selectedIndex = listBox.SelectedIndex;
            if (key == Key.Down && selectedIndex < listBox.Items.Count - 1)
            {
                listBox.SelectedIndex++;
            }
            else if (key == Key.Up && selectedIndex > 0)
            {
                listBox.SelectedIndex--;
            }

            listBox.ScrollIntoView(listBox.SelectedItem);
        }

        private static int GetTabCount(string line)
        {
            return line.TakeWhile(c => c == '\t').Count();
        }


        private async void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            cancellationTokenSource?.Cancel();

            cancellationTokenSource = new();

            try
            {
                if (popup != null)
                {
                    var caret = textEditor.TextArea.Caret.CalculateCaretRectangle();
                    var pointOnScreen = textEditor.TextArea.TextView.PointToScreen(caret.Location - textEditor.TextArea.TextView.ScrollOffset);

                    popup.HorizontalOffset = pointOnScreen.X + 10;
                    popup.VerticalOffset = pointOnScreen.Y + 10;
                }
                await Task.Delay(TimeSpan.FromSeconds(0.25), cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            string text = GetText();
            string lastWord = GetLastWord(text);

            if (IsInQuote(text, lastWord))
            {
                return;
            }

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
            if (text.Contains(' '))
            {
                var split = text.Split(' ');
                return split[^1];
            }

            return text;
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

        private void ShowCompletionWindow(string lastWord)
        {
            lastWord = lastWord.TrimStart();

            var completionList = CompletionData.GetCompletionData(lastWord, skEditor.GetTextEditor().Text);

            if (lastWord.Length <= 0 || !completionList.Any())
            {
                HidePopup();
                return;
            }

            if (popup != null && popup.IsOpen)
            {
                completionBindings.CompletionDataElements = completionList;
                completionWindow.completionList.SelectedIndex = 0;
            }
            else
            {
                completionWindow = new CompletionWindow();

                completionBindings ??= new CompletionBindings();
                completionBindings.CompletionDataElements = completionList;
                completionWindow.DataContext = completionBindings;

                popup = new Popup
                {
                    PlacementTarget = textEditor,
                    Placement = PlacementMode.Absolute,
                    HorizontalOffset = -5,
                    VerticalOffset = -5,
                    AllowsTransparency = true,
                    StaysOpen = false,
                    Child = completionWindow,
                    IsOpen = true
                };

                var caret = textEditor.TextArea.Caret.CalculateCaretRectangle();
                var pointOnScreen = textEditor.TextArea.TextView.PointToScreen(caret.Location - textEditor.TextArea.TextView.ScrollOffset);

                popup.HorizontalOffset = pointOnScreen.X + 10;
                popup.VerticalOffset = pointOnScreen.Y + 10;

                completionWindow.completionList.SelectedIndex = 0;
            }

        }

    }
}