using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkEditorPlus.Data
{
    public class CodeNode
    {
        public bool isRoot = false;

        public string line;

        public string header;

        public TreeViewItem item;

        public CodeNode parent;

        public int tabsBefore = 0;

        public int lineNumber = 0;

        public CodeNode(string line, string header, TreeViewItem item, CodeNode parent, int tabsBefore, int lineNumber)
        {
            this.line = line;
            this.header = header;
            this.item = item;
            this.parent = parent;
            this.tabsBefore = tabsBefore;
            this.lineNumber = lineNumber;
        }

        public CodeNode(TreeViewItem item)
        {
            isRoot = true;
            this.item = item;
        }

        public void SetParent(CodeNode parent)
        {
            this.parent = parent;
        }
    }
}
