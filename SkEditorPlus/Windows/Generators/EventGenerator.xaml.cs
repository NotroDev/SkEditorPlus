using HandyControl.Controls;

namespace SkEditorPlus.Windows.Generators
{
    public partial class EventGenerator : Window
    {
        private SkEditorAPI skEditor;

        public EventGenerator(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                eventGeneratorWindow.Close();
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
