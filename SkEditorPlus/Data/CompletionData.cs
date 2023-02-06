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
            new CompletionDataElement("options", "options:\n\t"),
            new CompletionDataElement("variables", "variables:\n\t"),
            new CompletionDataElement("trigger", "trigger:\n\t"),
            new CompletionDataElement("if", "if {c}:"),
            new CompletionDataElement("ifelse", "if {c}:\n\t\nelse:\n\t"),
            new CompletionDataElement("else"),
            new CompletionDataElement("send", "send \"{c}\""),
            new CompletionDataElement("sendallp", "send \"{c}\" to all players"),
        };

        public static ListBoxItem[] GetCompletionData(string word)
        {
            List<ListBoxItem> completions = new();
            foreach (var item in completionList)
            {
                if (item.Name.StartsWith(word))
                {
                    //if (item.Name.Equals(word)) continue;
                    completions.Add(new ListBoxItem { Content = item.Name });
                }
            }
            return completions.ToArray();
        }

    }
}
