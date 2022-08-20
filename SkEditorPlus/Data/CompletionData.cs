using AvalonEditB.CodeCompletion;
using AvalonEditB.Document;
using AvalonEditB.Editing;
using System;

namespace SkEditorPlus.Data
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(string text, string description = "Brak opisu")
        {
            this.Text = text;
            this.Description = description;
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
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}