using AvalonEditB;
using AvalonEditB.CodeCompletion;
using SkEditorPlus.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Window = System.Windows.Window;

namespace SkEditorPlus.Managers
{
    public class CompletionManager
    {
        static CompletionWindow completionWindow;

        public static TextEditor textEditor;

        public static void LoadCompletionManager(TextEditor textEditor)
        {
            CompletionManager.textEditor = textEditor;
            textEditor.TextArea.TextEntering += TextEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += TextEditor_TextArea_TextEntered;
        }

        public static MainWindow GetMainWindow()
        {
            List<Window> windowList = new();
            foreach (Window window in System.Windows.Application.Current.Windows)
                windowList.Add(window);
            return (MainWindow)windowList.Find(window => window.GetType() == typeof(MainWindow));
        }

        static void TextEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            completionWindow = new CompletionWindow(textEditor.TextArea)
            {
                StartOffset = 0
            };

            completionWindow.CompletionList.ListBox.ItemContainerStyle = (Style)Application.Current.FindResource("CompletionListStyle");

            completionWindow.CompletionList.ListBox.Background = System.Windows.Media.Brushes.Transparent;
            completionWindow.Background = System.Windows.Media.Brushes.Transparent;
            completionWindow.CompletionList.ListBox.BorderBrush = System.Windows.Media.Brushes.Transparent;
            completionWindow.CompletionList.ListBox.BorderThickness = new Thickness(0);
            
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            var caretOffset = textEditor.CaretOffset;
            var line = textEditor.Document.GetLineByOffset(caretOffset);
            var wordBeforeCaret = textEditor.Document.GetText(line.Offset, caretOffset - line.Offset);
            if (wordBeforeCaret.ToLower().Equals("command"))
            {
                data.Add(new CompletionData("Command", "Otwiera generator komendy"));
            }

            else if (data.Count != 0)
            {
                data.Remove(data.First());
            }


            if (data.Count != 0)
                completionWindow.Show();
            
            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };

            completionWindow.CompletionList.ListBox.SelectedIndex = 0;
        }

        static void TextEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }
    }
}