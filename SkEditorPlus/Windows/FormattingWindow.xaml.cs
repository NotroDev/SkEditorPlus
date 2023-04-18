using AvalonEditB;
using SkEditorPlus.Managers;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text;
using SkEditorPlus.Data;
using System.Windows.Input;
using Renci.SshNet;
using Renci.SshNet.Async;
using System.Threading.Tasks;
using Octokit;

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
            SettingsManager.SaveSettings();

            // Don't ask lol


            //using var client = new SftpClient(credentials);

            //client.Connect();

            //TreeViewItem treeViewItem = new()
            //{
            //    Header = FileManager.CreateIcon("\ue8b7", "FTP"),
            //    Tag = ".",
            //    IsExpanded = true
            //};

            //skEditor.GetMainWindow().fileTreeView.Items.Add(treeViewItem);

            //ProjectManager.instance.isFtp = true;
            //ProjectManager.client = client;
            //await ProjectManager.instance.AddDirectoriesAndFilesFTPAsync(client, treeViewItem);

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
            // In future there will be a setting for spaces per tab
            ConvertSpacesToTabs(4);
        }

        private void ConvertSpacesToTabs(int spacePerTab)
        {
            try
            {
                var lines = textEditor.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                var newLines = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    string lineToReplace = lines[i];

                    int spaces = lineToReplace.TakeWhile(c => c == ' ').Count();

                    if (spaces == 0)
                    {
                        newLines.Add(lineToReplace);
                        continue;
                    }

                    int tabs = spaces / spacePerTab;
                    int spacesToReplace = tabs * spacePerTab;

                    string newLine = lineToReplace.Replace(new string(' ', spacesToReplace), new string('\t', tabs));

                    newLines.Add(newLine);
                }

                string newCode = string.Join(Environment.NewLine, newLines);
                textEditor.Document.Text = newCode;
            }
            catch { }
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
