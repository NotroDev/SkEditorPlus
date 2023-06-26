using HandyControl.Controls;
using SkEditorPlus.Utilities;

namespace SkEditorPlus.Windows.Generators
{
    public partial class ParticleGenerator : Window
    {
        private SkEditorAPI skEditor;

        public ParticleGenerator(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixer.FixBackground(this);

        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                particleGeneratorWindow.Close();
            }
        }

        private void Generate(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void rowsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
