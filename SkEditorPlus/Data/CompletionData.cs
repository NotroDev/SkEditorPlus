using System.Collections.Generic;
using System.Linq;

namespace SkEditorPlus.Data
{
    public class CompletionData
    {
        public static HashSet<CompletionDataElement> completionSet = new()
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



        public static IEnumerable<CompletionDataElement> GetCompletionData(string word, string code)
        {
            var filteredList = completionSet
                .Where(item => item.Name.StartsWith(word))
                .AsEnumerable();

            foreach (var item in filteredList)
            {
                yield return item;
            }
        }
    }
}
