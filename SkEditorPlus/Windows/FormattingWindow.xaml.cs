using AvalonEditB;
using HandyControl.Controls;
using SkEditorPlus.Managers;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Windows.Shapes;
using AvalonEditB.Document;

namespace SkEditorPlus.Windows
{
    public partial class FormattingWindow : Window
    {
        private SkEditorAPI skEditor;

        private TextEditor textEditor;

        public FormattingWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
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

        private void FormatClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (variablesCheckBox.IsChecked == true) FixDotVariables();
            if (spacesCheckBox.IsChecked == true) SpacesToTabs();
            if (commentsCheckBox.IsChecked == true) RemoveComments();
            if (elseIfCheckBox.IsChecked == true) FixElseIf();

            Test();

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

        private void Test()
        {
            return;

            string code = textEditor.Text;

            string[] parts = Regex.Split(code, @"(?<=[:])");

            code = "";
            foreach (string element in parts)
            {

                code += element.Trim() + "\n\t";
            }
            textEditor.Document.Text = code;
        }

        private void RemoveComments()
        {
            foreach (string line in textEditor.Text.Split("\n"))
            {
                if (line.StartsWith("#"))
                {
                    int offset = GetOffsetByLine(line);

                    if (offset - 1 > 0)
                    {
                        textEditor.Document.Remove(offset - 1, line.Length + 1);
                    }
                    else
                    {
                        textEditor.Document.Remove(offset, line.Length + 1);
                    }
                }
            }
        }

        private void SpacesToTabs()
        {
            int howMuchSpacesInTab = 0;
            foreach (string line in textEditor.Text.Split("\n"))
            {
                if (line.StartsWith(" "))
                {
                    howMuchSpacesInTab = line.TakeWhile(c => c == ' ').Count();
                    break;
                }
            }

            if (howMuchSpacesInTab == 0) return;


            foreach (string line in textEditor.Text.Split("\n"))
            {
                int offset = GetOffsetByLine(line);
                int spaces = line.TakeWhile(c => c == ' ').Count();
                int tabs = spaces / howMuchSpacesInTab;
                int remainder = spaces % howMuchSpacesInTab;

                if (tabs > 0)
                {
                    textEditor.Document.Remove(offset, spaces);
                    textEditor.Document.Insert(offset, new string('\t', tabs));
                }
            }
        }

        private int GetOffsetByLine(string line)
        {
            int offset = 0;
            foreach (string element in textEditor.Text.Split("\n"))
            {
                if (element == line)
                {
                    return offset;
                }
                offset += element.Length + 1;
            }
            return -1;
        }

        private void FixElseIf()
        {
            string code = textEditor.Text;
            string[] lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmedLine = line.Trim();
                if (trimmedLine.Equals("else:"))
                {
                    if (i + 1 < lines.Length)
                    {
                        string nextLine = lines[i + 1];
                        string trimmedNextLine = nextLine.Trim();

                        //int y = 0;
                        //while (string.IsNullOrWhiteSpace(trimmedNextLine) || trimmedNextLine.StartsWith("#"))
                        //{
                        //    y++;
                        //    if (i + y + 1 < lines.Length)
                        //    {
                        //        nextLine = lines[i + y + 1];
                        //        trimmedNextLine = nextLine.Trim();
                        //    }
                        //    else
                        //    {
                        //        break;
                        //    }
                        //}

                        if (trimmedNextLine.StartsWith("if "))
                        {
                            string beforeElse = line[..line.IndexOf("else:")];

                            trimmedNextLine = trimmedNextLine[trimmedNextLine.IndexOf("if ")..];

                            string newLine = beforeElse + "else " + trimmedNextLine;

                            code = code.Replace(line, newLine.TrimEnd());

                            DocumentLine line1 = textEditor.Document.GetLineByOffset(textEditor.Document.Text.IndexOf(nextLine));
                            code = code.Remove(line1.Offset - 2, line1.Length + 2);

                            textEditor.Document.Text = code;
                        }
                    }
                }
            }
        }
    }
}
