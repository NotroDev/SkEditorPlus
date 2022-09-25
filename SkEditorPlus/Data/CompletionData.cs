using AvalonEditB.CodeCompletion;
using AvalonEditB.Document;
using AvalonEditB.Editing;
using System;
using SkEditorPlus.Windows;
using SkEditorPlus.Windows.Generators;
using System.Windows;
using System.Collections.Generic;

namespace SkEditorPlus.Data
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(string text, string description = "Brak opisu")
        {
            Text = text;
            Description = description;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        { get; private set; }

        double ICompletionData.Priority => 1;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            var caretOffset = textArea.Caret.Offset;
            var line = textArea.Document.GetLineByOffset(caretOffset);
            textArea.Document.Replace(line.Offset, caretOffset - line.Offset, "");

            CommandGenerator commandGenerator = new(GetMainWindow().skEditor);
            commandGenerator.ShowDialog();
        }
        public static MainWindow GetMainWindow()
        {
            List<Window> windowList = new();
            foreach (Window window in System.Windows.Application.Current.Windows)
                windowList.Add(window);
            return (MainWindow)windowList.Find(window => window.GetType() == typeof(MainWindow));
        }

    }
}