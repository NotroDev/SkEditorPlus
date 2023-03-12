using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkEditorPlus.Data
{
    public class CompletionData
    {

        public static List<CompletionDataElement> completionList = new()
        {
            new CompletionDataElement("command", "command /{c}:\n\t"),
            new CompletionDataElement("commandgen"),
            new CompletionDataElement("guigen"),
            new CompletionDataElement("options", "options:\n\t"),
            new CompletionDataElement("variables", "variables:\n\t"),
            new CompletionDataElement("trigger", "trigger:\n\t"),
            new CompletionDataElement("if", "if {c}:"),
            new CompletionDataElement("ifelse", "if {c}:\n\t\nelse:\n\t"),
            new CompletionDataElement("else"),
            new CompletionDataElement("send", "send \"{c}\""),
            new CompletionDataElement("sendallp", "send \"{c}\" to all players"),
            new CompletionDataElement("loop", "loop {c}:\n\t"),
            new CompletionDataElement("function", "function {c}:\n\t"),
        };

        public static ListBoxItem[] GetCompletionData(string word, string code)
        {
            return completionList
            .Where(item => item.Name.StartsWith(word))
            .Select(item => new ListBoxItem { Content = item.Name })
            .ToArray();
        }
    }
}
