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
            new CompletionDataElement("command2", "command2 /{c}:\n\t"),
            new CompletionDataElement("command3", "command3 /{c}:\n\t"),
            new CompletionDataElement("command4", "command4 /{c}:\n\t"),
            new CompletionDataElement("command5", "command5 /{c}:\n\t"),
            new CompletionDataElement("command6", "command6 /{c}:\n\t"),
            new CompletionDataElement("command7", "command7 /{c}:\n\t"),
            new CompletionDataElement("command8", "command8 /{c}:\n\t"),
            new CompletionDataElement("command9", "command9 /{c}:\n\t"),
            new CompletionDataElement("command10", "command10 /{c}:\n\t"),
            new CompletionDataElement("command11", "command11 /{c}:\n\t"),
            new CompletionDataElement("command12", "command12 /{c}:\n\t"),
            new CompletionDataElement("command13", "command13 /{c}:\n\t"),
            new CompletionDataElement("command14", "command14 /{c}:\n\t"),
            new CompletionDataElement("command15", "command15 /{c}:\n\t"),
            new CompletionDataElement("command16", "command16 /{c}:\n\t"),
            new CompletionDataElement("command17", "command17 /{c}:\n\t"),
            new CompletionDataElement("command18", "command18 /{c}:\n\t"),
            new CompletionDataElement("command19", "command19 /{c}:\n\t"),
            new CompletionDataElement("command20", "command20 /{c}:\n\t"),
            new CompletionDataElement("command21", "command21 /{c}:\n\t"),
            new CompletionDataElement("command22", "command22 /{c}:\n\t"),
            new CompletionDataElement("command23", "command23 /{c}:\n\t"),
            new CompletionDataElement("command24", "command24 /{c}:\n\t"),
            new CompletionDataElement("command25", "command25 /{c}:\n\t"),
            new CompletionDataElement("command26", "command26 /{c}:\n\t"),
            new CompletionDataElement("command27", "command27 /{c}:\n\t"),
            new CompletionDataElement("command28", "command28 /{c}:\n\t"),
            new CompletionDataElement("command29", "command29 /{c}:\n\t"),
            new CompletionDataElement("command30", "command30 /{c}:\n\t"),
            new CompletionDataElement("command31", "command31 /{c}:\n\t"),
            new CompletionDataElement("command32", "command32 /{c}:\n\t"),
            new CompletionDataElement("command33", "command33 /{c}:\n\t"),
            new CompletionDataElement("command34", "command34 /{c}:\n\t"),
            new CompletionDataElement("command35", "command35 /{c}:\n\t"),
            new CompletionDataElement("command36", "command36 /{c}:\n\t"),
            new CompletionDataElement("command37", "command37 /{c}:\n\t"),
            new CompletionDataElement("command38", "command38 /{c}:\n\t"),
            new CompletionDataElement("command39", "command39 /{c}:\n\t"),
            new CompletionDataElement("command40", "command40 /{c}:\n\t"),
            new CompletionDataElement("command41", "command41 /{c}:\n\t"),
            new CompletionDataElement("command42", "command42 /{c}:\n\t"),
            new CompletionDataElement("command43", "command43 /{c}:\n\t"),
            new CompletionDataElement("command44", "command44 /{c}:\n\t"),
            new CompletionDataElement("command45", "command45 /{c}:\n\t"),
            new CompletionDataElement("command46", "command46 /{c}:\n\t"),
            new CompletionDataElement("command47", "command47 /{c}:\n\t"),
            new CompletionDataElement("command48", "command48 /{c}:\n\t"),
            new CompletionDataElement("command49", "command49 /{c}:\n\t"),
            new CompletionDataElement("command50", "command50 /{c}:\n\t"),
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
            var filteredList = completionList
                .Where(item => item.Name.StartsWith(word))
                .ToList();

            foreach (var item in filteredList)
            {
                yield return item;
            }
        }
    }
}
