using AvalonEditB.Document;
using AvalonEditB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HandyControl.Controls;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Utilities.Controllers;

namespace SkEditorPlus.Utilities.Services
{
    public class TextEditorService
    {
        public static void OnTextChanged(object sender, EventArgs e)
        {
            TabItem tabItem = (TabItem)APIVault.GetAPIInstance().GetTabControl().SelectedItem;
            string path = tabItem.ToolTip.ToString();

            if (Properties.Settings.Default.AutoSave && !string.IsNullOrEmpty(path))
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                    APIVault.GetAPIInstance().GetTextEditor().Save(stream);
                    TabController.RemoveAsterisk(tabItem);
                }
                catch { }
            }
            else
            {
                TabController.AddAsterisk(tabItem);
            }
        }

        public static void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            TextEditor textEditor = APIVault.GetAPIInstance().GetTextEditor();
            char ch = e.Text[0];
            if (Properties.Settings.Default.AutoSecondCharacter)
            {

                string textToReplace = "";
                switch (ch)
                {
                    case '"':
                        textToReplace = "\"\"";
                        break;
                    case '{':
                        textToReplace = "{}";
                        break;
                    case '(':
                        textToReplace = "()";
                        break;
                    case '[':
                        textToReplace = "[]";
                        break;
                    case '%':
                        textToReplace = "%%";
                        break;
                }
                if (!string.IsNullOrEmpty(textToReplace))
                {
                    TextEditor codeEditor = textEditor;
                    int caretOffset = codeEditor.CaretOffset;
                    int lineStartOffset = codeEditor.Document.GetLineByOffset(caretOffset).Offset;
                    string textBeforeCaret = codeEditor.Document.GetText(lineStartOffset, caretOffset - lineStartOffset);
                    int quotesCount = textBeforeCaret.Count(c => c == '"');
                    if (quotesCount % 2 == 1)
                    {
                        return;
                    }
                    textEditor.Document.Insert(textEditor.CaretOffset, textToReplace);
                    e.Handled = true;
                    textEditor.CaretOffset--;
                }
            }

            if (!Properties.Settings.Default.AutoNewLineAndTab) return;

            switch (ch)
            {
                case ':':
                    DocumentLine currentLine = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
                    string currentLineText = textEditor.Document.GetText(currentLine.Offset, currentLine.Length);

                    int tabsCount = 0;
                    for (int i = 0; i < currentLineText.Length; i++)
                    {
                        if (currentLineText[i] == '\t')
                        {
                            tabsCount++;
                        }
                    }

                    if (StartsWithAny(currentLineText, new string[] { "command", "trigger", "if" }))
                    {
                        e.Handled = true;
                        textEditor.Document.Insert(textEditor.CaretOffset, ":\n");
                        for (int i = 0; i <= tabsCount; i++)
                        {
                            textEditor.Document.Insert(textEditor.CaretOffset, "\t");
                        }
                    }
                    break;
            }
        }

        public static void OnScrolling(object sender, MouseWheelEventArgs e)
        {
            try
            {
                var textEditor = APIVault.GetAPIInstance().GetTextEditor();
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    double fontSize = textEditor.FontSize += e.Delta / 25.0;

                    textEditor.FontSize = fontSize < 6 ? 6 : fontSize > 200 ? 200 : fontSize;

                    e.Handled = true;
                }
            }
            catch { }
        }

        private static bool StartsWithAny(string text, params string[] startsWith)
        {
            return startsWith.Any(s => text.TrimStart().StartsWith(s));
        }
    }
}
