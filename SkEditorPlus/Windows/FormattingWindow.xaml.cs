using AvalonEditB;
using HandyControl.Controls;
using SkEditorPlus.Managers;
using System.Linq;
using System.Text.RegularExpressions;

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
            if (variablesCheckBox.IsChecked == true)
            {
                FixDotVariables();
            }
            if (spacesCheckBox.IsChecked == true)
            {
                SpacesToTabs();
            }
            if (commentsCheckBox.IsChecked == true)
            {
                RemoveComments();
            }

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
                textEditor.Text = code;
            }
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
            textEditor.Text = code;
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
            foreach (string line in textEditor.Text.Split("\n"))
            {
                if (line.StartsWith("    "))
                {
                    var regex = new Regex("    ");
                    var lineWithTabs = regex.Replace(line, "\t", 1);

                    textEditor.Document.Replace(GetOffsetByLine(line), line.Length, lineWithTabs);
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
    }
}
