using HandyControl.Controls;
using SkEditorPlus.Utilities;

namespace SkEditorPlus.Windows
{
    public partial class ExportOptionsWindow : Window
    {
        private readonly SkEditorAPI skEditor;

        public ExportOptionsWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixer.FixBackground(this);

        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                exportOptionsWindow.Close();
            }
        }

        private void LocalPathChecked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void SftpChecked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
