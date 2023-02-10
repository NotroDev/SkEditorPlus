using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkEditorPlus.Data
{
    public class CompletionDataElement
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Word { get; set; }

        public CompletionDataElement(string name, string word = "none", string description = "none")
        {
            Name = name;

            if (!description.Equals("none"))
            {
                Description = description;
            }

            if (!word.Equals("none"))
            {
                Word = word;
                return;
            }
            Word = name;
        }
    }
}
