using SkEditorPlus.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkEditorPlus
{
    public interface SkEditorAPI
    {
        public MainWindow GetMainWindow();
        public event EventHandler WindowOpen;
        public string GetStartupFile();
    }
}
