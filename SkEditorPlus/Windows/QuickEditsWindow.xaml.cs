using AvalonEditB;
using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Controllers;
using SkEditorPlus.Utilities.Managers;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

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
            _spaceValue = 4;
            textEditor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();
            BackgroundFixer.FixBackground(this);

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
            if (tabsCheckBox.IsChecked == true) TabsToSpaces();
            if (commentsCheckBox.IsChecked == true) RemoveComments();
            if (elseIfCheckBox.IsChecked == true) FixElseIf();

            quickEditsWindow.Close();
        }


        private void FixDotVariables()
        {
            SettingsManager.SaveSettings();

            string code = textEditor.Text;

            Regex regex = new("{([^.}]*)\\.([^}]*)}");

            foreach (Match variableMatch in regex.Matches(code).Cast<Match>())
            {
                string variable = variableMatch.Value.Replace(".", "::");
                code = code.Replace(variableMatch.Value, variable);
            }
            textEditor.Document.Text = code;
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnQuickEdit(ISkEditorPlusAddon.QuickEditType.CHANGE_DOTS_TO_COLONS);
            });
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
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnQuickEdit(ISkEditorPlusAddon.QuickEditType.REMOVE_COMMENTS);
            });
        }




        private void SpacesToTabs()
        {
            int spacingAmount = DetectSpacingAmount();
            ConvertSpacesToTabs(spacingAmount);
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnQuickEdit(ISkEditorPlusAddon.QuickEditType.CHANGE_SPACES_TO_TABS);
            });
        }

        private void TabsToSpaces()
        {
            ConvertTabsToSpaces(_spaceValue);
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnQuickEdit(ISkEditorPlusAddon.QuickEditType.CHANGE_TABS_TO_SPACES);
            });
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

        private void ConvertTabsToSpaces(int spacePerTab)
        {
            try
            {
                var lines = textEditor.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                var newLines = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    string lineToReplace = lines[i];

                    if (lineToReplace.Trim().Length > 0 && lineToReplace.Trim()[0] == '#')
                    {
                        newLines.Add(lineToReplace);
                        continue;
                    }

                    string newLine = lineToReplace.Replace("\t", new string(' ', spacePerTab));

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
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnQuickEdit(ISkEditorPlusAddon.QuickEditType.SHORTEN_ELSE_IF);
            });
        }

        private int _spaceValue = 4;

        public int SpaceNumber
        {
            get => _spaceValue;
            set
            {
                if (value < 1 || value > 9)
                    return;

                var textBlock = ((Grid)tabsCheckBox.Content).Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock != null)
                {
                    var number = GetAllIntsInString(textBlock.Text);
                    textBlock.Text = textBlock.Text.Replace(number, value.ToString());
                }

                _spaceValue = value;
            }
        }

        private static string GetAllIntsInString(string inputString)
        {
            return new string(inputString.Where(char.IsDigit).ToArray());
        }


        private void OnSpaceUp(object sender, RoutedEventArgs e)
        {
            SpaceNumber++;
        }

        private void OnSpaceDown(object sender, RoutedEventArgs e)
        {
            SpaceNumber--;
        }

        private bool JustChecked;
        private void RadioButtonChecked(object sender, RoutedEventArgs e)
        {
            JustChecked = true;
        }


        private void RadioButtonClicked(object sender, RoutedEventArgs e)
        {
            if (JustChecked)
            {
                JustChecked = false;
                e.Handled = true;
                return;
            }

            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
                radioButton.IsChecked = false;
        }
    }
}
