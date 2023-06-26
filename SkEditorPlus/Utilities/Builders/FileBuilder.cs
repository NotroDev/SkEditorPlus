using AvalonEditB;
using AvalonEditB.Search;
using HandyControl.Controls;
using SkEditorPlus.Utilities.Services;
using SkEditorPlus.Utilities.Vaults;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus.Utilities.Builders
{
    public class FileBuilder
    {
        public static string newFileName = (string)Application.Current.Resources["NewFileName"];
        public static Regex regex = new(newFileName.Replace("{0}", @"[0-9]+"));

        public static void Build(string header, string tooltip = null)
        {
            var tabItem = new TabItem()
            {
                Header = header,
                ToolTip = tooltip ?? "",
                IsSelected = true,
            };
            IconElement.SetGeometry(tabItem, Geometry.Parse(GeometryUtility.Other));
            IconElement.SetHeight(tabItem, 16);
            IconElement.SetWidth(tabItem, 16);
            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            var codeEditor = new TextEditor()
            {
                Style = Application.Current.FindResource("TextEditorStyle") as Style,
                FontFamily = new FontFamily(Properties.Settings.Default.Font),
                WordWrap = Properties.Settings.Default.Wrapping,
            };
            codeEditor.PreviewMouseWheel += TextEditorService.OnScrolling;
            codeEditor.TextChanged += TextEditorService.OnTextChanged;
            codeEditor.TextArea.TextEntering += TextEditorService.OnTextEntering;
            codeEditor.TextArea.Caret.PositionChanged += TextEditorService.OnCaretPositionChanged;
            codeEditor.TextArea.SelectionChanged += TextEditorService.OnSelectionChanged;

            codeEditor.TextArea.TextView.LinkTextForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a94c4");
            codeEditor.TextArea.TextView.LinkTextUnderline = true;
            codeEditor.Options.AllowScrollBelowDocument = true;

            codeEditor.TextArea.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, e) =>
            {
                if (codeEditor.SelectionLength == 0)
                {
                    var line = codeEditor.Document.GetLineByOffset(codeEditor.CaretOffset);
                    codeEditor.Select(line.Offset, line.Length);
                }
                codeEditor.Copy();
            }, (sender, e) =>
            {
                e.CanExecute = true;
            }));

            RoutedCommand commentCommand = new();
            commentCommand.InputGestures.Add(new KeyGesture(Key.OemQuestion, ModifierKeys.Control));
            codeEditor.CommandBindings.Add(new CommandBinding(commentCommand, (sender, e) =>
            {
                var lines = codeEditor.Document.Lines
                    .Where(l => codeEditor.SelectionStart <= l.EndOffset && codeEditor.SelectionStart + codeEditor.SelectionLength >= l.Offset)
                    .ToList();

                var modifiedLines = new List<string>();

                foreach (var line in lines)
                {
                    var text = codeEditor.Document.GetText(line);
                    if (!text.StartsWith("#"))
                    {
                        modifiedLines.Add("#" + text);
                    }
                    else
                    {
                        modifiedLines.Add(text[1..]);
                    }
                }

                var replacement = string.Join("\n", modifiedLines);
                var startOffset = lines.First().Offset;
                var endOffset = lines.Last().EndOffset - startOffset;

                codeEditor.Document.Replace(startOffset, endOffset, replacement);
                codeEditor.Select(startOffset, replacement.Length);
            }, (sender, e) =>
            {
                e.CanExecute = true;
            }));

            tabItem.Content = codeEditor;

            var searchPanel = SearchPanel.Install(codeEditor.TextArea);
            searchPanel.ShowReplace = true;
            searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");
            searchPanel.Localization = new Data.Localization();

            APIVault.GetAPIInstance().GetTabControl().Items.Add(tabItem);
            GeometryUtility.ChangeGeometry();

            AddonVault.addons.ForEach(addon => addon.OnTabCreate());
        }

        public static int UntitledCount()
        {
            int tabIndex = 1;
            foreach (TabItem ti in APIVault.GetAPIInstance().GetTabControl().Items)
            {
                if (regex.IsMatch(ti.Header.ToString()))
                {
                    tabIndex++;
                }
            }
            return tabIndex;
        }
    }
}
