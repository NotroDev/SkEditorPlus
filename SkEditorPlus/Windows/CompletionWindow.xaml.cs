using SkEditorPlus.Managers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus.Windows
{
    public partial class CompletionWindow : UserControl
    {

        public CompletionWindow()
        {
            InitializeComponent();
        }

        private void OnCompletionListKeyDown(object sender, KeyEventArgs e)
        {
            CompletionManager.instance.OnKeyDown(sender, e);
        }
    }
}
