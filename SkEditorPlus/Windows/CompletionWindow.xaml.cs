using SkEditorPlus.Utilities.Managers;
using System.Windows.Controls;
using System.Windows.Input;

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
