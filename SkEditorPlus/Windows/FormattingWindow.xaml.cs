using AvalonEditB;
using HandyControl.Controls;
using SkEditorPlus.Managers;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Windows.Shapes;
using AvalonEditB.Document;
using System.Text;
using System.Windows.Controls;
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

        //public void AddQuickEdit(string name, string text)
        //{
        //    int checkBoxCount = LogicalTreeHelper.GetChildren(this).OfType<CheckBox>().Count();

        //    HandyControl.Controls.MessageBox.Show(checkBoxCount.ToString());

        //    CheckBox checkBox = new()
        //    {
        //        Name = name,
        //        Content = text,
        //        FontSize = 12,
        //        Margin = new(10, 102, 0, 0),
        //        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
        //        VerticalAlignment = System.Windows.VerticalAlignment.Top,
        //    };

        //    this.AddChild(checkBox);
        //}


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

            
            return;

            string valuePattern = @"%.+?%";
            string optionalPattern = @"\[.*?\]";
            string groupPattern = @"\(([^)]+)\)";
            string optionalGroupPattern = @"\[\(([^)]+)\)\]";

            Regex patternCompiler = new(
            "(?<VALUE>" + valuePattern + ")"
            + "|(?<OPTIONAL>" + optionalPattern + ")"
            + "|(?<GROUP>" + groupPattern + ")"
            + "|(?<OPTIONALGROUP>" + optionalGroupPattern + ")",
            RegexOptions.Compiled
            );

            string input = textEditor.Text;

            MatchCollection matches = patternCompiler.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups["OPTIONAL"].Success)
                {
                    string value = match.Groups["OPTIONAL"].Value;
                    value = value.Substring(1, value.Length - 2);
                    input = input.Replace(match.Value, value);
                }
                else if (match.Groups["GROUP"].Success)
                {
                    string value = match.Groups["GROUP"].Value;
                    value = value.Substring(1, value.Length - 2).Split('|')[0];
                    input = input.Replace(match.Value, value);
                }
                else if (match.Groups["OPTIONALGROUP"].Success)
                {
                    string value = match.Groups["OPTIONALGROUP"].Value;
                    value = value.Substring(2, value.Length - 4).Split('|')[0];
                    input = input.Replace(match.Value, value);
                }
            }

            textEditor.Document.Text = input;
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
