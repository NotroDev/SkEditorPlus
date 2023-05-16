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
using HandyControl.Controls;

namespace SkEditorPlus.Windows
{
    public partial class QuickEditsWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;

        private TextEditor textEditor;

        public static QuickEditsWindow instance;

        public QuickEditsWindow(SkEditorAPI skEditor)
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
                quickEditsWindow.Close();
            }
        }

        private void FormatClick(object sender, RoutedEventArgs e)
        {
            if (variablesCheckBox.IsChecked == true) FixDotVariables();
            if (spacesCheckBox.IsChecked == true) SpacesToTabs();
            if (commentsCheckBox.IsChecked == true) RemoveComments();
            if (elseIfCheckBox.IsChecked == true) FixElseIf();

            quickEditsWindow.Close();
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
            int spacingAmount = DetectSpacingAmount();
            ConvertSpacesToTabs(spacingAmount);
        }

        private int DetectSpacingAmount()
        {
            var lines = textEditor.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var newLines = new List<string>();

            for (int i = 0; i < lines.Length; i++) 
            {
                string line = lines[i];
                int spaces = line.TakeWhile(c => c == ' ').Count();
                if (spaces == 0 && line.Length > 0 && line[0] != '#' && !char.IsWhiteSpace(line[0]) && lines.Length > i + 1)
                {
                    int spacing = lines[i + 1].TakeWhile(c => c == ' ').Count();
                    return spacing;
                }
            }
            return 4;
        }

        private void ConvertSpacesToTabs(int spacePerTab)
        {
            try
            {
                var lines = textEditor.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                var newLines = new List<string>();

                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("#"))
                    {
                        newLines.Add(line);
                        continue;
                    }

                    var spacesOnlyLine = line.Replace("\t", new string(' ', spacePerTab));

                    var leadingSpacesCount = spacesOnlyLine.TakeWhile(c => c == ' ').Count();

                    var tabsCount = leadingSpacesCount / spacePerTab;
                    var spacesCount = leadingSpacesCount % spacePerTab;

                    var leadingTabs = new string('\t', tabsCount);
                    var leadingSpaces = new string(' ', spacesCount);
                    var newLine = string.Concat(leadingTabs, leadingSpaces, spacesOnlyLine.AsSpan(leadingSpacesCount));

                    newLines.Add(newLine);
                }

                var newCode = string.Join(Environment.NewLine, newLines);
                textEditor.Document.Text = newCode;
            }
            catch
            {
                // ignore because why not lmao
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
