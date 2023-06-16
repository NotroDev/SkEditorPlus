using AvalonEditB;
using SkEditorPlus.Data;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SkEditorPlus.Utilities.Builders
{
    public class StructureBuilder
    {
        public static void CreateStructure()
        {
            SkEditorAPI skEditor = APIVault.GetAPIInstance();
            if (!skEditor.IsFileOpen()) return;

            skEditor.GetMainWindow().structureTreeView.Items.Clear();

            System.Windows.Controls.TreeViewItem item = new()
            {
                Header = IconBuilder.Build("\ue8a5", Application.Current.FindResource("TypeScript") as string),
                IsExpanded = true,
            };

            List<CodeNode> parentNodes = new() { new CodeNode(item) };


            TextEditor textEditor = skEditor.GetTextEditor();

            string[] lines = textEditor.Text.Split('\n');


            Dictionary<Regex, string> regexes = new()
            {
                { new Regex(@"^command \/.*:$"), "command" },
                { new Regex(@"^on .*:$"), "event" },
                { new Regex(@"^function .*:$"), "function" },
                { new Regex(@"^loop .*:$"), "loop" },
                { new Regex(@"^if .*:$"), "if" },
                { new Regex(@"options:"), "options" },
                { new Regex(@"variables:"), "vars" },
                { new Regex(@"stop"), "stop" },
                { new Regex(@"cancel event"), "cancel event" },
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNumber = i + 1;

                int tabsBefore = line.TakeWhile(c => c == '\t' || c == ' ').Count();
                if (line.Trim().Length == 0)
                {
                    continue;
                }

                CodeNode parentNode = parentNodes.LastOrDefault(n => n.tabsBefore < tabsBefore, parentNodes.First());

                string header = line.Trim();

                if (parentNode != null)
                {
                    System.Windows.Controls.TreeViewItem nodeItem = new()
                    {
                        Header = header
                    };

                    foreach (KeyValuePair<Regex, string> regex in regexes)
                    {
                        if (header.StartsWith('#'))
                        {
                            string translatedString = Application.Current.FindResource("StructureHeaderComment") as string;
                            nodeItem.Header = translatedString;
                            break;
                        }
                        if (regex.Key.IsMatch(header))
                        {
                            var translations = new Dictionary<string, string>
                            {
                                { "command", "StructureHeaderCommand" },
                                { "event", "StructureHeaderEvent" },
                                { "function", "StructureHeaderFunction" },
                                { "options", "StructureHeaderOptions" },
                                { "vars", "StructureHeaderVars" },
                                { "stop", "StructureHeaderStop" },
                                { "cancel event", "StructureHeaderCancelEvent" },
                                { "loop", "StructureHeaderLoop" },
                                { "if", "StructureHeaderIf" }
                            };

                            if (translations.TryGetValue(regex.Value, out string resourceKey))
                            {
                                string translatedString = Application.Current.FindResource(resourceKey) as string;
                                string headerText = regex.Value switch
                                {
                                    "command" => $"/{new Regex(@"(?<=\/)[^\/\s:]+(?=[\s:])").Match(header).Value}",
                                    "event" => $"\"{header.TrimEnd(':')}\"",
                                    "function" => new Regex(@"(?<=function )[^\/\s:]+(?=[\s(])").Match(header).Value,
                                    _ => ""
                                };

                                nodeItem.Header = $"{translatedString} {headerText}".Trim();
                            }
                        }
                    }

                    nodeItem.MouseDoubleClick += (s, e) =>
                    {
                        e.Handled = true;

                        System.Windows.Controls.TreeViewItem tviSender = s as System.Windows.Controls.TreeViewItem;

                        if (!tviSender.IsSelected) return;

                        textEditor.ScrollToLine(lineNumber);

                        int lineOffset = textEditor.Document.GetLineByNumber(lineNumber).Offset;

                        int lineEndOffset = textEditor.Document.GetLineByNumber(lineNumber).EndOffset;

                        textEditor.Select(lineOffset, lineEndOffset - lineOffset);

                        textEditor.TextArea.Caret.Offset = textEditor.Document.GetLineByOffset(lineOffset).Offset;
                    };

                    CodeNode node = new(line, line.TrimStart(), nodeItem, parentNode, tabsBefore, i);

                    parentNode.item.Items.Add(nodeItem);
                    parentNodes.Add(node);
                }
            }

            if (item.Items.Count > 0)
            {
                skEditor.GetMainWindow().structureTreeView.Items.Add(item);
            }
        }
    }
}
