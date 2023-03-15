using AvalonEditB;
using SkEditorPlus.Managers;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Windows;

namespace SkEditorPlus.Windows
{
    public partial class FormattingWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;

        private TextEditor textEditor;

        public static FormattingWindow instance;

        public FormattingWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            instance = this;
            this.skEditor = skEditor;
            textEditor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();
            BackgroundFixManager.FixBackground(this);

        }


        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                formattingWindow.Close();
            }
        }

        private void FormatClick(object sender, RoutedEventArgs e)
        {
            if (variablesCheckBox.IsChecked == true) FixDotVariables();
            if (spacesCheckBox.IsChecked == true) SpacesToTabs();
            if (commentsCheckBox.IsChecked == true) RemoveComments();
            if (elseIfCheckBox.IsChecked == true) FixElseIf();

            formattingWindow.Close();
        }


        private void FixDotVariables()
        {

            string code = textEditor.Text;

            Regex regex = new("{([^.}]*)\\.([^}]*)}");

            foreach (Match variableMatch in regex.Matches(code).Cast<Match>())
            {
                string variable = variableMatch.Value.Replace(".", "::");
                code = code.Replace(variableMatch.Value, variable);
            }
            textEditor.Document.Text = code;
        }

        private void RemoveComments()
        {
            var linesToRemove = textEditor.Text.Split("\n")
                .Where(line => line.StartsWith("#"))
                .Select(line => GetOffsetByLine(line) - 1 > 0
                        ? new { Offset = GetOffsetByLine(line) - 1, Length = line.Length + 1 }
                        : new { Offset = GetOffsetByLine(line), Length = line.Length + 1 });

            foreach (var lineToRemove in linesToRemove)
            {
                textEditor.Document.Remove(lineToRemove.Offset, lineToRemove.Length);
            }
        }




        private void SpacesToTabs()
        {
            var lines = textEditor.Text.Split("\n");
            var firstIndentedLine = lines.FirstOrDefault(line => line.StartsWith(" "));
            if (firstIndentedLine == null) return;

            int howMuchSpacesInTab = firstIndentedLine.TakeWhile(c => c == ' ').Count();

            var linesToReplace = lines.Where(line => line.StartsWith(" "))
                .Select(line => new { Offset = GetOffsetByLine(line), Spaces = line.TakeWhile(c => c == ' ').Count() })
                .Where(x => x.Spaces > 0);

            foreach (var lineToReplace in linesToReplace)
            {
                int tabs = lineToReplace.Spaces / howMuchSpacesInTab;
                int remainder = lineToReplace.Spaces % howMuchSpacesInTab;

                textEditor.Document.Remove(lineToReplace.Offset, lineToReplace.Spaces);
                textEditor.Document.Insert(lineToReplace.Offset, new string('\t', tabs) + new string(' ', remainder));
            }
        }

        private int GetOffsetByLine(string line)
        {
            int index = textEditor.Text.IndexOf(line);
            return index == -1 ? -1 : index + textEditor.Document.GetLineByOffset(index).LineNumber;
        }

        private void FixElseIf()
        {
            string code = textEditor.Text;
            string[] lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var modifiedLines = lines
                .Select((line, i) =>
                {
                    if (line.Trim().Equals("else:") && i + 1 < lines.Length)
                    {
                        string nextLine = lines[i + 1].Trim();
                        if (nextLine.StartsWith("if "))
                        {
                            string beforeElse = line[..line.IndexOf("else:")];
                            string trimmedNextLine = nextLine[nextLine.IndexOf("if ")..];
                            return beforeElse + "else " + trimmedNextLine;
                        }
                    }
                    return line;
                })
                .ToList();

            code = string.Join(Environment.NewLine, modifiedLines);
            textEditor.Document.Text = code;
        }
    }
}
